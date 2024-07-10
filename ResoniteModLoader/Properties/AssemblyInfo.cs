using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Elements.Core;

[assembly: AssemblyTitle("ResoniteModLoader")]
[assembly: AssemblyProduct("ResoniteModLoader")]
[assembly: AssemblyDescription("A modloader for Resonite")]
[assembly: AssemblyCopyright("Copyright © 2024")]
[assembly: AssemblyVersion(ResoniteModLoader.ModLoader.VERSION_CONSTANT)]
[assembly: AssemblyFileVersion(ResoniteModLoader.ModLoader.VERSION_CONSTANT)]

[assembly: ComVisible(false)]

// Prevent FrooxEngine.Weaver from modifying this assembly, as it doesn't need anything done to it
// This keeps Weaver from overwriting AssemblyVersionAttribute
[module: Description("FROOXENGINE_WEAVED")]

//Mark as DataModelAssembly for the Plugin loading system to load this assembly
[assembly: DataModelAssembly(false)]
