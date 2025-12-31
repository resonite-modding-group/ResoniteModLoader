using Elements.Data;

using FrooxEngine;

using ResoniteModLoader.Assets;

/// <summary>
/// Definitions for settings categories in the UI, contains <see cref="SettingCategoryInfo"/>.
/// </summary>
[DataModelType]
[SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "Needs to exist in global namespace to be used")]
public static class SettingCategoryDefinitions {

	/// <summary>
	/// The RML settings category in the settings tab.
	/// </summary>
	[SettingCategory("ResoniteModLoader")]
	public static SettingCategoryInfo ResoniteModLoader => new(RMLAssets.Icon, 0L);
}
