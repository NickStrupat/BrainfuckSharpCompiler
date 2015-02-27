using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace BrainfuckSharpCompiler {
	abstract class CompilerBase {
		private readonly AssemblyBuilder assemblyBuilder;
		protected readonly TypeBuilder ProgramTypeBuilder;
		private readonly MethodBuilder mainMethodBuilder;
		protected readonly ILGenerator MainIlGenerator;

		private readonly String inputFileName;
		private readonly UInt32 stackSize;
		protected readonly Boolean Inline;

		Action EmitInvokeIncrementStackIndexMethodInstructions;
		Action EmitInvokeDecrementStackIndexMethodInstructions;
		Action EmitInvokeIncrementStackByteMethodInstructions;
		Action EmitInvokeDecrementStackByteMethodInstructions;
		Action EmitInvokeWriteStackByteMethodInstructions;
		Action EmitInvokeReadStackByteMethodInstructions;

		private MethodBuilder incrementStackIndexMethodBuilder;
		private MethodBuilder decrementStackIndexMethodBuilder;
		private MethodBuilder incrementStackByteMethodBuilder;
		private MethodBuilder decrementStackByteMethodBuilder;
		private MethodBuilder writeStackByteMethodBuilder;
		private MethodBuilder readStackByteMethodBuilder;

		protected CompilerBase(String inputFileName, UInt32 stackSize, Boolean inline) {
			this.inputFileName = inputFileName;
			this.stackSize = stackSize;
			this.Inline = inline;

			var assemblyName = new AssemblyName { Name = inputFileName };
			var appDomain = AppDomain.CurrentDomain;
			assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, inputFileName + ".exe");
			ProgramTypeBuilder = moduleBuilder.DefineType("Brainfuck.Program", TypeAttributes.Public | TypeAttributes.Class);

			mainMethodBuilder = ProgramTypeBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(void), new[] { typeof(String[]) });
			MainIlGenerator = mainMethodBuilder.GetILGenerator();
		}

		struct MethodEmitPair {
			public readonly String Name;
			public readonly Action<ILGenerator> MethodEmitter;
			public readonly Action EmitInvokeMethodInstructions;

			public MethodEmitPair(String name, Action<ILGenerator> methodEmitter, Action<Action> emitInvokeMethodInstructions) {
				Name = name;
				MethodEmitter = methodEmitter;
				EmitInvokeMethodInstructions = emitInvokeMethodInstructions;
			}
		}

		public void Compile() {
			if (Inline) {
				
			}
			else {
				var methodEmitPairs = new[] {
					                            new MethodEmitPair("IncrementStackIndex", EmitIncrementStackIndexMethodInstructions, a => EmitInvokeIncrementStackIndexMethodInstructions = a),
					                            new MethodEmitPair("DecrementStackIndex", EmitIncrementStackIndexMethodInstructions, a => EmitInvokeDecrementStackIndexMethodInstructions = a),
					                            new MethodEmitPair("IncrementStackByte",  EmitIncrementStackIndexMethodInstructions, a => EmitInvokeIncrementStackByteMethodInstructions = a),
					                            new MethodEmitPair("DecrementStackByte",  EmitIncrementStackIndexMethodInstructions, a => EmitInvokeDecrementStackByteMethodInstructions = a),
					                            new MethodEmitPair("WriteStackByte",      EmitIncrementStackIndexMethodInstructions, a => EmitInvokeWriteStackByteMethodInstructions = a),
					                            new MethodEmitPair("ReadStackByte",       EmitIncrementStackIndexMethodInstructions, a => EmitInvokeReadStackByteMethodInstructions = a),
				                            };
				foreach (var methodEmitPair in methodEmitPairs) {
					var methodBuilder = ProgramTypeBuilder.DefineMethod(methodEmitPair.Name, MethodAttributes.Static, CallingConventions.Standard, typeof (void), null);
					var ilGenerator = methodBuilder.GetILGenerator();
					methodEmitPair.MethodEmitter(ilGenerator);
					ilGenerator.Emit(OpCodes.Ret);
					methodEmitPair.EmitInvokeMethodInstructions(() => MainIlGenerator.Emit(OpCodes.Call, methodBuilder));
				}

				incrementStackIndexMethodBuilder = ProgramTypeBuilder.DefineMethod("IncrementStackIndex", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
				EmitIncrementStackIndexMethodInstructions(incrementStackIndexMethodBuilder.GetILGenerator());

				decrementStackIndexMethodBuilder = ProgramTypeBuilder.DefineMethod("DecrementStackIndex", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
				EmitDecrementStackIndexMethodInstructions(decrementStackIndexMethodBuilder.GetILGenerator());

				incrementStackByteMethodBuilder = ProgramTypeBuilder.DefineMethod("IncrementStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
				EmitIncrementStackByteMethodInstructions(incrementStackByteMethodBuilder.GetILGenerator());

				decrementStackByteMethodBuilder = ProgramTypeBuilder.DefineMethod("DecrementStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
				EmitDecrementStackByteMethodInstructions(decrementStackByteMethodBuilder.GetILGenerator());

				writeStackByteMethodBuilder = ProgramTypeBuilder.DefineMethod("WriteStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
				EmitWriteStackByteMethodInstructions(writeStackByteMethodBuilder.GetILGenerator());

				readStackByteMethodBuilder = ProgramTypeBuilder.DefineMethod("ReadStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
				EmitReadStackByteMethodInstructions(readStackByteMethodBuilder.GetILGenerator());
			}

			var instructionStream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
			Int32 byteRead;
			while ((byteRead = instructionStream.ReadByte()) != -1) {
				var instruction = (Char)byteRead;
				switch (instruction) {
					case '>':
						if (Inline)
							EmitIncrementStackIndexMethodInstructions(MainIlGenerator);
						else
							MainIlGenerator.Emit(OpCodes.Call, incrementStackIndexMethodBuilder);
						break;
					case '<':
						if (Inline)
							EmitDecrementStackIndexMethodInstructions(MainIlGenerator);
						else
							MainIlGenerator.Emit(OpCodes.Call, decrementStackIndexMethodBuilder);
						break;
					case '+':
						if (Inline)
							EmitIncrementStackByteMethodInstructions(MainIlGenerator);
						else
							MainIlGenerator.Emit(OpCodes.Call, incrementStackByteMethodBuilder);
						break;
					case '-':
						if (Inline)
							EmitDecrementStackByteMethodInstructions(MainIlGenerator);
						else
							MainIlGenerator.Emit(OpCodes.Call, decrementStackByteMethodBuilder);
						break;
					case '.':
						if (Inline)
							EmitWriteStackByteMethodInstructions(MainIlGenerator);
						else
							MainIlGenerator.Emit(OpCodes.Call, writeStackByteMethodBuilder);
						break;
					case ',':
						if (Inline)
							EmitReadStackByteMethodInstructions(MainIlGenerator);
						else
							MainIlGenerator.Emit(OpCodes.Call, readStackByteMethodBuilder);
						break;
					case '[':
						EmitBeginLoopMethodInstructions(MainIlGenerator);
						break;
					case ']':
						EmitEndLoopMethodInstructions(MainIlGenerator);
						break;
				}
			}

			MainIlGenerator.Emit(OpCodes.Ret);

			// Seal the lid on this type
			var type = ProgramTypeBuilder.CreateType();
			// Set the entrypoint (thereby declaring it an EXE)
			assemblyBuilder.SetEntryPoint(mainMethodBuilder, PEFileKinds.ConsoleApplication);
			assemblyBuilder.Save(inputFileName + ".exe");
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