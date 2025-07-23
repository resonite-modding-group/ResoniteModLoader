using FrooxEngine;

namespace ResoniteModLoader;

/// <summary>
/// Interfaces with Resonite's load progress indicator to set custom phases.
/// </summary>
internal static class LoadProgressIndicator {
	/// <summary>
	/// Sets a custom subphase message on the loading progress indicator.
	/// </summary>
	/// <param name="text">The message to display on the indicator</param>
	/// <returns>
	/// <c>true</c> if the indicator was set successfully or if visuals are hidden with <see cref="ModLoaderConfiguration.Get().HideVisuals"/> otherwise <c>false</c>.
	/// </returns>
	internal static bool SetSubphase(string text) {
		if (ModLoaderConfiguration.Get().HideVisuals) { return true; }
		try {
			Engine.Current.InitProgress.SetSubphase(text, true);
			return true;
		} catch (Exception ex) {
			Logger.ErrorInternal(ex);
			return false;
		}
		
	}
}

