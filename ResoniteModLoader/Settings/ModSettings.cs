using FrooxEngine;

namespace ResoniteModLoader;

[AutoRegisterSetting]
[SettingCategory("ResoniteModLoader")]
public sealed class ModSettings : SettingComponent<ModSettings> {
	public override bool UserspaceOnly => true;
}
