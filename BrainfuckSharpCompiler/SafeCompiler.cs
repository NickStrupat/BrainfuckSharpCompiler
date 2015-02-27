using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BrainfuckSharpCompiler {
	class SafeCompiler : CompilerBase {
		static FieldInfo stackFieldInfo;
		static FieldInfo stackIndexFieldInfo;
		readonly Stack<Label> loopLabels = new Stack<Label>();

		public SafeCompiler(String inputFileName, UInt32 stackSize, Boolean inline) : base(inputFileName, stackSize, inline) {
			
		}

		protected override void EmitIncrementStackIndexMethodInstructions(ILGenerator ilGenerator) {
			EmitStackIndexMethodInstructions(ilGenerator, OpCodes.Add);
		}

		protected override void EmitDecrementStackIndexMethodInstructions(ILGenerator ilGenerator) {
			EmitStackIndexMethodInstructions(ilGenerator, OpCodes.Sub);
		}

		protected override void EmitIncrementStackByteMethodInstructions(ILGenerator ilGenerator) {
			EmitStackByteMethodInstructions(ilGenerator, OpCodes.Add);
		}

		protected override void EmitDecrementStackByteMethodInstructions(ILGenerator ilGenerator) {
			EmitStackByteMethodInstructions(ilGenerator, OpCodes.Sub);
		}

		static readonly MethodInfo consoleWriteChar = new Action<Char>(Console.Write).Method;
		protected override void EmitWriteStackByteMethodInstructions(ILGenerator ilGenerator) {
			ilGenerator.Emit(OpCodes.Ldsfld, stackFieldInfo);
			ilGenerator.Emit(OpCodes.Ldsfld, stackIndexFieldInfo);
			ilGenerator.Emit(OpCodes.Ldelem_U1);
			ilGenerator.Emit(OpCodes.Call, consoleWriteChar);
			ilGenerator.Emit(OpCodes.Ret);
		}

		static readonly MethodInfo consoleRead = new Func<Int32>(Console.Read).Method;
		protected override void EmitReadStackByteMethodInstructions(ILGenerator ilGenerator) {
			ilGenerator.Emit(OpCodes.Ldsfld, stackFieldInfo);
			ilGenerator.Emit(OpCodes.Ldsfld, stackIndexFieldInfo);
			ilGenerator.Emit(OpCodes.Call, consoleRead);
			ilGenerator.Emit(OpCodes.Conv_U1);
			ilGenerator.Emit(OpCodes.Stelem_I1);
			ilGenerator.Emit(OpCodes.Ret);
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
			ilGenerator.Emit(OpCodes.Ldsfld, stackFieldInfo);
			ilGenerator.Emit(OpCodes.Ldsfld, stackIndexFieldInfo);
			ilGenerator.Emit(OpCodes.Ldelem_U1);
			ilGenerator.Emit(OpCodes.Brtrue, top);
		}

		static void EmitStackIndexMethodInstructions(ILGenerator ilGenerator, OpCode addOrSub) {
			ilGenerator.Emit(OpCodes.Ldsfld, stackIndexFieldInfo);
			ilGenerator.Emit(OpCodes.Ldc_I4_1);
			ilGenerator.Emit(addOrSub);
			ilGenerator.Emit(OpCodes.Stsfld, stackIndexFieldInfo);
			ilGenerator.Emit(OpCodes.Ret);
		}

		static void EmitStackByteMethodInstructions(ILGenerator ilGenerator, OpCode addOrSub) {
			ilGenerator.Emit(OpCodes.Ldsfld, stackFieldInfo);
			ilGenerator.Emit(OpCodes.Ldsfld, stackIndexFieldInfo);
			ilGenerator.Emit(OpCodes.Ldelema, arrayElementType);
			ilGenerator.Emit(OpCodes.Dup);
			ilGenerator.Emit(OpCodes.Ldobj, arrayElementType);
			ilGenerator.Emit(OpCodes.Ldc_I4_1);
			ilGenerator.Emit(addOrSub);
			ilGenerator.Emit(OpCodes.Conv_U1);
			ilGenerator.Emit(OpCodes.Stobj, arrayElementType);
			ilGenerator.Emit(OpCodes.Ret);
		}
	}
}
