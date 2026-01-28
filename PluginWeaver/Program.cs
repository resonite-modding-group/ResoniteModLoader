using System.Reflection;

using Mono.Cecil;
using Mono.Cecil.Pdb;

if (args.Length != 1) {
	Console.WriteLine("Missing DLL argument.");
	return 1;
}

var dllToProcess = Path.GetFullPath(args[0]);

var resonite = Environment.GetEnvironmentVariable("ResonitePath");

if (resonite is null) {
	Console.WriteLine("Missing env ResonitePath.");
	return 1;
}

// Resolve Resonite DLLs
var currentDomain = AppDomain.CurrentDomain;
currentDomain.AssemblyResolve += new ResolveEventHandler((_, args) => {
	var assemblyPath = Path.Combine(resonite, new AssemblyName(args.Name).Name + ".dll");
	if (!File.Exists(assemblyPath)) return null;
	var assembly = Assembly.LoadFrom(assemblyPath);
    return assembly;
});

// This makes Mono.Cecil correctly resolve DLLs
Directory.SetCurrentDirectory(resonite);

var asmBefore = AssemblyDefinition.ReadAssembly(dllToProcess);
Version originalVersion = asmBefore.Name.Version;
Console.WriteLine($"Original Version: {originalVersion}");
asmBefore.Dispose();

// Process the provided DLL
Weaver.Process(dllToProcess, resonite);

//If Resonite no longer rewrites the originalVersion to a datetime, we can remove this.
//Rewrite originalVersion on DLL
var asmAfter = AssemblyDefinition.ReadAssembly(dllToProcess,new ReaderParameters() { ReadWrite = true});
// Write [assembly: AssemblyVersion("4.2.0.0")]
asmAfter.Name.Version = originalVersion;
// Write [assembly: AssemblyFileVersion("4.2.0")]
CustomAttribute? afv = asmAfter.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.Name == "AssemblyFileVersionAttribute");
if (afv != null) {
	afv.ConstructorArguments.RemoveAt(0);
	afv.ConstructorArguments.Add(new CustomAttributeArgument(asmAfter.MainModule.ImportReference(typeof(string)), originalVersion.ToString(3)));
}

bool symbolsExist = File.Exists(Path.ChangeExtension(dllToProcess, ".pdb"));
asmAfter.Write(new WriterParameters {
	WriteSymbols = symbolsExist,
	SymbolWriterProvider = (symbolsExist ? new PdbWriterProvider() : null)
});
asmAfter.Dispose();

var asmVerify = AssemblyDefinition.ReadAssembly(dllToProcess);
Console.WriteLine($"Wrote new Version as {asmVerify.Name.Version}");
asmVerify.Dispose();

return 0;
