using FrooxEngine;

namespace ResoniteModLoader.Locale;
internal class LocaleLoader {
	internal static async Task InitLocales() {
		Engine.Current.LocalesUpdated += DelayedLocaleUpdate; //When Vanilla locales are updated on the filesystem
		Settings.RegisterValueChanges<LocaleSettings>(LocaleChanged); //When the User changes locale in settings
	}
	private static void LocaleChanged(LocaleSettings setting) {
		Logger.DebugInternal($"Locale setting changed to '{setting.ActiveLocaleCode}'");
		DelayedLocaleUpdate();
	}

	private static void DelayedLocaleUpdate() {
		Userspace.UserspaceWorld?.RunInUpdates(15, () => UpdateLocale());
	}

	internal static async void UpdateLocale() {
		string? targetLocale = null;
		try {
			targetLocale = Settings.GetActiveSetting<LocaleSettings>()!.ActiveLocaleCode;
			if (string.IsNullOrWhiteSpace(targetLocale)) {
				Logger.WarnInternal("Locale code empty or null when reading from LocaleSettings"); 
				return;
			}
			Logger.MsgInternal($"Updating locale to '{targetLocale}'");
		} catch (Exception) {
			Logger.ErrorInternal("Could not find ActiveLocaleCode from LocaleSettings");
		}

		Logger.DebugInternal($"Before apply: {Userspace.Current.GetCoreLocale()?.Asset?.Data.MessageCount} Keys");
		try {
			var assembly = Assembly.GetExecutingAssembly();
			var resName = assembly.GetManifestResourceNames().FirstOrDefault(r => r.EndsWith($"{targetLocale}.json", StringComparison.Ordinal));
			if (resName == null) {
				Logger.WarnInternal($"Embeded locale resource for '{targetLocale}' not found. falling back to 'en'");
				//For now, just load english if the target locale isn't found
				resName = assembly.GetManifestResourceNames().FirstOrDefault(r => r.EndsWith($"en.json", StringComparison.Ordinal));
			}

			using var stream = assembly.GetManifestResourceStream(resName);
			using var reader = new StreamReader(stream);
			string localeJson = await reader.ReadToEndAsync();

			Userspace.Current.GetCoreLocale()?.Asset?.Data?.LoadDataAdditively(localeJson);
			Logger.DebugInternal($"After apply: {Userspace.Current.GetCoreLocale()?.Asset?.Data.MessageCount} Keys");

			ReloadCurrentLocale();
		} catch (Exception ex) {
			Logger.ErrorInternal($"Failed to update locale: {ex}");
		}
	}

	internal static void ReloadCurrentLocale() {
		var localeProvider = Userspace.UserspaceWorld.GetCoreLocale();
		if (localeProvider?.Asset?.Data != null) {
			var lastOverrideLocale = "";
			if (localeProvider.OverrideLocale.Value != "-") {
				lastOverrideLocale = localeProvider.OverrideLocale.Value;
				localeProvider.OverrideLocale.Value = "-";
				Userspace.UserspaceWorld.RunInUpdates(1, () => {
					localeProvider.OverrideLocale.Value = lastOverrideLocale;
				});
			}
		}
	}




}
