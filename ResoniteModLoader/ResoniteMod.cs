namespace ResoniteModLoader;

/// <summary>
/// Base class for all mods.
/// Contains members that only the <see cref="ModLoader"/> or the Mod itself are intended to access.
/// </summary>
/// <remarks>
/// Inherit from this class when creating a new mod.
/// </remarks>
public abstract class ResoniteMod : ResoniteModBase {
	/// <summary>
	/// Gets whether debug logging is enabled.
	/// </summary>
	/// <returns><c>true</c> if debug logging is enabled.</returns>
	public static bool IsDebugEnabled() => Logger.IsDebugEnabled();

	/// <summary>
	/// Logs an object as a line in the log based on the value produced by the given function if debug logging is enabled..
	/// <para/>
	/// This is more efficient than passing an <see cref="object"/> or a <see cref="string"/> directly,
	/// as it won't be generated if debug logging is disabled.
	/// </summary>
	/// <param name="messageProducer">The function generating the object to log.</param>
	public static void DebugFunc(Func<object> messageProducer) => Logger.DebugFuncExternal(messageProducer);

	/// <summary>
	/// Logs the given object as a line in the log if debug logging is enabled.
	/// </summary>
	/// <param name="message">The object to log.</param>
	public static void Debug(object message) => Logger.DebugExternal(message);

	/// <summary>
	/// Logs the given objects as lines in the log if debug logging is enabled.
	/// </summary>
	/// <param name="messages">The objects to log.</param>
	public static void Debug(params object[] messages) => Logger.DebugListExternal(messages);

	/// <summary>
	/// Logs the given object as a regular line in the log.
	/// </summary>
	/// <param name="message">The object to log.</param>
	public static void Msg(object message) => Logger.MsgExternal(message);

	/// <summary>
	/// Logs the given objects as regular lines in the log.
	/// </summary>
	/// <param name="messages">The objects to log.</param>
	public static void Msg(params object[] messages) => Logger.MsgListExternal(messages);

	/// <summary>
	/// Logs the given object as a warning line in the log.
	/// </summary>
	/// <param name="message">The object to log.</param>
	public static void Warn(object message) => Logger.WarnExternal(message);

	/// <summary>
	/// Logs the given objects as warning lines in the log.
	/// </summary>
	/// <param name="messages">The objects to log.</param>
	public static void Warn(params object[] messages) => Logger.WarnListExternal(messages);

	/// <summary>
	/// Logs the given object as an error line in the log.
	/// </summary>
	/// <param name="message">The object to log.</param>
	public static void Error(object message) => Logger.ErrorExternal(message);

	/// <summary>
	/// Logs the given objects as error lines in the log.
	/// </summary>
	/// <param name="messages">The objects to log.</param>
	public static void Error(params object[] messages) => Logger.ErrorListExternal(messages);

	/// <summary>
	/// Called once immediately after ResoniteModLoader begins execution
	/// </summary>
	public virtual void OnEngineInit() { }

	/// <summary>
	/// Build the defined configuration for this mod.
	/// </summary>
	/// <returns>This mod's configuration definition.</returns>
	internal ModConfigurationDefinition? BuildConfigurationDefinition() {
		ModConfigurationDefinitionBuilder builder = new(this);
		builder.ProcessAttributes();
		DefineConfiguration(builder);
		return builder.Build();
	}

	/// <summary>
	/// Define this mod's configuration via a builder
	/// </summary>
	/// <param name="builder">A builder you can use to define the mod's configuration</param>
	public virtual void DefineConfiguration(ModConfigurationDefinitionBuilder builder) {
	}

	/// <summary>
	/// Defines handling of incompatible configuration versions
	/// </summary>
	/// <param name="serializedVersion">Configuration version read from the config file</param>
	/// <param name="definedVersion">Configuration version defined in the mod code</param>
	/// <returns></returns>
	public virtual IncompatibleConfigurationHandlingOption HandleIncompatibleConfigurationVersions(Version serializedVersion, Version definedVersion) {
		return IncompatibleConfigurationHandlingOption.ERROR;
	}
}
