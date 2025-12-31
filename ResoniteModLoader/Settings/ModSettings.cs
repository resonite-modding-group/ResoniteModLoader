using FrooxEngine;

namespace ResoniteModLoader;

/// <summary>
/// TODO: Settings UI for loaded mods.
/// </summary>
[AutoRegisterSetting]
[SettingCategory("ResoniteModLoader")]
public sealed class ModSettings : SettingComponent<ModSettings> {
	/// <inheritdoc/>
	public override bool UserspaceOnly => true;
}
