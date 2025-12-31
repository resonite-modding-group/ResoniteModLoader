using System.Globalization;
using FrooxEngine;
using LocaleResource = Elements.Assets.LocaleResource;
using Stream = System.IO.Stream;

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
		var res = Userspace.Current.GetCoreLocale()?.Asset?.Data;
		if (res == null) {
			Logger.WarnInternal("Userspace core locale asset is not loaded.");
			return;
		}

		CultureInfo? targetLocale = null;
		try {
			var targetLocaleString = Settings.GetActiveSetting<LocaleSettings>()!.ActiveLocaleCode;
			if (string.IsNullOrWhiteSpace(targetLocaleString)) {
				Logger.WarnInternal("Locale code empty or null when reading from LocaleSettings");
				return;
			}
			targetLocale = new CultureInfo(targetLocaleString);
			Logger.MsgInternal($"Updating locale to '{targetLocale.Name}'");
		}
		catch (Exception) {
			Logger.ErrorInternal("Could not find ActiveLocaleCode from LocaleSettings");
		}

		// Has the locale been modified?
		bool success = false;

		Logger.DebugInternal($"Before apply: {Userspace.Current.GetCoreLocale()?.Asset?.Data.MessageCount} Keys");
		try {
			await LoadLocaleData(res, targetLocale, Assembly.GetExecutingAssembly(), typeof(LocaleLoader).Namespace!, "ResoniteModLoader");
			success = true;
		}
		catch (Exception ex) {
			Logger.ErrorInternal($"Failed to update locale for ResoniteModLoader: {ex}");
		}

		foreach (var mod in ModLoader.Mods()) {
			var modNs = mod.GetType().Namespace;
			if (!mod.FinishedLoading || modNs == null) continue;
			try {
				await LoadLocaleData(res, targetLocale, mod.ModAssembly!.Assembly, modNs + ".Locale", "mod " + mod.Name);
				success = true;
			}
			catch (Exception ex) {
				Logger.ErrorInternal($"Failed to update locale for mod {mod.Name}: {ex}");
			}
		}

		if (!success)
			return;

		Logger.DebugInternal($"After apply: {Userspace.Current.GetCoreLocale()?.Asset?.Data.MessageCount} Keys");

		try {
			ReloadCurrentLocale();
		}
		catch (Exception ex) {
			Logger.ErrorInternal($"Failed to update locale: {ex}");
		}
	}

	private static async Task LoadLocaleData(LocaleResource res, CultureInfo? locale, Assembly assembly, string originNamespace, string originName) {
		if (locale != null && locale.Name != "en-US") {
			// Try getting locale data for "LANG-COUNTRY" and fall back to "LANG"
			// Example "en-GB": Try "en-gb.json" and fall back to "en.json"
			if (!await LoadLocaleDataResource(res, assembly,
					$"{originNamespace}.{locale.Name.ToLowerInvariant()}.json",
					$"{originNamespace}.{locale.TwoLetterISOLanguageName}.json")
			) {
				Logger.WarnInternal($"Embedded locale resource for '{locale.Name}' not found for {originName}. Using fallback.");
			}
		}

		//Always load fallback locale
		if (!await LoadLocaleDataResource(res, assembly, $"{originNamespace}.en.json")) {
			Logger.WarnInternal($"Embedded locale resource for fallback 'en' not found for {originName}.");
		}
	}

	/// <summary>
	///	Loads a locale file from the given <paramref name="assembly"/>, trying
	/// the different resource names in order, using the first one that exists.
	/// </summary>
	/// <param name="res">Locale to apply the locale data to.</param>
	/// <param name="assembly">Assembly to load the resource from.</param>
	/// <param name="candidates">Resource names to try.</param>
	/// <returns>Whether the resource was found and loaded.</returns>
	private static async Task<bool> LoadLocaleDataResource(LocaleResource res, Assembly assembly, params string[] candidates) {
		Stream? foundStream = null;
		foreach (var c in candidates) {
			foundStream = assembly.GetManifestResourceStream(c);
			if (foundStream != null) break;
		}
		if (foundStream == null) {
			return false;
		}
		using var stream = foundStream;
		using var reader = new StreamReader(stream);
		string localeJson = await reader.ReadToEndAsync();
		res.LoadDataAdditively(localeJson);
		return true;
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
