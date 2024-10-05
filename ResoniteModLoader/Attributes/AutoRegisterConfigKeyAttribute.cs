namespace ResoniteModLoader;
/// <summary>
/// Marks a field of type <see cref="ModConfigurationKey{T}"/> on a class
/// deriving from <see cref="ResoniteMod"/> to be automatically included in that mod's configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class AutoRegisterConfigKeyAttribute : Attribute {

	/// <summary>
	/// Defines a group that this configuration key belongs to, used by default configuration feed builder.
	/// </summary>
	public string? Group => _group;

	private readonly string? _group;

	/// <summary>
	/// Flag this field to be automatically registered as a configuration key for this mod that is not grouped with any other keys.
	/// </summary>
	public AutoRegisterConfigKeyAttribute() { }

	/// <summary>
	/// Flag this field to be automatically registered as a configuration key for this mod that is part of a group.
	/// </summary>
	/// <param name="group">The name of the group this configuration key belongs to.</param>
	public AutoRegisterConfigKeyAttribute(string group) {
		_group = group;
	}
}
