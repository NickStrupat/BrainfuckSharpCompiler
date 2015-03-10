# BrainfuckSharpCompiler
A brainfuck compiler written in C#. It compiles brainfuck programs to an executable .NET/Mono assembly.

There is some boring stuff like setting up the assembly, module, type, Main method, stack, and stack pointer for emitting IL instructions and finally by writing the assembly to the file system as a .NET/Mono executable. By default, the compiler builds a safe implementation. I went with using a `Byte[]` for the stack, and an `Int32` for an index of the stack (effectively acting like a stack pointer). If the unsafe switch (/u) is provided as a command line argument, the compiler will produce an unsafe implementation using a stack-allocated array of `Byte` for the stack and a `Byte *` for the stack pointer. This will be faster than a safe implementation in many cases, but is susceptible to undefined behaviour if a brainfuck program overruns the stack.

Here's the fun stuff...

When compiling the brainfuck program, we read the file character by character. The only valid ones, of course, are the eight valid operations...

    <>+-,.[]

... for each of these we just need to emit the correct IL instructions which correspond to the correct behaviour at run-time in the CLR environment.
