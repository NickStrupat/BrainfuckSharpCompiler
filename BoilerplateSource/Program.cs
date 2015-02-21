using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BoilerplateSource {
	class Program {
		const Int32 stackSize = 30000;
		static readonly Byte[] stack = new Byte[stackSize];
		const Int32 stackStartIndex = 0;
		const Int32 stackEndIndex = stackSize - 1;
		static Int32 stackIndex = stackStartIndex;
		static Stack<Int64> jumps = new Stack<Int64>(); // instruction indexes

		static void Main(string[] args) {
			
		}
		static void GreaterThan() {
			++stackIndex;
			if (stackIndex == stackEndIndex)
				stackIndex = stackStartIndex;
		}
		static void LessThan() {
			if (stackIndex == stackStartIndex)
				stackIndex = stackEndIndex;
			--stackIndex;
		}
		static void Plus() {
			++stack[stackIndex];
		}
		static void Minus() {
			--stack[stackIndex];			
		}
		static void Dot() {
			Console.Write((Char)stack[stackIndex]);
		}
		static void Comma() {
			stack[stackIndex] = (Byte)Console.Read();
		}
		static void LeftBracket() {
			if (stack[stackIndex] == 0) {
				while (instructionStream.ReadByte() != ']')
					;
			}
			else
				jumps.Push(instructionStream.Position);
		}
		static void RightBracket() {
			if (stack[stackIndex] != 0)
				instructionStream.Seek(jumps.Peek(), SeekOrigin.Begin);
			else
				jumps.Pop();
		}
	}
}
