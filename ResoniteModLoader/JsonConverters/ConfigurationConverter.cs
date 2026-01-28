using System.Text.Json;
using System.Text.Json.Serialization;

using ResoniteModLoader;

internal sealed class ModConfigurationConverter : JsonConverter<ModConfiguration> {
	private const string VERSION_JSON_KEY = "version";
	private const string VALUES_JSON_KEY = "values";

	// Thread local to prevent issues with multiple configs being
	// saved from different threads at the same time.
	private readonly ThreadLocal<Context> context = new();

	private struct Context {
		internal ModConfigurationDefinition? definition;
		internal ResoniteMod? mod;
		internal bool saveDefaultValues;
	}

	internal void SetContext(ModConfigurationDefinition definition, ResoniteMod mod, bool saveDefaultValues = false) {
		context.Value = new() {
			definition = definition,
			mod = mod,
			saveDefaultValues = saveDefaultValues,
		};
	}

	internal void ClearContext() {
		context.Value = default;
	}

	private Context TakeContext() {
		var ctx = context.Value;
		if (ctx.definition == null || ctx.mod == null) {
			throw new InvalidOperationException("Invalid state of converter");
		}
		context.Value = default;
		return ctx;
	}

	public override ModConfiguration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		var ctx = TakeContext();
		if (reader.TokenType != JsonTokenType.StartObject) {
			throw new JsonException($"Expected an object, got {reader.TokenType}");
		}
		reader.ReadOrThrow(); // Consume start of object

		// Read "version": "..."

		if (reader.GetString() != VERSION_JSON_KEY) {
			throw new JsonException($"Expected first property to be '{VERSION_JSON_KEY}'");
		}
		reader.ReadOrThrow();

		var versionString = reader.GetString()
			?? throw new JsonException("Version string is null");
		Logger.MsgInternal($"Version: '{versionString}'");
		Version version = new(versionString);
		reader.ReadOrThrow();

		if (!AreVersionsCompatible(version, ctx.definition!.Version)) {
			var handlingMode = ctx.mod!.HandleIncompatibleConfigurationVersions(ctx.definition.Version, version);
			switch (handlingMode) {
				case IncompatibleConfigurationHandlingOption.CLOBBER:
					Logger.WarnInternal($"{ctx.mod.Name} saved config version is {version} which is incompatible with mod's definition version {ctx.definition.Version}. Clobbering old config and starting fresh.");
					return new ModConfiguration(ctx.definition!);
				case IncompatibleConfigurationHandlingOption.FORCELOAD:
					break;
				case IncompatibleConfigurationHandlingOption.ERROR: // fall through to default
				default:
					ctx.mod!.AllowSavingConfiguration = false;
					throw new ModConfigurationException($"{ctx.mod.Name} saved config version is {version} which is incompatible with mod's definition version {ctx.definition.Version}");
			}
		}

		// Read "values": { ... }

		if (reader.GetString() != VALUES_JSON_KEY)
			throw new JsonException($"Expected second property to be '{VALUES_JSON_KEY}'");
		reader.ReadOrThrow();

		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"Expected an object, got {reader.TokenType}");
		reader.ReadOrThrow(); // Consume start of object

		var keys = ctx.definition.ConfigurationItemDefinitions.ToDictionary(key => key.Name);

		while (reader.TokenType != JsonTokenType.EndObject) {
			var name = reader.GetString()
				?? throw new JsonException("Object key is null");
			reader.ReadOrThrow();

			// Ignore unknown keys
			if (!keys.TryGetValue(name, out var key)) {
				Logger.WarnInternal($"{ctx.mod!.Name} saved config version contains entry '{name}' which does not exist in its configuration definition");
				continue;
			}

			var value = ReadGeneric(ref reader, key.ValueType(), options);
			key.Set(value);
			reader.ReadOrThrow();
		}
		reader.ReadOrThrow(); // Consume end of object

		if (reader.TokenType != JsonTokenType.EndObject) {
			throw new JsonException($"Extra keys in configuration object");
		}

		// Exit on end object token

		return new(ctx.definition);
	}

	public override void Write(Utf8JsonWriter writer, ModConfiguration value, JsonSerializerOptions options) {
		var ctx = TakeContext();

		writer.WriteStartObject();

		writer.WriteString(VERSION_JSON_KEY, ctx.definition!.Version.ToString());

		writer.WritePropertyName(VALUES_JSON_KEY);
		writer.WriteStartObject();

		foreach (var key in ctx.definition.ConfigurationItemDefinitions) {
			if (key.TryGetValue(out object? writtenValue)) {
				// write
			}
			else if (ctx.saveDefaultValues && key.TryComputeDefault(out writtenValue)) {
				// write
			}
			else {
				continue;
			}
			writer.WritePropertyName(key.Name);
			if (writtenValue == null) {
				writer.WriteNullValue();
				continue;
			}
			WriteGeneric(writer, key.ValueType(), writtenValue, options);
		}

		writer.WriteEndObject();
		writer.WriteEndObject();
	}

	private static bool AreVersionsCompatible(Version serializedVersion, Version currentVersion) {
		if (serializedVersion.Major != currentVersion.Major) {
			// major version differences are hard incompatible
			return false;
		}

		if (serializedVersion.Minor > currentVersion.Minor) {
			// if serialized config has a newer minor version than us
			// in other words, someone downgraded the mod but not the config
			// then we cannot load the config
			return false;
		}

		// none of the checks failed!
		return true;
	}

	private delegate object? ReadDelegate(ref Utf8JsonReader reader, JsonSerializerOptions options);

	private object? ReadGeneric(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		var converterType = typeof(InnerConverter<>).MakeGenericType([typeToConvert]);
		var read = converterType.GetMethod(nameof(InnerConverter<>.Read))!.CreateDelegate<ReadDelegate>();

		return read(ref reader, options);
	}

	private delegate void WriteDelegate(Utf8JsonWriter writer, object? value, JsonSerializerOptions options);

	private void WriteGeneric(Utf8JsonWriter writer, Type typeToConvert, object? value, JsonSerializerOptions options) {
		var converterType = typeof(InnerConverter<>).MakeGenericType([typeToConvert]);
		var write = converterType.GetMethod(nameof(InnerConverter<>.Write))!.CreateDelegate<WriteDelegate>();

		write(writer, value, options);
	}

	private static class InnerConverter<T> {
		public static object? Read(ref Utf8JsonReader reader, JsonSerializerOptions options) {
			var converter = (JsonConverter<T?>)options.GetConverter(typeof(T));
			return converter.Read(ref reader, typeof(T), options);
		}

		public static void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options) {
			var converter = (JsonConverter<T?>)options.GetConverter(typeof(T));
			converter.Write(writer, (T?)value, options);
		}
	}
}

internal static class Utf8JsonReaderExt {
	public static void ReadOrThrow(this ref Utf8JsonReader reader) {
		if (!reader.Read())
			throw new JsonException();
	}
}
