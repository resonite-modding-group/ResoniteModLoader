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

	public readonly RawOutput<bool> DefinitionFound;

	public readonly FieldDrive<T> TargetField;
#pragma warning restore CS8618, CA1051
	private ResoniteModBase? _mappedMod;

	private ModConfiguration? _mappedConfig;

	private ModConfigurationKey? _mappedKey;

	private bool _definitionFound;

	protected override void OnAwake() {
		base.OnAwake();
		TargetField.SetupValueSetHook((IField<T> field, T value) => {
			if (_mappedKey is not null) {
				if (_mappedKey.Validate(value)) {
					TargetField.Target.Value = value;
					_mappedConfig!.Set(_mappedKey, value);
				}
			}
		});
	}

	protected override void OnChanges() {
		base.OnChanges();
		Unregister();
		if (MapModConfigKey())
			Register();
		DefinitionFound.Value = _definitionFound;
	}

	protected override void OnDispose() {
		Unregister();
		base.OnDispose();
	}

	protected override void OnStart() {
		base.OnStart();
		if (MapModConfigKey())
			Register();
	}

	private bool MapModConfigKey() {
		if (string.IsNullOrEmpty(DefiningModAssembly.Value) || string.IsNullOrEmpty(ConfigurationKeyName.Value))
			return false;
		try {
			_mappedMod = ModLoader.Mods().Single((mod) => Path.GetFileNameWithoutExtension(mod.ModAssembly?.File) == DefiningModAssembly.Value);
			_mappedConfig = _mappedMod?.GetConfiguration();
			_mappedKey = _mappedConfig?.ConfigurationItemDefinitions.Single((key) => key.Name == ConfigurationKeyName.Value);
			if (_mappedMod is null || _mappedConfig is null || _mappedKey is null)
				return false;
			return _mappedKey.ValueType() == typeof(T);
		}
		catch (Exception) {
			return false;
		}
	}

	private void Register() {
		ConfigValueChanged(_mappedConfig.GetValue(_mappedKey));
		_mappedKey!.OnChanged += ConfigValueChanged;
		_definitionFound = true;
	}

	private void Unregister() {
		_mappedKey!.OnChanged -= ConfigValueChanged;
		_mappedMod = null;
		_mappedConfig = null;
		_mappedKey = null;
		_definitionFound = false;
	}

	private void ConfigValueChanged(object? value) {
		if (TargetField.IsLinkValid)
			TargetField.Target.Value = (T)value ?? default;
	}

	public void LoadConfigKey(ModConfiguration config, ModConfigurationKey key) {
		_mappedMod = config.Owner;
		_mappedConfig = config;
		_mappedKey = key;
		DefiningModAssembly.Value = Path.GetFileNameWithoutExtension(config.Owner.ModAssembly!.File);
		ConfigurationKeyName.Value = key.Name;
		Register();
	}
}

public static class ModConfigurationValueSyncExtensions {
	public static ModConfigurationValueSync<T> SyncWithModConfiguration<T>(this IField<T> field, ModConfiguration config, ModConfigurationKey key) {
		Logger.DebugInternal($"Syncing field with [{key}] from {config.Owner.Name}");
		ModConfigurationValueSync<T> driver = field.FindNearestParent<Slot>().AttachComponent<ModConfigurationValueSync<T>>();
		driver.LoadConfigKey(config, key);
		driver.TargetField.Target = field;

		return driver;
	}
}
