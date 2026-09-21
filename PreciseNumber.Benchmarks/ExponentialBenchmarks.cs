// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Benchmarks;

using BenchmarkDotNet.Attributes;

/// <summary>
/// Measures the exponentials, the logarithms, and the fractional power built on them.
/// </summary>
/// <remarks>
/// These replaced a <see cref="double"/> round trip, so the cost of correctness belongs on the
/// record rather than being discovered later. <see cref="DoubleExpBaseline"/> and
/// <see cref="DoubleLogBaseline"/> are that record: they are what <c>Exp</c> and a fractional
/// <c>Pow</c> used to do, and they answer in about fifteen correct digits whatever the
/// <c>Digits</c> axis says, so read them as a floor on the measurement rather than as an
/// alternative.
/// <para>
/// The <c>Digits</c> axis drives both the operand and the digits asked of the answer, since the
/// default precision follows the operand. Both series are summed at the requested precision, so
/// the cost grows with the digit count twice over: more terms are needed, and each term is a
/// wider <see cref="System.Numerics.BigInteger"/>. Read it against
/// <see cref="ArithmeticBenchmarks"/>'s division, which is the operation inside both loops.
/// </para>
/// <para>
/// <see cref="Exp10OfAnInteger"/> and <see cref="Log10OfAPowerOfTen"/> are the cases the
/// representation answers for free — an exponent shift and an exponent read. They should not move
/// with the <c>Digits</c> axis at all, and should allocate nothing beyond the one significand. A
/// regression there means a series is being run where none is needed.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class ExponentialBenchmarks
{
	private PreciseNumber value = PreciseNumber.Zero;
	private PreciseNumber exponent = PreciseNumber.Zero;
	private PreciseNumber fractionalPower = PreciseNumber.Zero;
	private PreciseNumber integerPower = PreciseNumber.Zero;
	private PreciseNumber powerOfTen = PreciseNumber.Zero;
	private PreciseNumber tenExponent = PreciseNumber.Zero;
	private double valueAsDouble;
	private double exponentAsDouble;

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
		// Kept near one so that the logarithm's mantissa reduction and the exponential's power-of-ten
		// reduction are both exercised without either dominating.
		value = Operands.Number(Digits, -(Digits - 1));
		exponent = Operands.Number(Digits, -Digits, offset: 11);
		fractionalPower = Operands.Number(Digits, -Digits, offset: 23);
		integerPower = 64.ToPreciseNumber();
		powerOfTen = PreciseNumber.Parse("1E50", System.Globalization.CultureInfo.InvariantCulture);
		tenExponent = 50.ToPreciseNumber();
		valueAsDouble = value.To<double>();
		exponentAsDouble = exponent.To<double>();
	}

	/// <summary>Takes the natural logarithm.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber Log() => PreciseNumber.Log(value);

	/// <summary>Raises e to a power.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber Exp() => PreciseNumber.Exp(exponent);

	/// <summary>Raises a value to a fractional power, which is an exponential of a logarithm.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber PowFractional() => value.Pow(fractionalPower);

	/// <summary>Raises a value to an integer power, which is exponentiation by squaring and exact.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber PowInteger() => value.Pow(integerPower);

	/// <summary>Raises ten to an integer power, which is an exponent shift and no series.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber Exp10OfAnInteger() => PreciseNumber.Exp10(tenExponent);

	/// <summary>Takes the base-10 logarithm of a power of ten, which is an exponent read and no series.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber Log10OfAPowerOfTen() => PreciseNumber.Log10(powerOfTen);

	/// <summary>The <see cref="double"/> logarithm these replaced, as a floor on the measurement.</summary>
	/// <returns>The result.</returns>
	[Benchmark(Baseline = true)]
	public double DoubleLogBaseline() => Math.Log(valueAsDouble);

	/// <summary>The <see cref="double"/> exponential these replaced, as a floor on the measurement.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public double DoubleExpBaseline() => Math.Exp(exponentAsDouble);
}
