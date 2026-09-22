// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Benchmarks;

using BenchmarkDotNet.Attributes;

/// <summary>
/// Measures the circular trigonometric functions and their inverses.
/// </summary>
/// <remarks>
/// These have no <see cref="double"/> predecessor in the type — there was no trigonometry before —
/// so the <see cref="DoubleSinBaseline"/> and <see cref="DoubleAtan2Baseline"/> are a floor rather
/// than a thing replaced: they answer in about fifteen correct digits whatever the <c>Digits</c>
/// axis says.
/// <para>
/// The <c>Digits</c> axis drives both the operand and the digits asked of the answer, since the
/// default precision follows the operand. The cost of a sine grows with the digit count on two
/// fronts: the series gains terms, and every term is a wider <see cref="System.Numerics.BigInteger"/>.
/// Read it against <see cref="ArithmeticBenchmarks"/>'s division, which sits inside every loop here.
/// </para>
/// <para>
/// <see cref="SinCosOfAnAngle"/> against <see cref="SinOfAnAngle"/> and <see cref="CosOfAnAngle"/>
/// is the measurement that justifies the combined member: it does one argument reduction where the
/// two separate calls do two, so it should cost noticeably less than their sum.
/// </para>
/// <para>
/// <see cref="SinOfALargeAngle"/> is the case the wide π pays for. Its reduction reads π to the
/// width of the argument on top of the answer, so it should sit above <see cref="SinOfAnAngle"/> by
/// the cost of that wider constant and its multiplication, and the gap should widen with the
/// <c>Digits</c> axis.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class TrigonometryBenchmarks
{
	private PreciseNumber angle = PreciseNumber.Zero;
	private PreciseNumber largeAngle = PreciseNumber.Zero;
	private PreciseNumber unitInterval = PreciseNumber.Zero;
	private PreciseNumber tangent = PreciseNumber.Zero;
	private PreciseNumber ordinate = PreciseNumber.Zero;
	private PreciseNumber abscissa = PreciseNumber.Zero;
	private double angleAsDouble;

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
		// A little over one radian, so the reduction into an octant and the series both run.
		angle = Operands.Number(Digits, -(Digits - 1));

		// Several thousand radians, so the reduction reads π wider than the answer.
		largeAngle = Operands.Number(Digits, -(Digits - 4), offset: 7);

		// In (0, 1), the domain of the inverse sine.
		unitInterval = Operands.Number(Digits, -Digits, offset: 13);

		tangent = Operands.Number(Digits, -(Digits - 1), offset: 29);
		ordinate = Operands.Number(Digits, -(Digits - 1), offset: 5);
		abscissa = -Operands.Number(Digits, -(Digits - 1), offset: 17);
		angleAsDouble = angle.To<double>();
	}

	/// <summary>Takes the sine of an angle.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber SinOfAnAngle() => PreciseNumber.Sin(angle);

	/// <summary>Takes the cosine of an angle.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber CosOfAnAngle() => PreciseNumber.Cos(angle);

	/// <summary>Takes the sine and cosine of an angle from one reduction.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public (PreciseNumber Sin, PreciseNumber Cos) SinCosOfAnAngle() => PreciseNumber.SinCos(angle);

	/// <summary>Takes the tangent of an angle, which is one reduction and a division.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber TanOfAnAngle() => PreciseNumber.Tan(angle);

	/// <summary>Takes the sine of a large angle, whose reduction reads a wide π.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber SinOfALargeAngle() => PreciseNumber.Sin(largeAngle);

	/// <summary>Takes the arc tangent, whose half-angle reduction precedes the series.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber AtanOfAValue() => PreciseNumber.Atan(tangent);

	/// <summary>Takes the arc sine, which is a square root and an arc tangent.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber AsinOfAValue() => PreciseNumber.Asin(unitInterval);

	/// <summary>Takes the two-argument arc tangent, with quadrant dispatch.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public PreciseNumber Atan2OfAPoint() => PreciseNumber.Atan2(ordinate, abscissa);

	/// <summary>The <see cref="double"/> sine, as a floor on the measurement.</summary>
	/// <returns>The result.</returns>
	[Benchmark(Baseline = true)]
	public double DoubleSinBaseline() => Math.Sin(angleAsDouble);

	/// <summary>The <see cref="double"/> two-argument arc tangent, as a floor on the measurement.</summary>
	/// <returns>The result.</returns>
	[Benchmark]
	public double DoubleAtan2Baseline() => Math.Atan2(angleAsDouble, -angleAsDouble);
}
