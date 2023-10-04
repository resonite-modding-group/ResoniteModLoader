# Linux Notes

### Note, I have not checked if these specific fixes are still required or if there are any new fixes that are needed.

ResoniteModLoader works on Linux, but in addition to the [normal install steps](../README.md#installation) there are some extra steps you may need to take until the issue is fixed.

The log directory on Linux is `$HOME/.local/share/Steam/steamapps/common/Resonite/Logs`

If your log contains the following, you need to set up a workaround for the issue.

```log
System.IO.DirectoryNotFoundException: Could not find a part of the path "/home/myusername/.local/share/Steam/steamapps/common/Resonite/Resonite_Data\Managed/FrooxEngine.dll".
```

To set up the workaround, run the following commands in your terminal:

```bash
cd "$HOME/.local/share/Steam/steamapps/common/Resonite"
ln -s Resonite_Data/Managed 'Resonite_Data\Managed'
```

## Headless

At the moment, the headless requires steam to be logged in and running to be used which can complicate a standalone headless setup such as in a container. This should be resolved soon when the game is fully released.