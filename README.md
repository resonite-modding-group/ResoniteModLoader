<p align="center">
	<img width="256" height="256" alt="RML_Logomark_256" src="https://github.com/user-attachments/assets/abfadf82-1f7a-45fb-a1f7-9128db06896d" />
</p>
<h1 align="center">ResoniteModLoader</h1>

A mod loader for [Resonite](https://resonite.com/) utilizing the built-in plugin system. Consider joining our community on [Discord][Resonite Modding Discord] for support, updates, and more.

## Installation

1. Download [ResoniteModLoader.dll](https://github.com/resonite-modding-group/ResoniteModLoader/releases/latest/download/ResoniteModLoader.dll) to Resonite's `Libraries` folder (`C:\Program Files (x86)\Steam\steamapps\common\Resonite\Libraries`). You may need to create this folder if it's missing. 
2. Place [0Harmony.dll](https://github.com/resonite-modding-group/ResoniteModLoader/releases/latest/download/0Harmony.dll) into a `rml_libs` folder under your Resonite install directory (`C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_libs`). You will need to create this folder.
3. Add the following to Resonite's [launch options](https://github.com/resonite-modding-group/ResoniteModLoader/wiki/Launch-Options): `-LoadAssembly Libraries/ResoniteModLoader.dll`. If you put `ResoniteModLoader.dll` somewhere else you will need to change the path.
4. Optionally, add mod DLL files to a `rml_mods` folder under your Resonite install directory (`C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods`). You can create this folder yourself or launch Resonite once with ResoniteModLoader installed and it will be created automatically.
5. Start the game. If you want to verify that ResoniteModLoader is working you can check the Resonite logs. (`C:\Program Files (x86)\Steam\steamapps\common\Resonite\Logs`). The modloader adds some very obvious logs on startup, and if they're missing something has gone wrong. Here is an [example log file](https://github.com/resonite-modding-group/ResoniteModLoader/wiki/Example-Log) where everything worked correctly.

If ResoniteModLoader isn't working after following these steps, take a look at our [troubleshooting page](doc/troubleshooting.md).

### Example Directory Structure

Your Resonite directory should now include the following folder structure (rml_config and rml_mods will be created automatically on first launch). Files not related to modding are not shown.

```
<Resonite Install Directory>
│   Resonite.exe
├───/Libraries
|       ResoniteModLoader.dll
├───/Logs
│       <Log files will generate here>
├───/rml_config
│       <Mod configs are created here>
├───/rml_libs
│       0Harmony.dll
│       <Addition libraries for mods here>
└───/rml_mods
        <Add mods here>
```
## Finding Mods

For an easy way to find and manage mods, check out [Resolute](https://github.com/Gawdl3y/Resolute). It simplifies the installation and updating for verified mods from the [mod manifest](https://github.com/resonite-modding-group/resonite-mod-manifest).

New mods and updates also are posted in [our Discord][Resonite Modding Discord].

## Frequently Asked Questions

Many questions about what RML is and how it works are answered on our [frequently asked questions page](doc/faq.md).

## Making a Mod

Check out the [Mod Creation Guide](https://github.com/resonite-modding-group/ResoniteModLoader/wiki/Creating-Mods).

## Configuration

ResoniteModLoader aims to have a reasonable default configuration, but certain things can be adjusted via an [optional config file](https://github.com/resonite-modding-group/ResoniteModLoader/wiki/Modloader-Config).

## Contributing

Issues and PRs are welcome. Please read our [Contributing Guidelines](.github/CONTRIBUTING.md)!

## Licensing and Credits

ResoniteModLoader is licensed under the GNU Lesser General Public License (LGPL). See [LICENSE.txt](LICENSE.txt) for the full license.

Third-party libraries distributed alongside ResoniteModLoader:

- [LibHarmony] ([MIT License](https://github.com/pardeike/Harmony/blob/v2.2.2.0/LICENSE))

Third-party libraries used in source:

- [.NET](https://github.com/dotnet) (Various licenses)
- [Resonite](https://resonite.com/) ([EULA](https://resonite.com/policies/EULA.html))
- [Json.NET](https://github.com/JamesNK/Newtonsoft.Json) ([MIT License](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md))

<!--- Link References -->
[LibHarmony]: https://github.com/pardeike/Harmony
[Resonite Modding Discord]: https://discord.gg/ZMRyQ8bryN
