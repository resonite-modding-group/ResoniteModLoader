using System.Text.Json;
using System.Text.Json.Serialization;

namespace ResoniteModLoader.JsonConverters;

// serializes and deserializes enums as strings
internal sealed class EnumConverter : JsonConverterFactory {

	public override bool CanConvert(Type typeToConvert) {
		var notNullType = Nullable.GetUnderlyingType(typeToConvert);
		return (notNullType ?? typeToConvert).IsEnum;
	}

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
		if (!CanConvert(typeToConvert))
			throw new InvalidOperationException($"Cannot convert type {typeToConvert}");

		var notNullType = Nullable.GetUnderlyingType(typeToConvert);

		var type = notNullType == null
			? typeof(Inner<>).MakeGenericType([typeToConvert])
			: typeof(InnerNullable<>).MakeGenericType([notNullType]);
		return (JsonConverter?)Activator.CreateInstance(type);
	}

	private sealed class Inner<T> : JsonConverter<T> where T : struct, Enum {
		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			switch (reader.TokenType) {
				case JsonTokenType.Null:
					return default;

				case JsonTokenType.String: {
						var value = reader.GetString()!;
						if (Enum.TryParse<T>(value, false, out T result))
							return result;
						else
							throw new JsonException(
								$"{typeToConvert} does not have a variant '{value}'"
							);
					}

				// handle old behavior where enums were serialized as underlying type
				case JsonTokenType.Number: {
						Type underlyingType = Enum.GetUnderlyingType(typeToConvert);
						if (underlyingType == typeof(ulong)) {
							// Edge case: ulong can represent more positive values than long
							var value = reader.GetUInt64();
							return (T)Enum.ToObject(typeof(T), value);
						}
						else {
							var value = reader.GetInt64();
							return (T)Enum.ToObject(typeof(T), value);
						}
					}

				default:
					throw new JsonException(
						$"Expected string or number when parsing {typeToConvert}, found {reader.TokenType}"
					);
			}
		}

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
			string? serialized = Enum.GetName<T>(value);
			writer.WriteStringValue(serialized);
		}
	}

	private sealed class InnerNullable<T> : JsonConverter<T?> where T : struct, Enum {
		public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			switch (reader.TokenType) {
				case JsonTokenType.Null:
					return null;

				case JsonTokenType.String: {
						var value = reader.GetString()!;
						if (Enum.TryParse<T>(value, false, out T result))
							return result;
						else
							throw new JsonException(
								$"{typeToConvert} does not have a variant '{value}'"
							);
					}

				// handle old behavior where enums were serialized as underlying type
				case JsonTokenType.Number: {
						Type underlyingType = Enum.GetUnderlyingType(typeToConvert);
						if (underlyingType == typeof(ulong)) {
							// Edge case: ulong can represent more positive values than long
							var value = reader.GetUInt64();
							return (T?)Enum.ToObject(typeof(T), value);
						}
						else {
							var value = reader.GetInt64();
							return (T?)Enum.ToObject(typeof(T), value);
						}
					}

				default:
					throw new JsonException(
						$"Expected string or number when parsing {typeToConvert}, found {reader.TokenType}"
					);
			}
		}

		public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options) {
			if (value == null) {
				writer.WriteNullValue();
			}
			else {
				string? serialized = Enum.GetName<T>((T)value);
				writer.WriteStringValue(serialized);
			}
		}
	}

}
