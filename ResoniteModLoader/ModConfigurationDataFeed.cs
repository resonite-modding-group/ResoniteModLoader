using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Core;
using FrooxEngine;
using SkyFrost.Base;

namespace ResoniteModLoader;

[Category(new string[] { "Userspace" })]
public class ModConfigurationDataFeed : Component, IDataFeedComponent, IDataFeed, IWorldElement
{
	#pragma warning disable CS1591
	public override bool UserspaceOnly => true;

	public bool SupportsBackgroundQuerying => true;

	public async IAsyncEnumerable<DataFeedItem> Enumerate(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, object viewData) {
		switch (path.Count)
		{
			case 0: {
				DataFeedLabel modLoaderVersion = new DataFeedLabel();
				modLoaderVersion.InitBase(
					itemKey: "ResoniteModLoder.Version",
					label: $"ResoniteModLoader version {ModLoader.VERSION}",
					path: null,
					groupingParameters: null
				);
				yield return modLoaderVersion;

				DataFeedIndicator<int> modLoaderLoadedModCount = new DataFeedIndicator<int>();
				modLoaderLoadedModCount.InitBase(
					itemKey: "ResoniteModLoder.LoadedModCount",
					label: "Loaded mods:",
					path: null,
					groupingParameters: null
				);
				modLoaderLoadedModCount.InitSetupValue((count) => count.Value = ModLoader.Mods().Count());
				yield return modLoaderLoadedModCount;

				foreach (ResoniteModBase mod in ModLoader.Mods()) yield return GenerateModFeedGroup(mod);
			}
			break;
			case 2: {

			}
			break;
		}

	}

	public void ListenToUpdates(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, DataFeedUpdateHandler handler, object viewData) {
		Logger.DebugInternal($"ModConfigurationDataFeed.ListenToUpdates called, handler: {handler}");
	}

	public LocaleString PathSegmentName(string segment, int depth) {
		return $"{segment} ({depth})";
	}

	public object RegisterViewData() {
		Logger.DebugInternal("ModConfigurationDataFeed.RegisterViewData called");
		return null!;
	}

	public void UnregisterListener(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, DataFeedUpdateHandler handler) {
		Logger.DebugInternal($"ModConfigurationDataFeed.UnregisterListener called, handler: {handler}");
	}

	public void UnregisterViewData(object data) {
		Logger.DebugInternal($"ModConfigurationDataFeed.UnregisterViewData called, object: {data}");
	}
	#pragma warning restore CS1591

	private static DataFeedGroup GenerateModFeedGroup(ResoniteModBase mod) {
		DataFeedGroup modFeedGroup = new DataFeedGroup();
		List<DataFeedItem> groupChildren = new List<DataFeedItem>(); // Could this be a pool list instead?
		string key = mod.ModAssembly!.Sha256;

		DataFeedIndicator<string> modAuthorIndicator = new DataFeedIndicator<string>();
		modAuthorIndicator.InitBase(
			itemKey: key + ".Author",
			label: "Author",
			path: null,
			groupingParameters: null
		);
		modAuthorIndicator.InitSetupValue((str) => str.Value = mod.Author);
		groupChildren.Add(modAuthorIndicator);

		DataFeedIndicator<string> modVersionIndicator = new DataFeedIndicator<string>();
		modVersionIndicator.InitBase(
			itemKey: key + ".Version",
			label: "Version",
			path: null,
			groupingParameters: null
		);
		modVersionIndicator.InitSetupValue((str) => str.Value = mod.Version);
		groupChildren.Add(modVersionIndicator);

		DataFeedIndicator<string> modAssemblyIndicator = new DataFeedIndicator<string>();
		modAssemblyIndicator.InitBase(
			itemKey: key + ".Assembly",
			label: "File",
			path: null,
			groupingParameters: null
		);
		modAssemblyIndicator.InitSetupValue((str) => str.Value = Path.GetFileName(mod.ModAssembly!.File));
		groupChildren.Add(modAssemblyIndicator);

		if (Uri.TryCreate(mod.Link, UriKind.Absolute, out var uri))
		{
			DataFeedAction modOpenLinkAction = new DataFeedAction();
			modOpenLinkAction.InitBase(
				itemKey: key + ".OpenLinkAction",
				label: $"Open mod link ({uri.Host})",
				path: null,
				groupingParameters: null
			);
			modOpenLinkAction.InitAction(delegate {
				Userspace.UserspaceWorld.RunSynchronously(delegate {
					Slot slot = Userspace.UserspaceWorld.AddSlot("Hyperlink");
					slot.PositionInFrontOfUser(float3.Backward);
					slot.AttachComponent<HyperlinkOpenDialog>().Setup(uri, "Outgoing hyperlink");
				});
			});
			groupChildren.Add(modOpenLinkAction);
		}

		if (mod.ModConfiguration is not null)
		{
			DataFeedCategory modConfigurationCategory = new DataFeedCategory();
			modConfigurationCategory.InitBase(
				itemKey: key + ".ConfigurationCategory",
				label: "Mod configuration",
				path: new string[] {key, "Configuration"},
				groupingParameters: null
			);
			groupChildren.Add(modConfigurationCategory);
		}

		modFeedGroup.InitBase(
			itemKey: key + ".Group",
			label: mod.Name,
			path: null,
			groupingParameters: null,
			subitems: groupChildren
		);

		return modFeedGroup;
	}
}
