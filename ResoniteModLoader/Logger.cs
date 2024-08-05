using System.Collections;
using System.Diagnostics;

using Elements.Core;

namespace ResoniteModLoader;

public sealed class Logger {
	public enum LogLevel { TRACE, DEBUG, INFO, WARN, ERROR }

	public readonly struct LogMessage {
		internal LogMessage(ResoniteModBase? mod, LogLevel level, string message, StackTrace? trace = null) {
			Time = DateTime.Now;
			Mod = mod;
			Level = level;
			Message = message;
			Trace = trace;
		}

		public DateTime Time { get; }
		public ResoniteModBase? Mod { get; }
		public LogLevel Level { get; }
		public string Message { get; }
		public StackTrace? Trace { get; }

		public override string ToString() => $"({Mod?.Name ?? "ResoniteModLoader"} @ {Time}) {LogTypeTag(Level)} {Message}";
	}

	public readonly struct LogException {
		internal LogException(Exception exception) {
			Time = DateTime.Now;
			Exception = exception;
		}

		internal LogException(Exception exception, Assembly? assembly) {
			Time = DateTime.Now;
			Exception = exception;
			Source = (assembly, null);
		}

		internal LogException(Exception exception, ResoniteModBase? mod) {
			Time = DateTime.Now;
			Exception = exception;
			Source = (mod?.ModAssembly?.Assembly, mod);
		}

		internal LogException(Exception exception, Assembly? assembly, ResoniteModBase? mod) {
			Time = DateTime.Now;
			Exception = exception;
			Source = (assembly, mod);
		}

		public DateTime Time { get; }
		public Exception Exception { get; }
		public (Assembly? Assembly, ResoniteModBase? Mod)? Source { get; }

		public override string ToString() => $"({Time}) [{Source?.Assembly?.FullName} ?? Unknown assembly] {Exception.Message}\n{Exception.StackTrace}";
	}

	// logged for null objects
	internal const string NULL_STRING = "null";

	private static List<LogMessage> _logBuffer = new();

	public static IReadOnlyList<LogMessage> Logs => _logBuffer.AsReadOnly();

	private static List<LogException> _exceptionBuffer = new();

	public static IReadOnlyList<LogException> Exceptions => _exceptionBuffer.AsReadOnly();

	internal static bool IsDebugEnabled() {
		return ModLoaderConfiguration.Get().Debug;
	}

	internal static void TraceFuncInternal(Func<string> messageProducer) {
		if (IsDebugEnabled()) {
			LogInternal(LogLevel.TRACE, messageProducer(), null, true);
		}
	}

	internal static void TraceFuncExternal(Func<object> messageProducer) {
		if (IsDebugEnabled()) {
			LogInternal(LogLevel.TRACE, messageProducer(), new(1), true);
		}
	}

	internal static void TraceInternal(string message) {
		if (IsDebugEnabled()) {
			LogInternal(LogLevel.TRACE, message, null, true);
		}
	}

	internal static void TraceExternal(object message) {
		if (IsDebugEnabled()) {
			LogInternal(LogLevel.TRACE, message, new(1), true);
		}
	}

	internal static void TraceListExternal(object[] messages) {
		if (IsDebugEnabled()) {
			LogListInternal(LogLevel.TRACE, messages, new(1), true);
		}
	}

	internal static void DebugFuncInternal(Func<string> messageProducer) {
		if (IsDebugEnabled()) {
			LogInternal(LogLevel.DEBUG, messageProducer());
		}
	}

	internal static void DebugFuncExternal(Func<object> messageProducer) {
		if (IsDebugEnabled()) {
			LogInternal(LogLevel.DEBUG, messageProducer(), new(1));
		}
	}

	internal static void DebugInternal(string message) {
		if (IsDebugEnabled()) {
			LogInternal(LogLevel.DEBUG, message);
		}
	}

	internal static void DebugExternal(object message) {
		if (IsDebugEnabled()) {
			LogInternal(LogLevel.DEBUG, message, new(1));
		}
	}

