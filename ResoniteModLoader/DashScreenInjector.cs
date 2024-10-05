using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;

namespace ResoniteModLoader;

internal sealed class DashScreenInjector {
	internal static RadiantDashScreen? InjectedScreen;

	internal static void PatchScreenManager(Harmony harmony) {
		MethodInfo setupDefaultMethod = AccessTools.DeclaredMethod(typeof(UserspaceScreensManager), "SetupDefaults");
		MethodInfo onLoadingMethod = AccessTools.DeclaredMethod(typeof(UserspaceScreensManager), "OnLoading");
		MethodInfo tryInjectScreenMethod = AccessTools.DeclaredMethod(typeof(DashScreenInjector), nameof(TryInjectScreen));
		harmony.Patch(setupDefaultMethod, postfix: new HarmonyMethod(tryInjectScreenMethod));
		harmony.Patch(onLoadingMethod, postfix: new HarmonyMethod(tryInjectScreenMethod));
		Logger.DebugInternal("UserspaceScreensManager patched");
	}

	internal static async void TryInjectScreen(UserspaceScreensManager __instance) {
		ModLoaderConfiguration config = ModLoaderConfiguration.Get();

		if (config.NoDashScreen) {
			Logger.DebugInternal("Dash screen will not be injected due to configuration file");
			return;
		}
		if (__instance.World != Userspace.UserspaceWorld) {
			Logger.WarnInternal("Dash screen will not be injected because we're somehow not in userspace (WTF?)"); // it stands for What the Froox :>
			return;
		}
		if (InjectedScreen is not null && !InjectedScreen.IsRemoved) {
			Logger.WarnInternal("Dash screen will not be injected again because it already exists");
			return;
		}

		Logger.DebugInternal("Injecting dash screen");

		RadiantDash dash = __instance.Slot.GetComponentInParents<RadiantDash>();
		InjectedScreen = dash.AttachScreen("Mods", RadiantUI_Constants.Neutrals.LIGHT, OfficialAssets.Graphics.Icons.Dash.Tools);

		InjectedScreen.Slot.OrderOffset = 128;
		InjectedScreen.Slot.PersistentSelf = false;

		SingleFeedView view = InjectedScreen.ScreenRoot.AttachComponent<SingleFeedView>();
		ModConfigurationDataFeed feed = InjectedScreen.ScreenRoot.AttachComponent<ModConfigurationDataFeed>();

		Slot templates = InjectedScreen.ScreenRoot.AddSlot("Template");
		templates.ActiveSelf = false;

		if (await templates.LoadObjectAsync(__instance.Cloud.Platform.GetSpawnObjectUri("Settings"), skipHolder: true)) {
			// we do a little bit of thievery
			RootCategoryView rootCategoryView = templates.GetComponentInChildren<RootCategoryView>();
			rootCategoryView.Slot.GetComponentInChildren<BreadcrumbManager>().Path.Target = view.Path;
			rootCategoryView.CategoryManager.ContainerRoot.Target.ActiveSelf = false;
			rootCategoryView.Slot.Children.First().Parent = InjectedScreen.ScreenCanvas.Slot;
			view.ItemsManager.TemplateMapper.Target = rootCategoryView.ItemsManager.TemplateMapper.Target;
			view.ItemsManager.ContainerRoot.Target = rootCategoryView.ItemsManager.ContainerRoot.Target;
			rootCategoryView.Destroy();
			templates.GetComponentInChildren<BreadcrumbInterface>().NameConverter.Target = view.PathSegmentName;
		}
		else if (config.Debug) {
			Logger.ErrorInternal("Failed to load SettingsItemMappers for dash screen, falling back to template.");
			DataFeedItemMapper itemMapper = templates.AttachComponent<DataFeedItemMapper>();
			Canvas tempCanvas = templates.AttachComponent<Canvas>(); // Needed for next method to work
			itemMapper.SetupTemplate();
			tempCanvas.Destroy();
			view.ItemsManager.TemplateMapper.Target = itemMapper;
			view.ItemsManager.ContainerRoot.Target = InjectedScreen.ScreenCanvas.Slot;
			InjectedScreen.ScreenCanvas.Slot.AttachComponent<VerticalLayout>(); // just for debugging
		}
		else {
			Logger.ErrorInternal("Failed to load SettingsItemMappers for dash screen, aborting and cleaning up.");
			InjectedScreen.Slot.Destroy();
			return;
		}

		InjectedScreen.ScreenCanvas.Slot.AttachComponent<Image>().Tint.Value = UserspaceRadiantDash.DEFAULT_BACKGROUND;
		view.Feed.Target = feed;
		view.SetCategoryPath(["ResoniteModLoader"]);

		Logger.DebugInternal("Dash screen should be injected!");
	}
}
