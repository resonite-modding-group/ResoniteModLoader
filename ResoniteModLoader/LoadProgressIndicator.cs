using FrooxEngine;

namespace ResoniteModLoader;

// Custom LoadProgressIndicator logic failing shouldn't stop the rest of the modloader.
internal static class LoadProgressIndicator {
	private static bool failed;

	private static FieldInfo? _showSubphase;
	private static FieldInfo? ShowSubphase {
		get {
			if (_showSubphase is null) {
				try {
					_showSubphase = typeof(EngineLoadProgress).GetField("_showSubphase", BindingFlags.NonPublic | BindingFlags.Instance);
				} catch (Exception ex) {
					if (!failed) {
						Logger.WarnInternal("_showSubphase not found: " + ex.ToString());
					}
					failed = true;
				}
			}
			return _showSubphase;
		}
	}

	// Returned true means success, false means something went wrong.
	internal static bool SetCustom(string text) {
		if (ModLoaderConfiguration.Get().HideVisuals) { return true; }
		if (!ModLoader.IsHeadless) {
			ShowSubphase?.SetValue(Engine.Current.InitProgress, text);
			return true;
		}
		return false;
	}
}

