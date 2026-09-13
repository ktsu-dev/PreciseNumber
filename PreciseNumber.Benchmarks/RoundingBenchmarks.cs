// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Benchmarks;

using BenchmarkDotNet.Attributes;

/// <summary>
/// Measures reducing a number's precision.
/// </summary>
/// <remarks>
/// Both operations build a rounding factor made of repeated digits and then divide by a power of
/// ten, so their cost tracks how many digits are being discarded rather than how many are kept.
/// </remarks>
[MemoryDiagnoser]
public class RoundingBenchmarks
{
	private PreciseNumber number = PreciseNumber.Zero;

	/// <summary>
	/// Gets or sets the number of significant digits in the operand.
	/// </summary>
	[Params(8, 30, 200)]
	public int Digits { get; set; }

	/// <summary>
	/// Prepares the operand.
	/// </summary>
	[GlobalSetup]
	public void Setup() => number = Operands.Number(Digits, -Digits);

	/// <summary>Rounds to three decimal places.</summary>
	/// <returns>The rounded value.</returns>
	[Benchmark(Baseline = true)]
	public PreciseNumber Round() => number.Round(3);

	/// <summary>Reduces the value to five significant digits.</summary>
	/// <returns>The reduced value.</returns>
	[Benchmark]
	public PreciseNumber ReduceSignificance() => number.ReduceSignificance(5);

	/// <summary>Clamps the value into a range it already sits inside.</summary>
	/// <returns>The clamped value.</returns>
	[Benchmark]
	public PreciseNumber Clamp() => PreciseNumber.Clamp(number, PreciseNumber.NegativeOne, PreciseNumber.One);
}
