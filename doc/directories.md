# Resonite Directories

If you've installed to a non-default location then finding the path is up to you.

| Directory | Description |
| --------- |------------ |
| Resonite Install Directory | Contains the game install itself, the `Logs` directory, and the `Libraries` directory |
| Log Directory | A `Logs` directory within the Resonite Install Directory. Contains the main game logs. |
| Libraries Directory | A `Libraries` directory within the  Resonite Install Directory. Plugins dlls go here.
| Data Directory | Contains the local db, Unity's player.log, and local assets directory. Location can be changed with `-DataPath <path>` argument. |
| Temporary Directory | Contains crash logs and the cache |
| Cache Directory | Contains cached remote assets. Located inside the Temporary Directory by default. Location can be changed with `-CachePath <path>` argument. |

## Windows

| Description | Typical Path |
| ----------- | ------------ |
| Resonite Install Directory (Steam) | `C:\Program Files (x86)\Steam\steamapps\common\Resonite` |
| Data Directory | `%userprofile%\AppData\LocalLow\Yellow Dog Man Studios\Resonite` |
| Temporary Directory | `%temp%\Yellow Dog Man Studios\Resonite` |
| Cache Directory | `%temp%\Yellow Dog Man Studios\Resonite\Cache` |

## Linux Native-

| Description | Typical Path |
| ----------- | ------------ |
| Resonite Install Directory (Steam) | `$HOME/.local/share/Steam/steamapps/common/Resonite` |
| Data Directory | `$HOME/.config/unity3d/Yellow Dog Man Studios/Resonite` |
| Temporary Directory | `/tmp/Yellow Dog Man Studios/Resonite` |
| Cache Directory | `/tmp/Yellow Dog Man Studios/Resonite/Cache` |

## Linux Proton/WINE

| Description | Typical Path |
| ----------- | ------------ |
| Resonite Install Directory (Steam) | `$HOME/.local/share/Steam/steamapps/common/Resonite` |
| Data Directory | `$HOME/.local/share/Steam/steamapps/compatdata/2519830/pfx/drive_c/users/steamuser/AppData/LocalLow/Yellow Dog Man Studios/Resonite` |
| Temporary Directory | `$HOME/.local/share/Steam/steamapps/compatdata/2519830/pfx/drive_c/users/steamuser/Temp/Yellow Dog Man Studios/Resonite` |
| Cache Directory | `$HOME/.local/share/Steam/steamapps/compatdata/2519830/pfx/drive_c/users/steamuser/Temp/Yellow Dog Man Studios/Resonite/Cache` |

## Drive Notes

- The actual Resonite install should be less than 1GB, but the log files can in certain cases be very large.
- The cache can get very large, upwards of 30GB so make sure the drive you save cache to has plenty of space. Resonite will benefit by having this on a faster drive such as a SSD. The cache directory can be deleted whenever you need without breaking Resonite. The cache directory can be changed with the `-CachePath <file path>` launch option.
- The data directory contains your localDB as well as locally saved assets. This can get to be around 10GB, or more if you store a lot in your local home. The data directory can be changed with the `-DataPath <file path>` launch option. Deleting this will:
  - Reset any non-cloud-synced Resonite settings. This will, for example, send you back to the tutorial (unless you use `-SkipIntroTutorial`)
  - Reset your local home and nuke anything that was stored in it.
  - Regenerate your machine ID
