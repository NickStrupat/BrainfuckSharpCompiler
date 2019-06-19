using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BrainfuckSharpCompiler {
	class SafeCompiler : Compiler {
		static readonly Type arrayElementType = typeof(Byte);
		static readonly Type arrayType = typeof(Byte[]);

		static FieldInfo stackFieldInfo;
		static FieldInfo stackIndexFieldInfo;
		readonly Stack<Label> loopLabels = new Stack<Label>();

		public SafeCompiler(String inputFilePath, UInt32 stackSize, Boolean inline) : base(inputFilePath, stackSize, inline) {
			stackFieldInfo = ProgramTypeBuilder.DefineField("stack", arrayType, FieldAttributes.Static);
			stackIndexFieldInfo = ProgramTypeBuilder.DefineField("stackIndex", typeof(Int32), FieldAttributes.Static);

			// initialize stack
			MainIlGenerator.Emit(OpCodes.Ldc_I4, stackSize);
			MainIlGenerator.Emit(OpCodes.Newarr, arrayElementType);
			MainIlGenerator.Emit(OpCodes.Stsfld, stackFieldInfo);

			// initialize stack index
			MainIlGenerator.Emit(OpCodes.Ldc_I4_0);
			MainIlGenerator.Emit(OpCodes.Stsfld, stackIndexFieldInfo);
		}

		protected override void EmitIncrementStackIndexMethodInstructions(ILGenerator ilGenerator) {
			EmitStackIndexMethodInstructions(ilGenerator, OpCodes.Add);
		}

		protected override void EmitDecrementStackIndexMethodInstructions(ILGenerator ilGenerator) {
			EmitStackIndexMethodInstructions(ilGenerator, OpCodes.Sub);
		}

		void EmitStackIndexMethodInstructions(ILGenerator ilGenerator, OpCode addOrSub) {
			ilGenerator.Emit(OpCodes.Ldsfld, stackIndexFieldInfo);
			ilGenerator.Emit(OpCodes.Ldc_I4_1);
			ilGenerator.Emit(addOrSub);
			ilGenerator.Emit(OpCodes.Stsfld, stackIndexFieldInfo);
		}

		protected override void EmitIncrementStackByteMethodInstructions(ILGenerator ilGenerator) {
			EmitStackByteMethodInstructions(ilGenerator, OpCodes.Add);
		}

		protected override void EmitDecrementStackByteMethodInstructions(ILGenerator ilGenerator) {
			EmitStackByteMethodInstructions(ilGenerator, OpCodes.Sub);
		}

		void EmitStackByteMethodInstructions(ILGenerator ilGenerator, OpCode addOrSub) {
			ilGenerator.Emit(OpCodes.Ldsfld, stackFieldInfo);
			ilGenerator.Emit(OpCodes.Ldsfld, stackIndexFieldInfo);
			ilGenerator.Emit(OpCodes.Ldelema, arrayElementType);
			ilGenerator.Emit(OpCodes.Dup);
			ilGenerator.Emit(OpCodes.Ldobj, arrayElementType);
			ilGenerator.Emit(OpCodes.Ldc_I4_1);
			ilGenerator.Emit(addOrSub);
			ilGenerator.Emit(OpCodes.Conv_U1);
			ilGenerator.Emit(OpCodes.Stobj, arrayElementType);
		}

		static readonly MethodInfo consoleWriteChar = new Action<Char>(Console.Write).Method;
		protected override void EmitWriteStackByteMethodInstructions(ILGenerator ilGenerator) {
			ilGenerator.Emit(OpCodes.Ldsfld, stackFieldInfo);
			ilGenerator.Emit(OpCodes.Ldsfld, stackIndexFieldInfo);
			ilGenerator.Emit(OpCodes.Ldelem_U1);
			ilGenerator.Emit(OpCodes.Call, consoleWriteChar);
		}

		static readonly MethodInfo consoleRead = new Func<Int32>(Console.Read).Method;
		protected override void EmitReadStackByteMethodInstructions(ILGenerator ilGenerator) {
			ilGenerator.Emit(OpCodes.Ldsfld, stackFieldInfo);
			ilGenerator.Emit(OpCodes.Ldsfld, stackIndexFieldInfo);
			ilGenerator.Emit(OpCodes.Call, consoleRead);
			ilGenerator.Emit(OpCodes.Conv_U1);
			ilGenerator.Emit(OpCodes.Stelem_I1);
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
	}
}
