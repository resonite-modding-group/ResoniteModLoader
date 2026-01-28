using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FrooxEngine;

using HarmonyLib;

using ResoniteModLoader.JsonConverters;

namespace ResoniteModLoader;

/// <summary>
/// Represents an interface for mod configurations.
/// </summary>
public interface IModConfigurationDefinition {
	/// <summary>
	/// Gets the mod that owns this configuration definition.
	/// </summary>
	ResoniteModBase Owner { get; }

	/// <summary>
	/// Gets the semantic version for this configuration definition. This is used to check if the defined and saved configs are compatible.
	/// </summary>
	Version Version { get; }

	/// <summary>
	/// Gets the set of configuration keys defined in this configuration definition.
	/// </summary>
	ISet<ModConfigurationKey> ConfigurationItemDefinitions { get; }
}

/// <summary>
/// Defines a mod configuration. This should be defined by a <see cref="ResoniteMod"/> using the <see cref="ResoniteMod.DefineConfiguration(ModConfigurationDefinitionBuilder)"/> method.
/// </summary>
public class ModConfigurationDefinition : IModConfigurationDefinition {
	/// <inheritdoc/>
	public ResoniteModBase Owner { get; private set; }

	/// <inheritdoc/>
	public Version Version { get; private set; }

	internal bool AutoSave;

	// this is a ridiculous hack because HashSet.TryGetValue doesn't exist in .NET 4.6.2
	private Dictionary<ModConfigurationKey, ModConfigurationKey> configurationItemDefinitionsSelfMap;

	/// <inheritdoc/>
	public ISet<ModConfigurationKey> ConfigurationItemDefinitions {
		// clone the collection because I don't trust giving public API users shallow copies one bit
		get => new HashSet<ModConfigurationKey>(configurationItemDefinitionsSelfMap.Keys);
	}

	internal bool TryGetDefiningKey(ModConfigurationKey key, out ModConfigurationKey? definingKey) {
		if (key.DefiningKey != null) {
			// we've already cached the defining key
			definingKey = key.DefiningKey;
			return true;
		}

		// first time we've seen this key instance: we need to hit the map
		if (configurationItemDefinitionsSelfMap.TryGetValue(key, out definingKey)) {
			// initialize the cache for this key
			key.DefiningKey = definingKey;
			return true;
		} else {
			// not a real key
			definingKey = null;
			return false;
		}

	}

	internal ModConfigurationDefinition(ResoniteModBase owner, Version version, HashSet<ModConfigurationKey> configurationItemDefinitions, bool autoSave) {
		Owner = owner;
		Version = version;
		AutoSave = autoSave;

		configurationItemDefinitionsSelfMap = new Dictionary<ModConfigurationKey, ModConfigurationKey>(configurationItemDefinitions.Count);
		foreach (ModConfigurationKey key in configurationItemDefinitions) {
			key.DefiningKey = key; // early init this property for the defining key itself
			configurationItemDefinitionsSelfMap.Add(key, key);
		}
	}
}

/// <summary>
/// The configuration for a mod. Each mod has zero or one configuration. The configuration object will never be reassigned once initialized.
/// </summary>
public class ModConfiguration : IModConfigurationDefinition {
	private readonly ModConfigurationDefinition Definition;

	private static readonly string ConfigDirectory = Path.Combine(Directory.GetCurrentDirectory(), "rml_config");
	private const string VERSION_JSON_KEY = "version";
	private const string VALUES_JSON_KEY = "values";

	/// <inheritdoc/>
	public ResoniteModBase Owner => Definition.Owner;

	/// <inheritdoc/>
	public Version Version => Definition.Version;

	/// <inheritdoc/>
	public ISet<ModConfigurationKey> ConfigurationItemDefinitions => Definition.ConfigurationItemDefinitions;

	private bool AutoSave => Definition.AutoSave;

