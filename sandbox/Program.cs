using System.Runtime.InteropServices;
using tcc.net;

using var c = new Compiler();
c.OutputType = OutputType.Memory;
c.SetLibPath(@"C:\Users\Brad\Projects\tcc\tinycc\win32");
c.Define("DELTA", "100");
c.Compile(@"

#include <setjmp.h>
#include <stdio.h>

jmp_buf jb;

void callback(int value);

void other()
{
    printf(""o1\n"");
    longjmp(jb, 1);
    printf(""o2\n"");
}

// This is C code
int add(int x, int y) 
{  
    printf(""a1\n"");
    if (setjmp(jb) == 0)
    {
        printf(""a2\n"");
        other();
        printf(""a3\n"");
    }
    printf(""a4\n"");
    return x + y + DELTA; 
} 

void main() 
{
    // nop
}
");

// Add callback symbol
void callback(int value)
{
    Console.WriteLine($"Callback with {value}");
}
del_callback cb = callback;
c.AddSymbol("callback", Marshal.GetFunctionPointerForDelegate(cb));

// Link
c.Relocate();

// List all symbols
Console.WriteLine("\nSymbols:");
foreach (var s in c.ListSymbols())
{
    Console.WriteLine($" {s.Value:X} {s.Name}");
}
Console.WriteLine();

// Call it
var code = c.GetSymbol("add");
var c_add = Marshal.GetDelegateForFunctionPointer<del_add>(code);
Console.WriteLine($"Hello TCC = {c_add(10, 13)}!");

// Delegate types
delegate int del_add(int a, int b);
delegate void del_callback(int value);
