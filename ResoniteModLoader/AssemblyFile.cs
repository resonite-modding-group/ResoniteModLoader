namespace ResoniteModLoader;

internal sealed class AssemblyFile {
	internal string File { get; }
	internal Assembly Assembly { get; set; }
	internal AssemblyFile(string file, Assembly assembly) {
		File = file;
		Assembly = assembly;
	}
	private string? sha256;
	internal string Sha256 {
		get {
			if (sha256 == null) {
				try {
					sha256 = Util.GenerateSHA256(File);
				} catch (Exception e) {
					Logger.ErrorInternal($"Exception calculating sha256 hash for {File}:\n{e}");
					sha256 = "failed to generate hash";
				}
			}
			return sha256;
		}
	}
}
