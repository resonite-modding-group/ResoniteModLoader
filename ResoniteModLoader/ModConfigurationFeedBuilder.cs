using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System.Collections;

namespace ResoniteModLoader;

public class ModConfigurationFeedBuilder {

	private readonly ModConfiguration Config;

	private readonly Dictionary<ModConfigurationKey, FieldInfo> KeyFields = new();

	private readonly Dictionary<string, HashSet<ModConfigurationKey>> KeyGrouping = new();

	private readonly Dictionary<string[], HashSet<ModConfigurationKey>> KeyCategories = new(new StringArrayEqualityComparer());

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

	private static bool IsFirstChild(string[] x, string[] y) {
		Logger.DebugInternal($"Is [({x.Length})[{string.Join(", ", x)}]] a first child to [({y.Length})[{string.Join(", ", y)}]]?");
		if (x.Length != y.Length + 1) return false;
		for (int i = 0; i < y.Length; i++)
			if (x[i] != y[i]) return false;
		Logger.DebugInternal("You are the father!");
		return true;
	}

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
		if (Logger.IsDebugEnabled()) {
			Logger.DebugInternal("--- ModConfigurationFeedBuilder instantiated ---");
			Logger.DebugInternal($"Config owner: {config.Owner.Name}");
			Logger.DebugInternal($"Total keys: {config.ConfigurationItemDefinitions.Count}");
			Logger.DebugInternal($"AutoRegistered keys: {autoConfigKeys.Count()}, Grouped: {groupedKeys.Count}, Categorized: {categorizedKeys.Count}");
			Logger.DebugInternal($"Key groups ({KeyGrouping.Keys.Count}): [{string.Join(", ", KeyGrouping.Keys)}]");
			List<string> categories = new();
			KeyCategories.Keys.Do((key) => categories.Add($"[{string.Join(", ", key)}]"));
			Logger.DebugInternal($"Key categories ({KeyCategories.Keys.Count}): {string.Join(", ", categories)}");
		}
	}

	public IEnumerable<DataFeedItem> GeneratePage(string[] path, string searchPhrase = "", bool includeInternal = false) {
		Logger.DebugInternal($"KeyCategories[({path.Length})[{string.Join(", ", path)}]].Contains");
		path = path ?? [];
		DataFeedGrid? subcategories = GenerateSubcategoryButtons(path);
		if (subcategories is not null) yield return subcategories;
		IEnumerable<ModConfigurationKey> filteredItems = string.IsNullOrEmpty(searchPhrase) ? Config.ConfigurationItemDefinitions.Where(KeyCategories[path].Contains) : Config.ConfigurationItemDefinitions;
		if (KeyGrouping.Any()) {
			foreach (string group in KeyGrouping.Keys) {
				DataFeedGroup container = FeedBuilder.Group(group, group);
				foreach (ModConfigurationKey key in filteredItems.Where(KeyGrouping[group].Contains)) {
					if (key.InternalAccessOnly && !includeInternal) continue;
					if (!string.IsNullOrEmpty(searchPhrase) && string.Join("\n", key.Name, key.Description).IndexOf(searchPhrase, StringComparison.InvariantCultureIgnoreCase) < 0) continue;
					container.AddSubitem(GenerateDataFeedItem(key));
				}
				if (container.SubItems?.Any() ?? false) yield return container;
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
		string label = (key.InternalAccessOnly ? "[INTERNAL] " : "") + key.Description ?? key.Name;
		if (typeof(T).IsAssignableFrom(typeof(float)) && KeyFields.TryGetValue(key, out FieldInfo field) && TryGetRangeAttribute(field, out RangeAttribute range) && range.Min is T min && range.Max is T max)
			return FeedBuilder.Slider<T>(key.Name, label, (field) => field.SyncWithModConfiguration(Config, key), min, max, range.TextFormat);
		else
			return FeedBuilder.ValueField<T>(key.Name, label, (field) => field.SyncWithModConfiguration(Config, key));
	}

	public DataFeedEnum<T> GenerateDataFeedEnum<T>(ModConfigurationKey key) where T : Enum {
		AssertChildKey(key);
		string label = (key.InternalAccessOnly ? "[INTERNAL] " : "") + key.Description ?? key.Name;
		return FeedBuilder.Enum<T>(key.Name, label, (field) => field.SyncWithModConfiguration(Config, key));
	}

	public DataFeedItem GenerateDataFeedItem(ModConfigurationKey key) {
		AssertChildKey(key);
		string label = (key.InternalAccessOnly ? "[INTERNAL] " : "") + key.Description ?? key.Name;
		Type valueType = key.ValueType();
		if (valueType == typeof(dummy))
			return FeedBuilder.Label(key.Name, label);
		else if (valueType == typeof(bool))
			return FeedBuilder.Toggle(key.Name, label, (field) => field.SyncWithModConfiguration(Config, key));
		else if (valueType != typeof(string) && valueType != typeof(Uri) && typeof(IEnumerable).IsAssignableFrom(valueType))
			return FeedBuilder.Category(key.Name, label);
		else if (valueType.InheritsFrom(typeof(Enum)))
			return (DataFeedItem)typeof(ModConfigurationFeedBuilder).GetMethod(nameof(GenerateDataFeedEnum)).MakeGenericMethod(key.ValueType()).Invoke(this, [key]);
		else
			return (DataFeedItem)typeof(ModConfigurationFeedBuilder).GetMethod(nameof(GenerateDataFeedField)).MakeGenericMethod(key.ValueType()).Invoke(this, [key]);
	}

	public DataFeedGrid? GenerateSubcategoryButtons(string[] currentPath) {
		if (!KeyCategories.Any()) return null;
		IEnumerable<string[]> subCategories = KeyCategories.Keys.Where((subPath) => subPath.Length == currentPath.Length + 1 && IsFirstChild(subPath, currentPath));
		if (subCategories is null || !subCategories.Any()) return null;
		DataFeedGrid container = FeedBuilder.Grid("SubcategoryButtonsGrid", "");
		foreach (string[] subCategory in subCategories)
			container.AddSubitem(FeedBuilder.Category(subCategory.Last(), subCategory.Last() + " >"));
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

internal class StringArrayEqualityComparer : EqualityComparer<string[]> {
	public override bool Equals(string[] x, string[] y) {
		Logger.DebugInternal($"Comparing [({x.Length})[{string.Join(", ", x)}]] to [({y.Length})[{string.Join(", ", y)}]]");
		if (x.Length != y.Length) return false;
		for (int i = 0; i < x.Length; i++)
			if (x[i] != y[i]) return false;
		Logger.DebugInternal("Values equal");
		return true;
	}

	public override int GetHashCode(string[] obj) {
		int hashCode = 699494;
		foreach (string item in obj)
			hashCode += item.GetHashCode() - (item.Length + 621) ^ 8;
		return hashCode;
	}
}
