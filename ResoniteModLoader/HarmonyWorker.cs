using HarmonyLib;

namespace ResoniteModLoader;
// this class does all the harmony-related RML work.
// this is needed to avoid importing harmony in ExecutionHook, where it may not be loaded yet.
internal sealed class HarmonyWorker {
	internal static void Init() {
		#if DEBUG
		Harmony.DEBUG = true;
		#endif
		Harmony harmony = new("com.resonitemodloader.ResoniteModLoader");
		ModLoader.LoadMods();
		ModConfiguration.RegisterShutdownHook(harmony);
		SettingsRelatedPatches.RunPatches(harmony);
	}
}
