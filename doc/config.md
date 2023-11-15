# ResoniteModLoader Configuration System

ResoniteModLoader provides a built-in configuration system that can be used to persist configuration values for mods.
Operations provided:

- Reading value of a config key
- Writing value to a config key
- Enumerating config keys for a mod
- Enumerating mods
- Saving a config to disk

Behind the scenes, configs are saved to a `rml_config` folder in the Resonite install directory. The `rml_config` folder contains JSON files, named after each mod dll that defines a config. End users and mod developers do not need to interact with this JSON directly. Mod developers should use the API exposed by ResoniteModLoader. End users should use interfaces exposed by configuration management mods.

## Overview

- Mods may define a configuration
- Configuration items must be declared alongside the mod itself. You cannot change your configuration schema at runtime.
- Configuration items may be of any type, however, there are considerations:
  - Json.NET is used to serialize the configuration, so the type must be JSON-compatible (e.g. no circular references). Lists, Sets, and Dictionary<string, T> will work fine.
  - Using complex types will make it more difficult for configuration manager UIs to interface with your mod. For best compatibility keep things simple (primitive types and basic collections)
- Reading/writing configuration values is done in-memory and is extremely cheap.
- Saving configuration to disk is more expensive but is done infrequently

## Working With Your Mod's Configuration

A simple example is below:

```csharp
using HarmonyLib; // HarmonyLib comes included with a ResoniteModLoader install
using ResoniteModLoader;
using System;
using System.Reflection;

namespace ConfigurationExampleMod;

public class ConfigurationExampleMod : ResoniteMod {
    public override string Name => "ConfigurationExampleMod";
    public override string Author => "YourNameHere";
    public override string Version => "1.0.0"; //Version of the mod, should match the AssemblyVersion
    public override string Link => "https://github.com/YourNameHere/ConfigurationExampleMod"; // Optional link to a repo where this mod would be located

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> KEY_COUNT = new ModConfigurationKey<int>("count", "Example counter", internalAccessOnly: true); //Mod config for an int

    private static ModConfiguration Config; //This holds your mods' ModConfiguration

    public override void OnEngineInit() {
        Config = GetConfiguration(); //Get this mods' current ModConfiguration
        int countValue = default(int);
        if (Config.TryGetValue(KEY_COUNT, out countValue)) {
            int oldValue = countValue++;
            Msg($"Incrementing count from {oldValue} to {countValue}");
        } else {
            Msg($"Initializing count to {countValue}");
        }

        Config.Set(KEY_COUNT, countValue);
    }
}
```

### Defining a Configuration

To define a configuration simply have at least one `ModConfigurationKey` field with the `[AutoRegisterConfigKey]` attribute applied.

If you need more options, implement the optional `DefineConfiguration` method in your mod. Here's an example:

```csharp
// this override lets us change optional settings in our configuration definition
public override void DefineConfiguration(ModConfigurationDefinitionBuilder builder) {
    builder
        .Version(new Version(1, 0, 0)) // manually set config version (default is 1.0.0)
        .AutoSave(false); // don't autosave on shutdown (default is true)
}
```

