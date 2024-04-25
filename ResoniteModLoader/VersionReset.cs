using System.ComponentModel;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

using Elements.Core;

using FrooxEngine;

using HarmonyLib;

namespace ResoniteModLoader;

internal static class VersionReset {
	// String used when AdvertiseVersion == true
	private const string RESONITE_MOD_LOADER = "ResoniteModLoader.dll";

	internal static void Initialize() {
		ModLoaderConfiguration config = ModLoaderConfiguration.Get();
		Engine engine = Engine.Current;

		// Get the version string before we mess with it
		string originalVersionString = engine.VersionString;

		List<string> extraAssemblies = Engine.ExtraAssemblies;
		string assemblyFilename = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
		bool rmlPresent = extraAssemblies.Remove(assemblyFilename);

		if (!rmlPresent) {
			throw new Exception($"Assertion failed: Engine.ExtraAssemblies did not contain \"{assemblyFilename}\"");
		}

		// Get all Weaved assemblies. This is useful, as plugins will always be Weaved.
		Assembly[] weavedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
			.Where(IsWeaved)
			.ToArray();

		Logger.DebugFuncInternal(() => {
			string potentialPlugins = weavedAssemblies
				.Select(a => Path.GetFileName(a.Location))
				.Join(delimiter: ", ");
			return $"Found {weavedAssemblies.Length} potential plugins: {potentialPlugins}";
		});

		HashSet<Assembly> expectedWeavedAssemblies = GetExpectedWeavedAssemblies();

		// Attempt to map the Weaved assemblies to Resonite's plugin list
		Dictionary<string, Assembly> plugins = new(weavedAssemblies.Length);
		Assembly[] unmatchedAssemblies = weavedAssemblies
			.Where(assembly => {
				string filename = Path.GetFileName(assembly.Location);
				if (extraAssemblies.Contains(filename)) {
					// okay, the assembly's filename is in the plugin list. It's probably a plugin.
					plugins.Add(filename, assembly);
					return false;
				} else {
					// remove certain expected assemblies from the "unmatchedAssemblies" naughty list
					return !expectedWeavedAssemblies.Contains(assembly);
				}
			})
			.ToArray();


		Logger.DebugFuncInternal(() => {
			string actualPlugins = plugins.Keys.Join(delimiter: ", ");
			return $"Found {plugins.Count} actual plugins: {actualPlugins}";
		});

		// Warn about assemblies that couldn't be mapped to plugins
		foreach (Assembly assembly in unmatchedAssemblies) {
			Logger.WarnInternal($"Unexpected Weaved assembly: \"{assembly.Location}\". If this is a plugin, then the plugin-detection code is faulty.");
		}

		// Warn about plugins that couldn't be mapped to assemblies
		HashSet<string> unmatchedPlugins = new(extraAssemblies);
		unmatchedPlugins.ExceptWith(plugins.Keys); // remove all matched plugins
		foreach (string plugin in unmatchedPlugins) {
			Logger.ErrorInternal($"Unmatched plugin: \"{plugin}\". RML could not find the assembly for this plugin, therefore RML cannot properly calculate the compatibility hash.");
		}

		// Flags used later to determine how to spoof
		bool includePluginsInHash = true;

		// If unsafe is true, we should pretend there are no plugins and spoof everything
		if (config.Unsafe) {
			if (!config.AdvertiseVersion) {
				extraAssemblies.Clear();
			}
			includePluginsInHash = false;
			Logger.WarnInternal("Unsafe mode is enabled! Not that you had a warranty, but now it's DOUBLE void!");
		}
		// else if unmatched plugins are present, we should not spoof anything
		else if (unmatchedPlugins.Count != 0) {
			Logger.ErrorInternal("Version spoofing was not performed due to some plugins having missing assemblies.");
			return;
		}
		// else we should spoof normally

		// get plugin assemblies sorted in the same order Resonite sorted them.
		Logger.DebugFuncInternal(() => $"Sorting Plugins");
		List<Assembly> sortedPlugins = extraAssemblies
			.Select(path => plugins[path])
			.ToList();

		if (config.AdvertiseVersion) {
			// put RML back in the version string
			Logger.MsgInternal($"Adding {RESONITE_MOD_LOADER} to version string because you have AdvertiseVersion set to true.");
			extraAssemblies.Insert(0, RESONITE_MOD_LOADER);
		}

		// we intentionally attempt to set the version string first, so if it fails the compatibilty hash is left on the original value
		// this is to prevent the case where a player simply doesn't know their version string is wrong
		if (!SpoofVersionString(engine, originalVersionString)) {
			Logger.WarnInternal("Version string spoofing failed");
			return;
		}

		if (!SpoofCompatibilityHash(engine, sortedPlugins, includePluginsInHash)) {
			Logger.WarnInternal("Compatibility hash spoofing failed");
			return;
		}

		Logger.MsgInternal("Compatibility hash spoofing succeeded");
	}

