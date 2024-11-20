using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System.Collections;

namespace ResoniteModLoader;

/// <summary>
/// A utility class that aids in the creation of mod configuration feeds.
/// </summary>
public class ModConfigurationFeedBuilder {
	/// <summary>
	/// A cache of <see cref="ModConfigurationFeedBuilder"/>, indexed by the <see cref="ModConfiguration"/> they belong to.
	/// New builders are automatically added to this cache upon instantiation, so you should try to get a cached builder before creating a new one.
	/// </summary>
	/// <example>
	/// <code>
	/// ModConfigurationFeedBuilder.CachedBuilders.TryGetValue(config, out var builder);
	/// builder ??= new ModConfigurationFeedBuilder(config);
	/// </code>
	/// </example>
	public readonly static Dictionary<ModConfiguration, ModConfigurationFeedBuilder> CachedBuilders = new();

	private readonly ModConfiguration Config;

	private readonly Dictionary<ModConfigurationKey, FieldInfo> KeyFields = new();

	private readonly Dictionary<string, HashSet<ModConfigurationKey>> KeyGrouping = new();

	private static bool HasAutoRegisterAttribute(FieldInfo field) => field.GetCustomAttribute<AutoRegisterConfigKeyAttribute>() is not null;

	private static bool TryGetAutoRegisterAttribute(FieldInfo field, out AutoRegisterConfigKeyAttribute attribute) {
		attribute = field.GetCustomAttribute<AutoRegisterConfigKeyAttribute>();
		return attribute is not null;
	}

	private static bool HasRangeAttribute(FieldInfo field) => field.GetCustomAttribute<RangeAttribute>() is not null;

	private static bool TryGetRangeAttribute(FieldInfo field, out RangeAttribute attribute) {
		attribute = field.GetCustomAttribute<RangeAttribute>();
		return attribute is not null;
	}

	private void AssertChildKey(ModConfigurationKey key) {
		if (!Config.IsKeyDefined(key))
			throw new InvalidOperationException($"Mod key ({key}) is not owned by {Config.Owner.Name}'s config");
	}

	private static void AssertMatchingType<T>(ModConfigurationKey key) {
		if (key.ValueType() != typeof(T))
			throw new InvalidOperationException($"Type of mod key ({key}) does not match field type {typeof(T)}");
	}

	private string GetKeyLabel(ModConfigurationKey key)
		=> (key.InternalAccessOnly ? "[INTERNAL] " : "")
		+ (PreferDescriptionLabels ? (key.Description ?? key.Name) : key.Name);

	private string GetKeyDescription(ModConfigurationKey key)
		=> PreferDescriptionLabels ? $"Key name: {key.Name}" : (key.Description ?? "(No description)");

	/// <summary>
	/// If <c>true</c>, configuration key descriptions will be used as the DataFeedItem's label if they exist.
	/// If <c>false</c>, the configuration key name will be used as the label.
	/// In both cases, the description will be the opposite field of the label.
	/// </summary>
	public bool PreferDescriptionLabels { get; set; } = true;

