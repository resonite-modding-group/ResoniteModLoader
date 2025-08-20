namespace ResoniteModLoader;

internal sealed class ModLoaderConfiguration {
	private const string CONFIG_FILENAME = "ResoniteModLoader.config";

	private static ModLoaderConfiguration? _configuration;

	internal static ModLoaderConfiguration Get() {
		if (_configuration == null) {
			// the config file can just sit next to the dll. Simple.
			string path = Path.Combine(GetAssemblyDirectory(), CONFIG_FILENAME);
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

						if (keyActions.TryGetValue(key, out Action<string> action)) {
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

	private static string GetAssemblyDirectory() {
		string path = Assembly.GetExecutingAssembly().Location;
		return Path.GetDirectoryName(path);
	}

	public bool Debug { get; internal set; }
	public bool NoMods { get; internal set; }
	public bool HideVisuals { get; internal set; }
	public bool AdvertiseVersion { get; internal set; }
	public bool LogConflicts { get; internal set; } = true;
}
