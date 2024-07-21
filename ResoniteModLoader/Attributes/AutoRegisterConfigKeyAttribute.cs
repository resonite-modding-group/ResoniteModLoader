namespace ResoniteModLoader;
/// <summary>
/// Marks a field of type <see cref="ModConfigurationKey{T}"/> on a class
/// deriving from <see cref="ResoniteMod"/> to be automatically included in that mod's configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class AutoRegisterConfigKeyAttribute : Attribute {
	public readonly string GroupName;

	// public readonly IReadOnlyList<string> Subpath;

	public AutoRegisterConfigKeyAttribute() { }

	public AutoRegisterConfigKeyAttribute(string groupName) {
		GroupName = groupName;
	}

	// public AutoRegisterConfigKeyAttribute(params string[] subpath) {
	// 	Subpath = subpath;
	// }

	// public AutoRegisterConfigKeyAttribute(string groupName, params string[] subpath) {
	// 	GroupName = groupName;
	// 	Subpath = subpath;
	// }
}
