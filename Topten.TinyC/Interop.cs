using System.Reflection;
using System.Runtime.InteropServices;

namespace Topten.TinyC;

public static partial class Interop
{
    const string libname = "libtcc";

    static Interop()
    {
        NativeLibrary.SetDllImportResolver(typeof(Interop).Assembly, ImportResolver);
    }

    private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        // libtcc?
        if (libraryName != libname)
            return IntPtr.Zero;

        // Get base directory
        string baseDir = Path.GetDirectoryName(typeof(Interop).Assembly.Location);

        var libpath = Path.Combine(baseDir, FormatNativeLibraryPath(libraryName));


        if (File.Exists(libpath))
            return NativeLibrary.Load(libpath);
        return IntPtr.Zero;
    }

    public static string FormatNativeLibraryPath(string libname)
    {
        // Work out library location and name
        string os = "";
        string prefix = "";
        string suffix = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            os = "win32";
            suffix = ".dll";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            os = "Linux";
            suffix = ".so";
        }

        string arch = "";
        switch (RuntimeInformation.OSArchitecture)
        {
            case Architecture.X64:
                arch = "x64";
                break;

            case Architecture.X86:
                arch = "x86";
                break;

            case Architecture.Arm:
                arch = "arm";
                break;

            case Architecture.Arm64:
                arch = "aarch64";
                break;
        }

        return Path.Combine(os, arch, $"{prefix}{libname}{suffix}");
    }


    [DllImport(libname)]
    public static extern IntPtr tcc_new();

    [DllImport(libname)]
    public static extern void tcc_delete(IntPtr s);

    [DllImport(libname)]
    public static extern int tcc_set_options(IntPtr s, [MarshalAs(UnmanagedType.LPStr)] string str);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tcc_error_delegate(IntPtr opaque, [MarshalAs(UnmanagedType.LPStr)] string msg);

    [DllImport(libname)]
    public static extern void tcc_set_error_func(IntPtr s, IntPtr error_opaque, tcc_error_delegate error_func);

    [DllImport(libname)]
    public static extern int tcc_add_file(IntPtr s, [MarshalAs(UnmanagedType.LPStr)] string filename);

    [DllImport(libname)]
    public static extern int tcc_compile_string(IntPtr s, [MarshalAs(UnmanagedType.LPStr)] string code);

    [DllImport(libname)]
    public static extern int tcc_set_output_type(IntPtr s, OutputType output_type);

    [DllImport(libname)]
    public static extern int tcc_add_include_path(IntPtr s, [MarshalAs(UnmanagedType.LPStr)] string pathname);

    [DllImport(libname)]
    public static extern int tcc_add_sysinclude_path(IntPtr s, [MarshalAs(UnmanagedType.LPStr)] string pathname);

    [DllImport(libname)]
    public static extern void tcc_define_symbol(IntPtr s, [MarshalAs(UnmanagedType.LPStr)] string sym, [MarshalAs(UnmanagedType.LPStr)] string value);

    [DllImport(libname)]
    public static extern void tcc_undefine_symbol(IntPtr s, [MarshalAs(UnmanagedType.LPStr)] string sym);

    [DllImport(libname)]
    public static extern void tcc_set_lib_path(IntPtr s, [MarshalAs(UnmanagedType.LPStr)] string path);

    public static IntPtr TCC_RELOCATE_AUTO = (IntPtr)1;

    [DllImport(libname)]
    public static extern int tcc_relocate(IntPtr s, IntPtr ptr);

    [DllImport(libname)]
    public static extern int tcc_add_symbol(IntPtr s, [MarshalAs(UnmanagedType.LPStr)] string name, IntPtr val);

    [DllImport(libname)]
    public static extern int tcc_add_library_path(IntPtr s, [MarshalAs(UnmanagedType.LPStr)] string pathname);

    [DllImport(libname)]
    public static extern int tcc_add_library(IntPtr s, [MarshalAs(UnmanagedType.LPStr)] string libraryname);

    [DllImport(libname)]
    public static extern int tcc_output_file(IntPtr s, [MarshalAs(UnmanagedType.LPStr)] string libraryname);

    [DllImport(libname)]
    public static extern int tcc_add_symbol(IntPtr s, [MarshalAs(UnmanagedType.LPStr)] string name, Delegate val);

    [DllImport(libname)]
    public static extern IntPtr tcc_get_symbol(IntPtr s, [MarshalAs(UnmanagedType.LPStr)] string name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tcc_list_symbols_delegate(IntPtr ctx, [MarshalAs(UnmanagedType.LPStr)] string name, IntPtr value);

    [DllImport(libname)]
    public static extern void tcc_list_symbols(IntPtr s, IntPtr ctx, tcc_list_symbols_delegate callback);
}
