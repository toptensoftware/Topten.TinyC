using System.Runtime.InteropServices;
using Topten.TinyC;

using var c = new Compiler();
c.OutputType = OutputType.Memory;
c.Define("DELTA", "100");
c.Compile(@"

#include <stdio.h>

void callback(int value);

void other()
{
    printf(""o1\n"");
    printf(""o2\n"");
}

// This is C code
int add(int x, int y) 
{  
    printf(""sizeof(void*) = %i\n"", sizeof(void*));
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
var m = c.Relocate();

// List all symbols
Console.WriteLine("\nSymbols:");
foreach (var s in m.Symbols)
{
    Console.WriteLine($" {s.Value:X} {s.Key}");
}
Console.WriteLine();

var sw = new StringWriter();
Console.SetOut(sw);

// Call it
var code = m.Symbols["add"];
var c_add = Marshal.GetDelegateForFunctionPointer<del_add>(code);
Console.WriteLine($"Hello TCC = {c_add(10, 13)}!");


// Delegate types
delegate int del_add(int a, int b);
delegate void del_callback(int value);
