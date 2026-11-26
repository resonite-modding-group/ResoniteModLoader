using Elements.Core;
using FrooxEngine;
using ResoniteModLoader.Utility;

namespace ResoniteModLoader;

/// <summary>
/// A custom data feed that can be used to show information about loaded mods, and alter their configuration. Path must start with "ResoniteModLoader"
/// </summary>
[Category(["ResoniteModLoader"])]
public class ModConfigurationDataFeed : Component, IDataFeedComponent, IDataFeed, IWorldElement {
#pragma warning disable CS1591
#if !DEBUG
	public override bool UserspaceOnly => true;
#endif

	public bool SupportsBackgroundQuerying => true;
#pragma warning restore CS1591
#pragma warning disable CS8618, CA1051 // FrooxEngine weaver will take care of these
	/// <summary>
	/// Show mod configuration keys marked as internal.
	/// </summary>
	public readonly Sync<bool> IncludeInternalConfigItems;
#pragma warning restore CS8618, CA1051
#pragma warning disable CS1591
	public async IAsyncEnumerable<DataFeedItem> Enumerate(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys,
		string searchPhrase, object viewData) {
		if (path.Count == 0 || !ModLoader.FileNameLookupMap.TryGetValue(path[0], out var mod)) {
			yield break;
		}

		yield return GenerateModInfoPanel(mod);
		yield return GenerateModConfigPanel(mod);
	}

	private DataFeedGroup GenerateModInfoPanel(ResoniteModBase mod) {
		string groupingBase = "ModSettings.ModInfo";
		string[] modGrouping = [groupingBase];

		var group = FeedBuilder.Group(groupingBase, "Settings.ModSettings.ModInfo".AsLocaleKey());

		group.AddSubitems(
			FeedBuilder.StringIndicator(groupingBase + ".Author", "Settings.ModSettings.Author".AsLocaleKey(),
				mod.Author, groupingParameters: modGrouping),
			FeedBuilder.StringIndicator(groupingBase + ".Version", "Settings.ModSettings.Version".AsLocaleKey(),
				mod.Version, groupingParameters: modGrouping)
		);

		if (ModLoaderConfiguration.Get().Debug) {
			group.AddSubitems(
				FeedBuilder.StringIndicator(groupingBase + ".Assembly", "Settings.ModSettings.Assembly".AsLocaleKey(),
					Path.GetFileName(mod.ModAssembly!.File), groupingParameters: modGrouping)
			);
		}

		if (Uri.TryCreate(mod.Link, UriKind.Absolute, out var uri)) {
			group.AddSubitems(
				FeedBuilder.ValueAction<Uri>(groupingBase + ".Link",
					"Settings.ModSettings.ModLink".AsLocaleKey("host", uri.Host),
					action => action.Target = ModSettings.OpenURI, uri, groupingParameters: modGrouping)
			);
		}

		return group;
	}

	private DataFeedGroup GenerateModConfigPanel(ResoniteModBase mod) {
		string groupingBase = "ModSettings.ModConfig";
		string[] modGrouping = [groupingBase];

		var group = FeedBuilder.Group(groupingBase, "Settings.ModSettings.ModConfig".AsLocaleKey());

		if (mod.GetConfiguration() is { } modConfig) {
			var configBuilder = modConfig.ConfigurationFeedBuilder();

			foreach (ModConfigurationKey key in modConfig.ConfigurationItemDefinitions) {
				var item = configBuilder.GenerateDataFeedItem(key, groupingParameters: modGrouping);
				if (key.InternalAccessOnly) {
					item.InitVisible(field => field.DriveFrom(IncludeInternalConfigItems));
				}

				group.AddSubitems(item);
			}

			if (modConfig.ConfigurationItemDefinitions.All(key => key.InternalAccessOnly)) {
				group.AddSubitems(
					FeedBuilder.Label(groupingBase + ".NoConfigs", "Settings.ModSettings.NoConfigs".AsLocaleKey(),
							groupingParameters: modGrouping)
						.WithVisible(field => field.DriveInverted(IncludeInternalConfigItems))
				);
			}
		}
		else {
			group.AddSubitems(
				FeedBuilder.Label(groupingBase + ".NoConfigs", "Settings.ModSettings.NoConfigs".AsLocaleKey(),
					groupingParameters: modGrouping)
			);
		}

		return group;
	}

	public void ListenToUpdates(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase,
		DataFeedUpdateHandler handler, object viewData) {
		Logger.DebugInternal(
			$"ModConfigurationDataFeed.ListenToUpdates called, handler: {handler}\n{Environment.StackTrace}");
	}

	public LocaleString PathSegmentName(string segment, int depth) => segment;

	public object RegisterViewData() {
		Logger.DebugInternal($"ModConfigurationDataFeed.RegisterViewData called\n{Environment.StackTrace}");
		return this;
	}

	public void UnregisterListener(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase,
		DataFeedUpdateHandler handler) {
		Logger.DebugInternal(
			$"ModConfigurationDataFeed.UnregisterListener called, handler: {handler}\n{Environment.StackTrace}");
	}

	public void UnregisterViewData(object data) {
		Logger.DebugInternal(
			$"ModConfigurationDataFeed.UnregisterViewData called, object: {data}\n{Environment.StackTrace}");
	}
}
