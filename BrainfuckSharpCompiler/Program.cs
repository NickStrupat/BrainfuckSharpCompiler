using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace BrainfuckSharpCompiler {
	class Program {
		static readonly Type arrayElementType = typeof(Byte);
		static readonly Type arrayType = typeof(Byte[]);
		static void Main(String[] args) {
			if (args.Length < 1)
				throw new ArgumentException(String.Format("Usage: {0} source_file_path", AppDomain.CurrentDomain.FriendlyName));

			var instructionStream = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);

			var an = new AssemblyName { Name = args[0] };
			var ad = AppDomain.CurrentDomain;
			var ab = ad.DefineDynamicAssembly(an, AssemblyBuilderAccess.Save);
			var mb = ab.DefineDynamicModule(an.Name, args[0] + ".exe");
			var tb = mb.DefineType("Brainfuck.Program", TypeAttributes.Public | TypeAttributes.Class);

			var stackFieldBuilder = tb.DefineField("stack", arrayType, FieldAttributes.Static);
			var stackIndexFieldBuilder = tb.DefineField("stackIndex", typeof (Int32), FieldAttributes.Static);

			var incrementStackIndexMethodBuilder = tb.DefineMethod("IncrementStackIndex", MethodAttributes.Static, CallingConventions.Standard, typeof (void), null);
			EmitIncrementStackIndexInstructions(incrementStackIndexMethodBuilder.GetILGenerator());

			var decrementStackIndexMethodBuilder = tb.DefineMethod("DecrementStackIndex", MethodAttributes.Static, CallingConventions.Standard, typeof(void), null);
			EmitDecrementStackIndexInstructions(decrementStackIndexMethodBuilder.GetILGenerator());

			var fb = tb.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(void), new [] { typeof(String[]) });
			
			var ilg = fb.GetILGenerator();

			var buffer = ilg.DeclareLocal(arrayType);
			var stackIndex = ilg.DeclareLocal(typeof (Int32));
			
			// initialize stack
			ilg.Emit(OpCodes.Ldc_I4, 30000);
			ilg.Emit(OpCodes.Newarr, arrayType);
			ilg.Emit(OpCodes.Stloc_0);

			// initialize stack index
			ilg.Emit(OpCodes.Ldc_I4_0);
			ilg.Emit(OpCodes.Stloc_1);

			Int32 byteRead;
			while ((byteRead = instructionStream.ReadByte()) != -1) {
				var instruction = (Char) byteRead;
				switch (instruction) {
					case '>':
						EmitIncrementStackIndexInstructions(ilg);
						break;
					case '<':
						EmitDecrementStackIndexInstructions(ilg);
						break;
					case '+':
						EmitIncrementByteOnStackInstructions(ilg);
						break;
					case '-':
						EmitDecrementByteOnStackInstructions(ilg);
						break;
					case '.':
						EmitWriteByteOnStackInstructions(ilg);
						break;
					case ',':
						EmitReadByteOnStackInstructions(ilg);
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

		static void EmitIncrementStackIndexInstructions(ILGenerator ilg) {
			EmitStackIndexInstructions(ilg, OpCodes.Add);
		}
		static void EmitDecrementStackIndexInstructions(ILGenerator ilg) {
			EmitStackIndexInstructions(ilg, OpCodes.Sub);
		}
		static void EmitStackIndexInstructions(ILGenerator ilg, OpCode addOrSub) {
			ilg.Emit(OpCodes.Ldloc_0);
			ilg.Emit(OpCodes.Ldc_I4_1);
			ilg.Emit(addOrSub);
			ilg.Emit(OpCodes.Stloc_1);
		}

		static void EmitIncrementByteOnStackInstructions(ILGenerator ilg) {
			EmitByteOnStackInstructions(ilg, OpCodes.Add);
		}
		static void EmitDecrementByteOnStackInstructions(ILGenerator ilg) {
			EmitByteOnStackInstructions(ilg, OpCodes.Sub);
		}
		static void EmitByteOnStackInstructions(ILGenerator ilg, OpCode addOrSub) {
			ilg.Emit(OpCodes.Ldloc_0);
			ilg.Emit(OpCodes.Ldloc_1);
			ilg.Emit(OpCodes.Ldelema, arrayElementType);
			ilg.Emit(OpCodes.Dup);
			ilg.Emit(OpCodes.Ldobj, arrayElementType);
			ilg.Emit(OpCodes.Ldc_I4_1);
			ilg.Emit(addOrSub);
			ilg.Emit(OpCodes.Conv_U1);
			ilg.Emit(OpCodes.Stobj, arrayElementType);
		}

		static readonly MethodInfo ConsoleWriteChar = new Action<Char>(Console.Write).Method;
		static void EmitWriteByteOnStackInstructions(ILGenerator ilg) {
			ilg.Emit(OpCodes.Ldloc_0);
			ilg.Emit(OpCodes.Ldloc_1);
			ilg.Emit(OpCodes.Ldelem_U1);
			ilg.Emit(OpCodes.Call, ConsoleWriteChar);
		}

		static readonly MethodInfo ConsoleRead = new Func<Int32>(Console.Read).Method;
		static void EmitReadByteOnStackInstructions(ILGenerator ilg) {
			ilg.Emit(OpCodes.Ldloc_0);
			ilg.Emit(OpCodes.Ldloc_1);
			ilg.Emit(OpCodes.Call, ConsoleRead);
			ilg.Emit(OpCodes.Conv_U1);
			ilg.Emit(OpCodes.Stelem_I1);
		}
	}
}
