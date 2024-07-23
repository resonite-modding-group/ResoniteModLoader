using Elements.Core;
using FrooxEngine;
using System.Collections;

namespace ResoniteModLoader;

/// <summary>
/// A custom data feed that can be used to show information about loaded mods, and alter their configuration. Path must start with "ResoniteModLoder"
/// </summary>
[Category(["ResoniteModLoder"])]
public class ModConfigurationDataFeed : Component, IDataFeedComponent, IDataFeed, IWorldElement {
#pragma warning disable CS1591
	public override bool UserspaceOnly => true;

	public bool SupportsBackgroundQuerying => true;
#pragma warning restore CS1591
#pragma warning disable CS8618, CA1051 // FrooxEngine weaver will take care of these
	/// <summary>
	/// Show mod configuration keys marked as internal.
	/// </summary>
	public readonly Sync<bool> IncludeInternalConfigItems;

	/// <summary>
	/// Enable or disable the use of custom configuration feeds.
	/// </summary>
	public readonly Sync<bool> IgnoreModDefinedEnumerate;
#pragma warning restore CS8618, CA1051
#pragma warning disable CS1591
	public async IAsyncEnumerable<DataFeedItem> Enumerate(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, object viewData) {
		switch (path.Count) {
			case 0: {
					yield return FeedBuilder.Category("ResoniteModLoader", "Open ResoniteModLoader category");
				}
				yield break;

			case 1: {
					if (path[0] != "ResoniteModLoader") yield break;

					if (string.IsNullOrEmpty(searchPhrase)) {
						yield return FeedBuilder.Group("ResoniteModLoder", "RML", [
							FeedBuilder.Label("ResoniteModLoder.Version", $"ResoniteModLoader version {ModLoader.VERSION}"),
							FeedBuilder.StringIndicator("ResoniteModLoder.LoadedModCount", "Loaded mods:", ModLoader.Mods().Count())
						]);

						List<DataFeedCategory> modCategories = new();
						foreach (ResoniteModBase mod in ModLoader.Mods())
							modCategories.Add(FeedBuilder.Category(KeyFromMod(mod), mod.Name));

						yield return FeedBuilder.Grid("Mods", "Mods", modCategories);
					}
					else {
						IEnumerable<ResoniteModBase> filteredMods = ModLoader.Mods().Where((mod) => mod.Name.IndexOf(searchPhrase, StringComparison.InvariantCultureIgnoreCase) >= 0);
						yield return FeedBuilder.Label("SearchResults", filteredMods.Any() ? $"Search results: {filteredMods.Count()} mods found." : "No results!");

						foreach (ResoniteModBase mod in filteredMods)
							yield return mod.GenerateModInfoGroup(false);
					}
				}
				yield break;

			case 2: {
					if (path[0] != "ResoniteModLoader" || !TryModFromKey(path[1], out var mod)) yield break;

					string key = KeyFromMod(mod);
					yield return mod.GenerateModInfoGroup(true);

					IEnumerable<Logger.LogMessage> modLogs = mod.Logs();
					if (modLogs.Any()) {
						IReadOnlyList<DataFeedItem> latestLogs = mod.GenerateModLogFeed(5, false).Append(FeedBuilder.Category("Logs", $"View full log ({modLogs.Count()})")).ToList().AsReadOnly();
						yield return FeedBuilder.Group(key + ".Logs", "Recent mod logs", latestLogs);
					}

					IEnumerable<Logger.LogException> modExceptions = mod.Exceptions();
					if (modExceptions.Any()) {
						IReadOnlyList<DataFeedItem> latestException = mod.GenerateModExceptionFeed(1, false).Append(FeedBuilder.Category("Exceptions", $"View all exceptions ({modExceptions.Count()})")).ToList().AsReadOnly();
						yield return FeedBuilder.Group(key + ".Exceptions", "Latest mod exception", latestException);
					}
				}
				yield break;

			case 3: {
					if (path[0] != "ResoniteModLoader" || !TryModFromKey(path[1], out var mod)) yield break;

					switch (path[2].ToLower()) {
						case "configuration": {
								foreach (DataFeedItem item in mod.GenerateModConfigurationFeed(path.Skip(3).ToArray(), groupKeys, searchPhrase, viewData, IncludeInternalConfigItems.Value, IgnoreModDefinedEnumerate.Value))
									yield return item;
							}
							yield break;

						case "logs": {
								foreach (DataFeedItem item in mod.GenerateModLogFeed(-1, true, searchPhrase))
									yield return item;
							}
							yield break;

						case "exceptions": {
								foreach (DataFeedItem item in mod.GenerateModExceptionFeed(-1, true, searchPhrase))
									yield return item;
							}
							yield break;

						default: {
								// Reserved for future use - mods defining their own subfeeds
							}
							yield break;
					}
				}
			case > 3: {
					if (path[0] != "ResoniteModLoader" || !TryModFromKey(path[1], out var mod)) yield break;

					if (path[2].ToLower() == "configuration") {
						foreach (DataFeedItem item in mod.GenerateModConfigurationFeed(path.Skip(3).ToArray(), groupKeys, searchPhrase, viewData, IncludeInternalConfigItems.Value, IgnoreModDefinedEnumerate.Value))
							yield return item;

						yield break;
					}
					else {
						// Reserved for future use - mods defining their own subfeeds
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
			3 => segment.Capitalize(),
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
	public static string KeyFromMod(ResoniteModBase mod) => Path.GetFileNameWithoutExtension(mod.ModAssembly!.File);

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
}

internal static class ModConfigurationDataFeedExtensions {
	/// <summary>
	/// Generates a DataFeedGroup that displays basic information about a mod.
	/// </summary>
	/// <param name="mod">The target mod</param>
	/// <param name="standalone">Set to true if this group will be displayed on its own page</param>
	/// <returns></returns>
	public static DataFeedGroup GenerateModInfoGroup(this ResoniteModBase mod, bool standalone) {
		DataFeedGroup modFeedGroup = new();
		List<DataFeedItem> groupChildren = new();
		string key = ModConfigurationDataFeed.KeyFromMod(mod);

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

		if (!standalone) {
			IEnumerable<Logger.LogMessage> modLogs = mod.Logs();
			IEnumerable<Logger.LogException> modExceptions = mod.Exceptions();
			if (modLogs.Any()) groupChildren.Add(FeedBuilder.Category(key + ".LogsCategory", $"Mod logs ({modLogs.Count()})", [key, "Logs"]));
			if (modExceptions.Any()) groupChildren.Add(FeedBuilder.Category(key + ".ExceptionsCategory", $"Mod exceptions ({modExceptions.Count()})", [key, "Exceptions"]));
		}

		return FeedBuilder.Group(key + ".Group", standalone ? "Mod info" : mod.Name, groupChildren);
	}

	public static IEnumerable<DataFeedItem> GenerateModConfigurationFeed(this ResoniteModBase mod, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, object viewData, bool includeInternal = false, bool forceDefaultBuilder = false) {
		if (path.FirstOrDefault() == "ResoniteModLoader")
			Logger.WarnInternal("Call to GenerateModConfigurationFeed may include full DataFeed path, if so expect broken behavior.");

		if (!mod.TryGetConfiguration(out ModConfiguration config) || !config.ConfigurationItemDefinitions.Any()) {
			yield return FeedBuilder.Label("NoConfig", "This mod does not define any configuration keys.", color.Red);
			yield break;
		}

		ModConfigurationFeedBuilder.CachedBuilders.TryGetValue(config, out ModConfigurationFeedBuilder builder);
		builder = builder ?? new ModConfigurationFeedBuilder(config);
		IEnumerable<DataFeedItem> items;

		if (!forceDefaultBuilder) {
			try {
				items = mod.BuildConfigurationFeed(path.Skip(3).ToArray(), groupKeys, searchPhrase, viewData, includeInternal);
			}
			catch (Exception ex) {
				Logger.ProcessException(ex, mod.ModAssembly!.Assembly);
				Logger.ErrorInternal($"Exception was thrown while running {mod.Name}'s BuildConfigurationFeed method");
				items = builder.GeneratePage(path.Skip(3).ToArray(), searchPhrase, includeInternal);
				items = items.Prepend(FeedBuilder.Label("BuildConfigurationFeedException", "Encountered error while building custom configuration feed!", color.Red));
			}
		}
		else {
			items = builder.GeneratePage(path.Skip(3).ToArray(), searchPhrase, includeInternal);
		}

		foreach (DataFeedItem item in items)
			yield return item;
	}

	private static DataFeedItem AsFeedItem(this string text, int index, bool copyable = true) {
		if (copyable)
			return FeedBuilder.ValueAction<string>(index.ToString(), text, (action) => action.Target = CopyText, text);
		else
			return FeedBuilder.Label(index.ToString(), text);
	}

	public static IEnumerable<DataFeedItem> GenerateModLogFeed(this ResoniteModBase mod, int last = -1, bool copyable = true, string? filter = null) {
		List<Logger.LogMessage> modLogs = mod.Logs().ToList();
		last = last < 0 ? int.MaxValue : last;
		last = Math.Min(modLogs.Count, last);
		modLogs = modLogs.GetRange(modLogs.Count - last, last);
		if (!string.IsNullOrEmpty(filter))
			modLogs = modLogs.Where((line) => line.Message.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();
		foreach (Logger.LogMessage line in modLogs)
			yield return line.ToString().AsFeedItem(line.Time.GetHashCode(), copyable);
	}

	public static IEnumerable<DataFeedItem> GenerateModExceptionFeed(this ResoniteModBase mod, int last = -1, bool copyable = true, string? filter = null) {
		List<Logger.LogException> modExceptions = mod.Exceptions().ToList();
		last = last < 0 ? int.MaxValue : last;
		last = Math.Min(modExceptions.Count, last);
		if (!string.IsNullOrEmpty(filter))
			modExceptions = modExceptions.Where((line) => line.Exception.ToString().IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();
		foreach (Logger.LogException line in modExceptions)
			yield return line.ToString().AsFeedItem(line.Time.GetHashCode(), copyable);
	}

	/// <summary>
	/// Spawns the prompt for a user to open a hyperlink.
	/// </summary>
	/// <param name="uri">The URI that the user will be prompted to open.</param>
	[SyncMethod(typeof(Action<Uri>), [])]
	public static void OpenURI(Uri uri) {
		Slot slot = Userspace.UserspaceWorld.AddSlot("Hyperlink");
		slot.PositionInFrontOfUser(float3.Backward);
		slot.AttachComponent<HyperlinkOpenDialog>().Setup(uri, "Outgoing hyperlink");
	}

	[SyncMethod(typeof(Action<string>), [])]
	public static void CopyText(string text) {
		Userspace.UserspaceWorld.InputInterface.Clipboard.SetText(text);
		NotificationMessage.SpawnTextMessage("Copied line", colorX.White);
	}
}
