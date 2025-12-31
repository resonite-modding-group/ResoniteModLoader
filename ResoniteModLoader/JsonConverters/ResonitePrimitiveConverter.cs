using Elements.Core;

using Newtonsoft.Json;

namespace ResoniteModLoader.JsonConverters;

internal sealed class ResonitePrimitiveConverter : JsonConverter {
	private static readonly Assembly ElementsCore = typeof(floatQ).Assembly;

	public override bool CanConvert(Type objectType) {
		// handle all non-enum Resonite Primitives in the Elements.Core assembly
		return !objectType.IsEnum && ElementsCore.Equals(objectType.Assembly) && Coder.IsEnginePrimitive(objectType);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
		if (reader.Value is string serialized) {
			// use Resonite's built-in decoding if the value was serialized as a string
			return typeof(Coder<>).MakeGenericType(objectType).GetMethod("DecodeFromString")!.Invoke(null, [serialized])!;
		}

		throw new ArgumentException($"Could not deserialize a Core Element type: {objectType} from a {reader?.Value?.GetType()}");
	}

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
		string serialized = (string)typeof(Coder<>).MakeGenericType(value!.GetType()).GetMethod("EncodeToString")!.Invoke(null, [value])!;
		writer.WriteValue(serialized);
	}
}
