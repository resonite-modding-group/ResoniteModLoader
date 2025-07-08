using FrooxEngine;

namespace ResoniteModLoader;

[AutoRegisterSetting]
[SettingCategory("ResoniteModLoader")]
public sealed class ModLoaderSettings : SettingComponent<ModLoaderSettings> {
	public override bool UserspaceOnly => true;

	[SettingIndicatorProperty]
	public readonly Sync<string> LoadedMods;
	[SettingIndicatorProperty]
	public readonly Sync<string> ModLoaderVersion;

	[SettingProperty("Debug Mode")]
	public readonly Sync<bool> DebugMode;

	public override void ResetToDefault() {
		DebugMode.Value = false;
	}
	protected override void OnChanges() {
		base.OnChanges();
		ModLoaderConfiguration.Get().Debug = DebugMode.Value;
	}
}
