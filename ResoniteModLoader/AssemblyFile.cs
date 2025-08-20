namespace ResoniteModLoader;

internal sealed class AssemblyFile {
	internal string File { get; }
	internal Assembly Assembly { get; set; }
	internal AssemblyFile(string file, Assembly assembly) {
		File = file;
		Assembly = assembly;
	}
	internal string Name => Assembly.GetName().Name ?? "Unknown";
	internal string Version => Assembly.GetName().Version?.ToString() ?? "Unknown";
	private string? sha256;
	internal string Sha256 {
		get {
			if (sha256 == null) {
				try {
					sha256 = Util.GenerateSHA256(File);
				} catch (Exception e) {
					Logger.ErrorInternal($"Exception calculating sha256 hash for {File}:\n{e}");
					sha256 = "Failed to generate hash";
				}
			}
			return sha256;
		}
	}
}
