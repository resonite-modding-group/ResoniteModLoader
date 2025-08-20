using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ResoniteModLoader;

internal static class DebugInfo {
	internal static void Log() {
		Logger.MsgInternal($"ResoniteModLoader v{ModLoader.VERSION} starting up!");
		Logger.DebugInternal("Debug logs enabled via config.");
		Logger.DebugFuncInternal(() => $"Launched with args: {string.Join(" ", Environment.GetCommandLineArgs())}");
		Logger.DebugInternal($"Using \"{Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName}\"");
		Logger.MsgInternal($".NET Runtime: {RuntimeInformation.FrameworkDescription} on {RuntimeInformation.RuntimeIdentifier}");
		Logger.MsgInternal($"Using Harmony v{GetAssemblyVersion(typeof(HarmonyLib.Harmony))}");
		Logger.MsgInternal($"Using Json.NET v{GetAssemblyVersion(typeof(Newtonsoft.Json.JsonSerializer))}");
	}

	private static string? GetAssemblyVersion(Type typeFromAssembly) {
		return typeFromAssembly.Assembly.GetName()?.Version?.ToString();
	}
}