	private static bool IsWeaved(Assembly assembly) {
		return assembly.Modules // in practice there will only be one module, and it will have the dll's name
			.SelectMany(module => module.GetCustomAttributes<DescriptionAttribute>())
			.Where(IsWeavedAttribute)
			.Any();
	}

	private static bool IsWeavedAttribute(DescriptionAttribute descriptionAttribute) {
		return descriptionAttribute.Description == "FROOXENGINE_WEAVED";
	}

	// Get all the non-plugin Weaved assemblies we expect to exist
	private static HashSet<Assembly> GetExpectedWeavedAssemblies() {
		List<Assembly?> list = new()
		{
			Type.GetType("FrooxEngine.IComponent, FrooxEngine")?.Assembly,
			Type.GetType("ProtoFlux.Nodes.FrooxEngine.ProtoFluxMapper, ProtoFlux.Nodes.FrooxEngine")?.Assembly,
			Type.GetType("ProtoFluxBindings.ProtoFluxMapper, ProtoFluxBindings")?.Assembly,
			Type.GetType("FrooxEngineBootstrap, Assembly-CSharp")?.Assembly,
			Assembly.GetExecutingAssembly(),
		};
		return list
			.Where(assembly => assembly != null)
			.ToHashSet()!;
	}

	private static bool SpoofCompatibilityHash(Engine engine, List<Assembly> plugins, bool includePluginsInHash) {
		string vanillaCompatibilityHash;
		int? vanillaProtocolVersionMaybe = GetVanillaProtocolVersion();
		if (vanillaProtocolVersionMaybe is int vanillaProtocolVersion) {
			Logger.DebugFuncInternal(() => $"Vanilla protocol version is: {vanillaProtocolVersion}");
			vanillaCompatibilityHash = CalculateCompatibilityHash(vanillaProtocolVersion, plugins, includePluginsInHash);
			Logger.DebugFuncInternal(() => $"Target CompatibilityHash version is: {vanillaCompatibilityHash}");
			return SetCompatibilityHash(engine, vanillaCompatibilityHash);
		} else {
			Logger.ErrorInternal("Unable to determine vanilla protocol version");
			return false;
		}
	}

	private static string CalculateCompatibilityHash(int ProtocolVersion, List<Assembly> plugins, bool includePluginsInHash) {
		using (MD5CryptoServiceProvider cryptoServiceProvider = new()) {
			using (ConcatenatedStream inputStream = new()) {
				inputStream.EnqueueStream(new MemoryStream(BitConverter.GetBytes(ProtocolVersion)));
				if (includePluginsInHash) {
					foreach (Assembly plugin in plugins) {
						try {
							Logger.DebugFuncInternal(() => $"Creating hash for {plugin.FullName}");
							FileStream fileStream = File.OpenRead(plugin.Location);
							fileStream.Seek(375L, SeekOrigin.Current);
							inputStream.EnqueueStream(fileStream);
						} catch (Exception ex) {
							Logger.ErrorInternal(ex.ToString());
							throw;
						}
					}
				}
				byte[] hash = cryptoServiceProvider.ComputeHash(inputStream);
				return Convert.ToBase64String(hash);
			}
		}
	}

