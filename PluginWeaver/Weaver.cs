static class Weaver {
	internal static void Process(string path, string root) {
		FrooxEngine.Weaver.AssemblyPostProcessor.Process(path, root);
	}
}
