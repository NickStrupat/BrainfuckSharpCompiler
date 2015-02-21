using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace BrainfuckSharpCompiler {
	class Program {
		static void Main(String[] args) {
			if (args.Length < 1)
				throw new ArgumentException(String.Format("Usage: {0} source_file_path", AppDomain.CurrentDomain.FriendlyName));

			var instructionStream = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);

			var an = new AssemblyName { Name = args[0] };
			var ad = AppDomain.CurrentDomain;
			var ab = ad.DefineDynamicAssembly(an, AssemblyBuilderAccess.Save);
			var mb = ab.DefineDynamicModule(an.Name, args[0] + ".exe");
			var tb = mb.DefineType("Brainfuck.Program", TypeAttributes.Public | TypeAttributes.Class);
			var fb = tb.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(void), new Type[] { typeof(string[]) });
			
			// Emit the ubiquitous "Hello, World!" method, in IL
			var ilg = fb.GetILGenerator();
			ilg.EmitWriteLine("Hello, World!");
			//ilg.Emit(OpCodes.Ldstr, "Hello, World!");
			//ilg.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new [] { typeof(String) }));
			ilg.Emit(OpCodes.Ret);
			
			// Seal the lid on this type
			var t = tb.CreateType();
			// Set the entrypoint (thereby declaring it an EXE)
			ab.SetEntryPoint(fb, PEFileKinds.ConsoleApplication);
			// Save it
			ab.Save(args[0] + ".exe");
		}
	}
}
