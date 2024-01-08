using System.Runtime.InteropServices;

namespace Topten.TinyC;

/// <summary>
/// TCC Compiler object
/// </summary>
public class Compiler : IDisposable
{
    /// <summary>
    /// Constructs a new TCC Copmiler
    /// </summary>
    public Compiler()
    {
        // Create state
        _state = Interop.tcc_new();

        // Setup error handler callback
        _error_handler = _on_error;
        Interop.tcc_set_error_func(_state, IntPtr.Zero, _error_handler);
    }

    /// <summary>
    /// Set compile options as per command line args
    /// </summary>
    /// <param name="str">The command line options to set</param>
    /// <exception cref="TinyCException">Exception if failed</exception>
    public void SetOptions(string str)
    {
        int err = Interop.tcc_set_options(_state, str);
        if (err != 0)
            throw new TinyCException($"Failed to set options {err}", err);
    }

    /// <summary>
    /// Sets the output type of the compiled module
    /// </summary>
    public OutputType OutputType
    {
        get => _outputType;
        set
        {
            if (_outputTypeLocked)
                throw new InvalidOperationException("Can't change the output type once compilation started");
            _outputType = value;
        }
    }
    OutputType _outputType;
    bool _outputTypeLocked;

    /// <summary>
    /// Callback to be invoked on error message from compiler
    /// </summary>
    public Action<string> ErrorHandler = Console.WriteLine;

    /// <summary>
    /// Set the TCC root directory
    /// </summary>
    /// <param name="path">The path to set</param>
    public void SetLibPath(string path)
    {
        Interop.tcc_set_lib_path(_state, path);
    }

    /// <summary>
    /// Add an include path
    /// </summary>
    /// <param name="path">The path to add</param>
    public void AddIncludePath(string path)
    {
        Interop.tcc_add_include_path(_state, path);
    }

    /// <summary>
    /// Add a system include path
    /// </summary>
    /// <param name="path">The path to add</param>
    public void AddSysIncludePath(string path)
    {
        Interop.tcc_add_sysinclude_path(_state, path);
    }

    /// <summary>
    /// Define a pre-processor symbol
    /// </summary>
    /// <param name="symbol">The symbol to define</param>
    /// <param name="value">The symbol value</param>
    public void Define(string symbol, string value = null)
    {
        Interop.tcc_define_symbol(_state, symbol, value);
    }

    /// <summary>
    /// Undefine a preprocessor symbol
    /// </summary>
    /// <param name="symbol">The symbol to undefine</param>
    public void Undefine(string symbol)
    {
        Interop.tcc_undefine_symbol(_state, symbol);
    }

    void init_compile()
    {
        // Setup output type
        if (OutputType == OutputType.Unknown)
            throw new InvalidOperationException("Output type must be set before compilation");
        Interop.tcc_set_output_type(_state, OutputType);
        _outputTypeLocked = true;
    }

    /// <summary>
    /// Add a C, dll, object, library or ld script file
    /// </summary>
    /// <param name="filename">The file to add</param>
    /// <exception cref="TinyCException">Exception if failed</exception>
    public void AddFile(string filename)
    {
        init_compile();

        // Add file
        int err = Interop.tcc_add_file(_state, filename);
        if (err != 0)
            throw new TinyCException($"Adding file failed with code {err}", err);
    }

    /// <summary>
    /// Compile C Code froma string
    /// </summary>
    /// <param name="code">The code to compile</param>
    /// <exception cref="TinyCException">Exception if failed</exception>
    public void Compile(string code)
    {
        init_compile();

        // Compile string
        int err = Interop.tcc_compile_string(_state, code);
        if (err != 0)
            throw new TinyCException($"Compilation failed with code {err}", err);
    }

    /// <summary>
    /// Add a library search path
    /// </summary>
    /// <param name="pathname">The path</param>
    /// <exception cref="TinyCException">Exception if failed</exception>
    public void AddLibraryPath(string pathname)
    {
        // Add file
        int err = Interop.tcc_add_library_path(_state, pathname);
        if (err != 0)
            throw new TinyCException($"Adding library path failed with code {err}", err);
    }


