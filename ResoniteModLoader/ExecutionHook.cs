using Elements.Core;
using FrooxEngine;

namespace ResoniteModLoader;

[ImplementableClass(true)]
internal class ExecutionHook {
#pragma warning disable CS0169, IDE0051, CA1823
	// fields must exist due to reflective access
	private static Type? __connectorType;
	private static Type? __connectorTypes;

	// implementation not strictly required, but method must exist due to reflective access
	private static DummyConnector InstantiateConnector() {
		return new DummyConnector();
	}
#pragma warning restore CS0169, IDE0051, CA1823

	static ExecutionHook() {
		Logger.MsgInternal("AM I LOADED???");
		try {
			BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
			var byName = (Dictionary<string, AssemblyTypeRegistry>)typeof(GlobalTypeRegistry).GetField("_byName", flags).GetValue(null);

			var firstAsm = byName.FirstOrDefault(asm => asm.Value.Assembly == typeof(ExecutionHook).Assembly);

			if (firstAsm.Value != null && byName.ContainsKey(firstAsm.Key)) {
				byName.Remove(firstAsm.Key);
				Logger.MsgInternal("Found my assembly in the global type registry, removing to retain compatibility. :) <3");
			}

			HashSet<Assembly> initialAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToHashSet();
			LoadProgressIndicator.SetCustom("Loading Libraries");
			AssemblyFile[] loadedAssemblies = AssemblyLoader.LoadAssembliesFromDir("rml_libs");
			// note that harmony may not be loaded until this point, so this class cannot directly import HarmonyLib.

			if (loadedAssemblies.Length != 0) {
				string loadedAssemblyList = string.Join("\n", loadedAssemblies.Select(a => a.Assembly.FullName + " Sha256=" + a.Sha256));
				Logger.MsgInternal($"Loaded libraries from rml_libs:\n{loadedAssemblyList}");
			}
			LoadProgressIndicator.SetCustom("Initializing");
			DebugInfo.Log();
			HarmonyWorker.LoadModsAndHideModAssemblies(initialAssemblies);
			LoadProgressIndicator.SetCustom("Loaded");
		} catch (Exception e) {
			// it's important that this doesn't send exceptions back to Resonite
			Logger.ErrorInternal($"Exception in execution hook!\n{e}");
		}
	}


	// type must match return type of InstantiateConnector()
	private sealed class DummyConnector : IConnector {
		public IImplementable? Owner { get; private set; }
		public void ApplyChanges() { }
		public void AssignOwner(IImplementable owner) => Owner = owner;
		public void Destroy(bool destroyingWorld) { }
		public void Initialize() { }
		public void RemoveOwner() => Owner = null;
	}
}