	/// <summary>
	/// Instantiates and caches a new builder for a specific <see cref="ModConfiguration"/>.
	/// Check if a cached builder exists in <see cref="CachedBuilders"/> before creating a new one!
	/// </summary>
	/// <param name="config">The mod configuration this builder will generate items for</param>
	public ModConfigurationFeedBuilder(ModConfiguration config) {
		Config = config;
		IEnumerable<FieldInfo> autoConfigKeys = config.Owner.GetType().GetDeclaredFields().Where(HasAutoRegisterAttribute);
		HashSet<ModConfigurationKey> groupedKeys = new();

		foreach (FieldInfo field in autoConfigKeys) {
			ModConfigurationKey key = (ModConfigurationKey)field.GetValue(field.IsStatic ? null : config.Owner);
			if (key is null) continue; // dunno why this would happen
			KeyFields[key] = field;

			AutoRegisterConfigKeyAttribute attribute = field.GetCustomAttribute<AutoRegisterConfigKeyAttribute>();
			if (attribute.Group is string groupName) {
				if (!KeyGrouping.ContainsKey(groupName))
					KeyGrouping[groupName] = new();
				KeyGrouping[groupName].Add(key);
				groupedKeys.Add(key);
			}
		}

		if (groupedKeys.Any()) {
			if (!KeyGrouping.ContainsKey("Uncategorized"))
				KeyGrouping["Uncategorized"] = new();

			foreach (ModConfigurationKey key in config.ConfigurationItemDefinitions)
				if (!groupedKeys.Contains(key))
					KeyGrouping["Uncategorized"].Add(key);
		}

		CachedBuilders[config] = this;

		if (Logger.IsDebugEnabled()) {
			Logger.DebugInternal("--- ModConfigurationFeedBuilder instantiated ---");
			Logger.DebugInternal($"Config owner: {config.Owner.Name}");
			Logger.DebugInternal($"Total keys: {config.ConfigurationItemDefinitions.Count}");
			Logger.DebugInternal($"AutoRegistered keys: {autoConfigKeys.Count()}, Grouped: {groupedKeys.Count}");
			Logger.DebugInternal($"Key groups ({KeyGrouping.Keys.Count}): [{string.Join(", ", KeyGrouping.Keys)}]");
		}
	}

	public IEnumerable<DataFeedItem> Page(IReadOnlyList<string> path, string searchPhrase = "", bool includeInternal = false)
	{
		if (path is null || !path.Any())
			foreach (DataFeedItem item in RootPage(searchPhrase, includeInternal))
				yield return item;

	}

	/// <summary>
	/// Generates a root config page containing all defined config keys.
	/// </summary>
	/// <param name="searchPhrase">If set, only show keys whose name or description contains this string</param>
	/// <param name="includeInternal">If <c>true</c>, also generate items for config keys marked as internal</param>
	/// <returns>Feed items for all defined config keys, plus buttons to save, discard, and reset the config.</returns>
	public IEnumerable<DataFeedItem> RootPage(string searchPhrase = "", bool includeInternal = false) {
		if (KeyGrouping.Any()) {
			foreach (string group in KeyGrouping.Keys) {
				DataFeedGroup container = FeedBuilder.Group(group, group);
				foreach (ModConfigurationKey key in Config.ConfigurationItemDefinitions.Where(KeyGrouping[group].Contains)) {
					if (key.InternalAccessOnly && !includeInternal) continue;
					if (!string.IsNullOrEmpty(searchPhrase) && string.Join("\n", key.Name, key.Description).IndexOf(searchPhrase, StringComparison.InvariantCultureIgnoreCase) < 0) continue;
					container.AddSubitems(GenerateDataFeedItem(key));
				}
				if (container.SubItems?.Any() ?? false) yield return container;
			}
		}
		else {
			foreach (ModConfigurationKey key in Config.ConfigurationItemDefinitions) {
				if (key.InternalAccessOnly && !includeInternal) continue;
				if (!string.IsNullOrEmpty(searchPhrase) && string.Join("\n", key.Name, key.Description).IndexOf(searchPhrase, StringComparison.InvariantCultureIgnoreCase) < 0) continue;
				yield return GenerateDataFeedItem(key);
			}
		}

		yield return GenerateSaveControlButtons();
	}