	/// <summary>
	/// The delegate that is called for configuration change events.
	/// </summary>
	/// <param name="configurationChangedEvent">The event containing details about the configuration change</param>
	public delegate void ConfigurationChangedHandler(ConfigurationChangedEvent configurationChangedEvent);

	/// <summary>
	/// Called if any config value for any mod changed.
	/// </summary>
	public static event ConfigurationChangedHandler? OnAnyConfigurationChanged;

	/// <summary>
	/// Called if one of the values in this mod's config changed.
	/// </summary>
	public event ConfigurationChangedHandler? OnThisConfigurationChanged;

	// used to track how frequenly Save() is being called
	private readonly Stopwatch saveTimer = new();

	// time that save must not be called for a save to actually go through
	private const int DEBOUNCE_MILLIS = 3000;

	// used to keep track of mods that spam Save():
	// any mod that calls Save() for the ModConfiguration within debounceMilliseconds of the previous call to the same ModConfiguration
	// will be put into Ultimate Punishment Mode, and ALL their Save() calls, regardless of ModConfiguration, will be debounced.
	// The naughty list is global, while the actual debouncing is per-configuration.
	private static readonly HashSet<string> naughtySavers = [];

	// used to keep track of the debouncers for this configuration.
	private readonly Dictionary<string, Action<bool>> saveActionForCallee = [];

	private static readonly JsonSerializerOptions jsonSerializerOptions;
	private static readonly JsonReaderOptions jsonReaderOptions;
	private static readonly JsonWriterOptions jsonWriterOptions;

	static ModConfiguration() {
		JsonSerializerOptions options = new(JsonSerializerDefaults.Strict) {
			MaxDepth = 32,
			ReferenceHandler = ReferenceHandler.IgnoreCycles,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
			WriteIndented = true,
			IndentSize = 2,
			TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
		};
		var converters = options.Converters;
		if (converters.Count != 0) {
			Logger.DebugFuncInternal(() => $"Using {converters.Count} default json converters");
		}
		converters.Add(new EnumConverter());
		converters.Add(new ResonitePrimitiveConverter());
		options.MakeReadOnly();
		jsonSerializerOptions = options;

		jsonReaderOptions = new() {
			MaxDepth = options.MaxDepth,
			AllowMultipleValues = false,
		};

		jsonWriterOptions = new() {
			Indented = options.WriteIndented,
			IndentSize = options.IndentSize,
			IndentCharacter = ' ',
		};
	}

	private ModConfiguration(ModConfigurationDefinition definition) {
		Definition = definition;
	}

	internal static void EnsureDirectoryExists() {
		Directory.CreateDirectory(ConfigDirectory);
	}

	private static string GetModConfigPath(ResoniteModBase mod) {
		if (mod.ModAssembly is null) {
			throw new ArgumentException("Cannot get the config path of a mod that has not been fully loaded");
		}

		string filename = Path.ChangeExtension(Path.GetFileName(mod.ModAssembly.File), ".json");
		return Path.Combine(ConfigDirectory, filename);
	}

	/// <summary>
	/// Checks if the given key is defined in this config.
	/// </summary>
	/// <param name="key">The key to check.</param>
	/// <returns><c>true</c> if the key is defined.</returns>
	public bool IsKeyDefined(ModConfigurationKey key) {
		// if a key has a non-null defining key it's guaranteed a real key. Lets check for that.
		ModConfigurationKey? definingKey = key.DefiningKey;
		if (definingKey != null) {
			return true;
		}

		// The defining key was null, so lets try to get the defining key from the hashtable instead
		if (Definition.TryGetDefiningKey(key, out definingKey)) {
			// we might as well set this now that we have the real defining key
			key.DefiningKey = definingKey;
			return true;
		}

		// there was no definition
		return false;
	}

	/// <summary>
	/// Checks if the given key is the defining key.
	/// </summary>
	/// <param name="key">The key to check.</param>
	/// <returns><c>true</c> if the key is the defining key.</returns>
	internal static bool IsKeyDefiningKey(ModConfigurationKey key) {
		// a key is the defining key if and only if its DefiningKey property references itself
		return ReferenceEquals(key, key.DefiningKey); // this is safe because we'll throw a NRE if key is null
	}

