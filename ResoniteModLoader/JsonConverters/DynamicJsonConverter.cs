using System.Text.Json;
using System.Text.Json.Serialization;

using ResoniteModLoader;

internal static class DynamicJsonConverter {
	private delegate object? ReadDelegate(ref Utf8JsonReader reader, JsonSerializerOptions options);

	internal static object? ReadDynamic(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		var converterType = typeof(InnerConverter<>).MakeGenericType([typeToConvert]);
		var read = converterType.GetMethod(nameof(InnerConverter<>.Read))!.CreateDelegate<ReadDelegate>();

		return read(ref reader, options);
	}

	private delegate void WriteDelegate(Utf8JsonWriter writer, object? value, JsonSerializerOptions options);

	internal static void WriteDynamic(Utf8JsonWriter writer, Type typeToConvert, object? value, JsonSerializerOptions options) {
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
