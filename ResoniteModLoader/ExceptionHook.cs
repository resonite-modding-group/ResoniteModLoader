using FrooxEngine;
using HarmonyLib;

namespace ResoniteModLoader;

internal sealed class ExceptionHook {
	internal static void RegisterExceptionHook(Harmony harmony) {
		MethodInfo? preprocessExceptionMethod = typeof(DebugManager).GetMethod(nameof(DebugManager.PreprocessException), BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(typeof(Exception));
		MethodInfo? exceptionHookMethod = typeof(ExceptionHook).GetMethod(nameof(InterceptException), BindingFlags.Static);

		if (preprocessExceptionMethod == null || exceptionHookMethod == null) {
			Logger.ErrorInternal("Could not find DebugManager.PreprocessException. Mod exceptions may not be logged.");
			return;
		}

		harmony.Patch(preprocessExceptionMethod, prefix: new HarmonyMethod(exceptionHookMethod));
		Logger.DebugInternal("DebugManager.PreprocessException patched");
	}

	internal static void InterceptException(Exception exception) {
		Logger.ProcessException(exception);
	}

}