	/// <summary>
	/// Get a value, throwing a <see cref="KeyNotFoundException"/> if the key is not found.
	/// </summary>
	/// <param name="key">The key to get the value for.</param>
	/// <returns>The value for the key.</returns>
	/// <exception cref="KeyNotFoundException">The given key does not exist in the configuration.</exception>
	public object GetValue(ModConfigurationKey key) {
		if (TryGetValue(key, out object? value)) {
			return value!;
		} else {
			throw new KeyNotFoundException($"{key.Name} not found in {Owner.Name} configuration");
		}
	}

	/// <summary>
	/// Get a value, throwing a <see cref="KeyNotFoundException"/> if the key is not found.
	/// </summary>
	/// <typeparam name="T">The type of the key's value.</typeparam>
	/// <param name="key">The key to get the value for.</param>
	/// <returns>The value for the key.</returns>
	/// <exception cref="KeyNotFoundException">The given key does not exist in the configuration.</exception>
	public T? GetValue<T>(ModConfigurationKey<T> key) {
		if (TryGetValue(key, out T? value)) {
			return value;
		} else {
			throw new KeyNotFoundException($"{key.Name} not found in {Owner.Name} configuration");
		}
	}

	/// <summary>
	/// Tries to get a value, returning <c>default</c> if the key is not found.
	/// </summary>
	/// <param name="key">The key to get the value for.</param>
	/// <param name="value">The value if the return value is <c>true</c>, or <c>default</c> if <c>false</c>.</param>
	/// <returns><c>true</c> if the value was read successfully.</returns>
	public bool TryGetValue(ModConfigurationKey key, out object? value) {
		if (!Definition.TryGetDefiningKey(key, out ModConfigurationKey? definingKey)) {
			// not in definition
			value = null;
			return false;
		}

		if (definingKey!.TryGetValue(out object? valueObject)) {
			value = valueObject;
			return true;
		} else if (definingKey.TryComputeDefault(out value)) {
			return true;
		} else {
			value = null;
			return false;
		}
	}


	/// <summary>
	/// Tries to get a value, returning <c>default(<typeparamref name="T"/>)</c> if the key is not found.
	/// </summary>
	/// <param name="key">The key to get the value for.</param>
	/// <param name="value">The value if the return value is <c>true</c>, or <c>default</c> if <c>false</c>.</param>
	/// <returns><c>true</c> if the value was read successfully.</returns>
	public bool TryGetValue<T>(ModConfigurationKey<T> key, out T? value) {
		if (TryGetValue(key, out object? valueObject)) {
			value = (T)valueObject!;
			return true;
		} else {
			value = default;
			return false;
		}
	}

	/// <summary>
	/// Sets a configuration value for the given key, throwing a <see cref="KeyNotFoundException"/> if the key is not found
	/// or an <see cref="ArgumentException"/> if the value is not valid for it.
	/// </summary>
	/// <param name="key">The key to get the value for.</param>
	/// <param name="value">The new value to set.</param>
	/// <param name="eventLabel">A custom label you may assign to this change event.</param>
	/// <exception cref="KeyNotFoundException">The given key does not exist in the configuration.</exception>
	/// <exception cref="ArgumentException">The new value is not valid for the given key.</exception>
	public void Set(ModConfigurationKey key, object? value, string? eventLabel = null) {
		if (!Definition.TryGetDefiningKey(key, out ModConfigurationKey? definingKey)) {
			throw new KeyNotFoundException($"{key.Name} is not defined in the config definition for {Owner.Name}");
		}

		if (value == null) {
			if (Util.CannotBeNull(definingKey!.ValueType())) {
				throw new ArgumentException($"null cannot be assigned to {definingKey.ValueType()}");
			}
		} else if (!definingKey!.ValueType().IsAssignableFrom(value.GetType())) {
			throw new ArgumentException($"{value.GetType()} cannot be assigned to {definingKey.ValueType()}");
		}

		if (!definingKey!.Validate(value)) {
			throw new ArgumentException($"\"{value}\" is not a valid value for \"{Owner.Name}{definingKey.Name}\"");
		}

		definingKey.Set(value);
		FireConfigurationChangedEvent(definingKey, eventLabel);
	}

