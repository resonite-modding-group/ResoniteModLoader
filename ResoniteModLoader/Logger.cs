using System.Collections;
using System.Diagnostics;

using Elements.Core;

namespace ResoniteModLoader;

/// <summary>
/// General class that manages all RML-related log/exception processing.
/// Use the inherited methods from the <see cref="ResoniteMod"/> class instead of UniLog or calling these methods directly!
/// Generally, you will only use this class to read RML/other mod logs, or directly pass exceptions to RML with <see cref="ProcessException"/>.
/// </summary>
public sealed class Logger {

	/// <summary>
	/// Represents the severity level of a log message.
	/// </summary>
	public enum LogLevel { TRACE, DEBUG, INFO, WARN, ERROR }

	/// <summary>
	/// Represents a single log entry.
	/// </summary>
	public class MessageItem {
		internal MessageItem(ResoniteModBase? mod, LogLevel level, string message, StackTrace? trace = null) {
			Time = DateTime.Now;
			Mod = mod;
			Level = level;
			Message = message;
			Trace = trace;
		}

		public DateTime Time { get; }

		/// <summary>
		/// The mod that created this log entry, or RML if null.
		/// </summary>
		public ResoniteModBase? Mod { get; }
		public LogLevel Level { get; }
		public string Message { get; }

		/// <summary>
		/// A stack trace relating to the log entry, if recorded.
		/// </summary>
		public StackTrace? Trace { get; }

		/// <inheritdoc/>
		public override string ToString() => $"({Mod?.Name ?? "ResoniteModLoader"} @ {Time}) {LogTypeTag(Level)} {Message}";
	}

	/// <summary>
	/// Represents an exception that was caught or passed for logging.
	/// </summary>
	public class ExceptionItem {
		internal ExceptionItem(System.Exception exception) {
			Time = DateTime.Now;
			Exception = exception;
		}

		internal ExceptionItem(System.Exception exception, Assembly? assembly) {
			Time = DateTime.Now;
			Exception = exception;
			Source = (assembly, null);
		}

		internal ExceptionItem(System.Exception exception, ResoniteModBase? mod) {
			Time = DateTime.Now;
			Exception = exception;
			Source = (mod?.ModAssembly?.Assembly, mod);
		}

		internal ExceptionItem(System.Exception exception, Assembly? assembly, ResoniteModBase? mod) {
			Time = DateTime.Now;
			Exception = exception;
			Source = (assembly, mod);
		}

		public DateTime Time { get; }
		public System.Exception Exception { get; }

		/// <summary>
		/// The (possible) source of the exception. Note the assembly and mod may be unrelated if both set!
		/// </summary>
		public (Assembly? Assembly, ResoniteModBase? Mod)? Source { get; }

		/// <inheritdoc/>
		public override string ToString() => $"({Time}) [{Source?.Assembly?.FullName} ?? Unknown assembly] {Exception.Message}\n{Exception.StackTrace}";
	}

	// logged for null objects
	internal const string NULL_STRING = "null";

	private static List<MessageItem> _logBuffer = new();

	/// <summary>
	/// Stores all logs posted by mods and RML itself.
	/// </summary>
	public static IReadOnlyList<MessageItem> Logs => _logBuffer.AsReadOnly();

	private static List<ExceptionItem> _exceptionBuffer = new();

	/// <summary>
	/// Stores all exceptions caught by RML or passed by mods for logging.
	/// </summary>
	public static IReadOnlyList<ExceptionItem> Exceptions => _exceptionBuffer.AsReadOnly();

	public delegate void MessageHandler(MessageItem message);

	/// <summary>
	/// Fired whenever a message is logged.
	/// </summary>
	public static event MessageHandler? OnMessagePosted;

	public delegate void ExceptionHandler(ExceptionItem exception);

	/// <summary>
	/// Fired whenever an exception is caught by RML or passed by a mod.
	/// </summary>
	public static event ExceptionHandler? OnExceptionPosted;

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
		stackTrace ??= new(1);
		ResoniteMod? source = Util.ExecutingMod(stackTrace);
		string logTypePrefix = LogTypeTag(logType);
		MessageItem item = new(source, logType, message.ToString(), stackTrace);
		_logBuffer.Add(item);
		OnMessagePosted?.SafeInvoke(item);
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

	/// <summary>
	/// Use to pass a caught exception to RML for logging purposes.
	/// Note that calling this will not automatically produce an error message, unless debug is enabled in RML's config.
	/// </summary>
	/// <param name="exception">The exception to be recorded</param>
	/// <param name="assembly">The assembly responsible for causing the exception, if known</param>
	/// <param name="mod">The mod where the exception occurred, if known</param>
	public static void ProcessException(System.Exception exception, Assembly? assembly = null, ResoniteModBase? mod = null) {
		ExceptionItem item = new(exception, assembly, mod);
		_exceptionBuffer.Add(item);
		OnExceptionPosted?.SafeInvoke(item);
		if (IsDebugEnabled()) {
			string? attribution = null;
			attribution ??= mod?.Name;
			attribution ??= assembly?.FullName;
			attribution ??= "unknown mod/assembly";
			LogInternal(LogLevel.ERROR, $"DEBUG EXCEPTION [{attribution}]: {exception.Message}", new StackTrace(exception), true);
		}
	}

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
		System.Exception exception = (System.Exception)args.ExceptionObject;
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

/// <summary>
/// Extension methods to filter logs/exceptions from a single mod.
/// </summary>
public static class LoggerExtensions {
	/// <summary>
	/// Gets messages that were logged by this mod.
	/// </summary>
	/// <param name="mod">The mod to filter messages from</param>
	/// <returns>Any messages logged by this mod.</returns>
	public static IEnumerable<Logger.MessageItem> Logs(this ResoniteModBase mod) => Logger.Logs.Where((line) => line.Mod == mod);

	/// <summary>
	/// Gets exceptions that are related to this mod.
	/// </summary>
	/// <param name="mod">The mod to filter exceptions on</param>
	/// <returns>Any exceptions related to this mod.</returns>
	public static IEnumerable<Logger.ExceptionItem> Exceptions(this ResoniteModBase mod) => Logger.Exceptions.Where((line) => line.Source?.Mod == mod);
}
