using HarmonyLib;

namespace ResoniteModLoader; 
// this class does all the harmony-related RML work.
// this is needed to avoid importing harmony in ExecutionHook, where it may not be loaded yet.
internal class HarmonyWorker {
	internal static void LoadModsAndHideModAssemblies(HashSet<Assembly> initialAssemblies) {
		Harmony harmony = new("com.resonitemodloader.ResoniteModLoader");
		ModLoader.LoadMods(harmony);
		AssemblyHider.PatchResonite(harmony, initialAssemblies);
	}
}