	/// <summary>
	/// Sets a configuration value for the given key, throwing a <see cref="KeyNotFoundException"/> if the key is not found
	/// or an <see cref="ArgumentException"/> if the value is not valid for it.
	/// </summary>
	/// <typeparam name="T">The type of the key's value.</typeparam>
	/// <param name="key">The key to get the value for.</param>
	/// <param name="value">The new value to set.</param>
	/// <param name="eventLabel">A custom label you may assign to this change event.</param>
	/// <exception cref="KeyNotFoundException">The given key does not exist in the configuration.</exception>
	/// <exception cref="ArgumentException">The new value is not valid for the given key.</exception>
	public void Set<T>(ModConfigurationKey<T> key, T value, string? eventLabel = null) {
		// the reason we don't fall back to untyped Set() here is so we can skip the type check

		if (!Definition.TryGetDefiningKey(key, out ModConfigurationKey? definingKey)) {
			throw new KeyNotFoundException($"{key.Name} is not defined in the config definition for {Owner.Name}");
		}

		if (!definingKey!.Validate(value)) {
			throw new ArgumentException($"\"{value}\" is not a valid value for \"{Owner.Name}{definingKey.Name}\"");
		}
		definingKey.Set(value);
		FireConfigurationChangedEvent(definingKey, eventLabel);
	}

	/// <summary>
	/// Removes a configuration value, throwing a <see cref="KeyNotFoundException"/> if the key is not found.
	/// </summary>
	/// <param name="key">The key to remove the value for.</param>
	/// <returns><c>true</c> if a value was successfully found and removed, <c>false</c> if there was no value to remove.</returns>
	/// <exception cref="KeyNotFoundException">The given key does not exist in the configuration.</exception>
	public bool Unset(ModConfigurationKey key) {
		if (Definition.TryGetDefiningKey(key, out ModConfigurationKey? definingKey)) {
			return definingKey!.Unset();
		} else {
			throw new KeyNotFoundException($"{key.Name} is not defined in the config definition for {Owner.Name}");
		}
	}

	private bool AnyValuesSet() {
		return ConfigurationItemDefinitions
			.Where(key => key.HasValue)
			.Any();
	}

	internal static ModConfiguration? LoadConfigForMod(ResoniteMod mod) {
		ModConfigurationDefinition? definition = mod.BuildConfigurationDefinition();
		if (definition == null) {
			// if there's no definition, then there's nothing for us to do here
			return null;
		}

		string configFile = GetModConfigPath(mod);

		try {
			var file = File.ReadAllBytes(configFile);
			Utf8JsonReader reader = new(file, jsonReaderOptions);
			return ReadModConfiguration(ref reader, jsonSerializerOptions, definition, mod);
		} catch (FileNotFoundException) {
			// return early and create a new config
			return new ModConfiguration(definition);
		} catch (Exception e) {
			// I know not what exceptions the JSON library will throw, but they must be contained
			mod.AllowSavingConfiguration = false;
			var backupPath = configFile + "." + Convert.ToBase64String(Encoding.UTF8.GetBytes(((int)DateTimeOffset.Now.TimeOfDay.TotalSeconds).ToString("X"))) + ".bak"; //ExampleMod.json.40A4.bak, unlikely to already exist
			Logger.ErrorInternal($"Error loading config for {mod.Name}, creating new config file (old file can be found at {backupPath}). Exception:\n{e}");
			File.Move(configFile, backupPath);
		}

		return new ModConfiguration(definition);
	}

