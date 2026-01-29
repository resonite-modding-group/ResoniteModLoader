using Mono.Cecil;
using Mono.Cecil.Cil;
static class Util {
	public static Version ReadAssemblyVersion(string dllToProcess) {
		using var asmBefore = AssemblyDefinition.ReadAssembly(dllToProcess);
		return asmBefore.Name.Version;
	}

	//If Resonite no longer rewrites the originalVersion to a datetime, we can remove this.
	//Rewrite originalVersion on DLL
	public static void WriteAssemblyVersion(string dllToProcess, Version version) {
		bool symbolsExist = File.Exists(Path.ChangeExtension(dllToProcess, ".pdb"));
		using var asmAfter = AssemblyDefinition.ReadAssembly(dllToProcess, new ReaderParameters() { ReadWrite = true });
		// Write [assembly: AssemblyVersion("4.2.0.0")]
		asmAfter.Name.Version = version;
		// Write [assembly: AssemblyFileVersion("4.2.0")]
		CustomAttribute? afv = asmAfter.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.Name == "AssemblyFileVersionAttribute");
		if (afv != null) {
			afv.ConstructorArguments.RemoveAt(0);
			afv.ConstructorArguments.Add(new CustomAttributeArgument(asmAfter.MainModule.ImportReference(typeof(string)), version.ToString(3)));
		}

		asmAfter.Write(new WriterParameters {
			WriteSymbols = symbolsExist,
			SymbolWriterProvider = (symbolsExist ? new PortablePdbWriterProvider() : null)
		});
	}
}
