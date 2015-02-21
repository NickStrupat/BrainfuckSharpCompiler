using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BoilerplateSource {
	class Program {
		//static Stack<Int64> jumps = new Stack<Int64>(); // instruction indexes

		const Int32 stackSize = 30000;
		const Int32 stackStartIndex = 0;
		const Int32 stackEndIndex = stackSize - 1;
		static Byte[] stack;
		static Int32 stackIndex;
		static void Main(string[] args) {
			stack = new Byte[stackSize];
			stackIndex = stackStartIndex;






		}
		static void IncrementStackByte() {
			++stack[stackIndex];
		}
		static void DecrementStackByte() {
			--stack[stackIndex];
		}
		static void IncrementStackIndex() {
			++stackIndex;
		}
		static void DecrementStackIndex() {
			--stackIndex;
		}
		static void WriteStackByte() {
			Console.Write((Char)stack[stackIndex]);
		}
		static void ReadStackByte() {
			stack[stackIndex] = (Byte)Console.Read();
		}

		//static void GreaterThan() {
		//	++stackIndex;
		//	//if (stackIndex == stackEndIndex)
		//	//	stackIndex = stackStartIndex;
		//}
		//static void LessThan() {
		//	//if (stackIndex == stackStartIndex)
		//	//	stackIndex = stackEndIndex;
		//	--stackIndex;
		//}
		//static void Plus() {
		//	++stack[stackIndex];
		//}
		//static void Minus() {
		//	--stack[stackIndex];			
		//}
		//static void Dot() {
		//	Console.Write((Char)stack[stackIndex]);
		//}
		//static void Comma() {
		//	stack[stackIndex] = (Byte)Console.Read();
		//}
		//static void LeftBracket() {
		//	if (stack[stackIndex] == 0) {
		//		while (instructionStream.ReadByte() != ']')
		//			;
		//	}
		//	else
		//		jumps.Push(instructionStream.Position);
		//}
		//static void RightBracket() {
		//	if (stack[stackIndex] != 0)
		//		instructionStream.Seek(jumps.Peek(), SeekOrigin.Begin);
		//	else
		//		jumps.Pop();
		//}
	}
}