	private static ModConfiguration ReadModConfiguration(
		ref Utf8JsonReader reader,
		JsonSerializerOptions options,
		ModConfigurationDefinition definition,
		ResoniteMod mod
	) {
		reader.Read();
		if (reader.TokenType != JsonTokenType.StartObject) {
			throw new JsonException($"Expected an object, got {reader.TokenType}");
		}
		reader.Read(); // Consume start of object

		// Read "version": "..."

		if (reader.GetString() != VERSION_JSON_KEY) {
			throw new JsonException($"Expected first property to be '{VERSION_JSON_KEY}'");
		}
		reader.Read();

		var versionString = reader.GetString()
			?? throw new JsonException("Version string is null");
		Logger.MsgInternal($"Version: '{versionString}'");
		Version version = new(versionString);
		reader.Read();

		if (!AreVersionsCompatible(version, definition.Version)) {
			var handlingMode = mod.HandleIncompatibleConfigurationVersions(version, definition.Version);
			switch (handlingMode) {
				case IncompatibleConfigurationHandlingOption.CLOBBER:
					Logger.WarnInternal($"{mod.Name} saved config version is {version} which is incompatible with mod's definition version {definition.Version}. Clobbering old config and starting fresh.");
					return new ModConfiguration(definition);
				case IncompatibleConfigurationHandlingOption.FORCELOAD:
					break;
				case IncompatibleConfigurationHandlingOption.ERROR: // fall through to default
				default:
					mod.AllowSavingConfiguration = false;
					throw new ModConfigurationException($"{mod.Name} saved config version is {version} which is incompatible with mod's definition version {definition.Version}");
			}
		}

		// Read "values": { ... }

		if (reader.GetString() != VALUES_JSON_KEY)
			throw new JsonException($"Expected second property to be '{VALUES_JSON_KEY}'");
		reader.Read();

		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"Expected an object, got {reader.TokenType}");
		reader.Read(); // Consume start of object

		var keys = definition.ConfigurationItemDefinitions.ToDictionary(key => key.Name);

		while (reader.TokenType != JsonTokenType.EndObject) {
			var name = reader.GetString()
				?? throw new JsonException("Object key is null");
			reader.Read();

			// Ignore unknown keys
			if (!keys.TryGetValue(name, out var key)) {
				Logger.WarnInternal($"{mod.Name} saved config version contains entry '{name}' which does not exist in its configuration definition");
				continue;
			}

			var value = DynamicJsonConverter.Read(ref reader, key.ValueType(), options);
			key.Set(value);
			reader.Read();
		}
		reader.Read(); // Consume end of object

		if (reader.TokenType != JsonTokenType.EndObject) {
			throw new JsonException($"Extra keys in configuration object");
		}

		// Exit on end object token

