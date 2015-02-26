using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace BrainfuckSharpCompiler {
	class Program {
		static readonly Type arrayElementType = typeof(Byte);
		static readonly Type arrayType = typeof(Byte[]);
		static FieldInfo stackFieldInfo;
		static FieldInfo stackIndexFieldInfo;
		static void Main(String[] args) {
			if (args.Length < 1)
				throw new ArgumentException(String.Format("Usage: {0} source_file_path", AppDomain.CurrentDomain.FriendlyName));
			String inputFileName = null;
			Int32 stackSize = 30000;
			foreach (var arg in args) {
				if (arg[0] == '/')
					switch (arg.Substring(1, 2)) {
						case "s:":
							stackSize = Int32.Parse(arg.Substring(3));
							break;
						default:
							throw new ArgumentException();
					}
				else
					inputFileName = arg;
			}

			var instructionStream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);

			var assemblyName = new AssemblyName { Name = inputFileName };
			var appDomain = AppDomain.CurrentDomain;
			var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, inputFileName + ".exe");
			var typeBuilder = moduleBuilder.DefineType("Brainfuck.Program", TypeAttributes.Public | TypeAttributes.Class);

			stackFieldInfo = typeBuilder.DefineField("stack", arrayType, FieldAttributes.Static);
			stackIndexFieldInfo = typeBuilder.DefineField("stackIndex", typeof(Int32), FieldAttributes.Static);

			var incrementStackIndexMethodBuilder = typeBuilder.DefineMethod("IncrementStackIndex", MethodAttributes.Static, CallingConventions.Standard, typeof (void), null);
			EmitIncrementStackIndexMethodInstructions(incrementStackIndexMethodBuilder.GetILGenerator());

			var decrementStackIndexMethodBuilder = typeBuilder.DefineMethod("DecrementStackIndex", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
			EmitDecrementStackIndexMethodInstructions(decrementStackIndexMethodBuilder.GetILGenerator());

			var incrementStackByteMethodBuilder = typeBuilder.DefineMethod("IncrementStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
			EmitIncrementStackByteMethodInstructions(incrementStackByteMethodBuilder.GetILGenerator());

			var decrementStackByteMethodBuilder = typeBuilder.DefineMethod("DecrementStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
			EmitDecrementStackByteMethodInstructions(decrementStackByteMethodBuilder.GetILGenerator());

			var writeStackByteMethodBuilder = typeBuilder.DefineMethod("WriteStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
			EmitWriteStackByteMethodInstructions(writeStackByteMethodBuilder.GetILGenerator());

			var readStackByteMethodBuilder = typeBuilder.DefineMethod("ReadStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
			EmitReadStackByteMethodInstructions(readStackByteMethodBuilder.GetILGenerator());

			var fb = typeBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(void), new [] { typeof(String[]) });
			var ilg = fb.GetILGenerator();

			// initialize stack
			ilg.Emit(OpCodes.Ldc_I4, stackSize);
			ilg.Emit(OpCodes.Newarr, arrayElementType);
			ilg.Emit(OpCodes.Stsfld, stackFieldInfo);

			// initialize stack index
			ilg.Emit(OpCodes.Ldc_I4_0);
			ilg.Emit(OpCodes.Stsfld, stackIndexFieldInfo);

			var labels = new Stack<Label>();
			Int32 byteRead;
			while ((byteRead = instructionStream.ReadByte()) != -1) {
				var instruction = (Char) byteRead;
				switch (instruction) {
					case '>':
						ilg.Emit(OpCodes.Call, incrementStackIndexMethodBuilder);
						break;
					case '<':
						ilg.Emit(OpCodes.Call, decrementStackIndexMethodBuilder);
						break;
					case '+':
						ilg.Emit(OpCodes.Call, incrementStackByteMethodBuilder);
						break;
					case '-':
						ilg.Emit(OpCodes.Call, decrementStackByteMethodBuilder);
						break;
					case '.':
						ilg.Emit(OpCodes.Call, writeStackByteMethodBuilder);
						break;
					case ',':
						ilg.Emit(OpCodes.Call, readStackByteMethodBuilder);
						break;
					case '[': {
						var top = ilg.DefineLabel();
						var bottom = ilg.DefineLabel();
						ilg.Emit(OpCodes.Br, bottom);
						ilg.MarkLabel(top);
						labels.Push(bottom);
						labels.Push(top);
						break;
					}
					case ']': {
						var top = labels.Pop();
						var bottom = labels.Pop();
						ilg.MarkLabel(bottom);
						ilg.Emit(OpCodes.Ldsfld, stackFieldInfo);
						ilg.Emit(OpCodes.Ldsfld, stackIndexFieldInfo);
						ilg.Emit(OpCodes.Ldelem_U1);
						ilg.Emit(OpCodes.Brtrue, top);
						break;
					}
				}
			}

			ilg.Emit(OpCodes.Ret);
			
			// Seal the lid on this type
			var t = typeBuilder.CreateType();
			// Set the entrypoint (thereby declaring it an EXE)
			assemblyBuilder.SetEntryPoint(fb, PEFileKinds.ConsoleApplication);
			// Save it
			assemblyBuilder.Save(inputFileName + ".exe");
		}

		static void EmitIncrementStackIndexMethodInstructions(ILGenerator ilg) {
			EmitStackIndexMethodInstructions(ilg, OpCodes.Add);
		}
		static void EmitDecrementStackIndexMethodInstructions(ILGenerator ilg) {
			EmitStackIndexMethodInstructions(ilg, OpCodes.Sub);
		}
		static void EmitStackIndexMethodInstructions(ILGenerator ilg, OpCode addOrSub) {
			ilg.Emit(OpCodes.Ldsfld, stackIndexFieldInfo);
			ilg.Emit(OpCodes.Ldc_I4_1);
			ilg.Emit(addOrSub);
			ilg.Emit(OpCodes.Stsfld, stackIndexFieldInfo);
			ilg.Emit(OpCodes.Ret);
		}

		static void EmitIncrementStackByteMethodInstructions(ILGenerator ilg) {
			EmitStackByteMethodInstructions(ilg, OpCodes.Add);
		}
		static void EmitDecrementStackByteMethodInstructions(ILGenerator ilg) {
			EmitStackByteMethodInstructions(ilg, OpCodes.Sub);
		}
		static void EmitStackByteMethodInstructions(ILGenerator ilg, OpCode addOrSub) {
			ilg.Emit(OpCodes.Ldsfld, stackFieldInfo);
			ilg.Emit(OpCodes.Ldsfld, stackIndexFieldInfo);
			ilg.Emit(OpCodes.Ldelema, arrayElementType);
			ilg.Emit(OpCodes.Dup);
			ilg.Emit(OpCodes.Ldobj, arrayElementType);
			ilg.Emit(OpCodes.Ldc_I4_1);
			ilg.Emit(addOrSub);
			ilg.Emit(OpCodes.Conv_U1);
			ilg.Emit(OpCodes.Stobj, arrayElementType);
			ilg.Emit(OpCodes.Ret);
		}

		static readonly MethodInfo ConsoleWriteChar = new Action<Char>(Console.Write).Method;
		static void EmitWriteStackByteMethodInstructions(ILGenerator ilg) {
			ilg.Emit(OpCodes.Ldsfld, stackFieldInfo);
			ilg.Emit(OpCodes.Ldsfld, stackIndexFieldInfo);
			ilg.Emit(OpCodes.Ldelem_U1);
			ilg.Emit(OpCodes.Call, ConsoleWriteChar);
			ilg.Emit(OpCodes.Ret);
		}

		static readonly MethodInfo ConsoleRead = new Func<Int32>(Console.Read).Method;
		static void EmitReadStackByteMethodInstructions(ILGenerator ilg) {
			ilg.Emit(OpCodes.Ldsfld, stackFieldInfo);
			ilg.Emit(OpCodes.Ldsfld, stackIndexFieldInfo);
			ilg.Emit(OpCodes.Call, ConsoleRead);
			ilg.Emit(OpCodes.Conv_U1);
			ilg.Emit(OpCodes.Stelem_I1);
			ilg.Emit(OpCodes.Ret);
		}
	}
}
