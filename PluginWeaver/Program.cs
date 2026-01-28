using System.Reflection;

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

var version = Util.ReadAssemblyVersion(dllToProcess);
Console.WriteLine($"Original Version: {version}");

// Process the provided DLL
Weaver.Process(dllToProcess, resonite);

Util.WriteAssemblyVersion(dllToProcess, version);

var newVersion = Util.ReadAssemblyVersion(dllToProcess);
Console.WriteLine($"Wrote new Version: {newVersion}");

return 0;
