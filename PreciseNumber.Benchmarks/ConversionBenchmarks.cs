// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Benchmarks;

using BenchmarkDotNet.Attributes;

/// <summary>
/// Measures conversion in both directions between <see cref="PreciseNumber"/> and the primitive
/// numeric types.
/// </summary>
/// <remarks>
/// Conversion in is the path most callers enter the library through, so its per-call cost is paid
/// far more often than any single arithmetic operation. Inputs are held in fields rather than
/// written as literals so that the JIT cannot fold the conversion away at compile time.
/// </remarks>
[MemoryDiagnoser]
public class ConversionBenchmarks
{
	private PreciseNumber number = PreciseNumber.Zero;
	private PreciseNumber unit = PreciseNumber.Zero;
	private int int32Value;
	private long int64Value;
	private double doubleValue;
	private float singleValue;
	private decimal decimalValue;

	/// <summary>
	/// Prepares the operands.
	/// </summary>
	[GlobalSetup]
	public void Setup()
	{
		int32Value = 1234567;
		int64Value = 1234567890123456L;
		doubleValue = 1234.5678;
		singleValue = 1234.5678f;
		decimalValue = 1234.5678m;
		number = doubleValue.ToPreciseNumber();
		unit = PreciseNumber.One;
	}

	/// <summary>Converts an <see cref="int"/> into a number.</summary>
	/// <returns>The converted number.</returns>
	[Benchmark(Baseline = true)]
	public PreciseNumber FromInt32() => int32Value.ToPreciseNumber();

	/// <summary>Converts a <see cref="long"/> into a number.</summary>
	/// <returns>The converted number.</returns>
	[Benchmark]
	public PreciseNumber FromInt64() => int64Value.ToPreciseNumber();

	/// <summary>Converts a <see cref="double"/> into a number.</summary>
	/// <returns>The converted number.</returns>
	[Benchmark]
	public PreciseNumber FromDouble() => doubleValue.ToPreciseNumber();

	/// <summary>Converts a <see cref="float"/> into a number.</summary>
	/// <returns>The converted number.</returns>
	[Benchmark]
	public PreciseNumber FromSingle() => singleValue.ToPreciseNumber();

	/// <summary>Converts a <see cref="decimal"/> into a number.</summary>
	/// <returns>The converted number.</returns>
	[Benchmark]
	public PreciseNumber FromDecimal() => decimalValue.ToPreciseNumber();

	/// <summary>Converts a number back into a <see cref="double"/>.</summary>
	/// <returns>The converted value.</returns>
	[Benchmark]
	public double ToDouble() => number.To<double>();

	/// <summary>Converts a number back into an <see cref="int"/>.</summary>
	/// <returns>The converted value.</returns>
	[Benchmark]
	public int ToInt32() => unit.To<int>();
}
