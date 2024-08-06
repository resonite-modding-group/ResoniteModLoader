namespace ResoniteModLoader;
/// <summary>
/// Marks a field of type <see cref="ModConfigurationKey{T}"/> on a class
/// deriving from <see cref="ResoniteMod"/> to be automatically included in that mod's configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class AutoRegisterConfigKeyAttribute : Attribute {
	public readonly string? Group;

	public AutoRegisterConfigKeyAttribute() { }

	public AutoRegisterConfigKeyAttribute(string group) {
		Group = group;
	}
}
