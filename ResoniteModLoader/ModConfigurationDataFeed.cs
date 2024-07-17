using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Elements.Core;
using FrooxEngine;
using SkyFrost.Base;

namespace ResoniteModLoader;

/// <summary>
/// A custom data feed that can be used to show information about loaded mods, and alter their configuration. Path must start with "ResoniteModLoder"
/// </summary>
[Category(["Userspace"])]
public class ModConfigurationDataFeed : Component, IDataFeedComponent, IDataFeed, IWorldElement {
#pragma warning disable CS1591
	public override bool UserspaceOnly => true;

	public bool SupportsBackgroundQuerying => true;

	public async IAsyncEnumerable<DataFeedItem> Enumerate(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, object viewData) {
		switch (path.Count) {
			case 0: {
					DataFeedCategory modLoaderCategory = new DataFeedCategory();
					modLoaderCategory.InitBase(
						itemKey: "ResoniteModLoader",
						label: $"Open ResoniteModLoader category",
						path: null,
						groupingParameters: null
					);
					yield return modLoaderCategory;
				}
				yield break;

			case 1: {
					if (path[0] != "ResoniteModLoader") yield break;

					DataFeedLabel modLoaderVersion = new DataFeedLabel();
					modLoaderVersion.InitBase(
						itemKey: "ResoniteModLoder.Version",
						label: $"ResoniteModLoader version {ModLoader.VERSION}",
						path: null,
						groupingParameters: null
					);
					yield return modLoaderVersion;

					DataFeedIndicator<string> modLoaderLoadedModCount = new DataFeedIndicator<string>(); // Todo: Make DataFeedIndicator<int> template
					modLoaderLoadedModCount.InitBase(
						itemKey: "ResoniteModLoder.LoadedModCount",
						label: "Loaded mods:",
						path: null,
						groupingParameters: null
					);
					modLoaderLoadedModCount.InitSetupValue((count) => count.Value = ModLoader.Mods().Count().ToString());
					yield return modLoaderLoadedModCount;

					foreach (ResoniteModBase mod in ModLoader.Mods())
						if (string.IsNullOrEmpty(searchPhrase) || mod.Name.ToLowerInvariant().Contains(searchPhrase.ToLowerInvariant()))
							yield return GenerateModInfoGroup(mod);
				}
				yield break;

			case 2: {
					if (path[0] != "ResoniteModLoader") yield break;
					string key = path[1];
					ResoniteModBase mod = ModFromKey(key);
					yield return GenerateModInfoGroup(mod, true);
					// GenerateModLogFeed
					// GenerateModExceptionFeed
				}
				yield break;

			case 3: {
					if (path[0] != "ResoniteModLoader") yield break;
					string key = path[1];
					ResoniteModBase mod = ModFromKey(key);
					switch (path[2].ToLowerInvariant()) {
						case "configuration": {

							}
							yield break;
						case "logs": {

							}
							yield break;
						case "exceptions": {

							}
							yield break;
					}
				}
				yield break;
		}
	}

	public void ListenToUpdates(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, DataFeedUpdateHandler handler, object viewData) {
		Logger.DebugInternal($"ModConfigurationDataFeed.ListenToUpdates called, handler: {handler}\n{Environment.StackTrace}");
	}

	public LocaleString PathSegmentName(string segment, int depth) {
		return $"{segment} ({depth})";
	}

	public object RegisterViewData() {
		Logger.DebugInternal($"ModConfigurationDataFeed.RegisterViewData called\n{Environment.StackTrace}");
		return null!;
	}

	public void UnregisterListener(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, DataFeedUpdateHandler handler) {
		Logger.DebugInternal($"ModConfigurationDataFeed.UnregisterListener called, handler: {handler}\n{Environment.StackTrace}");
	}

	public void UnregisterViewData(object data) {
		Logger.DebugInternal($"ModConfigurationDataFeed.UnregisterViewData called, object: {data}\n{Environment.StackTrace}");
	}
#pragma warning restore CS1591

