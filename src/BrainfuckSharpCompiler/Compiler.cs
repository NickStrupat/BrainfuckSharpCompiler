using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace BrainfuckSharpCompiler {
	abstract class Compiler {
		public static Compiler Create(String inputFilePath, UInt32 stackSize, Boolean inline, Boolean @unsafe) =>
			@unsafe
				? (Compiler) new UnsafeCompiler(inputFilePath, stackSize, inline)
				: (Compiler) new SafeCompiler(inputFilePath, stackSize, inline);

		private readonly AssemblyBuilder assemblyBuilder;
		protected readonly TypeBuilder ProgramTypeBuilder;
		private readonly MethodBuilder mainMethodBuilder;
		protected readonly ILGenerator MainIlGenerator;

		private readonly String inputFilePath;
		private readonly String inputFileName;
		private readonly UInt32 stackSize;
		protected readonly Boolean Inline;

		Action emitInvokeIncrementStackIndexMethodInstructions;
		Action emitInvokeDecrementStackIndexMethodInstructions;
		Action emitInvokeIncrementStackByteMethodInstructions;
		Action emitInvokeDecrementStackByteMethodInstructions;
		Action emitInvokeWriteStackByteMethodInstructions;
		Action emitInvokeReadStackByteMethodInstructions;

		protected Compiler(String inputFilePath, UInt32 stackSize, Boolean inline) {
			this.inputFilePath = inputFilePath;
			this.inputFileName = Path.GetFileName(inputFilePath) ?? "stdin";
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

		struct OperationEmitConfiguration {
			public readonly String Name;
			public readonly Action<ILGenerator> MethodEmitter;
			public readonly Action<Action> EmitInvokeMethodInstructions;

			public OperationEmitConfiguration(String name, Action<ILGenerator> methodEmitter, Action<Action> emitInvokeMethodInstructions) {
				Name = name;
				MethodEmitter = methodEmitter;
				EmitInvokeMethodInstructions = emitInvokeMethodInstructions;
			}
		}

		public void Compile() {
			var operationEmitConfigurations = new[] {
				new OperationEmitConfiguration("IncrementStackIndex", EmitIncrementStackIndexMethodInstructions, a => emitInvokeIncrementStackIndexMethodInstructions = a),
				new OperationEmitConfiguration("DecrementStackIndex", EmitDecrementStackIndexMethodInstructions, a => emitInvokeDecrementStackIndexMethodInstructions = a),
				new OperationEmitConfiguration("IncrementStackByte", EmitIncrementStackByteMethodInstructions, a => emitInvokeIncrementStackByteMethodInstructions = a),
				new OperationEmitConfiguration("DecrementStackByte", EmitDecrementStackByteMethodInstructions, a => emitInvokeDecrementStackByteMethodInstructions = a),
				new OperationEmitConfiguration("WriteStackByte", EmitWriteStackByteMethodInstructions, a => emitInvokeWriteStackByteMethodInstructions = a),
				new OperationEmitConfiguration("ReadStackByte", EmitReadStackByteMethodInstructions, a => emitInvokeReadStackByteMethodInstructions = a),
			};
			foreach (var operationEmitConfiguration in operationEmitConfigurations) {
				if (Inline)
					operationEmitConfiguration.EmitInvokeMethodInstructions(() => operationEmitConfiguration.MethodEmitter(MainIlGenerator));
				else {
					var methodBuilder = ProgramTypeBuilder.DefineMethod(operationEmitConfiguration.Name, MethodAttributes.Static, CallingConventions.Standard, typeof (void), null);
					var ilGenerator = methodBuilder.GetILGenerator();
					operationEmitConfiguration.MethodEmitter(ilGenerator);
					ilGenerator.Emit(OpCodes.Ret);
					operationEmitConfiguration.EmitInvokeMethodInstructions(() => MainIlGenerator.Emit(OpCodes.Call, methodBuilder));
				}
			}

			Stream GetInputStream(String inputFilePath)
			{
				if (inputFilePath == null)
				{
					Console.WriteLine("Reading source code from stdin...");
					return Console.OpenStandardInput();
				}
				Console.WriteLine($"Reading source code from {inputFileName}");
				return new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
			}

			Int32 byteRead;
			using (var instructionStream = GetInputStream(inputFilePath))
			{
				while ((byteRead = instructionStream.ReadByte()) != -1)
				{
					var instruction = (Char)byteRead;
					switch (instruction)
					{
						case '>':
							emitInvokeIncrementStackIndexMethodInstructions();
							break;
						case '<':
							emitInvokeDecrementStackIndexMethodInstructions();
							break;
						case '+':
							emitInvokeIncrementStackByteMethodInstructions();
							break;
						case '-':
							emitInvokeDecrementStackByteMethodInstructions();
							break;
						case '.':
							emitInvokeWriteStackByteMethodInstructions();
							break;
						case ',':
							emitInvokeReadStackByteMethodInstructions();
							break;
						case '[':
							EmitBeginLoopMethodInstructions(MainIlGenerator);
							break;
						case ']':
							EmitEndLoopMethodInstructions(MainIlGenerator);
							break;
					}
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