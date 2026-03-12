using System.Text.Json;
using System.Text.Json.Serialization;

using Elements.Core;

namespace ResoniteModLoader.JsonConverters;

internal sealed class ResonitePrimitiveConverter : JsonConverterFactory {

	private static readonly Assembly ElementsCore = typeof(floatQ).Assembly;

	public override bool CanConvert(Type typeToConvert)
		=> !typeToConvert.IsEnum
		&& ElementsCore.Equals(typeToConvert.Assembly)
		&& Coder.IsEnginePrimitive(typeToConvert);

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
		if (!CanConvert(typeToConvert))
			throw new InvalidOperationException($"Cannot convert type {typeToConvert}");

		var type = typeof(Inner<>).MakeGenericType([typeToConvert]);
		return (JsonConverter?)Activator.CreateInstance(type);
	}

	private sealed class Inner<T> : JsonConverter<T> {
		public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			var value = reader.GetString();
			if (value == null)
				return default;

			return Coder<T>.DecodeFromString(value);
		}

		public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options) {
			if (value == null) {
				writer.WriteNullValue();
			}
			else {
				var serialized = Coder<T>.EncodeToString(value);
				writer.WriteStringValue(serialized);
			}
		}
	}

}
