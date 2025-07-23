using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ResoniteModLoader;

internal static class DebugInfo {
	internal static void Log() {
		Logger.MsgInternal($"ResoniteModLoader v{ModLoader.VERSION} starting up!{(ModLoaderConfiguration.Get().Debug ? " Debug logs enabled via config." : "")}");
		Logger.DebugFuncInternal(() => $"Launched with args: {string.Join(" ", Environment.GetCommandLineArgs())}");
		Logger.MsgInternal($"CLR v{Environment.Version}");
		Logger.DebugFuncInternal(() => $"Using .NET Framework: \"{AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName}\"");
		Logger.DebugFuncInternal(() => $"Using .NET Core: \"{Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName}\"");
		Logger.MsgInternal($".NET Runtime: {RuntimeInformation.FrameworkDescription}");
		Logger.MsgInternal($"Using Harmony v{GetAssemblyVersion(typeof(HarmonyLib.Harmony))}");
		Logger.MsgInternal($"Using Elements.Core v{GetAssemblyVersion(typeof(Elements.Core.floatQ))}");
		Logger.MsgInternal($"Using FrooxEngine v{GetAssemblyVersion(typeof(FrooxEngine.IComponent))}");
		Logger.MsgInternal($"Using Json.NET v{GetAssemblyVersion(typeof(Newtonsoft.Json.JsonSerializer))}");
	}

	private static string? GetAssemblyVersion(Type typeFromAssembly) {
		return typeFromAssembly.Assembly.GetName()?.Version?.ToString();
	}
}
