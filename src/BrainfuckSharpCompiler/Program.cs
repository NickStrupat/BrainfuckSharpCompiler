using System;
using CommandLine;

namespace BrainfuckSharpCompiler
{
	class Program
	{
		static void Main(String[] args) =>
			Parser.Default
				.ParseArguments<Options>(args)
				.WithParsed(o => Compiler.Create(o.Input, o.StackSize, o.Inline, o.Unsafe).Compile());
	}

	class Options
	{
		[Value(0, HelpText = "Input source code file.")]
		public String Input { get; set; }

		[Option('s', "stack-size", Required = false, HelpText = "Set the stack size in bytes (default: 30,000).")]
		public UInt32 StackSize { get; set; } = 30_000;

		[Option('u', "unsafe", Required = false, HelpText = "Use an unsafe byte pointer instead of a byte array.")]
		public Boolean Unsafe { get; set; }

		[Option('i', "inline", Required = false, HelpText = "Inline all the operations instead of each being a method.")]
		public Boolean Inline { get; set; }
	}
}