	internal static void DebugListExternal(object[] messages) {
		if (IsDebugEnabled()) {
			LogListInternal(LogLevel.DEBUG, messages, new(1));
		}
	}

	internal static void MsgInternal(string message) => LogInternal(LogLevel.INFO, message);
	internal static void MsgExternal(object message) => LogInternal(LogLevel.INFO, message, new(1));
	internal static void MsgListExternal(object[] messages) => LogListInternal(LogLevel.INFO, messages, new(1));
	internal static void WarnInternal(string message) => LogInternal(LogLevel.WARN, message);
	internal static void WarnExternal(object message) => LogInternal(LogLevel.WARN, message, new(1));
	internal static void WarnListExternal(object[] messages) => LogListInternal(LogLevel.WARN, messages, new(1));
	internal static void ErrorInternal(string message) => LogInternal(LogLevel.ERROR, message);
	internal static void ErrorExternal(object message) => LogInternal(LogLevel.ERROR, message, new(1));
	internal static void ErrorListExternal(object[] messages) => LogListInternal(LogLevel.ERROR, messages, new(1));

	private static void LogInternal(LogLevel logType, object message, StackTrace? stackTrace = null, bool includeTrace = false) {
		message ??= NULL_STRING;
		stackTrace = stackTrace ?? new(1);
		ResoniteMod? source = Util.ExecutingMod(stackTrace);
		string logTypePrefix = LogTypeTag(logType);
		_logBuffer.Add(new(source, logType, message.ToString(), stackTrace));
		if (source == null) {
			UniLog.Log($"{logTypePrefix}[ResoniteModLoader] {message}", includeTrace);
		}
		else {
			UniLog.Log($"{logTypePrefix}[ResoniteModLoader/{source.Name}] {message}", includeTrace);
		}
	}

	private static void LogListInternal(LogLevel logType, object[] messages, StackTrace? stackTrace, bool includeTrace = false) {
		if (messages == null) {
			LogInternal(logType, NULL_STRING, stackTrace, includeTrace);
		}
		else {
			foreach (object element in messages) {
				LogInternal(logType, element.ToString(), stackTrace, includeTrace);
			}
		}
	}

	internal static void ProcessException(Exception exception, Assembly? assembly = null, ResoniteModBase? mod = null) => _exceptionBuffer.Add(new(exception, assembly));

	private static string LogTypeTag(LogLevel logType) => $"[{Enum.GetName(typeof(LogLevel), logType)}]";

	internal static void RegisterExceptionHook() {
		AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionProcessor;
		DebugInternal("Unhandled exception hook registered");
	}

	internal static void UnregisterExceptionHook() {
		AppDomain.CurrentDomain.UnhandledException -= UnhandledExceptionProcessor;
		DebugInternal("Unhandled exception hook unregistered");
	}

	private static void UnhandledExceptionProcessor(object sender, UnhandledExceptionEventArgs args) {
		Exception exception = (Exception)args.ExceptionObject;
		StackTrace trace = new StackTrace(exception);
		ResoniteModBase? mod = Util.ExecutingMod(trace);
		Assembly assembly = Assembly.GetAssembly(sender.GetType());
		// this should handle most uncaught cases in RML and mods
		if (mod is not null || assembly == Assembly.GetExecutingAssembly()) {
			if (IsDebugEnabled()) ErrorInternal($"Caught unhandled exception, {exception.Message}. Attributed to {mod?.Name ?? "No mod"} / {assembly.FullName}");
			ProcessException(exception, assembly, mod);
		}
	}
}

internal static class LoggerExtensions {
	internal static IEnumerable<Logger.LogMessage> Logs(this ResoniteModBase mod) => Logger.Logs.Where((line) => line.Mod == mod);

	internal static IEnumerable<Logger.LogException> Exceptions(this ResoniteModBase mod) => Logger.Exceptions.Where((line) => line.Source?.Mod == mod);
}
