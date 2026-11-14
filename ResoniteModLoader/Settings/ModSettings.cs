using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using ResoniteModLoader.Utility;

namespace ResoniteModLoader;

/// <summary>
/// Settings UI for loaded mods.
/// </summary>
[AutoRegisterSetting]
[SettingCategory("ResoniteModLoader")]
public sealed class ModSettings : SettingComponent<ModSettings> {
	/// <inheritdoc/>
	public override bool UserspaceOnly => true;

	private DataFeedItemMapper? _templateMapper;
	private static readonly string[] ModIndex = ModLoader.FileNameLookupMap.Keys.ToArray();
	private string _currentMod = "";

	/// <inheritdoc />
	protected override void OnStart() {
		base.OnStart();
		Persistent = false;
		_collectionData ??= new();
		var data = new CollectionData(ModToFeedItem, false);
		for (int i = 0; i < ModIndex.Length; i++) {
			var mod = ModIndex[i];
			InitInfo.syncMemberNameToIndex[mod] = 1000 + i;
			_collectionData[mod] = data;
		}

		ModConfigStub.Clear();
		ModConfigStub.AddRange(new []{0, 1});
	}

	internal ISyncMember GetFakeMember(int index) {
		Logger.DebugInternal("ModSettings.GetFakeMember " + index);
		ArgumentOutOfRangeException.ThrowIfLessThan(index, 1000);

		_currentMod = ModIndex[index - 1000];
		Logger.DebugInternal(_currentMod);
		return ModConfigStub;
	}

	private DataFeedItem ModToFeedItem(ISyncMember item) {
		var index = (item as Sync<int>)!.Value;
		var mod = ModLoader.FileNameLookupMap[_currentMod];
		return index switch {
			0 => GenerateModInfoPanel(mod),
			1 => GenerateModConfigPanel(mod),
			_ => throw new ArgumentOutOfRangeException(nameof(item))
		};
	}

	private bool ShouldUseFallback(DataFeedItem item)
	{
		if (_templateMapper == null) {
			// ¯\_(ツ)_/¯
			return false;
		}

		Type itemType = item.GetType();
		Type? itemGenericDefinition = itemType.IsGenericType ? itemType.GetGenericTypeDefinition() : null;
		foreach (DataFeedItemMapper.ItemMapping mapping in _templateMapper.Mappings)
		{
			if (mapping.Template.Target == null || mapping.MatchingType.Value == null || mapping.MatchingType.Value == typeof(DataFeedItem))
			{
				continue;
			}
			if (mapping.MatchingType.Value.IsAssignableFrom(itemType))
			{
				return false;
			}
			if (mapping.MatchingType.Value.IsGenericTypeDefinition && mapping.MatchingType.Value.IsAssignableFrom(itemGenericDefinition))
			{
				if (mapping.GenericReplacementTypes.Count == 0)
				{
					continue;
				}
				return false;
			}
		}
		return item is not DataFeedLabel;
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
					action => action.Target = OpenURI, uri, groupingParameters: modGrouping)
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

				// if (ShouldUseFallback(item)) {
				// 	try {
				// 		item = configBuilder.GenerateDataFeedFallbackEditor(key, groupingParameters: modGrouping);
				// 	}
				// 	catch (Exception e) {
				// 		item =  FeedBuilder.Label(key.Name,
				// 			$"Exception generating member editor for \"{key.Name}\": {e.Message}", colorX.Red,
				// 			groupingParameters: modGrouping);
				// 		Logger.ErrorInternal(e);
				// 	}
				// }

				if (key.InternalAccessOnly) {
					item.InitVisible(field => field.DriveFrom(ShowInternal));
				}

