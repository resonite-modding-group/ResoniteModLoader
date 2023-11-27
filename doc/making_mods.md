# Mod Creation Guide

If you have some level of familiarity with C#, getting started making mods should not be too difficult.

## Basic Visual Studio setup

1. Make a new .NET library against .NET version 4.7.2.
2. Add ResoniteModLoader.dll as a reference and optionally HarmonyLib (0harmony.dll)
3. Add references to Resonite libraries as needed (`C:\Program Files (x86)\Steam\steamapps\common\Resonite\Resonite_Data\Managed`)
4. Remove the reference to `System.Net.Http` if it was added automatically as it will make the compiler angry

## Decompilers

You'll likely want to grab a decompiler if you don't have one already to take a look at existing code. Here are a few popular options:

[DnSpyEx](https://github.com/dnSpyEx/dnSpy),
[dotPeek](https://www.jetbrains.com/decompiler/),
[ILSpy](https://github.com/icsharpcode/ILSpy)

## Hooks

### `OnEngineInit()`

Called once per mod during FrooxEngine initialization. This is where you will set up and apply any harmony patches or setup anything your mod will need.

Happens **before** `OnEngineInit()`

- Load Locales
- Configs
- Plugin initialization

Happens **after** `OnEngineInit()`

- Input/Head device setup
- Local DB initialization
- Networking initialization
- Audio initialization
- SkyFrost Interface
- RunPostInit
- Worlds loading, including Local home and Userspace


### RunPostInit

Add something to be run after init can be done with `Engine.Current.RunPostInit` added in your `OnEngineInit()`, here are 2 examples.

```csharp
Engine.Current.RunPostInit(FunctionToCall);
```
OR
```csharp
Engine.Current.RunPostInit(() => {
    //Code to call after Initialization
    FunctionToCall();
    AnotherFunctionToCall();
});
```

## Mod Configuration

ResoniteModLoader provides a built-in configuration system that can be used to persist configuration values for mods. More information is available in the [configuration system documentation](config.md).

## Example Mod

```csharp
using HarmonyLib; // HarmonyLib comes included with a ResoniteModLoader install
using ResoniteModLoader;
using System;
using System.Reflection;

namespace ExampleMod;

public class ExampleMod : ResoniteMod {
    public override string Name => "ExampleMod";
    public override string Author => "YourNameHere";
    public override string Version => "1.0.0"; //Version of the mod, should match the AssemblyVersion
    public override string Link => "https://github.com/YourNameHere/ExampleMod"; // Optional link to a repo where this mod would be located

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> enabled = new ModConfigurationKey<bool>("enabled", "Should the mod be enabled", () => true); //Optional config settings

    private static ModConfiguration Config;//If you use config settings, this will be where you interface with them

    public override void OnEngineInit() {
        Config = GetConfiguration(); //Get this mods' current ModConfiguration
        Config.Save(true); //If you'd like to save the default config values to file
        Harmony harmony = new Harmony("com.example.ExampleMod"); //typically a reverse domain name is used here (https://en.wikipedia.org/wiki/Reverse_domain_name_notation)
        harmony.PatchAll(); // do whatever LibHarmony patching you need, this will patch all [HarmonyPatch()] instances

        //Various log methods provided by the mod loader, below is an example of how they will look
        //3:14:42 AM.069 ( -1 FPS)  [INFO] [ResoniteModLoader/ExampleMod] a regular log
        Debug("a debug log");
        Msg("a regular log");
        Warn("a warn log");
        Error("an error log");
    }

    //Example of how a HarmonyPatch can be formatted, Note that the following isn't a real patch and will not compile.
    [HarmonyPatch(typeof(ClassName), "MethodName")]
    class ClassName_MethodName_Patch {
        //Postfix() here will be automatically applied as a PostFix Patch
        static void Postfix(ClassName __instance) {
            if(!Config.GetValue(enabled)) {//Use Config.GetValue() to use the ModConfigurationKey defined earlier
                return; //In this example if the mod is not enabled, we'll just return before doing anything
            }
            //Do stuff after everything in the original MethodName has run.
        }
    }
}

```

## Additional Resources

- [Quick C# Refresher](https://learnxinyminutes.com/docs/csharp/)
- [LibHarmony Documentation](https://harmony.pardeike.net/)
- [Unity API Documentation](https://docs.unity3d.com/ScriptReference/index.html)
