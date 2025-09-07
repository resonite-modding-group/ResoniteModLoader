using FrooxEngine;

namespace ResoniteModLoader.Locale;
internal class LocaleLoader {
	internal static async Task InitLocales() {
		Engine.Current.LocalesUpdated += DelayedLocaleUpdate; //When Vanilla locales are updated on the filesystem
		Settings.RegisterValueChanges<LocaleSettings>(LocaleChanged); //When the User changes locale in settings
	}
	private static void LocaleChanged(LocaleSettings setting) {
		Logger.DebugInternal($"Locale setting changed to: {setting.ActiveLocaleCode}");
		DelayedLocaleUpdate();
	}

	private static void DelayedLocaleUpdate() {
		Userspace.UserspaceWorld?.RunInUpdates(15, () => UpdateLocale());
	}

	internal static async void UpdateLocale() {
		string targetLocale = Settings.GetActiveSetting<LocaleSettings>()!.ActiveLocaleCode;

		if (string.IsNullOrWhiteSpace(targetLocale)) {
			Logger.WarnInternal("Locale code empty or null"); 
			return;
		}
		Logger.DebugInternal($"Updating locale to {targetLocale}");
		Logger.DebugInternal($"Before apply: {Userspace.Current.GetCoreLocale()?.Asset?.Data.MessageCount}");
		try {
			if (targetLocale != "en") {
				await LoadLocaleResource(targetLocale);
			}
			await LoadLocaleResource("en");
			ReloadCurrentLocale();
		} catch (Exception ex) {
			Logger.ErrorInternal($"Failed to update locale: {ex}");
		}
	}

	private static async Task LoadLocaleResource(string localeCode) {
		var assembly = Assembly.GetExecutingAssembly();
		var resName = assembly.GetManifestResourceNames().FirstOrDefault(r => r.EndsWith($"{localeCode}.json", StringComparison.Ordinal));
		if (resName == null) {
			Logger.WarnInternal($"Locale resource for {localeCode} not found.");
			return;
		}

		await using var stream = assembly.GetManifestResourceStream(resName);
		using var reader = new StreamReader(stream);
		string localeJson = await reader.ReadToEndAsync();

		Logger.DebugInternal($"Applying embedded locale: {resName}");
		Userspace.Current.GetCoreLocale()?.Asset?.Data?.LoadDataAdditively(localeJson);
		Logger.DebugInternal($"After apply {localeCode}: {Userspace.Current.GetCoreLocale()?.Asset?.Data.MessageCount}");
	}

	internal static void ReloadCurrentLocale() {
		var localeProvider = Userspace.UserspaceWorld.GetCoreLocale();

		// force asset update for locale provider
		if (localeProvider?.Asset?.Data != null && localeProvider.OverrideLocale.Value != "-") {
			var lastOverrideLocale = localeProvider.OverrideLocale.Value;
			localeProvider.OverrideLocale.Value = "-";
			Userspace.UserspaceWorld.RunInUpdates(1, () => {
				localeProvider.OverrideLocale.Value = lastOverrideLocale;
			});
		}
	}
}
