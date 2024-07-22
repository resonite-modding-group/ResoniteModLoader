namespace ResoniteModLoader;
/// <summary>
/// Marks a field of type <see cref="ModConfigurationKey{T}"/> on a class
/// deriving from <see cref="ResoniteMod"/> to be automatically included in that mod's configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class AutoRegisterConfigKeyAttribute : Attribute {
	public readonly string Group;

	public readonly IReadOnlyList<string> Path;

	public AutoRegisterConfigKeyAttribute() { }

	public AutoRegisterConfigKeyAttribute(string groupName) {
		Group = groupName;
	}

	public AutoRegisterConfigKeyAttribute(string[] subpath) {
		Path = subpath;
	}

	public AutoRegisterConfigKeyAttribute(string[] subpath, string groupName) {
		Group = groupName;
		Path = subpath;
	}
}
