using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BrainfuckSharpCompiler {
	class UnsafeCompiler : CompilerBase {
		readonly Stack<Label> loopLabels = new Stack<Label>();
		readonly FieldBuilder stackPointer;

		public UnsafeCompiler(string inputFileName, uint stackSize, bool inline) : base(inputFileName, stackSize, inline) {
			if (!inline)
				stackPointer = ProgramTypeBuilder.DefineField("stackPointer", typeof (Byte *), FieldAttributes.Static);
			var stack = MainIlGenerator.DeclareLocal(typeof(Byte *));
			// initialize stack
			MainIlGenerator.Emit(OpCodes.Ldc_I4, stackSize);
			MainIlGenerator.Emit(OpCodes.Conv_U);
			MainIlGenerator.Emit(OpCodes.Localloc);
			MainIlGenerator.Emit(OpCodes.Stloc_0);
			if (!inline) {
				// assign stack pointer to the address of the first element in the stack
				MainIlGenerator.Emit(OpCodes.Ldloc_0);
				MainIlGenerator.Emit(OpCodes.Stsfld, stackPointer);
			}
		}

		protected override void EmitIncrementStackIndexMethodInstructions(ILGenerator ilGenerator) {
			EmitStackIndexMethodInstructions(ilGenerator, OpCodes.Add);
		}

		protected override void EmitDecrementStackIndexMethodInstructions(ILGenerator ilGenerator) {
			EmitStackIndexMethodInstructions(ilGenerator, OpCodes.Sub);
		}

		void EmitStackIndexMethodInstructions(ILGenerator ilGenerator, OpCode addOrSub) {
			if (Inline)
				ilGenerator.Emit(OpCodes.Ldloc_0);
			else
				ilGenerator.Emit(OpCodes.Ldsfld, stackPointer);

			ilGenerator.Emit(OpCodes.Ldc_I4_1);
			ilGenerator.Emit(OpCodes.Conv_I);
			ilGenerator.Emit(addOrSub);
			
			if (Inline)
				ilGenerator.Emit(OpCodes.Stloc_0);
			else
				ilGenerator.Emit(OpCodes.Stsfld, stackPointer);
		}

		protected override void EmitIncrementStackByteMethodInstructions(ILGenerator ilGenerator) {
			EmitStackByteMethodInstructions(ilGenerator, OpCodes.Add);
		}

		protected override void EmitDecrementStackByteMethodInstructions(ILGenerator ilGenerator) {
			EmitStackByteMethodInstructions(ilGenerator, OpCodes.Sub);
		}

		void EmitStackByteMethodInstructions(ILGenerator ilGenerator, OpCode addOrSub) {
			if (Inline)
				ilGenerator.Emit(OpCodes.Ldloc_0);
			else
				ilGenerator.Emit(OpCodes.Ldsfld, stackPointer);

			ilGenerator.Emit(OpCodes.Dup);
			ilGenerator.Emit(OpCodes.Ldind_U1);
			ilGenerator.Emit(OpCodes.Ldc_I4_1);
			ilGenerator.Emit(addOrSub);
			ilGenerator.Emit(OpCodes.Conv_U1);
			ilGenerator.Emit(OpCodes.Stind_I1);
		}

		static readonly MethodInfo consoleWriteChar = new Action<Char>(Console.Write).Method;
		protected override void EmitWriteStackByteMethodInstructions(ILGenerator ilGenerator) {
			if (Inline)
				ilGenerator.Emit(OpCodes.Ldloc_0);
			else
				ilGenerator.Emit(OpCodes.Ldsfld, stackPointer);

			ilGenerator.Emit(OpCodes.Ldind_U1);
			ilGenerator.Emit(OpCodes.Call, consoleWriteChar);
		}

		static readonly MethodInfo consoleRead = new Func<Int32>(Console.Read).Method;
		protected override void EmitReadStackByteMethodInstructions(ILGenerator ilGenerator) {
			if (Inline)
				ilGenerator.Emit(OpCodes.Ldloc_0);
			else
				ilGenerator.Emit(OpCodes.Ldsfld, stackPointer);

			ilGenerator.Emit(OpCodes.Call, consoleRead);
			ilGenerator.Emit(OpCodes.Conv_U1);
			ilGenerator.Emit(OpCodes.Stind_I1);
		}

		protected override void EmitBeginLoopMethodInstructions(ILGenerator ilGenerator) {
			var top = ilGenerator.DefineLabel();
			var bottom = ilGenerator.DefineLabel();
			ilGenerator.Emit(OpCodes.Br, bottom);
			ilGenerator.MarkLabel(top);
			loopLabels.Push(bottom);
			loopLabels.Push(top);
		}

		protected override void EmitEndLoopMethodInstructions(ILGenerator ilGenerator) {
			var top = loopLabels.Pop();
			var bottom = loopLabels.Pop();
			ilGenerator.MarkLabel(bottom);
			if (Inline)
				ilGenerator.Emit(OpCodes.Ldloc_0);
			else
				ilGenerator.Emit(OpCodes.Ldsfld, stackPointer);
			ilGenerator.Emit(OpCodes.Ldind_U1);
			ilGenerator.Emit(OpCodes.Brtrue, top);
		}
	}
}
