# BrainfuckSharpCompiler
A brainfuck compiler written in C#. Compiles to an executable .NET assembly.

There is some boring stuff like setting up the assembly, module, type, Main method, stack, and stack pointer for emitting IL instructions and finally by writing the assembly to the filesystem as a .NET/Mono executable. I went with using a `Byte[]` for the stack, and an `Int32` for an index of the stack (effectively acting like a stack pointer).

Here's the fun stuff...

When compiling the brainfuck program, we read the file character by character. The only valid ones, of course, are the eight valid operations...

    <>+-,.[]

... for each of these we just need to emit the correct IL instructions which correspond to the correct behaviour at run-time in the CLR environment.

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
	ldc.i4.1                      ' 
	add                           ' 
	conv.u1                       ' 
	stobj uint8                   ' 