		return new(definition);
	}

	private static bool AreVersionsCompatible(Version serializedVersion, Version currentVersion) {
		if (serializedVersion.Major != currentVersion.Major) {
			// major version differences are hard incompatible
			return false;
		}

		if (serializedVersion.Minor > currentVersion.Minor) {
			// if serialized config has a newer minor version than us
			// in other words, someone downgraded the mod but not the config
			// then we cannot load the config
			return false;
		}

		// none of the checks failed!
		return true;
	}

	/// <summary>
	/// Persist this configuration to disk.<br/>
	/// This method is not called automatically.
	/// </summary>
	/// <param name="saveDefaultValues">If <c>true</c>, default values will also be persisted.</param>
	/// <remarks>
	/// Saving too often may result in save calls being debounced, with only the latest save call being used after a delay.
	/// </remarks>
	public void Save(bool saveDefaultValues = false) {
		SaveQueue(saveDefaultValues, false);
	}

	/// <summary>
	/// Asynchronously persists this configuration to disk.
	/// </summary>
	/// <param name="saveDefaultValues">If <c>true</c>, default values will also be persisted.</param>
	/// <param name="immediate">If <c>true</c>, skip the debouncing and save immediately.</param>
	/// <remarks>
	/// immediate isn't used anywhere nor exposed outside of internal, mods shouldn't be bypassing debounce.
	/// </remarks>
	internal void SaveQueue(bool saveDefaultValues = false, bool immediate = false) {
		Thread thread = Thread.CurrentThread;
		ResoniteMod? callee = Util.ExecutingMod(new(1));
		Action<bool>? saveAction = null;

		// get saved state for this callee
		if (callee != null && naughtySavers.Contains(callee.Name) && !saveActionForCallee.TryGetValue(callee.Name, out saveAction)) {
			// handle case where the callee was marked as naughty from a different ModConfiguration being spammed
			saveAction = Util.Debounce<bool>(SaveInternal, DEBOUNCE_MILLIS);
			saveActionForCallee.Add(callee.Name, saveAction);
		}

		if (saveTimer.IsRunning) {
			float elapsedMillis = saveTimer.ElapsedMilliseconds;
			saveTimer.Restart();
			if (elapsedMillis < DEBOUNCE_MILLIS) {
				Logger.WarnInternal($"ModConfiguration.Save({saveDefaultValues}) called for \"{Owner.Name}\" by \"{callee?.Name}\" from thread with id=\"{thread.ManagedThreadId}\", name=\"{thread.Name}\", bg=\"{thread.IsBackground}\", pool=\"{thread.IsThreadPoolThread}\". Last called {elapsedMillis / 1000f}s ago. This is very recent! Do not spam calls to ModConfiguration.Save()! All Save() calls by this mod are now subject to a {DEBOUNCE_MILLIS}ms debouncing delay.");
				if (saveAction == null && callee != null) {
					// congrats, you've switched into Ultimate Punishment Mode where now I don't trust you and your Save() calls get debounced
					saveAction = Util.Debounce<bool>(SaveInternal, DEBOUNCE_MILLIS);
					saveActionForCallee.Add(callee.Name, saveAction);
					naughtySavers.Add(callee.Name);
				}
			} else {
				Logger.DebugFuncInternal(() => $"ModConfiguration.Save({saveDefaultValues}) called for \"{Owner.Name}\" by \"{callee?.Name}\" from thread with id=\"{thread.ManagedThreadId}\", name=\"{thread.Name}\", bg=\"{thread.IsBackground}\", pool=\"{thread.IsThreadPoolThread}\". Last called {elapsedMillis / 1000f}s ago.");
			}
		} else {
			saveTimer.Start();
			Logger.DebugFuncInternal(() => $"ModConfiguration.Save({saveDefaultValues}) called for \"{Owner.Name}\" by \"{callee?.Name}\" from thread with id=\"{thread.ManagedThreadId}\", name=\"{thread.Name}\", bg=\"{thread.IsBackground}\", pool=\"{thread.IsThreadPoolThread}\"");
		}

		// prevent saving if we've determined something is amiss with the configuration
		if (!Owner.AllowSavingConfiguration) {
			Logger.WarnInternal($"ModConfiguration for {Owner.Name} will NOT be saved due to a safety check failing. This is probably due to you downgrading a mod.");
			return;
		}

		if (immediate || saveAction == null) {
			// infrequent callers get to save immediately
			Task.Run(() => SaveInternal(saveDefaultValues));
		} else {
			// bad callers get debounced
			saveAction(saveDefaultValues);
		}
	}

	/// <summary>
	/// Performs the actual, synchronous save
	/// </summary>
	/// <param name="saveDefaultValues">If true, default values will also be persisted</param>
	private void SaveInternal(bool saveDefaultValues = false) {
		Stopwatch stopwatch = Stopwatch.StartNew();
		string configFile = GetModConfigPath(Owner);

		using var file = File.Open(configFile, FileMode.Create, FileAccess.Write);
		using Utf8JsonWriter writer = new(file, jsonWriterOptions);
		WriteModConfiguration(writer, jsonSerializerOptions, saveDefaultValues);

		Logger.DebugFuncInternal(() => $"Saved ModConfiguration for \"{Owner.Name}\" in {stopwatch.ElapsedMilliseconds}ms");
	}

	private void WriteModConfiguration(Utf8JsonWriter writer, JsonSerializerOptions options, bool saveDefaultValues) {
		writer.WriteStartObject();

		writer.WriteString(VERSION_JSON_KEY, Definition.Version.ToString());

		writer.WritePropertyName(VALUES_JSON_KEY);
		writer.WriteStartObject();

		foreach (var key in Definition.ConfigurationItemDefinitions) {
			if (key.TryGetValue(out object? writtenValue)) {
				// write
			}
			else if (saveDefaultValues && key.TryComputeDefault(out writtenValue)) {
				// write
			}
			else {
				continue;
			}
			writer.WritePropertyName(key.Name);
			if (writtenValue == null) {
				writer.WriteNullValue();
				continue;
			}
			DynamicJsonConverter.Write(writer, key.ValueType(), writtenValue, options);
		}

		writer.WriteEndObject();
		writer.WriteEndObject();
	}

	private void FireConfigurationChangedEvent(ModConfigurationKey key, string? label) {
		try {
			OnAnyConfigurationChanged?.SafeInvoke(new ConfigurationChangedEvent(this, key, label));
		} catch (Exception e) {
			Logger.ErrorInternal($"An OnAnyConfigurationChanged event subscriber threw an exception:\n{e}");
		}

		try {
			OnThisConfigurationChanged?.SafeInvoke(new ConfigurationChangedEvent(this, key, label));
		} catch (Exception e) {
			Logger.ErrorInternal($"An OnThisConfigurationChanged event subscriber threw an exception:\n{e}");
		}
	}

	internal static void RegisterShutdownHook(Harmony harmony) {
		try {
			MethodInfo shutdown = AccessTools.DeclaredMethod(typeof(Engine), nameof(Engine.RequestShutdown));
			if (shutdown == null) {
				Logger.ErrorInternal("Could not find method Engine.RequestShutdown(). Will not be able to autosave configs on close!");
				return;
			}
			MethodInfo patch = AccessTools.DeclaredMethod(typeof(ModConfiguration), nameof(ShutdownHook));
			if (patch == null) {
				Logger.ErrorInternal("Could not find method ModConfiguration.ShutdownHook(). Will not be able to autosave configs on close!");
				return;
			}
			harmony.Patch(shutdown, prefix: new HarmonyMethod(patch));
		} catch (Exception e) {
			Logger.ErrorInternal($"Unexpected exception applying shutdown hook!\n{e}");
		}
	}

	internal static void ShutdownHook() {
		int count = 0;
		Stopwatch stopwatch = Stopwatch.StartNew();
		ModLoader.Mods()
			.Select(mod => mod.GetConfiguration())
			.Where(config => config != null)
			.Where(config => config!.AutoSave)
			.Where(config => config!.AnyValuesSet())
			.Do(config => {
				try {
					// synchronously save the config
					config!.SaveInternal();
				} catch (Exception e) {
					Logger.ErrorInternal($"Error saving configuration for {config!.Owner.Name}:\n{e}");
				}
				count += 1;
			});
		stopwatch.Stop();
		Logger.MsgInternal($"Configs saved for {count} mods in {stopwatch.ElapsedMilliseconds}ms.");
	}
}

/// <summary>
/// Represents an <see cref="Exception"/> encountered while loading a mod's configuration file.
/// </summary>
public class ModConfigurationException : Exception {
	internal ModConfigurationException(string message) : base(message) { }

	internal ModConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Defines options for the handling of incompatible configuration versions.
/// </summary>
public enum IncompatibleConfigurationHandlingOption {
	/// <summary>
	/// Fail to read the config, and block saving over the config on disk.
	/// </summary>
	ERROR,

	/// <summary>
	/// Destroy the saved config and start over from scratch.
	/// </summary>
	CLOBBER,

	/// <summary>
	/// Ignore the version number and attempt to load the config from disk.
	/// </summary>
	FORCELOAD,
}
