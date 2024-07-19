using System.Diagnostics;

using Elements.Core;

namespace ResoniteModLoader;

internal sealed class Logger {
	// logged for null objects
	internal const string NULL_STRING = "null";

	internal enum LogType { DEBUG, INFO, WARN, ERROR }

	internal static readonly List<(ResoniteModBase?, LogType, string, StackTrace)> LogBuffer = new();

	internal static bool IsDebugEnabled() {
		return ModLoaderConfiguration.Get().Debug;
	}

	internal static void DebugFuncInternal(Func<string> messageProducer) {
		if (IsDebugEnabled()) {
			LogInternal(LogType.DEBUG, messageProducer());
		}
	}

	internal static void DebugFuncExternal(Func<object> messageProducer) {
		if (IsDebugEnabled()) {
			LogInternal(LogType.DEBUG, messageProducer(), SourceFromStackTrace(new(1)));
		}
	}

	internal static void DebugInternal(string message) {
		if (IsDebugEnabled()) {
			LogInternal(LogType.DEBUG, message);
		}
	}

	internal static void DebugExternal(object message) {
		if (IsDebugEnabled()) {
			LogInternal(LogType.DEBUG, message, SourceFromStackTrace(new(1)));
		}
	}

	internal static void DebugListExternal(object[] messages) {
		if (IsDebugEnabled()) {
			LogListInternal(LogType.DEBUG, messages, SourceFromStackTrace(new(1)));
		}
	}

	internal static void MsgInternal(string message) => LogInternal(LogType.INFO, message);
	internal static void MsgExternal(object message) => LogInternal(LogType.INFO, message, SourceFromStackTrace(new(1)));
	internal static void MsgListExternal(object[] messages) => LogListInternal(LogType.INFO, messages, SourceFromStackTrace(new(1)));
	internal static void WarnInternal(string message) => LogInternal(LogType.WARN, message);
	internal static void WarnExternal(object message) => LogInternal(LogType.WARN, message, SourceFromStackTrace(new(1)));
	internal static void WarnListExternal(object[] messages) => LogListInternal(LogType.WARN, messages, SourceFromStackTrace(new(1)));
	internal static void ErrorInternal(string message) => LogInternal(LogType.ERROR, message);
	internal static void ErrorExternal(object message) => LogInternal(LogType.ERROR, message, SourceFromStackTrace(new(1)));
	internal static void ErrorListExternal(object[] messages) => LogListInternal(LogType.ERROR, messages, SourceFromStackTrace(new(1)));

	private static void LogInternal(LogType logType, object message, string? source = null) {
		message ??= NULL_STRING;
		string logTypePrefix = LogTypeTag(logType);
		if (source == null) {
			UniLog.Log($"{logTypePrefix}[ResoniteModLoader] {message}");
		}
		else {
			UniLog.Log($"{logTypePrefix}[ResoniteModLoader/{source}] {message}");
		}
	}

	private static void LogListInternal(LogType logType, object[] messages, string? source) {
		if (messages == null) {
			LogInternal(logType, NULL_STRING, source);
		}
		else {
			foreach (object element in messages) {
				LogInternal(logType, element.ToString(), source);
			}
		}
	}

	private static string? SourceFromStackTrace(StackTrace stackTrace) {
		// MsgExternal() and Msg() are above us in the stack
		return Util.ExecutingMod(stackTrace)?.Name;
	}

	private static string LogTypeTag(LogType logType) => $"[{Enum.GetName(typeof(LogType), logType)}]";
}
