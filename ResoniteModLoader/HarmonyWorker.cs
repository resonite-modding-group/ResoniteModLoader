using FrooxEngine;

using HarmonyLib;

using ResoniteModLoader.Locale;

namespace ResoniteModLoader;
// this class does all the harmony-related RML work.
// this is needed to avoid importing harmony in ExecutionHook, where it may not be loaded yet.
internal sealed class HarmonyWorker {
	internal static void Init() {
		Harmony harmony = new("com.resonitemodloader.ResoniteModLoader");
		LocaleLoader.InitLocales();
		ModLoader.LoadMods();
		ModConfiguration.RegisterShutdownHook(harmony);
	}
}
