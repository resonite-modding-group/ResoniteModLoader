#if DEBUG
using Elements.Core;
using FrooxEngine;
using HarmonyLib;

namespace ResoniteModLoader;

/// <summary>
/// Settings UI for loaded mods.
/// </summary>
[AutoRegisterSetting]
[SettingCategory("ResoniteModLoader")]
public sealed class ModSettings : SettingComponent<ModSettings> {
	/// <inheritdoc/>
	public override bool UserspaceOnly => true;

	/// <summary>
	/// Yields a data feed of configuration items for all loaded mods.
	/// </summary>
	[SettingSubcategoryEnumerator]
	public async IAsyncEnumerable<DataFeedItem> ModConfigurationSettings() {
		var seen = new HashSet<(string, string)>();
		foreach (var mod in ModLoader.Mods()) {
			// duplicate mods can happen with hot reloading
			// should the first or last seen instance of the mod be kept..
			// or perhaps this case shouldn't be handled at all?
			if (!seen.Add((mod.Author, mod.Name))) continue;
			yield return ModToItem(mod);
		}
	}

	internal static DataFeedItem ModToItem(ResoniteModBase mod) {
		// this likely doesn't follow some kind of internal convention about how `ItemKey`s should look, iprove that?
		var id = $"{mod.Author}.{mod.Name}";

		var config = mod.GetConfiguration();
		var definitions = config?.ConfigurationItemDefinitions?.OfType<ModConfigurationKey>() ?? [];

		var group =
			Item.Group(
				id: id,
				key: mod.Name,
				subitems: [
					Item.Indicator(id, "Name", mod.Name),
					Item.Indicator(id, "Author", mod.Author),
					Item.Indicator(id, "Version", mod.Version),
					Item.Indicator(id, "Link", mod.Link),
					.. definitions
						.Where(d => !d.InternalAccessOnly)
						.Select(key =>
							Item.Element(
								id: $"{id}.Configuration",
								key: key,
								label: key.Name,
								description: key.Description
							)
						)
				]
			);

		return group;
	}

	/// <inheritdoc/>
	public override void ResetToDefault() {
		// how should this be handled?
	}

	private static class Item {
		public static DataFeedGroup Group(string id, string key, IReadOnlyList<DataFeedItem> subitems, string? label = null) {
			var item = new DataFeedGroup();
			item.InitBase($"{id}.{key}", [], [], label ?? key, subitems: subitems);
			return item;
		}

		public static DataFeedItem Element(string id, ModConfigurationKey key, string? label = null, string? description = null) =>
		  (DataFeedItem)typeof(Item).GetGenericMethod(nameof(Element), BindingFlags.Static | BindingFlags.Public, [key.ValueType()])
			  .Invoke(null, [id, key, label, description])!;

		public static DataFeedValueElement<T> Element<T>(string id, ModConfigurationKey<T> key, string? label = null, string? description = null) {
			var item = key.ValueType() switch {
				var t when t == typeof(bool) => new DataFeedToggle() as DataFeedValueElement<T>,
				var t when t.IsEnum => typeof(DataFeedEnum<>).MakeGenericType(typeof(T)).CreateInstance() as DataFeedValueElement<T>,
				_ => new DataFeedValueField<T>(),
			};

			// unless FrooxEngine decides to change the type constraints or the type hierarchy ! should be valid here.
			item!.InitBase($"{id}.{key}", [], [], label ?? key.Name, description);
			item.InitSetupValue(field => SyncWithModConfigurationKey(field, key));
			return item;
		}

		public static DataFeedItem Indicator(string id, string key, Type type, object value, string? label = null, string? description = null) =>
		  (DataFeedItem)typeof(Item).GetGenericMethod(nameof(Indicator), BindingFlags.Static | BindingFlags.Public, [type])
			  .Invoke(null, [id, key, value, label, description])!;

		public static DataFeedIndicator<T> Indicator<T>(string id, string key, T value, string? label = null, string? description = null) {
			var item = new DataFeedIndicator<T>();
			item.InitBase($"{id}.{key}", [], [], label ?? key, description);
			item.InitSorting(0);
			item.InitSetupValue(f => f.Value = value);
			return item;
		}

		/// <summary>
		/// Sets up handlers to keep the value of <see cref="IField{T}"/> and <see cref="ModConfigurationKey{T}"/> in sync.
		/// </summary>
		private static void SyncWithModConfigurationKey<T>(IField<T> field, ModConfigurationKey<T> key) {
			// we know this should be valid even if it is null
			// even though ! marks it as not null, we're using it to mean we know it's null and valid in this case
			field.Value = key.Value!;

			// registering but not removing the events maybe leaks? haven't checked
			field.Changed += (_) => {
				var value = field.BoxedValue;
				if (key.Validate(value)) {
					key.Set(value);
				}
				else {
					// revert value if invalid
					if (key.TryGetValue(out var previousValue)) {
						field.BoxedValue = previousValue!;
					}
				}
			};

			key.OnChanged += (a) => {
				field.BoxedValue = a!;
			};
		}
	}
}
#endif