	private static bool SetCompatibilityHash(Engine engine, string Target) {
		// This is super sketchy and liable to break with new compiler versions.
		// I have a good reason for doing it though... if I just called the setter it would recursively
		// end up calling itself, because I'm HOOKING the CompatibilityHash setter.
		FieldInfo field = AccessTools.DeclaredField(typeof(Engine), $"<{nameof(Engine.CompatibilityHash)}>k__BackingField");

		if (field == null) {
			Logger.WarnInternal("Unable to write Engine.CompatibilityHash");
			return false;
		} else {
			Logger.DebugFuncInternal(() => $"Changing compatibility hash from {engine.CompatibilityHash} to {Target}");
			field.SetValue(engine, Target);
			return true;
		}
	}

	private static bool SpoofVersionString(Engine engine, string originalVersionString) {
		FieldInfo field = AccessTools.DeclaredField(engine.GetType(), "_versionString");
		if (field == null) {
			Logger.WarnInternal("Unable to write Engine._versionString");
			return false;
		}
		// null the cached value
		field.SetValue(engine, null);

		Logger.DebugFuncInternal(() => $"Changing version string from {originalVersionString} to {engine.VersionString}");
		return true;
	}

	// perform incredible bullshit to rip the hardcoded protocol version out of the dang IL
	private static int? GetVanillaProtocolVersion() {
		// raw IL immediately surrounding the number we need to find, which in this example is 12017 (0x2EF1)

		// ldc.i4		0x2EF1
		// call			uint8[] [mscorlib] System.BitConverter::GetBytes(int32)

		// we're going to search for that method call, then grab the operand of the ldc.i4 that precedes it
		MethodInfo targetCallee = AccessTools.DeclaredMethod(typeof(BitConverter), nameof(BitConverter.GetBytes), new Type[] { typeof(int) });
		if (targetCallee == null) {
			Logger.ErrorInternal("Could not find System.BitConverter::GetBytes(System.Int32)");
			return null;
		}

		//Locating 'private async Task InitializeAssemblies(LaunchOptions options)'
		MethodInfo initializeShim = AccessTools.DeclaredMethod(typeof(Engine), "InitializeAssemblies", new Type[] { typeof(LaunchOptions) });
		if (initializeShim == null) {
			Logger.ErrorInternal("Could not find Engine.InitializeAssemblies()");
			return null;
		}

		AsyncStateMachineAttribute asyncAttribute = (AsyncStateMachineAttribute)initializeShim.GetCustomAttribute(typeof(AsyncStateMachineAttribute));
		if (asyncAttribute == null) {
			Logger.ErrorInternal("Could not find AsyncStateMachine for Engine.InitializeAssemblies");
			return null;
		}

		// async methods are weird. Their body is just some setup code that passes execution... elsewhere.
		// The compiler generates a companion type for async methods. This companion type has some ridiculous nondeterministic name, but luckily
		// we can just ask this attribute what the type is. The companion type should have a MoveNext() method that contains the actual IL we need.

		Type asyncStateMachineType = asyncAttribute.StateMachineType;
		MethodInfo initializeImpl = AccessTools.DeclaredMethod(asyncStateMachineType, "MoveNext");
		if (initializeImpl == null) {
			Logger.ErrorInternal("Could not find MoveNext method for Engine.Initialize");
			return null;
		}

		List<CodeInstruction> instructions = PatchProcessor.GetOriginalInstructions(initializeImpl);
		for (int i = 1; i < instructions.Count; i++) {
			if (instructions[i].Calls(targetCallee)) {
				// we're guaranteed to have a previous instruction because we began iteration from 1
				CodeInstruction previous = instructions[i - 1];
				if (OpCodes.Ldc_I4.Equals(previous.opcode)) {
					return (int)previous.operand;
				}
			}
		}

		return null;
	}
}
