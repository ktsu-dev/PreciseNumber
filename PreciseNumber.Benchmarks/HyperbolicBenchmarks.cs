// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Benchmarks;

using BenchmarkDotNet.Attributes;

/// <summary>
/// Measures the hyperbolic functions and their inverses.
/// </summary>
/// <remarks>
/// None of these carries a series of its own except <c>Sinh</c> below one half, so read them against
/// <see cref="ExponentialBenchmarks"/> rather than against each other in isolation: each should cost
/// about what the exponential or logarithm underneath it costs, plus the arithmetic named below. A
/// row that is several times its underlying transcendental is doing work it should not be.
/// <para>
/// <see cref="SinhOfASmallValue"/> against <see cref="SinhOfAValue"/> is the pair worth watching.
/// The small case sums its own series and the larger one takes an exponential and a reciprocal, so
/// the two are different algorithms rather than the same one at different arguments. The series
/// should win at every point on the <c>Digits</c> axis — it exists for precision rather than speed,
/// but it would be worth knowing if it cost more than the path it replaces.
/// </para>
/// <para>
/// <see cref="CoshOfAValue"/> is one exponential, one reciprocal and one halving, which makes it the
/// floor for this file. <see cref="TanhOfASaturatedValue"/> is the opposite end: it returns
/// <c>±1</c> without taking an exponential at all, so it should be near free and should not move
/// with the <c>Digits</c> axis. Anything else there means the saturation test is not firing.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class HyperbolicBenchmarks
{
	private PreciseNumber value = PreciseNumber.Zero;
	private PreciseNumber smallValue = PreciseNumber.Zero;
	private PreciseNumber unitInterval = PreciseNumber.Zero;
	private PreciseNumber aboveOne = PreciseNumber.Zero;
	private PreciseNumber saturated = PreciseNumber.Zero;

	/// <summary>
	/// Gets or sets the number of significant digits in the operand, and so in the answer.
	/// </summary>
	[Params(8, 30, 200)]
	public int Digits { get; set; }

	/// <summary>
	/// Prepares the operands.
	/// </summary>
	[GlobalSetup]
	public void Setup()
	{
		// A little over one, so Sinh takes the exponential path rather than the series.
		value = Operands.Number(Digits, -(Digits - 1));

		// Well under one half, so Sinh sums its series instead.
		smallValue = Operands.Number(Digits, -(Digits + 1), offset: 13);

		// In (0, 1), the domain of Atanh.
		unitInterval = Operands.Number(Digits, -Digits, offset: 29);

		// Above one, the domain of Acosh.
		aboveOne = PreciseNumber.One + Operands.Number(Digits, -(Digits - 1), offset: 5);

		// Far enough out that Tanh returns without taking an exponential.
		saturated = Operands.Number(Digits, -(Digits - 4), offset: 17);
	}

	/// <summary>Takes the hyperbolic sine, by way of an exponential and a reciprocal.</summary>
	/// <returns>The result.</returns>
	[Benchmark(Baseline = true)]
	public PreciseNumber SinhOfAValue() => PreciseNumber.Sinh(value);

	/// <summary>Takes the hyperbolic sine of a small value, which sums its own series.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber SinhOfASmallValue() => PreciseNumber.Sinh(smallValue);

	/// <summary>Takes the hyperbolic cosine, which is the cheapest thing here.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber CoshOfAValue() => PreciseNumber.Cosh(value);

	/// <summary>Takes the hyperbolic tangent, which is one exponential and a division.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber TanhOfAValue() => PreciseNumber.Tanh(value);

	/// <summary>Takes the hyperbolic tangent of a value past saturation, which takes no exponential.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber TanhOfASaturatedValue() => PreciseNumber.Tanh(saturated);

	/// <summary>Takes the inverse hyperbolic sine, which is a root and a logarithm.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber AsinhOfAValue() => PreciseNumber.Asinh(value);

	/// <summary>Takes the inverse hyperbolic cosine, which is a root and a logarithm.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber AcoshOfAValue() => PreciseNumber.Acosh(aboveOne);

	/// <summary>Takes the inverse hyperbolic tangent, which is a division and a logarithm.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber AtanhOfAValue() => PreciseNumber.Atanh(unitInterval);
}