    /// <summary>
    /// Add a library
    /// </summary>
    /// <param name="libname">The library name</param>
    /// <exception cref="TinyCException">Exception if failed</exception>
    public void AddLibrary(string libname)
    {
        // Add file
        int err = Interop.tcc_add_library(_state, libname);
        if (err != 0)
            throw new TinyCException($"Adding library failed with code {err}", err);
    }

    /// <summary>
    /// Write an output file
    /// </summary>
    /// <param name="filename">The file to write</param>
    /// <exception cref="TinyCException">Exception if failed</exception>
    public void OutputFile(string filename)
    {
        // Check output type is "Memory"
        if (_outputType != OutputType.Dll && 
            _outputType != OutputType.Exe && 
            _outputType != OutputType.Obj)
            throw new InvalidOperationException("Invalid output type for writing output file");

        // Add file
        int err = Interop.tcc_output_file(_state, filename);
        if (err != 0)
            throw new TinyCException($"Outputing file failed with code {err}", err);
    }

    /// <summary>
    /// Relocate the compiled module in memory
    /// </summary>
    /// <exception cref="TinyCException">Exception if failed</exception>
    public Module Relocate()
    {
        // Check output type is "Memory"
        if (_outputType != OutputType.Memory)
            throw new InvalidOperationException("Relocate can only be used with OutputType.Memory");

        // Allocate memory
        int size = Interop.tcc_relocate(_state, IntPtr.Zero);
        if (size < 0)
            throw new TinyCException("Failed to relocate", -1);
        IntPtr mem = Marshal.AllocHGlobal(size);

        // Relocate
        int err = Interop.tcc_relocate(_state, mem);
        if (err != 0)
        {
            Marshal.FreeHGlobal(mem);
            throw new TinyCException($"Relocation failed with code {err}", err);
        }

        // Build a dictionary of symbols
        var dict = new Dictionary<string, IntPtr>();
        Interop.tcc_list_symbols(_state, IntPtr.Zero, (ctx, sym, val) =>
        {
            dict.Add(sym, val);
        });

        // Get all symbols
        return new Module(mem, dict);
    }

    /// <summary>
    /// Add a symbol
    /// </summary>
    /// <param name="name">Name of the symbol</param>
    /// <param name="value">The symbol's value</param>
    /// <exception cref="TinyCException">Exception is failed</exception>
    public void AddSymbol(string name, IntPtr value)
    {
        int err = Interop.tcc_add_symbol(_state, name, value);
        if (err != 0)
            throw new TinyCException($"Failed to add symbol with code {err}", err);
    }

    /// <summary>
    /// Add a function delegate symbol
    /// </summary>
    /// <param name="name">Name of the symbol</param>
    /// <param name="value">The symbol's value</param>
    /// <exception cref="TinyCException">Exception is failed</exception>
    public void AddSymbol(string name, Delegate value)
    {
        int err = Interop.tcc_add_symbol(_state, name, value);
        if (err != 0)
            throw new TinyCException($"Failed to add symbol with code {err}", err);
    }

    /// <summary>
    /// Get the value of a symbol
    /// </summary>
    /// <param name="name">The symbol name</param>
    /// <returns>The symbol value</returns>
    /*
    public IntPtr GetSymbol(string name)
    {
        return Interop.tcc_get_symbol(_state, name);
    }
    */

    // Callback from TCC on error
    void _on_error(IntPtr opaque, string message)
    {
        // Invoke error handler action
        ErrorHandler?.Invoke(message);
    }

    Interop.tcc_error_delegate _error_handler;

    IntPtr _state;

    #region Dispose Pattern
    private bool isDisposed;
    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
            }

            Interop.tcc_delete(_state);
            isDisposed = true;
        }
    }

    ~Compiler()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
