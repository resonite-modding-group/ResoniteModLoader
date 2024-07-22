using System.Diagnostics;

using Elements.Core;

namespace ResoniteModLoader;

public sealed class Logger {
	// logged for null objects
	internal const string NULL_STRING = "null";

	public enum LogType { TRACE, DEBUG, INFO, WARN, ERROR }

	public readonly struct LogMessage {
		public LogMessage(DateTime time, ResoniteModBase? mod, LogType level, string message, StackTrace trace) {
			Time = time;
			Mod = mod;
			Level = level;
			Message = message;
			Trace = trace;
		}

		public DateTime Time { get; }
		public ResoniteModBase? Mod { get; }
		public LogType Level { get; }
		public string Message { get; }
		public StackTrace Trace { get; }

		public override string ToString() => $"({Mod?.Name ?? "ResoniteModLoader"} @ {Time}) {LogTypeTag(Level)} {Message}";
	}

	public readonly struct LogException {
		public LogException(DateTime time, Assembly? assembly, Exception exception) {
			Time = time;
			Assembly = assembly;
			Exception = exception;
		}

		public DateTime Time { get; }
		public Assembly? Assembly { get; }
		public Exception Exception { get; }

		public override string ToString() => $"({Time}) [{Assembly?.FullName} ?? Unknown assembly] {Exception.Message}\n{Exception.StackTrace}";
	}

	private static List<LogMessage> _logBuffer = new();

	public static IReadOnlyList<LogMessage> Logs => _logBuffer.AsReadOnly();

	private static List<LogException> _exceptionBuffer = new();

	public static IReadOnlyList<LogException> Exceptions => _exceptionBuffer.AsReadOnly();

	internal static bool IsDebugEnabled() {
		return ModLoaderConfiguration.Get().Debug;
	}

	internal static void TraceFuncInternal(Func<string> messageProducer) {
		if (IsDebugEnabled()) {
			LogInternal(LogType.TRACE, messageProducer(), null, true);
		}
	}

	internal static void TraceFuncExternal(Func<object> messageProducer) {
		if (IsDebugEnabled()) {
			LogInternal(LogType.TRACE, messageProducer(), new(1), true);
		}
	}

	internal static void TraceInternal(string message) {
		if (IsDebugEnabled()) {
			LogInternal(LogType.TRACE, message, null, true);
		}
	}

	internal static void TraceExternal(object message) {
		if (IsDebugEnabled()) {
			LogInternal(LogType.TRACE, message, new(1), true);
		}
	}

	internal static void TraceListExternal(object[] messages) {
		if (IsDebugEnabled()) {
			LogListInternal(LogType.TRACE, messages, new(1), true);
		}
	}

	internal static void DebugFuncInternal(Func<string> messageProducer) {
		if (IsDebugEnabled()) {
			LogInternal(LogType.DEBUG, messageProducer());
		}
	}

	internal static void DebugFuncExternal(Func<object> messageProducer) {
		if (IsDebugEnabled()) {
			LogInternal(LogType.DEBUG, messageProducer(), new(1));
		}
	}

	internal static void DebugInternal(string message) {
		if (IsDebugEnabled()) {
			LogInternal(LogType.DEBUG, message);
		}
	}

	internal static void DebugExternal(object message) {
		if (IsDebugEnabled()) {
			LogInternal(LogType.DEBUG, message, new(1));
		}
	}

	internal static void DebugListExternal(object[] messages) {
		if (IsDebugEnabled()) {
			LogListInternal(LogType.DEBUG, messages, new(1));
		}
	}

	internal static void MsgInternal(string message) => LogInternal(LogType.INFO, message);
	internal static void MsgExternal(object message) => LogInternal(LogType.INFO, message, new(1));
	internal static void MsgListExternal(object[] messages) => LogListInternal(LogType.INFO, messages, new(1));
	internal static void WarnInternal(string message) => LogInternal(LogType.WARN, message);
	internal static void WarnExternal(object message) => LogInternal(LogType.WARN, message, new(1));
	internal static void WarnListExternal(object[] messages) => LogListInternal(LogType.WARN, messages, new(1));
	internal static void ErrorInternal(string message) => LogInternal(LogType.ERROR, message);
	internal static void ErrorExternal(object message) => LogInternal(LogType.ERROR, message, new(1));
	internal static void ErrorListExternal(object[] messages) => LogListInternal(LogType.ERROR, messages, new(1));

	private static void LogInternal(LogType logType, object message, StackTrace? stackTrace = null, bool includeTrace = false) {
		message ??= NULL_STRING;
		stackTrace = stackTrace ?? new(1);
		ResoniteMod? source = Util.ExecutingMod(stackTrace);
		string logTypePrefix = LogTypeTag(logType);
		_logBuffer.Add(new LogMessage(DateTime.Now, source, logType, message.ToString(), stackTrace));
		if (source == null) {
			UniLog.Log($"{logTypePrefix}[ResoniteModLoader] {message}", includeTrace);
		}
		else {
			UniLog.Log($"{logTypePrefix}[ResoniteModLoader/{source.Name}] {message}", includeTrace);
		}
	}

	private static void LogListInternal(LogType logType, object[] messages, StackTrace? stackTrace, bool includeTrace = false) {
		if (messages == null) {
			LogInternal(logType, NULL_STRING, stackTrace, includeTrace);
		}
		else {
			foreach (object element in messages) {
				LogInternal(logType, element.ToString(), stackTrace, includeTrace);
			}
		}
	}

	private static string LogTypeTag(LogType logType) => $"[{Enum.GetName(typeof(LogType), logType)}]";
}

public static class LoggerExtensions {
	public static IEnumerable<Logger.LogMessage> Logs(this ResoniteModBase mod) => Logger.Logs.Where((line) => line.Mod == mod);

	public static IEnumerable<Logger.LogException> Exceptions(this ResoniteModBase mod) => Logger.Exceptions.Where((line) => line.Assembly == mod.ModAssembly!.Assembly);
}
