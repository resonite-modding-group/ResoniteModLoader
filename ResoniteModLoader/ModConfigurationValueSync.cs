using Elements.Core;
using FrooxEngine;

namespace ResoniteModLoader;

[Category(["ResoniteModLoder"])]
public class ModConfigurationValueSync<T> : Component {
#pragma warning disable CS1591
	public override bool UserspaceOnly => true;
#pragma warning restore CS1591
#pragma warning disable CS8618, CA1051
	public readonly Sync<string> DefiningModAssembly;

	public readonly Sync<string> ConfigurationKeyName;

	public readonly Sync<bool> DefinitionFound;

	public readonly FieldDrive<T> TargetField;
#pragma warning restore CS8618, CA1051
	private ResoniteModBase _mappedMod;

	private ModConfiguration _mappedConfig;

	private ModConfigurationKey _mappedKey;

	public void LoadConfigKey(ModConfiguration config, ModConfigurationKey key) {

		_mappedMod = config.Owner;
		_mappedConfig = config;
		_mappedKey = key;
		DefiningModAssembly.Value = Path.GetFileNameWithoutExtension(config.Owner.ModAssembly!.File);
		ConfigurationKeyName.Value = key.Name;
	}
}

public static class ModConfigurationValueSyncExtensions {
	public static ModConfigurationValueSync<T> SyncWithModConfiguration<T>(this IField<T> field, ModConfiguration config, ModConfigurationKey key) {
		ModConfigurationValueSync<T> driver = field.FindNearestParent<Slot>().AttachComponent<ModConfigurationValueSync<T>>();
		driver.LoadConfigKey(config, key);
		driver.TargetField.Target = field;

		return driver;
	}
}
