namespace ResoniteModLoader;

internal sealed class LoadedResoniteMod {
	internal LoadedResoniteMod(ResoniteMod resoniteMod, AssemblyFile modAssembly) {
		ResoniteMod = resoniteMod;
		ModAssembly = modAssembly;
	}

	internal ResoniteMod ResoniteMod { get; private set; }
	internal AssemblyFile ModAssembly { get; private set; }
	internal ModConfiguration? ModConfiguration { get; set; }
	internal bool AllowSavingConfiguration = true;
	internal bool FinishedLoading { get => ResoniteMod.FinishedLoading; set => ResoniteMod.FinishedLoading = value; }
	internal string Name => ResoniteMod.Name;
}
