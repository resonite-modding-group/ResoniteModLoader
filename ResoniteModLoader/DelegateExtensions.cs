namespace ResoniteModLoader;

internal static class DelegateExtensions {
	internal static void SafeInvoke(this Delegate del, params object[] args) {
		var exceptions = new List<Exception>();

		foreach (var handler in del.GetInvocationList()) {
			try {
				handler.Method.Invoke(handler.Target, args);
			} catch (Exception ex) {
				exceptions.Add(ex);
			}
		}

		if (exceptions.Count != 0) {
			throw new AggregateException(exceptions);
		}
	}
}
