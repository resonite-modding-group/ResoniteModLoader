namespace ResoniteModLoader.Benchmarks;

using BenchmarkDotNet.Running;
public class BenchmarkRunner {
	public static void Main(string[] args) {
		BenchmarkSwitcher.FromAssembly(typeof(BenchmarkRunner).Assembly).Run(args);
	}
}
