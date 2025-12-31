using HarmonyLib;

namespace ResoniteModLoader;
/// <summary>
/// Represents a fluent configuration interface to define mod configurations.
/// </summary>
public class ModConfigurationDefinitionBuilder {
	private readonly ResoniteModBase Owner;
	private Version ConfigVersion = new(1, 0, 0);
	private readonly HashSet<ModConfigurationKey> Keys = [];
	private bool AutoSaveConfig = true;

	internal ModConfigurationDefinitionBuilder(ResoniteModBase owner) {
		Owner = owner;
	}

	/// <summary>
	/// Sets the semantic version of this configuration definition. Default is 1.0.0.
	/// </summary>
	/// <param name="version">The config's semantic version.</param>
	/// <returns>This builder.</returns>
	public ModConfigurationDefinitionBuilder Version(Version version) {
		ConfigVersion = version;
		return this;
	}

	/// <summary>
	/// Sets the semantic version of this configuration definition. Default is 1.0.0.
	/// </summary>
	/// <param name="version">The config's semantic version, as a string.</param>
	/// <returns>This builder.</returns>
	public ModConfigurationDefinitionBuilder Version(string version) {
		ConfigVersion = new Version(version);
		return this;
	}

	/// <summary>
	/// Adds a new key to this configuration definition.
	/// </summary>
	/// <param name="key">A configuration key.</param>
	/// <returns>This builder.</returns>
	public ModConfigurationDefinitionBuilder Key(ModConfigurationKey key) {
		Keys.Add(key);
		return this;
	}

	/// <summary>
	/// Sets the AutoSave property of this configuration definition. Default is <c>true</c>.
	/// </summary>
	/// <param name="autoSave">If <c>false</c>, the config will not be autosaved on Resonite close.</param>
	/// <returns>This builder.</returns>
	public ModConfigurationDefinitionBuilder AutoSave(bool autoSave) {
		AutoSaveConfig = autoSave;
		return this;
	}

	internal void ProcessAttributes() {
		AccessTools.GetDeclaredFields(Owner.GetType())
			.Where(field => field.GetCustomAttribute<AutoRegisterConfigKeyAttribute>() is not null)
			.Do(ProcessField);
	}

	private void ProcessField(FieldInfo field) {
		if (!typeof(ModConfigurationKey).IsAssignableFrom(field.FieldType)) {
			Logger.WarnInternal($"{Owner.Name} had an [AutoRegisterConfigKey] field of the wrong type: {field}");
			return;
		}

		if (field.GetValue(field.IsStatic ? null : Owner) is ModConfigurationKey fieldValue) {
			Keys.Add(fieldValue);
		}
	}

	internal ModConfigurationDefinition? Build() {
		if (Keys.Count > 0) {
			return new ModConfigurationDefinition(Owner, ConfigVersion, Keys, AutoSaveConfig);
		}
		return null;
	}
}
