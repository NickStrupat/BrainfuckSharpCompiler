# BrainfuckSharpCompiler
A brainfuck compiler written in C#. It compiles brainfuck programs to an executable .NET/Mono assembly.

There is some boring stuff like setting up the assembly, module, type, Main method, stack, and stack pointer for emitting IL instructions and finally by writing the assembly to the file system as a .NET/Mono executable. By default, the compiler builds a safe implementation. I went with using a `Byte[]` for the stack, and an `Int32` for an index of the stack (effectively acting like a stack pointer). If the unsafe switch (/u) is provided as a command line argument, the compiler will produce an unsafe implementation using a stack-allocated array of `Byte` for the stack and a `Byte *` for the stack pointer. This will be faster than a safe implementation in many cases, but is susceptible to undefined behaviour if a brainfuck program overruns the stack.

Here's the fun stuff...

When compiling the brainfuck program, we read the file character by character. The only valid ones, of course, are the eight valid operations...

    <>+-,.[]

... for each of these we just need to emit the correct IL instructions which correspond to the correct behaviour at run-time in the CLR environment.

## Safe instructions emitted without the unsafe switch (/u)

If we find a `<`, let's emit the IL instructions which subtract 1 from the stack index, effectively moving the stack pointer one to the "left"

	ldsfld int32 stackIndex       ' load a static field of type int32 named stackIndex
	ldc.i4.1                      ' load an int32 of value 1
	sub                           ' subtract the first value from the second
	stsfld int32 stackIndex       ' store the result into a static field of type in32 named stackIndex
	
The IL instructions for `>` are the same except we want to add 1 to the stack index, effectively moving the stack pointer one to the "right"

	ldsfld int32 stackIndex       ' load a static field of type int32 named stackIndex
	ldc.i4.1                      ' load an int32 of value 1
	add                           ' add the two values
	stsfld int32 stackIndex       ' store the result into a static field of type in32 named stackIndex

If we find a `+`, let's emit the IL instructions which add 1 to the byte on the stack currently indexed by the stack index

	ldsfld uint8[] stack          ' load a static field of type uint8[] named stack
	ldsfld int32 stackIndex       ' load a static field of type int32 named stackIndex
	ldelema uint8                 ' load the address of the element in the array referenced by the first value, indexed by the second value
	dup                           ' 
	ldobj uint8                   ' 
	ldc.i4.1                      ' load an in32 of value 1
	add                           ' add the two values
	conv.u1                       ' convert to byte
	stobj uint8                   ' store the byte in the stack array at the position indexed by stackIndex