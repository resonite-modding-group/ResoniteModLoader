using Elements.Data;

using FrooxEngine;

using ResoniteModLoader.Assets;

[DataModelType]
[SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "Needs to exist in global namespace to be used")]
public static class SettingCategoryDefinitions {
	[SettingCategory("ResoniteModLoader")]
	public static SettingCategoryInfo ResoniteModLoader => new(RMLAssets.Icon, 0L);
}