This `ModConfigurationDefinitionBuilder` allows you to change the default version and autosave values. [Version](#configuration-version) and [AutoSave](#saving-the-configuration) will be discussed in separate sections..

#### Configuration Version

You may optionally specify a version for your configuration. This is separate from your mod's version. By default, the version will be 1.0.0. The version should be a [semantic version][semver]—in summary the major version should be bumped for hard breaking changes, and the minor version should be bumped if you break backwards compatibility. ResoniteModLoader uses this version number to check the saved configuration against your definition and ensure they are compatible.

#### Configuration Keys

Configuration keys define the values your mod's config can store. The relevant class is `ModConfigurationKey<T>`, which has the following constructor:

```csharp
public ModConfigurationKey(string name, string description, Func<T> computeDefault = null, bool internalAccessOnly = false, Predicate<T> valueValidator = null)
```

|Parameter | Description | Default |
| -------- | ----------- | ------- |
| name | Unique name of this config item | *required* |
| description | Human-readable description of this config item | *required* |
| computeDefault | Function that, if present, computes a default value for this key | `null` |
| internalAccessOnly | If true, only the owning mod should have access to this config item. Note that this is *not* enforced by ResoniteModLoader itself. | `false` |
| valueValidator | A custom function that (if present) checks if a value is valid for this configuration item | `null` |

### Saving the Configuration

Configurations should be saved to disk by calling the `ModConfiguration.Save()` method. If you don't call `ModConfiguration.Save()`, your changes will still be available in memory. This allows multiple changes to be batched before you write them all to disk at once. Saving to disk is a relatively expensive operation and should not be performed at high frequency.

ResoniteModLoader will automatically call `Save()` for you when the game is shutting down. This will not occur if Resonite crashes, so to avoid data loss you should manually call `Save()` when appropriate. If you'd like to opt out of this autosave-on-shutdown functionality, use the `ModConfigurationDefinitionBuilder` discussed in the [Defining a Configuration](#defining-a-configuration) section.

### Getting the Configuration

To get the configuration, call `ResoniteModBase.GetConfiguration()`. Some notes:

- This will return `null` if the mod does not have a configuration.
- You must not call `ResoniteModBase.GetConfiguration()` before OnEngineInit() is called, as the mod may still be initializing.
- The returned `ModConfiguration` instance is guaranteed to be the same reference for all calls to `ResoniteModBase.GetConfiguration()`. Therefore, it is safe to save a reference to your `ModConfiguration`.
- Other mods may modify the `ModConfiguration` instance you are working with.
- A `ModConfiguration.TryGetValue()` call will always return the current value for that config item. If you need notice that someone else has changed one of your configs, there are events you can subscribe to. However, the `ModConfiguration.GetValue()` and `TryGetValue()` API is very inexpensive so it is fine to poll.

### Events

The `ModConfiguration` class provides two events you can subscribe to:

- The static event `OnAnyConfigurationChanged` is called if any config value for any mod changed.
- The instance event `OnThisConfigurationChanged` is called if one of the values in this mod's config changed.

Both of these events use the following delegate:

```csharp
public delegate void ConfigurationChangedHandler(ConfigurationChangedEvent configurationChangedEvent);
```

A `ConfigurationChangedEvent` has the following properties:

- `ModConfiguration Config` is the configuration the change occurred in
- `ModConfigurationKey Key` is the specific key who's value changed
- `string Label` is a custom label that may be set by whoever changed the configuration. This may be `null`.

To subscribe to either of these events, add 
```csharp
public override void OnEngineInit() {
    Config = GetConfiguration();

    ModConfiguration.OnAnyConfigurationChanged += OnConfigurationChanged; //Subscribe to any mod configuration changing
    Config.OnThisConfigurationChanged += OnThisConfigurationChanged; //Subscribe to when any key in this mod has changed
}
```

For individual `ModConfigurationKey`s there is also an `OnChanged` Event for when that specific key has changed
```csharp
ExampleConfigKey.OnChanged += (value) => { Msg($"Key set to {value}"); }
```
### Handling Incompatible Configuration Versions

You may optionally override a `HandleIncompatibleConfigurationVersions()` function in your ResoniteMod to define how incompatible versions are handled. You have two options:

- `IncompatibleConfigurationHandlingOption.ERROR`: Fail to read the config, and block saving over the config on disk.
- `IncompatibleConfigurationHandlingOption.CLOBBER`: Destroy the saved config and start over from scratch.
- `IncompatibleConfigurationHandlingOption.FORCE_LOAD`: Ignore the version number and load the config anyways. This may throw exceptions and break your mod.

If you do not override `HandleIncompatibleConfigurationVersions()`, the default is to return `ERROR` on all incompatibilities. `HandleIncompatibleConfigurationVersions()` is only called for configs that are detected to be incompatible under [semantic versioning][semver].

Here's an example implementation that can detect mod downgrades and conditionally avoid clobbering your new config:

```csharp
public override IncompatibleConfigurationHandlingOption HandleIncompatibleConfigurationVersions(Version serializedVersion, Version definedVersion) {
    if (serializedVersion > definedVersion) {
        // someone has dared to downgrade my mod
        // this will break the old version instead of nuking my config
        return IncompatibleConfigurationHandlingOption.ERROR;
    } else {
        // there's an old incompatible config version on disk
        // lets just nuke it instead of breaking
        return IncompatibleConfigurationHandlingOption.CLOBBER;
    }
}
```

### Breaking Changes in Configuration Definition

There are two cases to consider:

- **Forwards Compatible**: Can mod v2 load config v1?
- **Backwards Compatible**: Can mod v1 load config v2?

| Action | Forwards Compatible | Backwards Compatible |
| ------ | ------------------- | ---------------------|
| Adding a brand-new key | Yes | Yes |
| Removing an existing key | Yes | Yes |
| Adding, altering, or removing a key's default value | Yes | Maybe* |
| Restricting a key's validator | Yes** | Yes |
| Relaxing a key's validator | Yes | Maybe* |
| Changing `internalAccessOnly` to `false` | Yes | Maybe* |
| Changing `internalAccessOnly` to `true` | Yes** |Yes |
| Altering a key's type (removing and re-adding later counts!) | **No** | **No** |

<sup>\* ResoniteModLoader is compatible, but the old version of your mod's code may not be</sup>  
<sup>\*\* Assuming the new version of your mod properly accounts for reading old configs</sup>

## Working With Other Mods' Configurations

An example of enumerating all configs:

```csharp
void EnumerateConfigs() {
    IEnumerable<ResoniteModBase> mods = ModLoader.Mods();
    foreach (ResoniteModBase mod in mods) {
        ModConfiguration config = mod.GetConfiguration();
        if (config != null) {
            foreach (ModConfigurationKey key in config.ConfigurationItemDefinitions) {
                // while we COULD read internal configs, we shouldn't.
                if (!key.InternalAccessOnly) {
                    if (config.TryGetValue(key, out object value)) {
                        Msg($"{mod.Name} has configuration {key.Name} with type {key.ValueType()} and value {value}");
                    } else {
                        Msg($"{mod.Name} has configuration {key.Name} with type {key.ValueType()} and no value");
                    }
                }
            }
        }
    }
}
```

Worth noting here is that this API works with raw untyped objects, because as an external mod you lack the compile-time type information. The API performs its own type checking behind the scenes to prevent incorrect types from being written.

[semver]: https://semver.org/
