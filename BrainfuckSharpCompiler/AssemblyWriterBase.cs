using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace BrainfuckSharpCompiler {
	abstract class CompilerBase {
		protected static readonly Type arrayElementType = typeof(Byte);
		protected static readonly Type arrayType = typeof(Byte[]);

		private readonly Stream inputSource;
		private readonly UInt32 stackSize;
		private readonly Boolean inline;

		protected CompilerBase(String inputFileName, UInt32 stackSize, Boolean inline) {
			this.stackSize = stackSize;
			this.inline = inline;

			var assemblyName = new AssemblyName { Name = inputFileName };
			var appDomain = AppDomain.CurrentDomain;
			var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, inputFileName + ".exe");
			var typeBuilder = moduleBuilder.DefineType("Brainfuck.Program", TypeAttributes.Public | TypeAttributes.Class);

			stackFieldInfo = typeBuilder.DefineField("stack", arrayType, FieldAttributes.Static);
			stackIndexFieldInfo = typeBuilder.DefineField("stackIndex", typeof(Int32), FieldAttributes.Static);

			if (!inline) {
				var incrementStackIndexMethodBuilder = typeBuilder.DefineMethod("IncrementStackIndex", MethodAttributes.Static, CallingConventions.Standard, typeof (void), null);
				EmitIncrementStackIndexMethodInstructions(incrementStackIndexMethodBuilder.GetILGenerator());

				var decrementStackIndexMethodBuilder = typeBuilder.DefineMethod("DecrementStackIndex", MethodAttributes.Static, CallingConventions.Standard, typeof (void), null);
				EmitDecrementStackIndexMethodInstructions(decrementStackIndexMethodBuilder.GetILGenerator());

				var incrementStackByteMethodBuilder = typeBuilder.DefineMethod("IncrementStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof (void), null);
				EmitIncrementStackByteMethodInstructions(incrementStackByteMethodBuilder.GetILGenerator());

				var decrementStackByteMethodBuilder = typeBuilder.DefineMethod("DecrementStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof (void), null);
				EmitDecrementStackByteMethodInstructions(decrementStackByteMethodBuilder.GetILGenerator());

				var writeStackByteMethodBuilder = typeBuilder.DefineMethod("WriteStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof (void), null);
				EmitWriteStackByteMethodInstructions(writeStackByteMethodBuilder.GetILGenerator());

				var readStackByteMethodBuilder = typeBuilder.DefineMethod("ReadStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof (void), null);
				EmitReadStackByteMethodInstructions(readStackByteMethodBuilder.GetILGenerator());
			}

			var fb = typeBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(void), new[] { typeof(String[]) });
			var ilg = fb.GetILGenerator();
			this.inputSource = inputSource;


		}

		protected abstract void EmitIncrementStackIndexMethodInstructions(ILGenerator ilGenerator);
		protected abstract void EmitDecrementStackIndexMethodInstructions(ILGenerator ilGenerator);
		protected abstract void EmitIncrementStackByteMethodInstructions(ILGenerator ilGenerator);
		protected abstract void EmitDecrementStackByteMethodInstructions(ILGenerator ilGenerator);
		protected abstract void EmitWriteStackByteMethodInstructions(ILGenerator ilGenerator);
		protected abstract void EmitReadStackByteMethodInstructions(ILGenerator ilGenerator);
		protected abstract void EmitBeginLoopMethodInstructions(ILGenerator ilGenerator);
		protected abstract void EmitEndLoopMethodInstructions(ILGenerator ilGenerator);
	}
}