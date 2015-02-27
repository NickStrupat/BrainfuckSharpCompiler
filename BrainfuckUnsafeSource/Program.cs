using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrainfuckUnsafeSource {
	class Program {
		static unsafe void Main(string[] args) {
			Byte * stackPointer = stackalloc Byte[30000];
			++stackPointer;
			--stackPointer;
			++(*stackPointer);
			--(*stackPointer);
			*stackPointer = (Byte)Console.Read();
			Console.WriteLine(*stackPointer);

		}
	}
}
