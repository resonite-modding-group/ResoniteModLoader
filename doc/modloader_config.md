# Modloader Configuration

ResoniteModLoader aims to have a reasonable default configuration, but certain things can be adjusted via an optional config file. The config file isn't generated automatically, but you can create it manually by adding a `ResoniteModLoader.config` file in the same directory as `ResoniteModLoader.dll`. `ResoniteModLoader.config` is a simple text file that supports keys and values in the following format:

```ini
debug=true
nomods=false
```

All config keys are optional to include, missing keys will use the defaults outlined below:

| Configuration      | Default | Description |
| ------------------ | ------- | ----------- |
| `debug`            | `false` | If `true`, ResoniteMod.Debug() logs will appear in your log file. Otherwise, they are hidden. |
| `hidevisuals`      | `false` | If `true`, RML won't show the LoadProgressIndicator on the splash screen. |
| `nomods`           | `false` | If `true`, mods will not be loaded from `rml_mods`. |
| `advertiseversion` | `false` | If `false`, your version will be spoofed and will resemble `2024.4.25.1389`. If `true`, your version will be left unaltered and will resemble `2024.4.25.1389+ResoniteModLoader.dll`. This version string is visible to other players under certain circumstances. |
| `unsafe`           | `false` | If `true`, the version spoofing safety check is disabled and it will still work even if you have other Resonite plugins. DO NOT load plugin components in multiplayer sessions, as it will break things and cause crashes. Plugin components should only be used in your local home or userspace. |
| `logconflicts`     | `true`  | If `false`, conflict logging will be disabled. If `true`, potential mod conflicts will be logged. If `debug` is also `true` this will be more verbose. |
| `hidemodtypes`     | `true`  | If `true`, mod-related types will be hidden in-game. If `false`, no types will be hidden, which makes RML detectable in-game. |
| `hidelatetypes`    | `true`  | If `true` and `hidemodtypes` is `true`, late loaded types will be hidden in-game. If `false`, late loaded types will be shown |