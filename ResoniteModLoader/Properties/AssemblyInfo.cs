using System.ComponentModel;
using System.Runtime.InteropServices;
using Elements.Core;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ResoniteModLoader")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("ResoniteModLoader")]
[assembly: AssemblyCopyright("Copyright © 2023")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("d4627c7f-8091-477a-abdc-f1465d94d8d9")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(ResoniteModLoader.ModLoader.VERSION_CONSTANT)]
[assembly: AssemblyFileVersion(ResoniteModLoader.ModLoader.VERSION_CONSTANT)]

// Mark this assembly as part of the data model so that FrooxEngine knows to load it properly
[assembly: DataModelAssembly(false)]

// Prevent FrooxEngine.Weaver from modifying this assembly, as it doesn't need anything done to it
// This keeps Weaver from overwriting AssemblyVersionAttribute
[module: Description("FROOXENGINE_WEAVED")]

