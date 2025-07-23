using System.Diagnostics;
using FrooxEngine;

namespace ResoniteModLoader;
internal class ModLoaderInit {

	internal static HashSet<string>? initialAssembliesString;

	internal static void Initialize() {
		Logger.DebugInternal($"Start of ModLoader Initialization");
		Stopwatch initializationTimer = Stopwatch.StartNew();

		try {
			HashSet<Assembly> initialAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToHashSet();
			initialAssembliesString = initialAssemblies.Select(a => a.FullName).ToHashSet();
			LoadProgressIndicator.SetSubphase("Loading Libraries");
			AssemblyFile[] loadedAssemblies = AssemblyLoader.LoadAssembliesFromDir("rml_libs");
			// note that harmony may not be loaded until this point, so this class cannot directly import HarmonyLib.

			if (loadedAssemblies.Length != 0) {
				string loadedAssemblyList = string.Join("\n", loadedAssemblies.Select(a => a.Name + ", Version=" + a.Version + ", Sha256=" + a.Sha256));
				Logger.MsgInternal($"Loaded libraries from rml_libs:\n{loadedAssemblyList}");
			}
			LoadProgressIndicator.SetSubphase("Initializing");
			DebugInfo.Log();
			HarmonyWorker.LoadModsAndHideModAssemblies(initialAssemblies);
			LoadProgressIndicator.SetSubphase("Loaded");
		} catch (Exception e) {
			// it's important that this doesn't send exceptions back to Resonite
			Logger.ErrorInternal($"Exception during initialization!\n{e}");
		}
		initializationTimer.Stop();
		Logger.MsgInternal($"Initialization completed in {initializationTimer.ElapsedMilliseconds}ms");
	}

}
