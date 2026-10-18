using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader.Utility;

namespace ResoniteModLoader;

static class SettingsRelatedPatches {
	internal static void RunPatches(Harmony harmony) {
		var patchMethod =
			AccessTools.DeclaredMethod(typeof(SettingsRelatedPatches), nameof(InjectModGrid));
		var targetMethod = AccessTools.DeclaredMethod(typeof(SettingsDataFeed), "GenerateItem",
		[
			typeof(SettingsDataFeed.ActionIdentity), typeof(SettingPropertyAttribute), typeof(IReadOnlyList<string>),
			typeof(IReadOnlyList<string>)
		]);
		var patched = harmony.Patch(targetMethod, prefix: new HarmonyMethod(patchMethod));
		Logger.DebugInternal("SettingsDataFeedPatcher patch 1 success " + patched);

		patchMethod =
			AccessTools.DeclaredMethod(typeof(SettingsRelatedPatches), nameof(InjectExtraMembers));
		targetMethod = AccessTools.DeclaredMethod(typeof(ModSettings), "GetSyncMember");
		patched = harmony.Patch(targetMethod, prefix: new HarmonyMethod(patchMethod));
		Logger.DebugInternal("SettingsDataFeedPatcher patch 2 success " + patched);
	}

	internal static bool InjectModGrid(ref DataFeedItem __result, SettingsDataFeed.ActionIdentity identity,
		IReadOnlyList<string> path, IReadOnlyList<string> grouping) {
		// Userspace refresh stage check prevents leaking mod list in worldspace
		if (Util.InUserspaceContext() && identity.settingType == typeof(ModSettings) && identity.MemberName == nameof(ModSettings.ModList)) {
			var activeSetting = Settings.GetActiveSetting<ModSettings>();
			__result = activeSetting!.GenerateModGrid(path, grouping);
			return false;
		}

		return true;
	}

	internal static bool InjectExtraMembers(ModSettings __instance, ref ISyncMember __result, int index) {
		if (index >= 1000) {
			__result = __instance.GetFakeMember(index);
			return false;
		}

		return true;
	}


	internal static IAsyncEnumerable<DataFeedItem> GetSubcategoryItems(object setting, string subcategory) =>
		throw new NotImplementedException("Stub");
}
