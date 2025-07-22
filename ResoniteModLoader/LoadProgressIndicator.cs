using FrooxEngine;

namespace ResoniteModLoader;

// Custom LoadProgressIndicator logic failing shouldn't stop the rest of the modloader.
internal static class LoadProgressIndicator {
	// Returned true means success, false means something went wrong.
	internal static bool SetCustom(string text) {
		if (ModLoaderConfiguration.Get().HideVisuals) { return true; }
		Engine.Current.InitProgress.SetSubphase(text, true);
		return true;
	}
}

