using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BoilerplateSource {
	class Program {
		//static Stack<Int64> jumps = new Stack<Int64>(); // instruction indexes

		static void Main(string[] args) {
			const Int32 stackSize = 30000;
			const Int32 stackStartIndex = 0;
			const Int32 stackEndIndex = stackSize - 1;
			var stack = new Byte[stackSize];
			var stackIndex = stackStartIndex;

			++stackIndex;

			--stackIndex;

			++stack[stackIndex];

			--stack[stackIndex];

			Console.Write((Char)stack[stackIndex]);

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
