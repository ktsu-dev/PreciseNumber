// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Benchmarks;

using BenchmarkDotNet.Attributes;

/// <summary>
/// Measures raising a number to an integer power.
/// </summary>
/// <remarks>
/// Kept apart from the other arithmetic because its cost is driven by the exponent rather than by
/// the operand's digit count, and because each multiplication grows the running result, so the
/// work per step is not constant.
/// </remarks>
[MemoryDiagnoser]
public class PowBenchmarks
{
	private PreciseNumber baseValue = PreciseNumber.Zero;
	private PreciseNumber power = PreciseNumber.Zero;

	/// <summary>
	/// Gets or sets the power to raise the base value to.
	/// </summary>
	[Params(2, 10, 64)]
	public int Power { get; set; }

	/// <summary>
	/// Prepares the operands.
	/// </summary>
	[GlobalSetup]
	public void Setup()
	{
		baseValue = Operands.Number(8, -4);
		power = Power.ToPreciseNumber();
	}

	/// <summary>Raises the base value to an integer power.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber Pow() => baseValue.Pow(power);
}
