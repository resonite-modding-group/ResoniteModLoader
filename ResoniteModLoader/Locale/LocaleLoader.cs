using FrooxEngine;

namespace ResoniteModLoader.Locale;
internal class LocaleLoader {
	internal static void InitLocales() {
		Engine.Current.LocalesUpdated += DelayedLocaleUpdate; //When Vanilla locales are updated on the filesystem
		Settings.RegisterValueChanges<LocaleSettings>(LocaleChanged); //When the User changes locale in settings
	}
	private static void LocaleChanged(LocaleSettings setting) {
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
			var targetRes = assembly.GetManifestResourceNames()
				.FirstOrDefault(r => r.EndsWith($"{targetLocale}.json", StringComparison.Ordinal));
			//Load target locale if not en
			if (!targetLocale.Equals("en")) {
				if (targetRes is not null) {
					using var s = assembly.GetManifestResourceStream(targetRes);
					using var r = new StreamReader(s);
					string localeJson = await r.ReadToEndAsync();
					Userspace.Current.GetCoreLocale()?.Asset?.Data?.LoadDataAdditively(localeJson);
				} else {
					Logger.WarnInternal($"Embedded locale resource for '{targetLocale}' not found. Using fallback.");
				}
			}

			//Always load fallback locale
			var fallbackRes = assembly.GetManifestResourceNames()
				.FirstOrDefault(r => r.EndsWith("en.json", StringComparison.Ordinal));

			if (fallbackRes is not null) {
				using var s = assembly.GetManifestResourceStream(fallbackRes);
				using var r = new StreamReader(s);
				string fallbackLocale = await r.ReadToEndAsync();
				Userspace.Current.GetCoreLocale()?.Asset?.Data?.LoadDataAdditively(fallbackLocale);
			} else {
				Logger.WarnInternal("Embedded locale resource for fallback 'en' not found.");
			}

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
