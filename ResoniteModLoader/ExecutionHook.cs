using System.Runtime.CompilerServices;

using FrooxEngine;

namespace ResoniteModLoader;

public class ExecutionHook : IPlatformConnector {

#pragma warning disable CS1591
	public PlatformInterface Platform { get; private set; }
	public int Priority => -10;
	public string PlatformName => "ResoniteModLoader";
	public string Username => null;
	public string PlatformUserId => null;
	public bool IsPlatformNameUnique => false;
	public void SetCurrentStatus(World world, bool isPrivate, int totalWorldCount) { }
	public void ClearCurrentStatus() { }
	public void Update() { }
	public void Dispose() {
		GC.SuppressFinalize(this);
	}
	public void NotifyOfLocalUser(User user) { }
	public void NotifyOfFile(string file, string name) { }
	public void NotifyOfScreenshot(World world, string file, ScreenshotType type, DateTime time) { }

	public async Task<bool> Initialize(PlatformInterface platformInterface) {
		Logger.DebugInternal("Initialize() from platformInterface");
		Platform = platformInterface;
		return true;
	}
#pragma warning restore CS1591

#pragma warning disable CA2255
	/// <summary>
	/// One method that can start the static constructor of the mod loader.
	/// </summary>
	[ModuleInitializer]
	public static void Init() {
		Logger.DebugInternal("Init() from ModuleInitializer");
	}
#pragma warning restore CA2255

	/// <summary>
	/// Static constructor for <see cref="ExecutionHook"/>. This is called when the assembly is loaded and starts the mod loader initialization process.
	/// </summary>
	static ExecutionHook() {
		Logger.DebugInternal("Start of ExecutionHook");
		ModLoaderInit.Initialize();
	}
}
