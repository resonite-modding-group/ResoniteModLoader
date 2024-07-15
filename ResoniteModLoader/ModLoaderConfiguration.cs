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
				{ "unsafe", (value) => _configuration.Unsafe = bool.Parse(value) },
				{ "debug", (value) => _configuration.Debug = bool.Parse(value) },
				{ "hidevisuals", (value) => _configuration.HideVisuals = bool.Parse(value) },
				{ "nomods", (value) => _configuration.NoMods = bool.Parse(value) },
				{ "advertiseversion", (value) => _configuration.AdvertiseVersion = bool.Parse(value) },
				{ "logconflicts", (value) => _configuration.LogConflicts = bool.Parse(value) },
				{ "hidemodtypes", (value) => _configuration.HideModTypes = bool.Parse(value) },
				{ "hidelatetypes", (value) => _configuration.HideLateTypes = bool.Parse(value) },
				{ "nodashscreen", (value) => _configuration.NoDashScreen = bool.Parse(value) },
			};

			// .NET's ConfigurationManager is some hot trash to the point where I'm just done with it.
			// Time to reinvent the wheel. This parses simple key=value style properties from a text file.
			try {
				var lines = File.ReadAllLines(path);
				foreach (var line in lines) {
					int splitIdx = line.IndexOf('=');
					if (splitIdx != -1) {
						string key = line.Substring(0, splitIdx);
						string value = line.Substring(splitIdx + 1);

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
		string codeBase = Assembly.GetExecutingAssembly().CodeBase;
		UriBuilder uri = new(codeBase);
		string path = Uri.UnescapeDataString(uri.Path);
		return Path.GetDirectoryName(path);
	}

#pragma warning disable CA1805
	public bool Unsafe { get; private set; } = false;
	public bool Debug { get; private set; } = false;
	public bool NoMods { get; private set; } = false;
	public bool HideVisuals { get; private set; } = false;
	public bool AdvertiseVersion { get; private set; } = false;
	public bool LogConflicts { get; private set; } = true;
	public bool HideModTypes { get; private set; } = true;
	public bool HideLateTypes { get; private set; } = true;
	public bool NoDashScreen { get; private set; } = false;
#pragma warning restore CA1805
}
