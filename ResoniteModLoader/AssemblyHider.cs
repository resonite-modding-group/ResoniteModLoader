using System.Diagnostics;

using Elements.Core;
using FrooxEngine;

using HarmonyLib;

namespace ResoniteModLoader;

internal static class AssemblyHider {
	/// <summary>
	/// Companies that indicate an assembly is part of .NET
	/// This list was found by debug logging the AssemblyCompany
	/// for all loaded assemblies.
	/// </summary>
	private static HashSet<string> knownDotNetCompanies = new List<string>() {
		"Mono development team", // used by .NET stuff and Mono.Security
	}.Select(company => company.ToLower()).ToHashSet();

	/// <summary>
	/// Products that indicate an assembly is part of .NET.
	/// This list was found by debug logging the AssemblyProductAttribute for all loaded assemblies.
	/// </summary>
	private static HashSet<string> knownDotNetProducts = new List<string>() {
		"Microsoft® .NET", // used by a few System.* assemblies
		"Microsoft® .NET Framework", // used by most of the System.* assemblies
		"Mono Common Language Infrastructure", // used by mscorlib stuff
	}.Select(product => product.ToLower()).ToHashSet();

	/// <summary>
	/// Assemblies that were already loaded when RML started up, minus a couple known non-assemblies.
	/// </summary>
	private static HashSet<Assembly>? resoniteAssemblies;

	/// <summary>
	/// Assemblies that 100% exist due to a mod
	/// </summary>
	private static HashSet<Assembly>? modAssemblies;

	/// <summary>
	/// .NET assembiles we want to ignore in some cases, like the callee check for the AppDomain.GetAssemblies() patch
	/// </summary>
	private static HashSet<Assembly>? dotNetAssemblies;

	/// <summary>
	/// Patch Resonite's type lookup code to not see mod-related types. This is needed, because users can pass
	/// arbitrary strings to TypeHelper.FindType(), which can be used to detect if someone is running mods.
	/// </summary>
	/// <param name="harmony">Our RML harmony instance</param>
	/// <param name="initialAssemblies">Assemblies that were loaded when RML first started</param>
	internal static void PatchResonite(Harmony harmony, HashSet<Assembly> initialAssemblies) {
		if (ModLoaderConfiguration.Get().HideModTypes) {
			// initialize the static assembly sets that our patches will need later
			resoniteAssemblies = GetResoniteAssemblies(initialAssemblies);
			modAssemblies = GetModAssemblies(resoniteAssemblies);
			dotNetAssemblies = resoniteAssemblies.Where(LooksLikeDotNetAssembly).ToHashSet();

			// TypeHelper.FindType explicitly does a type search
			MethodInfo findTypeTarget = AccessTools.DeclaredMethod(typeof(TypeHelper), nameof(TypeHelper.FindType), new Type[] { typeof(string) });
			MethodInfo findTypePatch = AccessTools.DeclaredMethod(typeof(AssemblyHider), nameof(FindTypePostfix));
			harmony.Patch(findTypeTarget, postfix: new HarmonyMethod(findTypePatch));

			// WorkerManager.IsValidGenericType checks a type for validity, and if it returns `true` it reveals that the type exists
			MethodInfo isValidGenericTypeTarget = AccessTools.DeclaredMethod(typeof(WorkerManager), nameof(WorkerManager.IsValidGenericType), new Type[] { typeof(Type), typeof(bool) });
			MethodInfo isValidGenericTypePatch = AccessTools.DeclaredMethod(typeof(AssemblyHider), nameof(IsValidTypePostfix));
			harmony.Patch(isValidGenericTypeTarget, postfix: new HarmonyMethod(isValidGenericTypePatch));

			// WorkerManager.GetType uses FindType, but upon failure fails back to doing a (strangely) exhausitive reflection-based search for the type
			MethodInfo getTypeTarget = AccessTools.DeclaredMethod(typeof(WorkerManager), nameof(WorkerManager.GetType), new Type[] { typeof(string) });
			MethodInfo getTypePatch = AccessTools.DeclaredMethod(typeof(AssemblyHider), nameof(FindTypePostfix));
			harmony.Patch(getTypeTarget, postfix: new HarmonyMethod(getTypePatch));

			// FrooxEngine likes to enumerate all types in all assemblies, which is prone to issues (such as crashing FrooxCode if a type isn't loadable)
			MethodInfo getAssembliesTarget = AccessTools.DeclaredMethod(typeof(AppDomain), nameof(AppDomain.GetAssemblies), Array.Empty<Type>());
			MethodInfo getAssembliesPatch = AccessTools.DeclaredMethod(typeof(AssemblyHider), nameof(GetAssembliesPostfix));
			harmony.Patch(getAssembliesTarget, postfix: new HarmonyMethod(getAssembliesPatch));
		}
	}

