using FrooxEngine;
using HarmonyLib;

namespace ResoniteModLoader;

internal sealed class DashScreenInjector
{
	internal static RadiantDashScreen? InjectedScreen;

	internal static void PatchScreenManager(Harmony harmony)
	{
		MethodInfo setupDefaultMethod = AccessTools.DeclaredMethod(typeof(UserspaceScreensManager), "SetupDefaults");
		MethodInfo onLoadingMethod = AccessTools.DeclaredMethod(typeof(UserspaceScreensManager), "OnLoading");
		MethodInfo tryInjectScreenMethod = AccessTools.DeclaredMethod(typeof(DashScreenInjector), nameof(TryInjectScreen));
		harmony.Patch(setupDefaultMethod, postfix: new HarmonyMethod(tryInjectScreenMethod));
		harmony.Patch(onLoadingMethod, postfix: new HarmonyMethod(tryInjectScreenMethod));
		Logger.DebugInternal("UserspaceScreensManager patched");
	}

	internal static async void TryInjectScreen(UserspaceScreensManager __instance)
	{
		if (ModLoaderConfiguration.Get().NoDashScreen)
		{
			Logger.DebugInternal("Dash screen will not be injected due to configuration file");
			return;
		}
		if (__instance.World != Userspace.UserspaceWorld)
		{
			Logger.WarnInternal("Dash screen will not be injected because we're somehow not in userspace (WTF?)"); // it stands for What the Froox :>
			return;
		}
		if (InjectedScreen is not null && !InjectedScreen.IsRemoved)
		{
			Logger.WarnInternal("Dash screen will not be injected again because it already exists");
		}

		Logger.DebugInternal("Injecting dash screen");

		RadiantDash dash = __instance.Slot.GetComponentInParents<RadiantDash>();
		InjectedScreen = dash.AttachScreen("Mods", RadiantUI_Constants.Hero.RED, OfficialAssets.Graphics.Icons.General.BoxClosed); // Replace with RML icon later

		Slot screenSlot = InjectedScreen.Slot;
		screenSlot.OrderOffset = 128;
		screenSlot.PersistentSelf = false;

		SingleFeedView view = screenSlot.AttachComponent<SingleFeedView>();
		ModConfigurationDataFeed feed = screenSlot.AttachComponent<ModConfigurationDataFeed>();

		Slot templates = screenSlot.AddSlot("Template");
		templates.ActiveSelf = false;

		if (await templates.LoadObjectAsync(__instance.Cloud.Platform.GetSpawnObjectUri("SettingsItemMappers"), skipHolder: true))
		{
			DataFeedItemMapper itemMapper = templates.FindChild("ItemsMapper").GetComponent<DataFeedItemMapper>();
			view.ItemsManager.TemplateMapper.Target = itemMapper;
			view.ItemsManager.ContainerRoot.Target = InjectedScreen.ScreenRoot;
		}
		else
		{
			Logger.ErrorInternal("Failed to load SettingsItemMappers for dash screen, aborting.");
			InjectedScreen.Slot.Destroy();
		}

		view.Feed.Target = feed;

		Logger.DebugInternal("Dash screen should be injected!");
	}
}