	public static string KeyFromMod(ResoniteModBase mod) => mod.ModAssembly!.Sha256;

	public static ResoniteModBase? ModFromKey(string key) => ModLoader.Mods().First((mod) => KeyFromMod(mod) == key);

	[SyncMethod(typeof(Action<Uri>), [])]
	public static void OpenURI(Uri uri) {
		Userspace.UserspaceWorld.RunSynchronously(delegate {
			Slot slot = Userspace.UserspaceWorld.AddSlot("Hyperlink");
			slot.PositionInFrontOfUser(float3.Backward);
			slot.AttachComponent<HyperlinkOpenDialog>().Setup(uri, "Outgoing hyperlink");
		});
	}

	private static DataFeedGroup GenerateModInfoGroup(ResoniteModBase mod, bool standalone = false) {
		DataFeedGroup modFeedGroup = new DataFeedGroup();
		List<DataFeedItem> groupChildren = new List<DataFeedItem>(); // Could this be a pool list instead?
		string key = KeyFromMod(mod);

		if (standalone) {
			DataFeedIndicator<string> modNameIndicator = new DataFeedIndicator<string>();
			modNameIndicator.InitBase(
				itemKey: key + ".Name",
				label: "Name",
				path: null,
				groupingParameters: null
			);
			modNameIndicator.InitSetupValue((str) => str.Value = mod.Name);
			groupChildren.Add(modNameIndicator);
		}

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

		if (standalone) {
			DataFeedIndicator<string> modAssemblyFileIndicator = new DataFeedIndicator<string>();
			modAssemblyFileIndicator.InitBase(
				itemKey: key + ".AssemblyFile",
				label: "Assembly file",
				path: null,
				groupingParameters: null
			);
			modAssemblyFileIndicator.InitSetupValue((str) => str.Value = Path.GetFileName(mod.ModAssembly!.File));
			groupChildren.Add(modAssemblyFileIndicator);

			DataFeedIndicator<string> modAssemblyHashIndicator = new DataFeedIndicator<string>();
			modAssemblyHashIndicator.InitBase(
				itemKey: key + ".AssemblyHash",
				label: "Assembly hash",
				path: null,
				groupingParameters: null
			);
			modAssemblyHashIndicator.InitSetupValue((str) => str.Value = mod.ModAssembly!.Sha256);
			groupChildren.Add(modAssemblyHashIndicator);

			// TODO: Add initialization time recording
		}

		if (Uri.TryCreate(mod.Link, UriKind.Absolute, out var uri)) {
			DataFeedValueAction<Uri> modOpenLinkAction = new DataFeedValueAction<Uri>();
			modOpenLinkAction.InitBase(
				itemKey: key + ".OpenLinkAction",
				label: $"Open mod link ({uri.Host})",
				path: null,
				groupingParameters: null
			);
			modOpenLinkAction.InitAction((action) => action.Target = OpenURI, uri);
			groupChildren.Add(modOpenLinkAction);
		}

		if (mod.ModConfiguration is not null) {
			DataFeedCategory modConfigurationCategory = new DataFeedCategory();
			modConfigurationCategory.InitBase(
				itemKey: key + ".ConfigurationCategory",
				label: "Mod configuration",
				path: [key, "Configuration"],
				groupingParameters: null
			);
			modConfigurationCategory.SetOverrideSubpath(standalone ? ["Configuration"] : [key, "Configuration"]);
			groupChildren.Add(modConfigurationCategory);
		}

		modFeedGroup.InitBase(
			itemKey: key + ".Group",
			label: standalone ? "Mod info" : mod.Name,
			path: null,
			groupingParameters: null,
			subitems: groupChildren
		);

		return modFeedGroup;
	}

	// private static DataFeedGroup GenerateModLogFeed(ResoniteModBase mod, int last = -1) {

	// }

	// private static DataFeedGroup GenerateModExceptionFeed(ResoniteModBase mod, int last = -1) {

	// }
}
