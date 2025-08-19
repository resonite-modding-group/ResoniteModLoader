using Elements.Data;

using FrooxEngine;

using ResoniteModLoader.Assets;

[DataModelType]
public static class SettingCategoryDefinitions {
	[SettingCategory("ResoniteModLoader")]
	public static SettingCategoryInfo ResoniteModLoader => new SettingCategoryInfo(RMLAssets.Icon, 0L);
}
