namespace ResoniteModLoader;

internal sealed class ModLoaderConfiguration {
	private const string CONFIG_FILENAME = "ResoniteModLoader.config";

	private static ModLoaderConfiguration? _configuration;

	internal static ModLoaderConfiguration Get() {
		if (_configuration == null) {
			// the config file can just sit next to the dll. Simple.
			string path = GetConfigPath();
			_configuration = new ModLoaderConfiguration();

			Dictionary<string, Action<string>> keyActions = new() {
				{ "debug", (value) => _configuration.Debug = bool.Parse(value) },
				{ "hidevisuals", (value) => _configuration.HideVisuals = bool.Parse(value) },
				{ "nomods", (value) => _configuration.NoMods = bool.Parse(value) },
				{ "advertiseversion", (value) => _configuration.AdvertiseVersion = bool.Parse(value) },
				{ "logconflicts", (value) => _configuration.LogConflicts = bool.Parse(value) },
			};

			// .NET's ConfigurationManager is some hot trash to the point where I'm just done with it.
			// Time to reinvent the wheel. This parses simple key=value style properties from a text file.
			try {
				var lines = File.ReadAllLines(path);
				foreach (var line in lines) {
					int splitIdx = line.IndexOf('=');
					if (splitIdx != -1) {
						string key = line[..splitIdx];
						string value = line[(splitIdx + 1)..];

						if (keyActions.TryGetValue(key, out Action<string>? action)) {
							try {
								action(value);
							} catch (Exception) {
								Logger.WarnInternal($"Unable to parse value: '{value}' for config key: '{key}', this key will be ignored");
							}
						}
					}
				}
			} catch (Exception e) {
				if (e is FileNotFoundException) {
					Logger.MsgInternal($"No modloader config found at {path}, using defaults. This is probably fine.");
				} else if (e is DirectoryNotFoundException || e is IOException || e is UnauthorizedAccessException) {
					Logger.WarnInternal(e.ToString());
				} else {
					throw;
				}
			}
		}
		return _configuration;
	}

	private static string GetConfigPath() {
		var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if (dir == null) {
			return CONFIG_FILENAME;
		}
		return Path.Combine(dir, CONFIG_FILENAME);
	}

	/// <summary>
	/// When enabled, messages logged with <see cref="ResoniteMod.Debug(object)"/> or internal Debug methods will be added to the log file.
	/// Additonally, mods may enable their own debug functionality via <see cref="ResoniteMod.IsDebugEnabled()"/> that isn't limited to logging.
	/// </summary>
	public bool Debug { get; internal set; }

	/// <summary>
	/// No mods will be loaded if this option is set.
	/// </summary>
	public bool NoMods { get; internal set; }

	/// <summary>
	/// No loading progress subphase names will be set if this option is active.
	/// </summary>
	public bool HideVisuals { get; internal set; }

	/// <summary>
	/// Currently this option does nothing. Previously, this would enable showing the unmodified version string.
	/// </summary>
	public bool AdvertiseVersion { get; internal set; }

	/// <summary>
	/// Whether to log warnings if one method is patched by multiple mods. Enabled by default.
	/// </summary>
	public bool LogConflicts { get; internal set; } = true;
}
