using System.Diagnostics;
using FrooxEngine;

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

	public TimeSpan InitializationTime { get; internal set; }

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

	public bool TryGetConfiguration(out ModConfiguration configuration) {
		configuration = ModConfiguration!;
		return configuration is not null;
	}

	/// <summary>
	/// Define a custom configuration DataFeed for this mod.
	/// </summary>
	/// <param name="path">Starts empty at the root of the configuration category, allows sub-categories to be used.</param>
	/// <param name="groupKeys">Passed-through from <see cref="ModConfigurationDataFeed"/>'s Enumerate call.</param>
	/// <param name="searchPhrase">A phrase by which configuration items should be filtered. Passed-through from <see cref="ModConfigurationDataFeed"/>'s Enumerate call</param>
	/// <param name="viewData">Passed-through from <see cref="ModConfigurationDataFeed"/>'s Enumerate call.</param>
	/// <param name="includeInternal">Indicates whether the user has requested that internal configuration keys are included in the returned feed.</param>
	/// <returns>DataFeedItem's to be directly returned by the calling <see cref="ModConfigurationDataFeed"/>.</returns>
	internal abstract IEnumerable<DataFeedItem> BuildConfigurationFeed(IReadOnlyList<string> path, IReadOnlyList<string> groupKeys, string searchPhrase, object viewData, bool includeInternal = false);

	// Why would anyone need an async config? They depend on Microsoft.Bcl.AsyncInterfaces too

	internal bool FinishedLoading { get; set; }
}
