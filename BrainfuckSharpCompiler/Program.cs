using System;
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

			var instructionStream = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);

			var an = new AssemblyName { Name = args[0] };
			var ad = AppDomain.CurrentDomain;
			var ab = ad.DefineDynamicAssembly(an, AssemblyBuilderAccess.Save);
			var mb = ab.DefineDynamicModule(an.Name, args[0] + ".exe");
			var tb = mb.DefineType("Brainfuck.Program", TypeAttributes.Public | TypeAttributes.Class);

			stackFieldInfo = tb.DefineField("stack", arrayType, FieldAttributes.Static);
			stackIndexFieldInfo = tb.DefineField("stackIndex", typeof(Int32), FieldAttributes.Static);

			var incrementStackIndexMethodBuilder = tb.DefineMethod("IncrementStackIndex", MethodAttributes.Static, CallingConventions.Standard, typeof (void), null);
			EmitIncrementStackIndexMethodInstructions(incrementStackIndexMethodBuilder.GetILGenerator());

			var decrementStackIndexMethodBuilder = tb.DefineMethod("DecrementStackIndex", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
			EmitDecrementStackIndexMethodInstructions(decrementStackIndexMethodBuilder.GetILGenerator());

			var incrementStackByteMethodBuilder = tb.DefineMethod("IncrementStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
			EmitIncrementStackByteMethodInstructions(incrementStackByteMethodBuilder.GetILGenerator());

			var decrementStackByteMethodBuilder = tb.DefineMethod("DecrementStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
			EmitDecrementStackByteMethodInstructions(decrementStackByteMethodBuilder.GetILGenerator());

			var writeStackByteMethodBuilder = tb.DefineMethod("WriteStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
			EmitWriteStackByteMethodInstructions(writeStackByteMethodBuilder.GetILGenerator());

			var readStackByteMethodBuilder = tb.DefineMethod("ReadStackByte", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
			EmitReadStackByteMethodInstructions(readStackByteMethodBuilder.GetILGenerator());

			var fb = tb.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(void), new [] { typeof(String[]) });
			var ilg = fb.GetILGenerator();

			// initialize stack
			ilg.Emit(OpCodes.Ldc_I4, 30000);
			ilg.Emit(OpCodes.Newarr, arrayElementType);
			ilg.Emit(OpCodes.Stsfld, stackFieldInfo);

			// initialize stack index
			ilg.Emit(OpCodes.Ldc_I4_0);
			ilg.Emit(OpCodes.Stsfld, stackIndexFieldInfo);

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
					case '[':
						//if (stack[stackIndex] == 0) {
						//	while (instructionStream.ReadByte() != ']')
						//		;
						//}
						//else
						//	jumps.Push(instructionStream.Position);
						break;
					case ']':
						//if (stack[stackIndex] != 0)
						//	instructionStream.Seek(jumps.Peek(), SeekOrigin.Begin);
						//else
						//	jumps.Pop();
						break;
				}
			}

			ilg.Emit(OpCodes.Ret);
			
			// Seal the lid on this type
			var t = tb.CreateType();
			// Set the entrypoint (thereby declaring it an EXE)
			ab.SetEntryPoint(fb, PEFileKinds.ConsoleApplication);
			// Save it
			ab.Save(args[0] + ".exe");
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
