using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrainfuckUnsafeSource {
	class Program {
		static unsafe Byte * stackPointer;
		static unsafe void Main(string[] args) {
			Byte * stack = stackalloc Byte[30000];
			stackPointer = stack;
			++stackPointer;
			--stackPointer;
			++(*stackPointer);
			--(*stackPointer);
			*stackPointer = (Byte)Console.Read();
			Console.WriteLine(*stackPointer);

			while (*stackPointer != 0) {
				++stackPointer;
			}
		}
	}
}
