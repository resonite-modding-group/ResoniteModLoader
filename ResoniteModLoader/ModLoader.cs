using HarmonyLib;

namespace ResoniteModLoader;

/// <summary>
/// Contains the actual mod loader.
/// </summary>
public class ModLoader {
	internal const string VERSION_CONSTANT = "2.5.2";
	/// <summary>
	/// ResoniteModLoader's version
	/// </summary>
	public static readonly string VERSION = VERSION_CONSTANT;
	private static readonly Type RESONITE_MOD_TYPE = typeof(ResoniteMod);
	private static readonly List<LoadedResoniteMod> LoadedMods = new(); // used for mod enumeration
	internal static readonly Dictionary<Assembly, ResoniteMod> AssemblyLookupMap = new(); // used for logging
	private static readonly Dictionary<string, LoadedResoniteMod> ModNameLookupMap = new(); // used for duplicate mod checking
	/// <summary>
	/// True if ResoniteModLoader is being loaded by a headless server
	/// </summary>
	public static bool IsHeadless // Extremely thorough, but doesn't rely on any specific class to check for headless presence
    {
        get
        {
            return _isHeadless ??= AppDomain.CurrentDomain.GetAssemblies().Any(a =>
            {
                IEnumerable<Type> types;
                try
                {
                    types = a.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }
                return types.Any(t => t != null && t.Namespace == "FrooxEngine.Headless");
            });
        }
    }

    private static bool? _isHeadless;

	/// <summary>
	/// Allows reading metadata for all loaded mods
	/// </summary>
	/// <returns>A new list containing each loaded mod</returns>
	public static IEnumerable<ResoniteModBase> Mods() {
		return LoadedMods
			.Select(m => (ResoniteModBase)m.ResoniteMod)
			.ToList();
	}

	internal static void LoadMods(Harmony harmony) {
		ModLoaderConfiguration config = ModLoaderConfiguration.Get();
		if (config.NoMods) {
			Logger.DebugInternal("Mods will not be loaded due to configuration file");
			return;
		}
		LoadProgressIndicator.SetCustom("Gathering mods");
		// generate list of assemblies to load
		AssemblyFile[] modsToLoad;
		if (AssemblyLoader.LoadAssembliesFromDir("rml_mods") is AssemblyFile[] arr) {
			modsToLoad = arr;
		} else {
			return;
		}

		ModConfiguration.EnsureDirectoryExists();

		// Call InitializeMod() for each mod
		foreach (AssemblyFile mod in modsToLoad) {
			try {
				LoadedResoniteMod? loaded = InitializeMod(mod);
				if (loaded != null) {
					// if loading succeeded, then we need to register the mod
					RegisterMod(loaded);
				}
			} catch (ReflectionTypeLoadException reflectionTypeLoadException) {
				// this exception type has some inner exceptions we must also log to gain any insight into what went wrong
				StringBuilder sb = new();
				sb.AppendLine(reflectionTypeLoadException.ToString());
				foreach (Exception loaderException in reflectionTypeLoadException.LoaderExceptions) {
					sb.AppendLine($"Loader Exception: {loaderException.Message}");
					if (loaderException is FileNotFoundException fileNotFoundException) {
						if (!string.IsNullOrEmpty(fileNotFoundException.FusionLog)) {
							sb.Append("    Fusion Log:\n    ");
							sb.AppendLine(fileNotFoundException.FusionLog);
						}
					}
				}
				Logger.ErrorInternal($"ReflectionTypeLoadException initializing mod from {mod.File}:\n{sb}");
			} catch (Exception e) {
				Logger.ErrorInternal($"Unexpected exception initializing mod from {mod.File}:\n{e}");
			}
		}

		ModConfiguration.RegisterShutdownHook(harmony);

		foreach (LoadedResoniteMod mod in LoadedMods) {
			try {
				HookMod(mod);
			} catch (Exception e) {
				Logger.ErrorInternal($"Unexpected exception in OnEngineInit() for mod {mod.ResoniteMod.Name} from {mod.ModAssembly.File}:\n{e}");
			}
		}

		// Log potential conflicts
		if (config.LogConflicts) {
			LoadProgressIndicator.SetCustom("Looking for conflicts");
			IEnumerable<MethodBase> patchedMethods = Harmony.GetAllPatchedMethods();
			foreach (MethodBase patchedMethod in patchedMethods) {
				Patches patches = Harmony.GetPatchInfo(patchedMethod);
				HashSet<string> owners = new(patches.Owners);
				if (owners.Count > 1) {
					Logger.WarnInternal($"Method \"{patchedMethod.FullDescription()}\" has been patched by the following:");
					foreach (string owner in owners) {
						Logger.WarnInternal($"    \"{owner}\" ({TypesForOwner(patches, owner)})");
					}
				} else if (config.Debug) {
					string owner = owners.FirstOrDefault();
					Logger.DebugFuncInternal(() => $"Method \"{patchedMethod.FullDescription()}\" has been patched by \"{owner}\"");
				}
			}
		}
	}

