using Elements.Core;
using FrooxEngine;

namespace ResoniteModLoader;

/// <summary>
/// Bi-directionally syncs a field with a specific mod configuration key.
/// </summary>
/// <typeparam name="T">The mod configuration key type</typeparam>
[Category(["ResoniteModLoader"])]
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
#pragma warning disable CS1591
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
#pragma warning restore CS1591
	/// <summary>
	/// Attempts to match the supplied <see cref="DefiningModAssembly"/> and <see cref="ConfigurationKeyName"/> fields to a mod config and key
	/// </summary>
	/// <returns>Success</returns>
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

	/// <summary>
	/// Call AFTER mapping has been confirmed to begin syncing the target field
	/// </summary>
	private void Register() {
		ConfigValueChanged(_mappedConfig.GetValue(_mappedKey));
		_mappedKey!.OnChanged += ConfigValueChanged;
		_definitionFound = true;
	}

	/// <summary>
	/// Stop syncing, call whenever any field has changed to make sure the rug isn't pulled out from under us.
	/// </summary>
	private void Unregister() {
		if (_mappedKey is not null)
			_mappedKey.OnChanged -= ConfigValueChanged;
		_mappedMod = null;
		_mappedConfig = null;
		_mappedKey = null;
		_definitionFound = false;
	}

	private void ConfigValueChanged(object? value) {
		if (TargetField.IsLinkValid)
			TargetField.Target.Value = (T)value ?? default;
	}

	/// <summary>
	/// Sets the <see cref="DefiningModAssembly"/> and <see cref="ConfigurationKeyName"/> fields to match the supplied config and key.
	/// </summary>
	/// <param name="config">The configuration the key belongs to</param>
	/// <param name="key">Any key with a matching type</param>
	public void LoadConfigKey(ModConfiguration config, ModConfigurationKey<T> key) {
		if (!config.IsKeyDefined(key))
			throw new InvalidOperationException($"Mod key ({key}) is not owned by {config.Owner.Name}'s config");

		_mappedMod = config.Owner;
		_mappedConfig = config;
		_mappedKey = key;
		DefiningModAssembly.Value = Path.GetFileNameWithoutExtension(config.Owner.ModAssembly!.File);
		ConfigurationKeyName.Value = key.Name;
		Register();
	}
}

/// <summary>
/// Utilities methods that attaches <see cref="ModConfigurationValueSync{T}"/> to stuff.
/// </summary>
public static class ModConfigurationValueSyncExtensions {
	/// <summary>
	/// Syncs a target IField with a mod configuration key.
	/// </summary>
	/// <typeparam name="T">The field and key type</typeparam>
	/// <param name="field">The field to bi-directionally sync</param>
	/// <param name="config">The configuration the key belongs to</param>
	/// <param name="key">Any key with a matching type</param>
	/// <returns>A new <see cref="ModConfigurationValueSync{T}"/> component that was attached to the same slot as the field.</returns>
	/// <exception cref="InvalidOperationException">Thrown if key doesn't belong to config, or is of wrong type</exception>
	public static ModConfigurationValueSync<T> SyncWithModConfiguration<T>(this IField<T> field, ModConfiguration config, ModConfigurationKey key) {
		if (!config.IsKeyDefined(key))
			throw new InvalidOperationException($"Mod key ({key}) is not owned by {config.Owner.Name}'s config");
		if (key.ValueType() != typeof(T))
			throw new InvalidOperationException($"Type of mod key ({key}) does not match field type {typeof(T)}");

		Logger.DebugInternal($"Syncing field with [{key}] from {config.Owner.Name}");
		ModConfigurationValueSync<T> driver = field.FindNearestParent<Slot>().AttachComponent<ModConfigurationValueSync<T>>();
		driver.LoadConfigKey(config, key as ModConfigurationKey<T>);
		driver.TargetField.Target = field;

		return driver;
	}
}
