namespace ResoniteModLoader;

/// <summary>
/// Contains public metadata about a mod.
/// </summary>
public abstract class ResoniteModBase {
	/// <summary>
	/// Gets the mod's name. This must be unique.
	/// </summary>
	public abstract string Name { get; }

	/// <summary>
	/// Gets the mod's author.
	/// </summary>
	public abstract string Author { get; }

	/// <summary>
	/// Gets the mod's semantic version.
	/// </summary>
	public abstract string Version { get; }

	/// <summary>
	/// Gets an optional hyperlink to the mod's repo or homepage.
	/// </summary>
	public virtual string? Link { get; }

	/// <summary>
	/// A reference to the AssemblyFile that this mod was loaded from.
	/// The reference is set once the mod is successfully loaded, and is null before that.
	/// </summary>
	internal AssemblyFile? ModAssembly { get; set; }

	internal ModConfiguration? ModConfiguration { get; set; }
	internal bool AllowSavingConfiguration = true;

	/// <summary>
	/// Gets this mod's current <see cref="ModConfiguration"/>.
	/// <para/>
	/// This will always be the same instance.
	/// </summary>
	/// <returns>This mod's current configuration.</returns>
	public ModConfiguration? GetConfiguration() {
		if (!FinishedLoading) {
			throw new ModConfigurationException($"GetConfiguration() was called before {Name} was done initializing. Consider calling GetConfiguration() from within OnEngineInit()");
		}
		return ModConfiguration;
	}

	internal bool FinishedLoading { get; set; }

	internal bool IsLocalized { get; set; }
}