	/// <summary>
	/// We have a bunch of maps and things the mod needs to be registered in. This method does all that jazz.
	/// </summary>
	/// <param name="mod">The successfully loaded mod to register</param>
	private static void RegisterMod(LoadedResoniteMod mod) {
		try {
			ModNameLookupMap.Add(mod.ResoniteMod.Name, mod);
		} catch (ArgumentException) {
			LoadedResoniteMod existing = ModNameLookupMap[mod.ResoniteMod.Name];
			Logger.ErrorInternal($"{mod.ModAssembly.File} declares duplicate mod {mod.ResoniteMod.Name} already declared in {existing.ModAssembly.File}. The new mod will be ignored.");
			return;
		}

		LoadedMods.Add(mod);
		AssemblyLookupMap.Add(mod.ModAssembly.Assembly, mod.ResoniteMod);
		mod.ResoniteMod.loadedResoniteMod = mod; // complete the circular reference (used to look up config)
		mod.FinishedLoading = true; // used to signal that the mod is truly loaded
	}

	private static string TypesForOwner(Patches patches, string owner) {
		bool ownerEquals(Patch patch) => Equals(patch.owner, owner);
		int prefixCount = patches.Prefixes.Where(ownerEquals).Count();
		int postfixCount = patches.Postfixes.Where(ownerEquals).Count();
		int transpilerCount = patches.Transpilers.Where(ownerEquals).Count();
		int finalizerCount = patches.Finalizers.Where(ownerEquals).Count();
		return $"prefix={prefixCount}; postfix={postfixCount}; transpiler={transpilerCount}; finalizer={finalizerCount}";
	}

	// Loads mod class and mod config
	private static LoadedResoniteMod? InitializeMod(AssemblyFile mod) {
		if (mod.Assembly == null) {
			return null;
		}

		Type[] modClasses = mod.Assembly.GetLoadableTypes(t => t.IsClass && !t.IsAbstract && RESONITE_MOD_TYPE.IsAssignableFrom(t)).ToArray();
		if (modClasses.Length == 0) {
			Logger.ErrorInternal($"No loadable mod found in {mod.File}");
			return null;
		} else if (modClasses.Length != 1) {
			Logger.ErrorInternal($"More than one mod found in {mod.File}. File will not be loaded.");
			return null;
		} else {
			Type modClass = modClasses[0];
			ResoniteMod? resoniteMod = null;
			try {
				resoniteMod = (ResoniteMod)AccessTools.CreateInstance(modClass);
			} catch (Exception e) {
				Logger.ErrorInternal($"Error instantiating mod {modClass.FullName} from {mod.File}:\n{e}");
				return null;
			}
			if (resoniteMod == null) {
				Logger.ErrorInternal($"Unexpected null instantiating mod {modClass.FullName} from {mod.File}");
				return null;
			}

			LoadProgressIndicator.SetCustom($"Loading configuration for [{resoniteMod.Name}/{resoniteMod.Version}]");
			LoadedResoniteMod loadedMod = new(resoniteMod, mod);
			Logger.MsgInternal($"Loaded mod [{resoniteMod.Name}/{resoniteMod.Version}] ({Path.GetFileName(mod.File)}) by {resoniteMod.Author} with Sha256: {mod.Sha256}");
			loadedMod.ModConfiguration = ModConfiguration.LoadConfigForMod(loadedMod);
			return loadedMod;
		}
	}

	private static void HookMod(LoadedResoniteMod mod) {
		LoadProgressIndicator.SetCustom($"Starting mod [{mod.ResoniteMod.Name}/{mod.ResoniteMod.Version}]");
		Logger.DebugFuncInternal(() => $"calling OnEngineInit() for [{mod.ResoniteMod.Name}/{mod.ResoniteMod.Version}]");
		try {
			mod.ResoniteMod.OnEngineInit();
		} catch (Exception e) {
			Logger.ErrorInternal($"Mod {mod.ResoniteMod.Name} from {mod.ModAssembly.File} threw error from OnEngineInit():\n{e}");
		}
	}
}
