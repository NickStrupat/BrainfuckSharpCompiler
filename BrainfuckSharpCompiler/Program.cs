using System;

namespace BrainfuckSharpCompiler {
	class Program {
		static readonly Type arrayElementType = typeof(Byte);
		static readonly Type arrayType = typeof(Byte[]);

		static void ThrowArgumentException() {
			throw new ArgumentException(String.Format("Usage: {0} source_file_path [/s:30000] [/u] [/i]", AppDomain.CurrentDomain.FriendlyName));
		}

		static void Main(String[] args) {
			if (args.Length < 1)
				ThrowArgumentException();
			String inputFilePath = null;
			UInt32 stackSize = 30000;
			Boolean @unsafe = false;
			Boolean inline = false;
			foreach (var arg in args) {
				if (arg[0] == '/')
					switch (arg[1]) {
						case 's':
							stackSize = UInt32.Parse(arg.Substring(3));
							break;
						case 'u':
							@unsafe = true;
							break;
						case 'i':
							inline = true;
							break;
						default:
							ThrowArgumentException();
							break;
					}
				else
					inputFilePath = arg;
			}

			CompilerBase compiler;
			if (@unsafe)
				compiler = new UnsafeCompiler(inputFilePath, stackSize, inline);
			else
				compiler = new SafeCompiler(inputFilePath, stackSize, inline);
			compiler.Compile();
		}
	}
}
