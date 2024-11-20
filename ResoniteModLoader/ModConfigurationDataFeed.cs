﻿using Elements.Core;
using FrooxEngine;
using System.Collections;

namespace ResoniteModLoader;

/// <summary>
/// A custom data feed that can be used to show information about loaded mods, and alter their configuration. Path must start with "ResoniteModLoader"
/// </summary>
[Category(["ResoniteModLoader"])]
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
						yield return FeedBuilder.Group("ResoniteModLoader", "RML", [
							FeedBuilder.Label("ResoniteModLoader.Version", $"ResoniteModLoader version {ModLoader.VERSION}"),
							FeedBuilder.StringIndicator("ResoniteModLoader.LoadedModCount", "Loaded mods", ModLoader.Mods().Count()),
							FeedBuilder.StringIndicator("ResoniteModLoader.InitializationTime", "Startup time", DebugInfo.InitializationTime.Milliseconds + "ms")
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

					IEnumerable<Logger.MessageItem> modLogs = mod.Logs();
					if (modLogs.Any()) {
						IReadOnlyList<DataFeedItem> latestLogs = mod.GenerateModLogFeed(5, false).Append(FeedBuilder.Category("Logs", $"View full log ({modLogs.Count()})")).ToList().AsReadOnly();
						yield return FeedBuilder.Group(key + ".Logs", "Recent mod logs", latestLogs);
					}

					IEnumerable<Logger.ExceptionItem> modExceptions = mod.Exceptions();
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
	internal static string KeyFromMod(ResoniteModBase mod) => Path.GetFileNameWithoutExtension(mod.ModAssembly!.File);

	/// <summary>
	/// Returns the mod that corresponds to a unique key.
	/// </summary>
	/// <param name="key">A unique key from <see cref="KeyFromMod"/>.</param>
	/// <returns>The mod that corresponds with the unique key, or null if one couldn't be found.</returns>
	/// <seealso cref="TryModFromKey"/>
	internal static ResoniteModBase? ModFromKey(string key) => ModLoader.Mods().First((mod) => KeyFromMod(mod) == key);

	/// <summary>
	/// Tries to get the mod that corresponds to a unique key.
	/// </summary>
	/// <param name="key">A unique key from <see cref="KeyFromMod"/>.</param>
	/// <param name="mod">Set if a matching mod is found.</param>
	/// <returns><c>true</c> if a matching mod is found, <c>false</c> otherwise.</returns>
	internal static bool TryModFromKey(string key, out ResoniteModBase mod) {
		mod = ModFromKey(key)!;
		return mod is not null;
	}
}

internal static class ModConfigurationDataFeedExtensions {
	/// <summary>
	/// Generates a DataFeedGroup that displays basic information about a mod.
	/// </summary>
	/// <param name="mod">The target mod</param>
	/// <param name="standalone">Set to <c>true</c> if this group will be displayed on its own page</param>
	/// <returns>A group containing indicators for the mod's info, as well as categories to view its config/logs/exceptions.</returns>
	internal static DataFeedGroup GenerateModInfoGroup(this ResoniteModBase mod, bool standalone) {
		DataFeedGroup modFeedGroup = new();
		List<DataFeedItem> groupChildren = new();
		string key = ModConfigurationDataFeed.KeyFromMod(mod);

		if (standalone) groupChildren.Add(FeedBuilder.Indicator(key + ".Name", "Name", mod.Name));
		groupChildren.Add(FeedBuilder.Indicator(key + ".Author", "Author", mod.Author));
		groupChildren.Add(FeedBuilder.Indicator(key + ".Version", "Version", mod.Version));

		if (standalone) {
			groupChildren.Add(FeedBuilder.StringIndicator(key + ".InitializationTime", "Startup impact", mod.InitializationTime.Milliseconds + "ms"));
			groupChildren.Add(FeedBuilder.Indicator(key + ".AssemblyFile", "Assembly file", Path.GetFileName(mod.ModAssembly!.File)));
			groupChildren.Add(FeedBuilder.Indicator(key + ".AssemblyHash", "Assembly hash", mod.ModAssembly!.Sha256));
		}

		if (Uri.TryCreate(mod.Link, UriKind.Absolute, out var uri)) groupChildren.Add(FeedBuilder.ValueAction<Uri>(key + ".OpenLinkAction", $"Open mod link ({uri.Host})", (action) => action.Target = OpenURI, uri));
		if (mod.GetConfiguration() is not null) groupChildren.Add(FeedBuilder.Category(key + ".ConfigurationCategory", "Mod configuration", standalone ? ["Configuration"] : [key, "Configuration"]));

		if (!standalone) {
			IEnumerable<Logger.MessageItem> modLogs = mod.Logs();
			IEnumerable<Logger.ExceptionItem> modExceptions = mod.Exceptions();
			if (modLogs.Any()) groupChildren.Add(FeedBuilder.Category(key + ".LogsCategory", $"Mod logs ({modLogs.Count()})", [key, "Logs"]));
			if (modExceptions.Any()) groupChildren.Add(FeedBuilder.Category(key + ".ExceptionsCategory", $"Mod exceptions ({modExceptions.Count()})", [key, "Exceptions"]));
		}

		return FeedBuilder.Group(key + ".Group", standalone ? "Mod info" : mod.Name, groupChildren);
	}

	internal static IEnumerable<DataFeedItem> GenerateModConfigurationFeed(this ResoniteModBase mod, IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, object viewData, bool includeInternal = false, bool forceDefaultBuilder = false) {
		if (path.FirstOrDefault() == "ResoniteModLoader")
			Logger.WarnInternal("Call to GenerateModConfigurationFeed may include full DataFeed path, if so expect broken behavior.");

		if (!mod.TryGetConfiguration(out ModConfiguration config) || !config.ConfigurationItemDefinitions.Any()) {
			yield return FeedBuilder.Label("NoConfig", "This mod does not define any configuration keys.", color.Red);
			yield break;
		}

		List<DataFeedItem> items = new();
		bool failed = false;

		if (!forceDefaultBuilder) {
			try {
				items = mod.BuildConfigurationFeed(path, groupKeys, searchPhrase, viewData, includeInternal).ToList();
			}
			catch (Exception ex) {
				failed = true;
				Logger.ProcessException(ex, mod.ModAssembly!.Assembly);
				Logger.ErrorInternal($"Exception was thrown while running {mod.Name}'s BuildConfigurationFeed method");
			}
		}

		if (failed || !items.Any()) {
			ModConfigurationFeedBuilder.CachedBuilders.TryGetValue(config, out var builder);
			builder ??= new ModConfigurationFeedBuilder(config);
			items = builder.RootPage(searchPhrase, includeInternal).ToList();
		}

		Logger.DebugInternal($"GenerateModConfigurationFeed output for {mod.Name} @ {string.Join("/", path)}");
		foreach (DataFeedItem item in items) {
			Logger.DebugInternal($"\t{item.GetType().Name} : {item.ItemKey}");
			yield return item;
		}
	}

	private static DataFeedItem AsFeedItem(this string text, int index, bool copyable = true) {
		if (copyable)
			return FeedBuilder.ValueAction<string>(index.ToString(), text, (action) => action.Target = CopyText, text);
		else
			return FeedBuilder.Label(index.ToString(), text);
	}

	internal static IEnumerable<DataFeedItem> GenerateModLogFeed(this ResoniteModBase mod, int last = -1, bool copyable = true, string? filter = null) {
		List<Logger.MessageItem> modLogs = mod.Logs().ToList();
		last = last < 0 ? int.MaxValue : last;
		last = Math.Min(modLogs.Count, last);
		modLogs = modLogs.GetRange(modLogs.Count - last, last);
		if (!string.IsNullOrEmpty(filter))
			modLogs = modLogs.Where((line) => line.Message.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();
		foreach (Logger.MessageItem line in modLogs)
			yield return line.ToString().AsFeedItem(line.Time.GetHashCode(), copyable);
	}

	internal static IEnumerable<DataFeedItem> GenerateModExceptionFeed(this ResoniteModBase mod, int last = -1, bool copyable = true, string? filter = null) {
		List<Logger.ExceptionItem> modExceptions = mod.Exceptions().ToList();
		last = last < 0 ? int.MaxValue : last;
		last = Math.Min(modExceptions.Count, last);
		if (!string.IsNullOrEmpty(filter))
			modExceptions = modExceptions.Where((line) => line.Exception.ToString().IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();
		foreach (Logger.ExceptionItem line in modExceptions)
			yield return line.ToString().AsFeedItem(line.Time.GetHashCode(), copyable);
	}

	[SyncMethod(typeof(Action<Uri>), [])]
	private static void OpenURI(Uri uri) {
		Slot slot = Userspace.UserspaceWorld.AddSlot("Hyperlink");
		slot.PositionInFrontOfUser(float3.Backward);
		slot.AttachComponent<HyperlinkOpenDialog>().Setup(uri, "Outgoing hyperlink");
	}

	[SyncMethod(typeof(Action<string>), [])]
	private static void CopyText(string text) {
		Userspace.UserspaceWorld.InputInterface.Clipboard.SetText(text);
		NotificationMessage.SpawnTextMessage("Copied line", colorX.White);
	}
}
