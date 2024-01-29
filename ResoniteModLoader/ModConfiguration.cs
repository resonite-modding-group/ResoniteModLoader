using System.Diagnostics;

using FrooxEngine;

using HarmonyLib;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
	private Stopwatch saveTimer = new();

	// time that save must not be called for a save to actually go through
	private int debounceMilliseconds = 3000;

	// used to keep track of mods that spam Save():
	// any mod that calls Save() for the ModConfiguration within debounceMilliseconds of the previous call to the same ModConfiguration
	// will be put into Ultimate Punishment Mode, and ALL their Save() calls, regardless of ModConfiguration, will be debounced.
	// The naughty list is global, while the actual debouncing is per-configuration.
	private static HashSet<string> naughtySavers = new HashSet<string>();

	// used to keep track of the debouncers for this configuration.
	private Dictionary<string, Action<bool>> saveActionForCallee = new();

	private static readonly JsonSerializer jsonSerializer = CreateJsonSerializer();

	private static JsonSerializer CreateJsonSerializer() {
		JsonSerializerSettings settings = new() {
			MaxDepth = 32,
			ReferenceLoopHandling = ReferenceLoopHandling.Error,
			DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
			Formatting = Formatting.Indented
		};
		List<JsonConverter> converters = new();
		IList<JsonConverter> defaultConverters = settings.Converters;
		if (defaultConverters != null && defaultConverters.Count != 0) {
			Logger.DebugFuncInternal(() => $"Using {defaultConverters.Count} default json converters");
			converters.AddRange(defaultConverters);
		}
		converters.Add(new EnumConverter());
		converters.Add(new ResonitePrimitiveConverter());
		settings.Converters = converters;
		return JsonSerializer.Create(settings);
	}

	private ModConfiguration( ModConfigurationDefinition definition) {
		Definition = definition;
	}

	internal static void EnsureDirectoryExists() {
		Directory.CreateDirectory(ConfigDirectory);
	}

	private static string GetModConfigPath(ResoniteModBase mod) {
		if (mod.ModAssembly is null) throw new ArgumentException("Cannot get the config path of a mod that has not been fully loaded");

		string filename = Path.ChangeExtension(Path.GetFileName(mod.ModAssembly.File), ".json");
		return Path.Combine(ConfigDirectory, filename);
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
			using StreamReader file = File.OpenText(configFile);
			using JsonTextReader reader = new(file);
			JObject json = JObject.Load(reader);
			Version version = new(json[VERSION_JSON_KEY]!.ToObject<string>(jsonSerializer));
			if (!AreVersionsCompatible(version, definition.Version)) {
				var handlingMode = mod.HandleIncompatibleConfigurationVersions(definition.Version, version);
				switch (handlingMode) {
					case IncompatibleConfigurationHandlingOption.CLOBBER:
						Logger.WarnInternal($"{mod.Name} saved config version is {version} which is incompatible with mod's definition version {definition.Version}. Clobbering old config and starting fresh.");
						return new ModConfiguration(definition);
					case IncompatibleConfigurationHandlingOption.FORCE_LOAD:
					case IncompatibleConfigurationHandlingOption.FORCELOAD:
						break;
					case IncompatibleConfigurationHandlingOption.ERROR: // fall through to default
					default:
						mod.AllowSavingConfiguration = false;
						throw new ModConfigurationException($"{mod.Name} saved config version is {version} which is incompatible with mod's definition version {definition.Version}");
				}
			}
			foreach (ModConfigurationKey key in definition.ConfigurationItemDefinitions) {
				string keyName = key.Name;
				try {
					JToken? token = json[VALUES_JSON_KEY]?[keyName];
					if (token != null) {
						object? value = token.ToObject(key.ValueType(), jsonSerializer);
						key.Set(value);
					}
				} catch (Exception e) {
					// I know not what exceptions the JSON library will throw, but they must be contained
					mod.AllowSavingConfiguration = false;
					throw new ModConfigurationException($"Error loading {key.ValueType()} config key \"{keyName}\" for {mod.Name}", e);
				}
			}
		} catch (FileNotFoundException) {
			// return early and create a new config
			return new ModConfiguration(definition);
		} catch (Exception e) {
			// I know not what exceptions the JSON library will throw, but they must be contained
			mod.AllowSavingConfiguration = true;
			var backupPath = configFile + "." + Convert.ToBase64String(Encoding.UTF8.GetBytes(((int)DateTimeOffset.Now.TimeOfDay.TotalSeconds).ToString("X"))) + ".bak"; //ExampleMod.json.40A4.bak, unlikely to already exist
			Logger.ErrorExternal($"Error loading config for {mod.Name}, creating new config file (old file can be found at {backupPath}). Exception:\n{e}");
			File.Move(configFile, backupPath);
		}

		return new ModConfiguration(definition);
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
			saveAction = Util.Debounce<bool>(SaveInternal, debounceMilliseconds);
			saveActionForCallee.Add(callee.Name, saveAction);
		}

		if (saveTimer.IsRunning) {
			float elapsedMillis = saveTimer.ElapsedMilliseconds;
			saveTimer.Restart();
			if (elapsedMillis < debounceMilliseconds) {
				Logger.WarnInternal($"ModConfiguration.Save({saveDefaultValues}) called for \"{Owner.Name}\" by \"{callee?.Name}\" from thread with id=\"{thread.ManagedThreadId}\", name=\"{thread.Name}\", bg=\"{thread.IsBackground}\", pool=\"{thread.IsThreadPoolThread}\". Last called {elapsedMillis / 1000f}s ago. This is very recent! Do not spam calls to ModConfiguration.Save()! All Save() calls by this mod are now subject to a {debounceMilliseconds}ms debouncing delay.");
				if (saveAction == null && callee != null) {
					// congrats, you've switched into Ultimate Punishment Mode where now I don't trust you and your Save() calls get debounced
					saveAction = Util.Debounce<bool>(SaveInternal, debounceMilliseconds);
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
		JObject json = new() {
			[VERSION_JSON_KEY] = JToken.FromObject(Definition.Version.ToString(), jsonSerializer)
		};

		JObject valueMap = new();
		foreach (ModConfigurationKey key in ConfigurationItemDefinitions) {
			if (key.TryGetValue(out object? value)) {
				valueMap[key.Name] = value == null ? null : JToken.FromObject(value, jsonSerializer);
			} else if (saveDefaultValues && key.TryComputeDefault(out object? defaultValue)) {
				valueMap[key.Name] = defaultValue == null ? null : JToken.FromObject(defaultValue, jsonSerializer);
			}
		}

		json[VALUES_JSON_KEY] = valueMap;

		string configFile = GetModConfigPath(Owner);

		File.WriteAllText(configFile, json.ToString());

		Logger.DebugFuncInternal(() => $"Saved ModConfiguration for \"{Owner.Name}\" in {stopwatch.ElapsedMilliseconds}ms");
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

	private static void ShutdownHook() {
		int count = 0;
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
		Logger.MsgInternal($"Configs saved for {count} mods.");
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
	[Obsolete ("Use IncompatibleConfigurationHandlingOption.FORCELOAD Instead")]
	FORCE_LOAD,

	/// <summary>
	/// Ignore the version number and attempt to load the config from disk.
	/// </summary>
	FORCELOAD,
}