				group.AddSubitems(item);
			}

			if (modConfig.ConfigurationItemDefinitions.All(key => key.InternalAccessOnly)) {
				group.AddSubitems(
					FeedBuilder.Label(groupingBase + ".NoConfigs", "Settings.ModSettings.NoConfigs".AsLocaleKey(),
							groupingParameters: modGrouping)
						.WithVisible(field => field.DriveInverted(ShowInternal))
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

	// public new bool IsSubcategoryPublic(string subcategory) {
	// 	Logger.DebugInternal("ModSettings.IsSubcategoryPublic " + subcategory);
	// 	return base.IsSubcategoryPublic(subcategory) || ModLoader.FileNameLookupMap.ContainsKey(subcategory);
	// }
	//
	// public new async IAsyncEnumerable<DataFeedItem> GetSubcategoryItems(string subcategory) {
	// 	Logger.DebugInternal("ModSettings.GetSubcategoryItems " + subcategory);
	// 	if (!IsSubcategoryPublic(subcategory)) {
	// 		yield return FeedBuilder.Label("ModInfo." + subcategory, $"Invalid mod {subcategory}");
	// 		yield break;
	// 	}
	//
	// 	if (subcategory == ModListName) {
	// 		yield return GenerateModGrid();
	// 		yield return GenerateModConfigPanel(subcategory);
	// 	}
	//
	// 	yield return GenerateModInfoPanel(subcategory, false);
	// }

	private bool FindTemplateMapper(IField field) {
		var slot = field.FindNearestParent<Slot>();
		var view = slot.GetComponentInParents<RootCategoryView>(rcv => slot.IsChildOf(rcv.ItemsManager.ContainerRoot));
		var found = view?.ItemsManager.TemplateMapper.Target;
		if (found == null) {
			return false;
		}

		Logger.DebugInternal("Found TemplateMapper: " + found);
		_templateMapper = found;
		return true;
	}

	internal DataFeedItem GenerateModGrid(IReadOnlyList<string> path = null!, IReadOnlyList<string> grouping = null!) {
		if (!ModLoader.Mods().Any()) {
			return FeedBuilder.Label("NoMods", "Settings.ModSettings.NoMods".AsLocaleKey());
		}

		List<DataFeedItem> modLinks = new();

		foreach (var (modKey, mod) in ModLoader.FileNameLookupMap) {
			var link = FeedBuilder.Category(modKey, mod.Name, groupingParameters: [nameof(ModList)]);
			link.SetOverrideSubpath("ModSettings." + modKey);
			if (mod.GetConfiguration() is { } config) {
				if (config.ConfigurationItemDefinitions.All(key => key.InternalAccessOnly)) {
					link.InitVisible(field =>
						field.SetupBoolConditionDriver(MultiBoolConditionDriver.ConditionMode.Any,
							ShowAll, ShowInternal));
				}
			}
			else {
				link.InitVisible(field => field.DriveFrom(ShowAll));
			}

			modLinks.Add(link);
		}

		var grid = FeedBuilder.Grid(nameof(ModList), "Settings.ModSettings.ModList".AsLocaleKey(), path, grouping,
			subitems: modLinks);
		grid.InitEnabled(field => FindTemplateMapper(field));

		if (ModLoader.Mods().All(mod => mod.GetConfiguration() == null)) {
			grid.InitVisible(field => field.DriveFrom(ShowAll));
			modLinks.Add(
				FeedBuilder.Label("NoModsWithConfig", "Settings.ModSettings.NoModsWithConfig".AsLocaleKey(),
						groupingParameters: [nameof(ModSettings)])
					.WithVisible(field => field.DriveInverted(ShowAll))
			);
		}

		return grid;
	}

	[SettingSubcategoryEnumerator]
	public async IAsyncEnumerable<DataFeedItem> ModList() {
		// Ideally this would get replaced by SettingsDataFeedPatcher, but if the patch fails this is a fallback.
		yield return GenerateModGrid();
	}

	[SettingProperty] public readonly Sync<bool> ShowInternal;

	[SettingProperty] public readonly Sync<bool> ShowAll;

	[HideInInspector] private readonly SyncFieldList<int> ModConfigStub;

	[SyncMethod(typeof(Action<Uri>), [])]
	internal static void OpenURI(Uri uri) {
		Slot slot = Userspace.UserspaceWorld.AddSlot("Hyperlink");
		slot.PositionInFrontOfUser(float3.Backward);
		slot.AttachComponent<HyperlinkOpenDialog>().Setup(uri, null!);
	}
}
