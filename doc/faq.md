# Frequently Asked Questions

## Something is broken! Where can I get help?

Please take a look at our [troubleshooting page](troubleshooting.md).

## Do you have a Discord server?

Yes. [Here it is.][Resonite Modding Discord]

## What is a mod?

Mods are .dll files loaded by ResoniteModLoader that change the behavior of your Resonite client in some way. Unlike plugins, mods are specifically designed to work in multiplayer.

## What does ResoniteModLoader do?

ResoniteModLoader is a Resonite Plugin that does a lot of the boilerplate necessary to get mods working in a reasonable way. In summary, it:

1. Initializes earlier than a normal plugin
2. Ensures that Resonite's compatibility check doesn't prevent you from joining other players. For safety reasons this will only work if ResoniteModLoader is the only plugin.
3. Loads mod .dll files and calls their `OnEngineInit()` function so the mods can begin executing

## Is using ResoniteModLoader allowed?

Yes, so long as Resonite's Guidelines are followed.

## Will people know I'm using mods?

- By default, ResoniteModLoader does not do anything identifiable over the network. You will appear to be running the vanilla Resonite version to any component that shows your version strings or compatibility hash.
- If you are running other plugins, they will alter your version strings and compatibility hash.
- ResoniteModLoader logs to the same log file Resonite uses. If you send your logs to anyone, it will be obvious that you are using a plugin. This is intended.
- ResoniteModLoader mods may have effects visible to other users, depending on the mod.
- If you wish to opt-in to using your real version string you can set `advertiseversion=true` in the [Modloader Config](modloader_config.md).
- If ResoniteModLoader breaks due to a bad install or a Resonite update, it will be unable to hide its own existence and your real version string will be shown.

## Are mods safe?

Mods are not sandboxed in any way. In other words, they run with the same level of privilege as Resonite itself. A poorly written mod could cause performance or stability issues. A maliciously designed mod could give a malicious actor a dangerous level of control over your computer. **Make sure you only use mods from sources you trust.**

We'll be setting up a list of mods that have been manually audited to ensure they aren't malicious or evil. While this process isn't 100% foolproof, the mods that will be on this list are significantly more trustworthy than an unvetted DLL.

If you aren't sure if you can trust a mod and you have some level of ability to read code, you can look at its source code. If the source code is unavailable or you suspect it may differ from the contents of the .dll file, you can inspect the mod with a [C# decompiler](https://www.google.com/search?q=c%23+decompiler). Things to be particularly wary of include:

- Obfuscated code
- Sending or receiving data over the internet
- Interacting with the file system (reading, writing, or executing files from disk)

## Where does ResoniteModLoader log to?

The regular Resonite logs: `C:\Program Files (x86)\Steam\steamapps\common\Resonite\Logs`.

If you are experiencing issues, check our [troubleshooting page](troubleshooting.md).


## Is ResoniteModLoader compatible with other mod loaders?

Yes, **however** other mod loaders are likely to come with LibHarmony, and you need to ensure you only have one. Therefore you may need to remove `0Harmony.dll` from your Resonite install directory or your `rml_libs` folder. If another mod loader's LibHarmony version is significantly different from the standard Harmony 2 library, then it will not be compatible with ResoniteModLoader at all.

## Why are you using a custom mod loader for Resonite?

1. Resonite Plugins are the officially supported means to extend functionality of the game, we can expect them to continue working with relatively little change, if any, to the modloader itself even through major engine changes, for example if Resonite ever switches to a non-Unity engine.
2. Some other mod loaders may do unnecessary additional steps to load mods while we already have an intended 'entry point' from the plugin system.
3. Other mod loaders may be designed for specifically Unity games. While Resonite uses Unity, it isn't your typical Unity game.
4. It affords us some extra flexibility to change the modloader directly as we see fit instead of needing an additional layer on another modloader to change what it is doing.

## As a content creator, when is a mod the right solution?

Check out our documentation describing various [Problem Solving Techniques](problem_solving_techniques.md) to determine if a mod may be the solution.

## As a mod developer, why should I use ResoniteModLoader over a Resonite Plugin?

If you are just trying to make a new Component or ProtoFlux node, you should use a plugin. The plugin system is specifically designed for that and is the supported method of extending the functionality of the engine directly.

If you are trying to modify Resonite's existing behavior without adding any new components, ResoniteModLoader offers the following:

- [LibHarmony] is a dependency of ResoniteModLoader, so as a mod developer you don't need to worry about making sure it's installed
- Resonite Plugins normally break multiplayer compatibility. The ResoniteModLoader plugin has been specifically designed to remain compatible. This feature will only work if ResoniteModLoader.dll is the *only* plugin you are using.
- Resonite Plugins can normally execute when Local Home loads at the earliest. ResoniteModLoader begins executing significantly earlier, giving you more room to alter Resonite's behavior before it finishes initializing.
- Steam has a relatively small character limit on launch options, and every Resonite plugin you install pushes you closer to that limit. Having more than a handful plugins will therefore prevent you from using Steam to launch the game, and mods using ResoniteModLoader are unaffected by this issue as you only need one launch option

## Can mods depend on other mods?

Yes. All mod assemblies are loaded before any mod hooks are called, so no special setup is needed if your mod provides public methods.

Mod hooks are called alphabetically by the mod filename, so you can purposefully alter your filename (`0_mod.dll`) to make sure your hooks run first. Please only do this after trying other avenues of resolving mod dependancies.

## Can ResoniteModLoader load Resonite plugins?

No. You need to use `-LoadAssembly <path>` to load plugins. There is important plugin handling code that does not run for ResoniteModLoader mods.

## Are ResoniteModLoader mods plugins?

No. ResoniteModLoader mods will not work if used as a Resonite plugin.

<!--- Link References -->
[LibHarmony]: https://github.com/pardeike/Harmony
[Mod & Plugin Policy]: https://resonite.com/policies/index.html
[Resonite Discord]: https://discord.gg/resonite
[Resonite Modding Discord]: https://discord.gg/ZMRyQ8bryN
