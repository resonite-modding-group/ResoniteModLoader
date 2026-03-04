namespace ResoniteModLoader;

internal static class AssemblyLoader {
	private static string[]? GetAssemblyPathsFromDir(string dirName) {

		string assembliesDirectory = Path.Combine(Directory.GetCurrentDirectory(), dirName);

		Logger.MsgInternal($"Loading assemblies from {dirName}");

		string[]? assembliesToLoad = null;
		try {
			// Directory.GetFiles and Directory.EnumerateFiles have a fucked up API: https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.getfiles?view=netframework-4.6.2#system-io-directory-getfiles(system-string-system-string-system-io-searchoption)
			// Searching for "*.dll" would return results like "foo.dll_disabled"

			assembliesToLoad = Directory.EnumerateFiles(assembliesDirectory, "*.dll")
				.Where(file => file.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
				.ToArray();
			Array.Sort(assembliesToLoad, string.CompareOrdinal);
		} catch (DirectoryNotFoundException) {
			Logger.MsgInternal($"{dirName} directory not found, creating it now.");
			try {
				Directory.CreateDirectory(assembliesDirectory);
			} catch (Exception e2) {
				Logger.ErrorInternal($"Error creating ${dirName} directory:\n{e2}");
			}
		} catch (Exception e) {
			Logger.ErrorInternal($"Error enumerating ${dirName} directory:\n{e}");
		}
		return assembliesToLoad;
	}

	private static Assembly? LoadAssembly(string filepath) {
		string filename = Path.GetFileName(filepath);
		LoadProgressIndicator.SetSubphase($"Loading file: {filename}");
		Assembly assembly;
		try {
			Logger.DebugFuncInternal(() => $"Load assembly {filename}");
			assembly = Assembly.LoadFrom(filepath);
		} catch (Exception e) {
			Logger.ErrorInternal($"Error loading assembly from {filepath}: {e}");
			return null;
		}
		if (assembly == null) {
			Logger.ErrorInternal($"Unexpected null loading assembly from {filepath}");
			return null;
		}
		return assembly;
	}

	internal static AssemblyFile[] LoadAssembliesFromDir(string dirName) {
		List<AssemblyFile> assemblyFiles = new();
		if (GetAssemblyPathsFromDir(dirName) is string[] assemblyPaths) {
			foreach (string assemblyFilepath in assemblyPaths) {
				try {
					if (LoadAssembly(assemblyFilepath) is Assembly assembly) {
						assemblyFiles.Add(new AssemblyFile(assemblyFilepath, assembly));
					}
				} catch (Exception e) {
					Logger.ErrorInternal($"Unexpected exception loading assembly from {assemblyFilepath}:\n{e}");
				}
			}
		}
		return assemblyFiles.ToArray();
	}

	private static bool _resolveHookRegistered;
	private static readonly Assembly RMLAssembly = Assembly.GetExecutingAssembly();

	internal static void SetupResolveHook() {
		if (_resolveHookRegistered) { return; }
		AppDomain.CurrentDomain.AssemblyResolve += RMLResolveEventHandler;
		Logger.DebugInternal("Resolve event handler registered");
		_resolveHookRegistered = true;
	}

	private static Assembly? RMLResolveEventHandler(object? sender, ResolveEventArgs args) {
		try {
			AssemblyName target = new AssemblyName(args.Name);
			if (target is { Name: "ResoniteModLoader", Version.Major: > 2023 }) {
				Logger.DebugInternal($"Resolving assembly reference for {args.RequestingAssembly?.FullName}");
				return RMLAssembly;
			}
		}
		catch (Exception e) {
			// I don't know why the above block would ever throw, but just in case it does...
			string message = $"Exception while handling assembly resolution! [{args.RequestingAssembly?.FullName}] resolving [{args.Name}]: {e.Message}";
			if (args.Name.StartsWith("ResoniteModLoader", StringComparison.Ordinal)) {
				Logger.WarnInternal(message);
			}
			else {
				Logger.DebugInternal(message);
			}
		}

		return null;
	}
}
