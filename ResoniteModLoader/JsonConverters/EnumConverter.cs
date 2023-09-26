using Newtonsoft.Json;

namespace ResoniteModLoader.JsonConverters;

// serializes and deserializes enums as strings
internal sealed class EnumConverter : JsonConverter {
	public override bool CanConvert(Type objectType) {
		return objectType.IsEnum;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
		// handle old behavior where enums were serialized as underlying type
		Type underlyingType = Enum.GetUnderlyingType(objectType);
		if (TryConvert(reader!.Value!, underlyingType, out object? deserialized)) {
			Logger.DebugFuncInternal(() => $"Deserializing a Core Element type: {objectType} from a {reader!.Value!.GetType()}");
			return deserialized!;
		}

		// handle new behavior where enums are serialized as strings
		if (reader.Value is string serialized) {
			return Enum.Parse(objectType, serialized);
		}

		throw new ArgumentException($"Could not deserialize a Core Element type: {objectType} from a {reader?.Value?.GetType()}. Expected underlying type was {underlyingType}");
	}

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
		string serialized = Enum.GetName(value!.GetType(), value);
		writer.WriteValue(serialized);
	}

	private bool TryConvert(object value, Type newType, out object? converted) {
		try {
			converted = Convert.ChangeType(value, newType);
			return true;
		} catch {
			converted = null;
			return false;
		}
	}
}
