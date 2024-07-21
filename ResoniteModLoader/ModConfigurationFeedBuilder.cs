using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System.Collections;

namespace ResoniteModLoader;

public class ModConfigurationFeedBuilder {

	private readonly ModConfiguration Config;

	private readonly Dictionary<ModConfigurationKey, FieldInfo> KeyFields = new();

	public readonly static Dictionary<ModConfiguration, ModConfigurationFeedBuilder> CachedBuilders = new();

	private static bool HasAutoRegisterAttribute(FieldInfo field) => field.GetCustomAttribute<AutoRegisterConfigKeyAttribute>() is not null;

	private static bool HasRangeAttribute(FieldInfo field, out RangeAttribute attribute) {
		attribute = field.GetCustomAttribute<RangeAttribute>();
		return attribute is not null;
	}

	private void AssertChildKey(ModConfigurationKey key) {
		if (!Config.IsKeyDefined(key))
			throw new InvalidOperationException($"Mod key ({key}) is not owned by {Config.Owner.Name}'s config");
	}

	public ModConfigurationFeedBuilder(ModConfiguration config) {
		Config = config;
		IEnumerable<FieldInfo> autoConfigKeys = config.Owner.GetType().GetDeclaredFields().Where(HasAutoRegisterAttribute);
		foreach (FieldInfo field in autoConfigKeys) {
			ModConfigurationKey key = (ModConfigurationKey)field.GetValue(field.IsStatic ? null : config.Owner);
			KeyFields[key] = field;
		}
		CachedBuilders[config] = this;
	}

	public IEnumerable<DataFeedItem> RootPage(string searchPhrase = "", bool includeInternal = false) {
		foreach (ModConfigurationKey key in Config.ConfigurationItemDefinitions) {
			if (key.InternalAccessOnly && !includeInternal) continue;
			if (!string.IsNullOrEmpty(searchPhrase) && string.Join("\n", key.Name, key.Description).IndexOf(searchPhrase, StringComparison.InvariantCultureIgnoreCase) < 0) continue;
			yield return GenerateDataFeedItem(key);
		}
	}

	public IEnumerable<DataFeedOrderedItem<int>> OrderedPage(ModConfigurationKey key) {
		AssertChildKey(key);
		if (!typeof(IEnumerable).IsAssignableFrom(key.ValueType())) yield break;
		var value = (IEnumerable)Config.GetValue(key);
		int i = 0;
		foreach (object item in value)
			yield return FeedBuilder.OrderedItem<int>(key.Name + i, key.Name, item.ToString(), i++);
	}

	public DataFeedValueField<T> GenerateDataFeedField<T>(ModConfigurationKey key) {
		AssertChildKey(key);
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
		else if (valueType == typeof(bool))
			return FeedBuilder.Toggle(key.Name, key.Description ?? key.Name, (field) => field.SyncWithModConfiguration(Config, key));
		else if (valueType == typeof(float) && HasRangeAttribute(KeyFields[key], out RangeAttribute range))
			return FeedBuilder.Slider<float>(key.Name, key.Description ?? key.Name, (field) => field.SyncWithModConfiguration(Config, key), range.Min, range.Max, range.TextFormat);
		else if (valueType != typeof(string) && valueType != typeof(Uri) && typeof(IEnumerable).IsAssignableFrom(valueType))
			return FeedBuilder.Category(key.Name, key.Description ?? key.Name);
		else if (valueType.InheritsFrom(typeof(Enum)))
			return (DataFeedItem)typeof(ModConfigurationFeedBuilder).GetMethod(nameof(GenerateDataFeedEnum)).MakeGenericMethod(key.ValueType()).Invoke(this, [key]);
		else
			return (DataFeedItem)typeof(ModConfigurationFeedBuilder).GetMethod(nameof(GenerateDataFeedField)).MakeGenericMethod(key.ValueType()).Invoke(this, [key]);
	}
}
