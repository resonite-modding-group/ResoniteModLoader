using System.Globalization;

using FrooxEngine;

namespace ResoniteModLoader;

[AutoRegisterSetting]
[SettingCategory("ResoniteModLoader")]
public sealed class ModLoaderSettings : SettingComponent<ModLoaderSettings> {
	/// <inheritdoc/>
	public override bool UserspaceOnly => true;

	[SettingIndicatorProperty]
	public readonly Sync<string> LoadedMods;

	[SettingIndicatorProperty]
	public readonly Sync<string> ModLoaderVersion;

	[SettingProperty]
	public readonly Sync<bool> DebugMode;

	[SettingProperty]
	public readonly Sync<bool> HideVisuals;

	//TODO make clickable link in UI
	[SettingIndicatorProperty]
	public readonly Sync<string> Link;

	/// <inheritdoc/>
	public override void ResetToDefault() {
		DebugMode.Value = false;
		HideVisuals.Value = false;
	}

	/// <inheritdoc/>
	protected override void OnChanges() {
		base.OnChanges();
		ModLoaderConfiguration.Get().Debug = DebugMode.Value;
		Logger.DebugInternal($"Setting changed, changed debug values {ModLoaderConfiguration.Get().Debug}");

		ModLoaderVersion.Value = ModLoader.VERSION;
		LoadedMods.Value = ModLoader.Mods().Count().ToString(CultureInfo.InvariantCulture);
	}
	protected override void OnStart() {
		base.OnStart();
		ModLoaderVersion.Value = ModLoader.VERSION;
		LoadedMods.Value = ModLoader.Mods().Count().ToString(CultureInfo.InvariantCulture);
		DebugMode.Value = ModLoaderConfiguration.Get().Debug;
		HideVisuals.Value = ModLoaderConfiguration.Get().HideVisuals;
		Link.Value = "https://github.com/resonite-modding-group/ResoniteModLoader";
	}
}
