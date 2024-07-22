using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System.Collections;

namespace ResoniteModLoader;

public class ModConfigurationFeedBuilder {

	private readonly ModConfiguration Config;

	private readonly Dictionary<ModConfigurationKey, FieldInfo> KeyFields = new();

	private readonly Dictionary<string, HashSet<ModConfigurationKey>> KeyGrouping = new();

	private readonly Dictionary<string[], HashSet<ModConfigurationKey>> KeyCategories = new();

	public readonly static Dictionary<ModConfiguration, ModConfigurationFeedBuilder> CachedBuilders = new();

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

	private static bool AStartsWithB<T>(T[] A, T[] B) => string.Join("\t", A).StartsWith(string.Join("\t", B), StringComparison.InvariantCultureIgnoreCase);

	public ModConfigurationFeedBuilder(ModConfiguration config) {
		Config = config;
		IEnumerable<FieldInfo> autoConfigKeys = config.Owner.GetType().GetDeclaredFields().Where(HasAutoRegisterAttribute);
		HashSet<ModConfigurationKey> groupedKeys = new();
		HashSet<ModConfigurationKey> categorizedKeys = new();
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
			if (attribute.Path is string[] categoryPath) {
				if (!KeyCategories.ContainsKey(categoryPath))
					KeyCategories[categoryPath] = new();
				KeyCategories[categoryPath].Add(key);
				categorizedKeys.Add(key);
			}
		}
		foreach (ModConfigurationKey key in config.ConfigurationItemDefinitions) {
			if (groupedKeys.Any() && !groupedKeys.Contains(key)) {
				if (!KeyGrouping.ContainsKey("Uncategorized"))
					KeyGrouping["Uncategorized"] = new();
				KeyGrouping["Uncategorized"].Add(key);
			}
			if (!categorizedKeys.Contains(key)) {
				if (!KeyCategories.ContainsKey([]))
					KeyCategories[[]] = new();
				KeyCategories[[]].Add(key);
			}
		}
		CachedBuilders[config] = this;
	}

	public IEnumerable<DataFeedItem> GeneratePage(string[] path, string searchPhrase = "", bool includeInternal = false) {
		path = path ?? [];
		DataFeedGrid? subcategories = GenerateSubcategoryButtons(path);
		if (subcategories is not null) yield return subcategories;
		Logger.DebugInternal($"KeyCategories[{string.Join(", ", path)}].Contains");
		IEnumerable<ModConfigurationKey> filteredItems = string.IsNullOrEmpty(searchPhrase) ? Config.ConfigurationItemDefinitions.Where(KeyCategories[path].Contains) : Config.ConfigurationItemDefinitions;
		if (KeyGrouping.Any()) {
			foreach (string group in KeyGrouping.Keys) {
				DataFeedGroup container = FeedBuilder.Group(group, group);
				foreach (ModConfigurationKey key in filteredItems.Where(KeyGrouping[group].Contains)) {
					if (key.InternalAccessOnly && !includeInternal) continue;
					if (!string.IsNullOrEmpty(searchPhrase) && string.Join("\n", key.Name, key.Description).IndexOf(searchPhrase, StringComparison.InvariantCultureIgnoreCase) < 0) continue;
					container.Subitem(GenerateDataFeedItem(key));
				}
				if (container.SubItems is not null && container.SubItems.Any()) yield return container;
			}
		}
		else {
			foreach (ModConfigurationKey key in filteredItems) {
				if (key.InternalAccessOnly && !includeInternal) continue;
				if (!string.IsNullOrEmpty(searchPhrase) && string.Join("\n", key.Name, key.Description).IndexOf(searchPhrase, StringComparison.InvariantCultureIgnoreCase) < 0) continue;
				yield return GenerateDataFeedItem(key);
			}
		}
		yield return GenerateSaveControlButtons();
	}

	public IEnumerable<DataFeedOrderedItem<int>> OrderedItem(ModConfigurationKey key) {
		AssertChildKey(key);
		if (!typeof(IEnumerable).IsAssignableFrom(key.ValueType())) yield break;
		var value = (IEnumerable)Config.GetValue(key);
		int i = 0;
		foreach (object item in value)
			yield return FeedBuilder.OrderedItem<int>(key.Name + i, key.Name, item.ToString(), i++);
	}

	public DataFeedValueField<T> GenerateDataFeedField<T>(ModConfigurationKey key) {
		AssertChildKey(key);
		if (typeof(T) == typeof(bool))
			return (DataFeedValueField<T>)(object)FeedBuilder.Toggle(key.Name, key.Description ?? key.Name, (field) => field.SyncWithModConfiguration(Config, key));
		else if (typeof(T).IsAssignableFrom(typeof(float)) && KeyFields.TryGetValue(key, out FieldInfo field) && TryGetRangeAttribute(field, out RangeAttribute range) && range.Min is T min && range.Max is T max)
			return FeedBuilder.Slider<T>(key.Name, key.Description ?? key.Name, (field) => field.SyncWithModConfiguration(Config, key), min, max, range.TextFormat);
		else
			return FeedBuilder.ValueField<T>(key.Name, key.Description ?? key.Name, (field) => field.SyncWithModConfiguration(Config, key));
	}

	public DataFeedEnum<T> GenerateDataFeedEnum<T>(ModConfigurationKey key) where T : Enum {
		AssertChildKey(key);
		return FeedBuilder.Enum<T>(key.Name, key.Description ?? key.Name, (field) => field.SyncWithModConfiguration(Config, key));
	}

	public DataFeedItem GenerateDataFeedItem(ModConfigurationKey key) {
		AssertChildKey(key);
		Type valueType = key.ValueType();
		if (valueType == typeof(dummy))
			return FeedBuilder.Label(key.Name, key.Description ?? key.Name);
		else if (valueType != typeof(string) && valueType != typeof(Uri) && typeof(IEnumerable).IsAssignableFrom(valueType))
			return FeedBuilder.Category(key.Name, key.Description ?? key.Name);
		else if (valueType.InheritsFrom(typeof(Enum)))
			return (DataFeedItem)typeof(ModConfigurationFeedBuilder).GetMethod(nameof(GenerateDataFeedEnum)).MakeGenericMethod(key.ValueType()).Invoke(this, [key]);
		else
			return (DataFeedItem)typeof(ModConfigurationFeedBuilder).GetMethod(nameof(GenerateDataFeedField)).MakeGenericMethod(key.ValueType()).Invoke(this, [key]);
	}

	public DataFeedGrid? GenerateSubcategoryButtons(string[] currentPath) {
		if (!KeyCategories.Any()) return null;
		IEnumerable<string[]> subCategories = KeyCategories.Keys.Where((subPath) => subPath.Length == currentPath.Length + 1 && AStartsWithB(subPath, currentPath));
		if (subCategories is null || !subCategories.Any()) return null;
		DataFeedGrid container = FeedBuilder.Grid("SubcategoryButtonsGrid", "");
		foreach (string[] subCategory in subCategories)
			container.Subitem(FeedBuilder.Category(subCategory.Last(), subCategory.Last() + " >"));
		return container;
	}

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
	public static void SaveConfig(string configName) {

	}

	[SyncMethod(typeof(Action<string>), [])]
	public static void DiscardConfig(string configName) {

	}

	[SyncMethod(typeof(Action<string>), [])]
	public static void ResetConfig(string configName) {

	}
}