	/// <summary>
	/// (NOT YET IMPLEMENTED) Generates a subpage for an indexed/enumerable config key.
	/// ie. arrays, lists, dictionaries, sets.
	/// </summary>
	/// <param name="key">A key with an enumerable type</param>
	/// <param name="reorderOnly">If <c>true</c>, items may only be reordered, not added/removed.</param>
	/// <returns>A ordered feed item for each element in the key's value, plus a group of buttons to add/remove items if set.</returns>
	private IEnumerable<DataFeedOrderedItem<int>> EnumerablePage(ModConfigurationKey key, bool reorderOnly = false) {
		AssertChildKey(key);
		if (!typeof(IEnumerable).IsAssignableFrom(key.ValueType())) yield break;
		var value = (IEnumerable)Config.GetValue(key);
		int i = 0;
		foreach (object item in value)
			yield return FeedBuilder.OrderedItem<int>(key.Name + i, key.Name, item.ToString(), i++);
		if (reorderOnly) yield break;
		// Group that contains input field plus buttons to prepend/append, and remove first/last item
	}

	// these generate methods need to be cleaned up and more strongly typed
	// todo: Make all these methods use generic keys

	/// <summary>
	/// Generates a slider for the defining key if it is a float has a range attribute, otherwise generates a generic value field.
	/// </summary>
	/// <typeparam name="T">The value type of the supplied key</typeparam>
	/// <param name="key">The key to generate the item from</param>
	/// <returns>A DataFeedSlider if possible, otherwise a DataFeedValueField.</returns>
	/// <seealso cref="GenerateDataFeedItem"/>
	public DataFeedValueField<T> GenerateDataFeedField<T>(ModConfigurationKey key) {
		AssertChildKey(key);
		AssertMatchingType<T>(key);
		string label = GetKeyLabel(key);
		string description = GetKeyDescription(key);
		if (typeof(T).IsAssignableFrom(typeof(float)) && KeyFields.TryGetValue(key, out FieldInfo field) && TryGetRangeAttribute(field, out RangeAttribute range) && range.Min is T min && range.Max is T max)
			return FeedBuilder.Slider<T>(key.Name, label, description, (field) => field.SyncWithModConfiguration(Config, key), min, max, range.TextFormat);
		// If range attribute wasn't limited to floats, we could also make ClampedValueField's
		else
			return FeedBuilder.ValueField<T>(key.Name, label, description, (field) => field.SyncWithModConfiguration(Config, key));
	}

	/// <summary>
	/// Generates an enum field for a specific configuration key.
	/// </summary>
	/// <typeparam name="E">The enum type of the supplied key</typeparam>
	/// <param name="key">The key to generate the item from</param>
	/// <returns>A physical mango if it is opposite day.</returns>
	/// <seealso cref="GenerateDataFeedItem"/>
	public DataFeedEnum<E> GenerateDataFeedEnum<E>(ModConfigurationKey key) where E : Enum {
		AssertChildKey(key);
		AssertMatchingType<E>(key);
		string label = GetKeyLabel(key);
		string description = GetKeyDescription(key);
		return FeedBuilder.Enum<E>(key.Name, label, description, (field) => field.SyncWithModConfiguration(Config, key));
	}

	/// <summary>
	/// Generates the appropriate DataFeedItem for any config key type.
	/// </summary>
	/// <param name="key">The key to generate the item from</param>
	/// <returns>Automatically picks the best item type for the config key type.</returns>
	public DataFeedItem GenerateDataFeedItem(ModConfigurationKey key) {
		AssertChildKey(key);
		string label = GetKeyLabel(key);
		string description = GetKeyDescription(key);
		Type valueType = key.ValueType();
		if (valueType == typeof(dummy))
			return FeedBuilder.Label(key.Name, label, description);
		else if (valueType == typeof(bool))
			return FeedBuilder.Toggle(key.Name, label, description, (field) => field.SyncWithModConfiguration(Config, key));
		else if (valueType != typeof(string) && valueType != typeof(Uri) && typeof(IEnumerable).IsAssignableFrom(valueType))
			return FeedBuilder.Category(key.Name, label, description);
		else if (valueType.InheritsFrom(typeof(Enum)))
			return (DataFeedItem)typeof(ModConfigurationFeedBuilder).GetMethod(nameof(GenerateDataFeedEnum)).MakeGenericMethod(key.ValueType()).Invoke(this, [key]);
		else
			return (DataFeedItem)typeof(ModConfigurationFeedBuilder).GetMethod(nameof(GenerateDataFeedField)).MakeGenericMethod(key.ValueType()).Invoke(this, [key]);
	}

