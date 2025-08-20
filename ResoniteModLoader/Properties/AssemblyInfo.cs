using System.Runtime.InteropServices;

using Elements.Data;

[assembly: AssemblyTitle("ResoniteModLoader")]
[assembly: AssemblyProduct("ResoniteModLoader")]
[assembly: AssemblyDescription("A modloader for Resonite")]
[assembly: AssemblyCopyright("Copyright © 2025")]
[assembly: AssemblyVersion(ResoniteModLoader.ModLoader.VERSION_CONSTANT)]
[assembly: AssemblyFileVersion(ResoniteModLoader.ModLoader.VERSION_CONSTANT)]

[assembly: ComVisible(false)]

//Mark as DataModelAssembly for the Plugin loading system to load this assembly
[assembly: DataModelAssembly(DataModelAssemblyType.UserspaceCore)]
