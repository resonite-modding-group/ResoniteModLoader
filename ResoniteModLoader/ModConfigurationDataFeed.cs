using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Core;
using FrooxEngine;
using SkyFrost.Base;

namespace ResoniteModLoader;

[GloballyRegistered]
[Category(new string[] { "Userspace" })]
public class ModConfigurationDataFeed : Component, IDataFeedComponent, IDataFeed, IWorldElement
{
	public override bool UserspaceOnly => true;

	public bool SupportsBackgroundQuerying => true;

	public async IAsyncEnumerable<DataFeedItem> Enumerate(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, object viewData) {
		{
			DataFeedLabel modLoaderVersion = new DataFeedLabel();
			modLoaderVersion.InitBase("ResoniteModLoder.Version", null, null, $"ResoniteModLoader version {ModLoader.VERSION}");
			yield return modLoaderVersion;

			DataFeedLabel modLoaderLoadedModCount = new DataFeedLabel();
			modLoaderLoadedModCount.InitBase("ResoniteModLoder.LoadedModCount", null, null, $"{ModLoader.Mods().Count()} mods loaded");
			yield return modLoaderLoadedModCount;
		}

		foreach (ResoniteModBase mod in ModLoader.Mods())
		{
			DataFeedGroup modDataFeedGroup = new DataFeedGroup();
			modDataFeedGroup.InitBase(mod.Name + ".Group", null, null, mod.Name);
			yield return modDataFeedGroup;

			DataFeedLabel authorDataFeedLabel = new DataFeedLabel();
			authorDataFeedLabel.InitBase(mod.Name + ".Author", null, null, $"Author: {mod.Author}");
			yield return authorDataFeedLabel;

			DataFeedLabel versionDataFeedLabel = new DataFeedLabel();
			versionDataFeedLabel.InitBase(mod.Name + ".Version", null, null, $"Version: {mod.Version}");
			yield return versionDataFeedLabel;

			DataFeedLabel isLoadedDataFeedLabel = new DataFeedLabel();
			isLoadedDataFeedLabel.InitBase(mod.Name + ".IsLoaded", null, null, $"IsLoaded: {mod.FinishedLoading}");
			yield return isLoadedDataFeedLabel;
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
		return null;
	}

	public void UnregisterListener(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, DataFeedUpdateHandler handler) {
		Logger.DebugInternal($"ModConfigurationDataFeed.UnregisterListener called, handler: {handler}");
	}

	public void UnregisterViewData(object data) {
		Logger.DebugInternal($"ModConfigurationDataFeed.UnregisterViewData called, object: {data}");
	}
}
