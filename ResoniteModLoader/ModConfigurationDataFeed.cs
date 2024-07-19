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
#pragma warning restore CS1591
#pragma warning disable CS8618, CA1051 // FrooxEngine weaver will take care of these
	/// <summary>
	/// Show mod configuration keys marked as internal. Default: False.
	/// </summary>
	public readonly Sync<bool> IncludeInternalConfigItems;

	/// <summary>
	/// Enable or disable the use of custom configuration feeds. Default: True.
	/// </summary>
	public readonly Sync<bool> UseModDefinedEnumerate;
#pragma warning restore CS8618, CA1051
#pragma warning disable CS1591
	protected override void OnAttach() {
		base.OnAttach();
		IncludeInternalConfigItems.Value = false;
		UseModDefinedEnumerate.Value = true;
	}

	public async IAsyncEnumerable<DataFeedItem> Enumerate(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, object viewData) {
		switch (path.Count) {
			case 0: {
					yield return FeedBuilder.Category("ResoniteModLoader", "Open ResoniteModLoader category");
				}
				yield break;

			case 1: {
					if (path[0] != "ResoniteModLoader") yield break;

					yield return FeedBuilder.Label("ResoniteModLoder.Version", $"ResoniteModLoader version {ModLoader.VERSION}");
					yield return FeedBuilder.StringIndicator("ResoniteModLoder.LoadedModCount", "Loaded mods:", ModLoader.Mods().Count());

					List<DataFeedItem> groupChildren = Pool.BorrowList<DataFeedItem>();
					foreach (ResoniteModBase mod in ModLoader.Mods())
						if (string.IsNullOrEmpty(searchPhrase) || mod.Name.IndexOf(searchPhrase, StringComparison.InvariantCultureIgnoreCase) >= 0)
							yield return GenerateModInfoGroup(mod, false, groupChildren);
					Pool.Return(ref groupChildren);
				}
				yield break;

			case 2: {
					if (path[0] != "ResoniteModLoader" || !TryModFromKey(path[1], out var mod)) yield break;

					List<DataFeedItem> groupChildren = Pool.BorrowList<DataFeedItem>();
					yield return GenerateModInfoGroup(mod, true, groupChildren);
					Pool.Return(ref groupChildren);
					// GenerateModLogFeed
					// GenerateModExceptionFeed
				}
				yield break;

			case 3: {
					if (path[0] != "ResoniteModLoader" || !TryModFromKey(path[1], out var mod)) yield break;
					switch (path[2].ToLower()) {
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
		return depth switch {
			2 => ModFromKey(segment)?.Name ?? "INVALID",
			_ => segment
		};
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
	/// <summary>
	/// Returns a unique key that can be used to reference this mod.
	/// </summary>
	/// <param name="mod">The mod to return a key from.</param>
	/// <returns>A unique key representing the mod.</returns>
	/// <seealso cref="ModFromKey"/>
	/// <seealso cref="TryModFromKey"/>
	public static string KeyFromMod(ResoniteModBase mod) => mod.ModAssembly!.Sha256;

	/// <summary>
	/// Returns the mod that corresponds to a unique key.
	/// </summary>
	/// <param name="key">A unique key from <see cref="KeyFromMod"/>.</param>
	/// <returns>The mod that corresponds with the unique key, or null if one couldn't be found.</returns>
	/// <seealso cref="TryModFromKey"/>
	public static ResoniteModBase? ModFromKey(string key) => ModLoader.Mods().First((mod) => KeyFromMod(mod) == key);

	/// <summary>
	/// Tries to get the mod that corresponds to a unique key.
	/// </summary>
	/// <param name="key">A unique key from <see cref="KeyFromMod"/>.</param>
	/// <param name="mod">Set if a matching mod is found.</param>
	/// <returns>True if a matching mod is found, false otherwise.</returns>
	public static bool TryModFromKey(string key, out ResoniteModBase mod) {
		mod = ModFromKey(key)!;
		return mod is not null;
	}

	/// <summary>
	/// Spawns the prompt for a user to open a hyperlink.
	/// </summary>
	/// <param name="uri">The URI that the user will be prompted to open.</param>
	[SyncMethod(typeof(Action<Uri>), [])]
	public static void OpenURI(Uri uri) {
		Userspace.UserspaceWorld.RunSynchronously(delegate {
			Slot slot = Userspace.UserspaceWorld.AddSlot("Hyperlink");
			slot.PositionInFrontOfUser(float3.Backward);
			slot.AttachComponent<HyperlinkOpenDialog>().Setup(uri, "Outgoing hyperlink");
		});
	}

	private static DataFeedGroup GenerateModInfoGroup(ResoniteModBase mod, bool standalone = false, List<DataFeedItem> groupChildren = null!) {
		DataFeedGroup modFeedGroup = new();
		groupChildren = groupChildren ?? new();
		groupChildren.Clear();
		string key = KeyFromMod(mod);

		if (standalone) groupChildren.Add(FeedBuilder.Indicator(key + ".Name", "Name", mod.Name));
		groupChildren.Add(FeedBuilder.Indicator(key + ".Author", "Author", mod.Author));
		groupChildren.Add(FeedBuilder.Indicator(key + ".Version", "Version", mod.Version));

		if (standalone) {
			groupChildren.Add(FeedBuilder.Indicator(key + ".AssemblyFile", "Assembly file", Path.GetFileName(mod.ModAssembly!.File)));
			groupChildren.Add(FeedBuilder.Indicator(key + ".AssemblyHash", "Assembly hash", mod.ModAssembly!.Sha256));
			// TODO: Add initialization time recording
		}

		if (Uri.TryCreate(mod.Link, UriKind.Absolute, out var uri)) groupChildren.Add(FeedBuilder.ValueAction<Uri>(key + ".OpenLinkAction", $"Open mod link ({uri.Host})", (action) => action.Target = OpenURI, uri));
		if (mod.GetConfiguration() is not null) groupChildren.Add(FeedBuilder.Category(key + ".ConfigurationCategory", "Mod configuration", standalone ? ["Configuration"] : [key, "Configuration"]));

		return FeedBuilder.Group(key + ".Group", standalone ? "Mod info" : mod.Name, groupChildren);
	}

	// private static DataFeedGroup GenerateModLogFeed(ResoniteModBase mod, int last = -1) {

	// }

	// private static DataFeedGroup GenerateModExceptionFeed(ResoniteModBase mod, int last = -1) {

	// }
}