	private static HashSet<Assembly> GetResoniteAssemblies(HashSet<Assembly> initialAssemblies) {
		// Remove RML itself, as its types should be hidden but it's guaranteed to be loaded.
		initialAssemblies.Remove(Assembly.GetExecutingAssembly());

		// Remove Harmony, as users who aren't using rml_libs will already have it loaded.
		initialAssemblies.Remove(typeof(Harmony).Assembly);

		return initialAssemblies;
	}

	private static HashSet<Assembly> GetModAssemblies(HashSet<Assembly> resoniteAssemblies) {
		// start with ALL assemblies
		HashSet<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().ToHashSet();

		// remove assemblies that we know to have come with Resonite
		assemblies.ExceptWith(resoniteAssemblies);

		// what's left are assemblies that magically appeared during the mod loading process. So mods and their dependencies.
		return assemblies;
	}

	/// <summary>
	/// Checks if an <see cref="Assembly"/> belongs to a mod or not.
	/// </summary>
	/// <param name="assembly">The <see cref="Assembly"/> to check.</param>
	/// <param name="typeOrAssembly">Type of root check being performed. Should be "type" or  "assembly". Used in logging.</param>
	/// <param name="name">Name of the root check being performed. Used in logging.</param>
	/// <param name="log">If `true`, this will emit logs. If `false`, this function will not log.</param>
	/// <param name="forceShowLate">If `true`, then this function will always return `false` for late-loaded types</param>
	/// <returns>`true` if this assembly belongs to a mod.</returns>
	private static bool IsModAssembly(Assembly assembly, string typeOrAssembly, string name, bool log, bool forceShowLate) {
		if (resoniteAssemblies!.Contains(assembly)) {
			// the type belongs to a Resonite assembly
			return false; // don't hide the thing
		} else {
			if (modAssemblies!.Contains(assembly)) {
				// known type from a mod assembly
				if (log) {
					Logger.DebugFuncInternal(() => $"Hid {typeOrAssembly} \"{name}\" from Resonite");
				}
				return true; // hide the thing
			} else {
				// an assembly was in neither resoniteAssemblies nor modAssemblies
				// this implies someone late-loaded an assembly after RML, and it was later used in-game
				// this is super weird, and probably shouldn't ever happen... but if it does, I want to know about it.
				// since this is an edge case users may want to handle in different ways, the HideLateTypes rml config option allows them to choose.
				bool hideLate = ModLoaderConfiguration.Get().HideLateTypes;
				if (log) {
					Logger.WarnInternal($"The \"{name}\" {typeOrAssembly} does not appear to part of Resonite or a mod. It is unclear whether it should be hidden or not. Due to the HideLateTypes config option being {hideLate} it will be {(hideLate ? "Hidden" : "Shown")}");
				}
				// if forceShowLate == true, then this function will always return `false` for late-loaded types
				// if forceShowLate == false, then this function will return `true` when hideLate == true
				return hideLate && !forceShowLate;
			}
		}
	}