	/// <summary>
	/// Generates buttons to save/discard changes, or reset all config keys to their defaults.
	/// </summary>
	/// <returns>A group with the aforementioned options.</returns>
	public DataFeedGrid GenerateSaveControlButtons() {
		string configName = Path.GetFileNameWithoutExtension(Config.Owner.ModAssembly!.File);
		DataFeedGrid container = FeedBuilder.Grid("SaveControlButtonsGrid", "", [
			FeedBuilder.ValueAction<string>("Save", "Save changes", (action) => action.Target = SaveConfig, configName),
			FeedBuilder.ValueAction<string>("Discard", "Discard changes", (action) => action.Target = DiscardConfig, configName),
			FeedBuilder.ValueAction<string>("Reset", "Reset all options", (action) => action.Target = ResetConfig, configName)
		]);
		return container;
	}

	[SyncMethod(typeof(Action<string>), [])]
	private static void SaveConfig(string configName) {
		if (ModConfiguration.configNameMap.TryGetValue(configName, out var config)) {
			config.SaveQueue(false, true);
			NotificationMessage.SpawnTextMessage("Saved successfully", colorX.White);
		} else
			NotificationMessage.SpawnTextMessage("Failed to save!", colorX.Red);
	}

	[SyncMethod(typeof(Action<string>), [])]
	private static void DiscardConfig(string configName) {
		Userspace.OpenContextMenu(
			Userspace.UserspaceWorld.GetGloballyRegisteredComponent<UserspaceRadiantDash>().Slot,
			new ContextMenuOptions { disableFlick = true },
			async (menu) => {
				menu.AddItem(
					"Really discard changes",
					OfficialAssets.Graphics.Icons.Inspector.DestroyPreservingAssets,
					colorX.Red
				).Button.LocalPressed += (_, _) => {
					NotificationMessage.SpawnTextMessage("Not implemented", colorX.Yellow);
					menu.Close();
				};
				menu.AddItem("Cancel", (Uri)null!, colorX.White)
				.Button.LocalPressed += (_, _) => menu.Close();
			}
		);
	}

	[SyncMethod(typeof(Action<string>), [])]
	private static void ResetConfig(string configName) {
		Userspace.OpenContextMenu(
			Userspace.UserspaceWorld.GetGloballyRegisteredComponent<UserspaceRadiantDash>().Slot,
			new ContextMenuOptions { disableFlick = true },
			async (menu) => {
				menu.AddItem(
					"Really reset configuration",
					OfficialAssets.Graphics.Icons.Inspector.Destroy,
					colorX.Red
				).Button.LocalPressed += (_, _) => {
					NotificationMessage.SpawnTextMessage("Not implemented", colorX.Yellow);
					menu.Close();
				};
				menu.AddItem("Cancel", (Uri)null!, colorX.White)
				.Button.LocalPressed += (_, _) => menu.Close();
			}
		);
	}
}

/// <summary>
/// Extentions that work with <see cref="ModConfigurationFeedBuilder"/>'s
/// </summary>
public static class ModConfigurationFeedBuilderExtensions {
	/// <summary>
	/// Returns a cached <see cref="ModConfigurationFeedBuilder"/>, or creates a new one.
	/// </summary>
	/// <param name="config">The <see cref="ModConfiguration"/> the builder belongs to</param>
	/// <returns>A cached or new builder.</returns>
	public static ModConfigurationFeedBuilder ConfigurationFeedBuilder(this ModConfiguration config) {
		ModConfigurationFeedBuilder.CachedBuilders.TryGetValue(config, out var builder);
		return builder ?? new ModConfigurationFeedBuilder(config);
	}
}