	/// <summary>
	/// Checks if an <see cref="Assembly"/> belongs to a mod or not.
	/// </summary>
	/// <param name="assembly">The <see cref="Assembly"/> to check</param>
	/// <param name="forceShowLate">If <c>true</c>, then this function will always return <c>false</c> for late-loaded types.</param>
	/// <returns><c>true</c> if this <see cref="Assembly"/> belongs to a mod.</returns>
	private static bool IsModAssembly(Assembly assembly, bool forceShowLate = false) {
		// this generates a lot of logspam, as a single call to AppDomain.GetAssemblies() calls this many times
		return IsModAssembly(assembly, "assembly", assembly.ToString(), log: false, forceShowLate);
	}

	/// <summary>
	/// Checks if a <see cref="Type"/> belongs to a mod or not.
	/// </summary>
	/// <param name="type">The <see cref="Type"/> to check.</param>
	/// <returns><c>true</c> if this <see cref="Type"/> belongs to a mod.</returns>
	private static bool IsModType(Type type) {
		return IsModAssembly(type.Assembly, "type", type.ToString(), log: true, forceShowLate: false);
	}

	// postfix for a method that searches for a type, and returns a reference to it if found (TypeHelper.FindType and WorkerManager.GetType)
	private static void FindTypePostfix(ref Type? __result) {
		if (__result != null) {
			// we only need to think about types if the method actually returned a non-null result
			if (IsModType(__result)) {
				__result = null;
			}
		}
	}

	// postfix for a method that validates a type (WorkerManager.IsValidGenericType)
	private static void IsValidTypePostfix(ref bool __result, Type type) {
		if (__result == true) {
			// we only need to think about types if the method actually returned a true result
			if (IsModType(type)) {
				__result = false;
			}
		}
	}

	private static void GetAssembliesPostfix(ref Assembly[] __result) {
		Assembly? callingAssembly = GetCallingAssembly(new(1));
		if (callingAssembly != null && resoniteAssemblies!.Contains(callingAssembly)) {
			// if we're being called by Resonite code, then hide mod assemblies
			Logger.DebugFuncInternal(() => $"Intercepting call to AppDomain.GetAssemblies() from {callingAssembly}");
			__result = __result
				.Where(assembly => !IsModAssembly(assembly, forceShowLate: true)) // it turns out Resonite itself late-loads a bunch of stuff, so we force-show late-loaded assemblies here
				.ToArray();
		}
	}

	/// <summary>
	/// Get the calling <see cref="Assembly"/> using stack trace analysis, ignoring .NET assemblies.
	/// This implementation is SPECIFICALLY for the <see cref="AppDomain.GetAssemblies"/> patch and may not be valid for other use-cases.
	/// </summary>
	/// <param name="stackTrace">The stack trace captured by the callee.</param>
	/// <returns>The calling <see cref="Assembly"/>, or null if none was found.</returns>
	private static Assembly? GetCallingAssembly(StackTrace stackTrace) {
		for (int i = 0; i < stackTrace.FrameCount; i++) {
			Assembly? assembly = stackTrace.GetFrame(i)?.GetMethod()?.DeclaringType?.Assembly;
			// .NET calls AppDomain.GetAssemblies() a bunch internally, and we don't want to intercept those calls UNLESS they originated from Resonite code.
			if (assembly != null && !dotNetAssemblies!.Contains(assembly)) {
				return assembly;
			}
		}
		return null;
	}

	private static bool LooksLikeDotNetAssembly(Assembly assembly) {
		// check the assembly's company
		string? company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
		if (company != null && knownDotNetCompanies.Contains(company.ToLower())) {
			return true;
		}

		// check the assembly's product
		string? product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
		if (product != null && knownDotNetProducts.Contains(product.ToLower())) {
			return true;
		}

		// nothing matched, this is probably not part of .NET
		return false;
	}
}
