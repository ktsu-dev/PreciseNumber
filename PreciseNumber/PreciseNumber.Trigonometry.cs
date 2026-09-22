// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber;

using System;
using System.Numerics;

/// <summary>
/// Circular trigonometric functions and their inverses, plus <see cref="Atan2(PreciseNumber, PreciseNumber)"/>,
/// none of which route through <see cref="double"/>.
/// </summary>
/// <remarks>
/// The accuracy of a trigonometric function is the accuracy of the constant it reduces by. Reducing
/// an argument of magnitude <c>10^d</c> to <c>n</c> correct digits needs π to roughly <c>d + n</c>
/// digits, so every reduction here reads <see cref="PiTo(int)"/> at the width the argument demands
/// rather than the <see cref="Pi"/> property — which is exactly why <see cref="Sin(PreciseNumber)"/>
/// of a large angle is meaningful at all.
/// <para>
/// <see cref="Sin(PreciseNumber)"/> and <see cref="Cos(PreciseNumber)"/> reduce modulo <c>π/2</c>
/// into <c>[-π/4, π/4]</c> with an octant index, so one kernel serves both and the series stays
/// short. <see cref="SinCos(PreciseNumber)"/> exists to do that reduction once for a caller that
/// needs both, and both individual members read from it.
/// </para>
/// <para>
/// The half-turn family (<see cref="SinPi(PreciseNumber)"/> and the rest) reduces modulo two on the
/// argument <em>before</em> multiplying by π, so the multiplication never magnifies the argument and
/// no wide π is needed: <c>SinPi(1e20)</c> is well-defined where <c>Sin(1e20 · π)</c> is not.
/// </para>
/// <para>
/// <see cref="PreciseNumber"/> has no NaN, so <see cref="Asin(PreciseNumber)"/> and
/// <see cref="Acos(PreciseNumber)"/> throw outside <c>[-1, 1]</c> where a <see cref="double"/> would
/// return NaN and carry on, the same way <see cref="Sqrt(PreciseNumber)"/> does.
/// </para>
/// </remarks>
public readonly partial record struct PreciseNumber
	: ITrigonometricFunctions<PreciseNumber>
{
	/// <summary>
	/// Digits computed past the ones the caller asked for, so that the digit the final rounding
	/// decision is made on is itself correct.
	/// </summary>
	/// <remarks>
	/// The same width the exponential series uses, and for the same reason: a series makes one
	/// rounding decision per term, and the doubling that follows argument halving compounds whatever
	/// relative error it is handed.
	/// </remarks>
	private const int TrigonometricGuardDigits = 10;

	/// <summary>
	/// Times the sine/cosine kernel may halve its argument before summing.
	/// </summary>
	/// <remarks>
	/// The argument reaching the kernel is at most <c>π/4</c>, so six halvings always bring it under
	/// <see cref="SeriesArgumentLimit"/>; the rest is slack for an argument that landed a little
	/// outside the octant because the reduction rounded.
	/// </remarks>
	private const int MaximumTrigonometricHalvings = 12;

	/// <summary>
	/// Times <c>Atan</c> may apply its half-angle reduction before summing.
	/// </summary>
	/// <remarks>
	/// The first reduction brings any magnitude down to about one, and each after that roughly halves
	/// the argument, so reaching <see cref="AtanReductionLimit"/> from one takes about six more. The
	/// bound is generous against an argument that starts just above the limit.
	/// </remarks>
	private const int MaximumAtanReductions = 64;

	/// <summary>The message carried by the exception thrown for an inverse sine or cosine outside <c>[-1, 1]</c>.</summary>
	private const string InverseDomainMessage = "The inverse sine and cosine are only defined on the interval [-1, 1].";

	/// <summary>One hundred and eighty, the degrees in a half turn.</summary>
	private static PreciseNumber OneEighty { get; } = new(0, 180);

	/// <summary>
	/// The magnitude at or below which <c>Atan</c> sums its series directly rather than reducing first.
	/// </summary>
	/// <remarks>
	/// One sixty-fourth, matching the exponential's <see cref="SeriesArgumentLimit"/>. Below it the
	/// arctangent series converges in a handful of terms.
	/// </remarks>
	private static PreciseNumber AtanReductionLimit { get; } = SeriesArgumentLimit;

	/// <summary>
	/// Returns the sine of an angle in radians.
	/// </summary>
	/// <param name="x">The angle, in radians.</param>
	/// <returns>The sine of <paramref name="x"/>.</returns>
	/// <remarks>
	/// Produced to the significant digits of <paramref name="x"/>, and never fewer than
	/// <see cref="MinimumDivisionPrecision"/>. Use <see cref="Sin(PreciseNumber, int)"/> to choose
	/// that precision.
	/// </remarks>
	public static PreciseNumber Sin(PreciseNumber x) =>
		Sin(x, DefaultTrigonometricPrecision(x));

	/// <summary>
	/// Returns the sine of an angle in radians, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The angle, in radians.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The sine of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	public static PreciseNumber Sin(PreciseNumber x, int significantDigits) =>
		SinCos(x, significantDigits).Sin;

	/// <summary>
	/// Returns the cosine of an angle in radians.
	/// </summary>
	/// <param name="x">The angle, in radians.</param>
	/// <returns>The cosine of <paramref name="x"/>.</returns>
	/// <remarks>
	/// Produced to the significant digits of <paramref name="x"/>, and never fewer than
	/// <see cref="MinimumDivisionPrecision"/>. Use <see cref="Cos(PreciseNumber, int)"/> to choose
	/// that precision.
	/// </remarks>
	public static PreciseNumber Cos(PreciseNumber x) =>
		Cos(x, DefaultTrigonometricPrecision(x));

	/// <summary>
	/// Returns the cosine of an angle in radians, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The angle, in radians.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The cosine of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	public static PreciseNumber Cos(PreciseNumber x, int significantDigits) =>
		SinCos(x, significantDigits).Cos;

	/// <summary>
	/// Returns the sine and cosine of an angle in radians.
	/// </summary>
	/// <param name="x">The angle, in radians.</param>
	/// <returns>A tuple of the sine and cosine of <paramref name="x"/>.</returns>
	/// <remarks>
	/// Both come from one argument reduction and one kernel, which is the reason to prefer this over a
	/// separate <see cref="Sin(PreciseNumber)"/> and <see cref="Cos(PreciseNumber)"/> when both are
	/// wanted.
	/// </remarks>
	public static (PreciseNumber Sin, PreciseNumber Cos) SinCos(PreciseNumber x) =>
		SinCos(x, DefaultTrigonometricPrecision(x));

	/// <summary>
	/// Returns the sine and cosine of an angle in radians, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The angle, in radians.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>A tuple of the sine and cosine of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <remarks>
	/// <c>x = q · π/2 + r</c> with <c>q</c> the nearest integer and <c>r</c> in <c>[-π/4, π/4]</c>.
	/// The kernel evaluates the sine and cosine of <c>r</c>, and <c>q</c> mod four selects which, and
	/// with which sign, becomes the sine and cosine of <c>x</c>. The <c>π/2</c> the reduction
	/// subtracts is read wide enough that the integer part it cancels was present in the constant
	/// rather than invented.
	/// </remarks>
	public static (PreciseNumber Sin, PreciseNumber Cos) SinCos(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return (Zero, One);
		}

		int working = significantDigits + TrigonometricGuardDigits;

		// The subtraction below cancels the integer part of x / (π/2), so the constant has to carry
		// that many digits past the answer or the remainder is only as good as what was left over.
		int argumentDigits = IntegerDigitCount(x);
		int reductionDigits = working + argumentDigits + TrigonometricGuardDigits;

		PreciseNumber piOverTwo = Divide(PiTo(reductionDigits), Two, reductionDigits);
		BigInteger quadrant = RoundToNearestInteger(Divide(x, piOverTwo, reductionDigits));
		PreciseNumber remainder = Subtract(x, Multiply(new(0, quadrant), piOverTwo))
			.ReduceSignificance(working);

		(PreciseNumber sinRemainder, PreciseNumber cosRemainder) = SmallAngleSinCos(remainder, working);
		return SelectOctant(quadrant, sinRemainder, cosRemainder, significantDigits);
	}

	/// <summary>
	/// Returns the tangent of an angle in radians.
	/// </summary>
	/// <param name="x">The angle, in radians.</param>
	/// <returns>The tangent of <paramref name="x"/>.</returns>
	/// <remarks>
	/// Produced to the significant digits of <paramref name="x"/>, and never fewer than
	/// <see cref="MinimumDivisionPrecision"/>. Use <see cref="Tan(PreciseNumber, int)"/> to choose
	/// that precision.
	/// </remarks>
	public static PreciseNumber Tan(PreciseNumber x) =>
		Tan(x, DefaultTrigonometricPrecision(x));

	/// <summary>
	/// Returns the tangent of an angle in radians, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The angle, in radians.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The tangent of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <exception cref="DivideByZeroException">Thrown when the cosine of <paramref name="x"/> is zero.</exception>
	/// <remarks>
	/// <c>sin / cos</c> from one reduction and a single division, rather than two independent series.
	/// </remarks>
	public static PreciseNumber Tan(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);
		int working = significantDigits + TrigonometricGuardDigits;
		(PreciseNumber sin, PreciseNumber cos) = SinCos(x, working);
		return Divide(sin, cos, significantDigits);
	}

	/// <summary>
	/// Returns the arc sine of a value, in radians.
	/// </summary>
	/// <param name="x">The value, which must lie in <c>[-1, 1]</c>.</param>
	/// <returns>The angle in radians whose sine is <paramref name="x"/>, in <c>[-π/2, π/2]</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> lies outside <c>[-1, 1]</c>.</exception>
	public static PreciseNumber Asin(PreciseNumber x) =>
		Asin(x, DefaultTrigonometricPrecision(x));

	/// <summary>
	/// Returns the arc sine of a value, in radians, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value, which must lie in <c>[-1, 1]</c>.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The angle in radians whose sine is <paramref name="x"/>, in <c>[-π/2, π/2]</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="x"/> lies outside <c>[-1, 1]</c>, or when
	/// <paramref name="significantDigits"/> is less than one.
	/// </exception>
	/// <remarks>
	/// <c>asin x = atan( x / √(1 - x²) )</c>, with <c>asin(±1) = ±π/2</c> special-cased because the
	/// division is by zero exactly there.
	/// </remarks>
	public static PreciseNumber Asin(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		PreciseNumber magnitude = Abs(x);
		if (magnitude > One)
		{
			throw new ArgumentOutOfRangeException(nameof(x), x, InverseDomainMessage);
		}

		if (x.Significand.IsZero)
		{
			return Zero;
		}

		int working = significantDigits + TrigonometricGuardDigits;

		if (magnitude == One)
		{
			PreciseNumber halfPi = Divide(PiTo(working), Two, significantDigits);
			return x.Significand.Sign > 0 ? halfPi : -halfPi;
		}

		PreciseNumber denominator = Sqrt(Subtract(One, Multiply(x, x)), working);
		return Atan(Divide(x, denominator, working), significantDigits);
	}

	/// <summary>
	/// Returns the arc cosine of a value, in radians.
	/// </summary>
	/// <param name="x">The value, which must lie in <c>[-1, 1]</c>.</param>
	/// <returns>The angle in radians whose cosine is <paramref name="x"/>, in <c>[0, π]</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> lies outside <c>[-1, 1]</c>.</exception>
	public static PreciseNumber Acos(PreciseNumber x) =>
		Acos(x, DefaultTrigonometricPrecision(x));

	/// <summary>
	/// Returns the arc cosine of a value, in radians, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value, which must lie in <c>[-1, 1]</c>.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The angle in radians whose cosine is <paramref name="x"/>, in <c>[0, π]</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="x"/> lies outside <c>[-1, 1]</c>, or when
	/// <paramref name="significantDigits"/> is less than one.
	/// </exception>
	/// <remarks>
	/// <c>acos x = π/2 - asin x</c>, with the endpoints returned exactly: <c>acos(1) = 0</c> and
	/// <c>acos(-1) = π</c>.
	/// </remarks>
	public static PreciseNumber Acos(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		PreciseNumber magnitude = Abs(x);
		if (magnitude > One)
		{
			throw new ArgumentOutOfRangeException(nameof(x), x, InverseDomainMessage);
		}

		int working = significantDigits + TrigonometricGuardDigits;

		if (x == One)
		{
			return Zero;
		}

		if (x == NegativeOne)
		{
			return PiTo(working).ReduceSignificance(significantDigits);
		}

		PreciseNumber halfPi = Divide(PiTo(working), Two, working);
		return Subtract(halfPi, Asin(x, working)).ReduceSignificance(significantDigits);
	}

	/// <summary>
	/// Returns the arc tangent of a value, in radians.
	/// </summary>
	/// <param name="x">The value.</param>
	/// <returns>The angle in radians whose tangent is <paramref name="x"/>, in <c>(-π/2, π/2)</c>.</returns>
	public static PreciseNumber Atan(PreciseNumber x) =>
		Atan(x, DefaultTrigonometricPrecision(x));

	/// <summary>
	/// Returns the arc tangent of a value, in radians, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The angle in radians whose tangent is <paramref name="x"/>, in <c>(-π/2, π/2)</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <remarks>
	/// <c>atan x = 2 · atan( x / (1 + √(1 + x²)) )</c>, applied until the argument is small, then the
	/// Taylor series. Each application roughly halves the argument, so any magnitude is brought into
	/// range in a bounded number of steps and the result is scaled back by the matching power of two.
	/// </remarks>
	public static PreciseNumber Atan(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return Zero;
		}

		int working = significantDigits + TrigonometricGuardDigits;

		int reductions = 0;
		PreciseNumber argument = x;
		while (Abs(argument) > AtanReductionLimit && reductions < MaximumAtanReductions)
		{
			PreciseNumber root = Sqrt(Add(One, Multiply(argument, argument)), working + reductions);
			argument = Divide(argument, Add(One, root), working + reductions);
			reductions++;
		}

		PreciseNumber series = AtanSeries(argument, working + reductions);
		PreciseNumber result = reductions == 0
			? series
			: Multiply(new(0, BigInteger.One << reductions), series);

		return result.ReduceSignificance(significantDigits);
	}

	/// <summary>
	/// Returns the angle in radians whose tangent is <c>y / x</c>, using the signs of both to place
	/// the angle in the correct quadrant.
	/// </summary>
	/// <param name="y">The ordinate.</param>
	/// <param name="x">The abscissa.</param>
	/// <returns>The angle in radians, in <c>(-π, π]</c>.</returns>
	/// <remarks>
	/// Not part of <see cref="ITrigonometricFunctions{TSelf}"/>: that lives on
	/// <see cref="IFloatingPointIeee754{TSelf}"/>, which <see cref="PreciseNumber"/> does not
	/// implement because it has no NaN or infinity to give the interface's edge cases meaning. It is
	/// offered here as a bespoke static because a conversion from Cartesian to polar coordinates needs
	/// it. Produced to the greater of the two arguments' significant digits, and never fewer than
	/// <see cref="MinimumDivisionPrecision"/>.
	/// </remarks>
	public static PreciseNumber Atan2(PreciseNumber y, PreciseNumber x) =>
		Atan2(y, x, Math.Max(DefaultTrigonometricPrecision(y), DefaultTrigonometricPrecision(x)));

	/// <summary>
	/// Returns the angle in radians whose tangent is <c>y / x</c>, to a chosen number of significant
	/// digits, using the signs of both to place the angle in the correct quadrant.
	/// </summary>
	/// <param name="y">The ordinate.</param>
	/// <param name="x">The abscissa.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The angle in radians, in <c>(-π, π]</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <remarks>
	/// Quadrant dispatch on <c>atan(y / x)</c>, with the axes handled explicitly: on the positive
	/// abscissa the arc tangent stands alone; on the negative abscissa it is offset by <c>±π</c> to
	/// carry the angle into the correct half; and where the abscissa is zero the angle is <c>±π/2</c>,
	/// or zero when the ordinate is zero as well.
	/// </remarks>
	public static PreciseNumber Atan2(PreciseNumber y, PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);
		int working = significantDigits + TrigonometricGuardDigits;

		if (x.Significand.IsZero)
		{
			if (y.Significand.IsZero)
			{
				return Zero;
			}

			PreciseNumber halfPi = Divide(PiTo(working), Two, significantDigits);
			return y.Significand.Sign > 0 ? halfPi : -halfPi;
		}

		PreciseNumber baseAngle = Atan(Divide(y, x, working), working);

		if (x.Significand.Sign > 0)
		{
			return baseAngle.ReduceSignificance(significantDigits);
		}

		PreciseNumber pi = PiTo(working);
		PreciseNumber shifted = y.Significand.Sign < 0
			? Subtract(baseAngle, pi)
			: Add(baseAngle, pi);
		return shifted.ReduceSignificance(significantDigits);
	}

	/// <summary>
	/// Returns the sine of a value given in half turns.
	/// </summary>
	/// <param name="x">The argument, in half turns, so that <c>SinPi(x) = Sin(x · π)</c>.</param>
	/// <returns>The sine of <c>x · π</c>.</returns>
	/// <remarks>
	/// The argument is reduced modulo two before the multiplication by π, so a large argument keeps
	/// its meaning: <c>SinPi(1e20)</c> is well-defined where <c>Sin(1e20 · π)</c> is not.
	/// </remarks>
	public static PreciseNumber SinPi(PreciseNumber x) =>
		SinPi(x, DefaultTrigonometricPrecision(x));

	/// <summary>
	/// Returns the sine of a value given in half turns, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The argument, in half turns, so that <c>SinPi(x) = Sin(x · π)</c>.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The sine of <c>x · π</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	public static PreciseNumber SinPi(PreciseNumber x, int significantDigits) =>
		SinCosPi(x, significantDigits).SinPi;

	/// <summary>
	/// Returns the cosine of a value given in half turns.
	/// </summary>
	/// <param name="x">The argument, in half turns, so that <c>CosPi(x) = Cos(x · π)</c>.</param>
	/// <returns>The cosine of <c>x · π</c>.</returns>
	/// <remarks>
	/// The argument is reduced modulo two before the multiplication by π, so a large argument keeps
	/// its meaning: <c>CosPi(1e20)</c> is well-defined where <c>Cos(1e20 · π)</c> is not.
	/// </remarks>
	public static PreciseNumber CosPi(PreciseNumber x) =>
		CosPi(x, DefaultTrigonometricPrecision(x));

	/// <summary>
	/// Returns the cosine of a value given in half turns, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The argument, in half turns, so that <c>CosPi(x) = Cos(x · π)</c>.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The cosine of <c>x · π</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	public static PreciseNumber CosPi(PreciseNumber x, int significantDigits) =>
		SinCosPi(x, significantDigits).CosPi;

	/// <summary>
	/// Returns the sine and cosine of a value given in half turns.
	/// </summary>
	/// <param name="x">The argument, in half turns.</param>
	/// <returns>A tuple of the sine and cosine of <c>x · π</c>.</returns>
	public static (PreciseNumber SinPi, PreciseNumber CosPi) SinCosPi(PreciseNumber x) =>
		SinCosPi(x, DefaultTrigonometricPrecision(x));

	/// <summary>
	/// Returns the sine and cosine of a value given in half turns, to a chosen number of significant
	/// digits.
	/// </summary>
	/// <param name="x">The argument, in half turns.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>A tuple of the sine and cosine of <c>x · π</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <remarks>
	/// <c>x = q/2 + f</c> with <c>q</c> the nearest integer number of quarter turns and <c>f</c> in
	/// <c>[-1/4, 1/4]</c> half turns. Only <c>f · π</c> reaches a series, and it is small however
	/// large <c>x</c> is, so π is needed only to the width of the answer — the argument's magnitude
	/// went into the integer <c>q</c>, which the octant selection consumes exactly.
	/// </remarks>
	public static (PreciseNumber SinPi, PreciseNumber CosPi) SinCosPi(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return (Zero, One);
		}

		int working = significantDigits + TrigonometricGuardDigits;

		// q counts quarter turns; the subtraction of q/2 is exact, so the argument's magnitude never
		// reaches the multiplication by π and no wide constant is needed.
		BigInteger quarters = RoundToNearestInteger(Multiply(x, Two));
		PreciseNumber fraction = Subtract(x, Divide(new(0, quarters), Two, working));

		if (fraction.Significand.IsZero)
		{
			return SelectOctant(quarters, Zero, One, significantDigits);
		}

		PreciseNumber radians = Multiply(fraction, PiTo(working)).ReduceSignificance(working);
		(PreciseNumber sinRadians, PreciseNumber cosRadians) = SmallAngleSinCos(radians, working);
		return SelectOctant(quarters, sinRadians, cosRadians, significantDigits);
	}

	/// <summary>
	/// Returns the tangent of a value given in half turns.
	/// </summary>
	/// <param name="x">The argument, in half turns, so that <c>TanPi(x) = Tan(x · π)</c>.</param>
	/// <returns>The tangent of <c>x · π</c>.</returns>
	public static PreciseNumber TanPi(PreciseNumber x) =>
		TanPi(x, DefaultTrigonometricPrecision(x));

	/// <summary>
	/// Returns the tangent of a value given in half turns, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The argument, in half turns, so that <c>TanPi(x) = Tan(x · π)</c>.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The tangent of <c>x · π</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <exception cref="DivideByZeroException">Thrown when the cosine of <c>x · π</c> is zero.</exception>
	public static PreciseNumber TanPi(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);
		int working = significantDigits + TrigonometricGuardDigits;
		(PreciseNumber sin, PreciseNumber cos) = SinCosPi(x, working);
		return Divide(sin, cos, significantDigits);
	}

	/// <summary>
	/// Returns the arc sine of a value, in half turns.
	/// </summary>
	/// <param name="x">The value, which must lie in <c>[-1, 1]</c>.</param>
	/// <returns><c>asin(x) / π</c>, in <c>[-1/2, 1/2]</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> lies outside <c>[-1, 1]</c>.</exception>
	public static PreciseNumber AsinPi(PreciseNumber x) =>
		AsinPi(x, DefaultTrigonometricPrecision(x));

	/// <summary>
	/// Returns the arc sine of a value, in half turns, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value, which must lie in <c>[-1, 1]</c>.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns><c>asin(x) / π</c>, in <c>[-1/2, 1/2]</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="x"/> lies outside <c>[-1, 1]</c>, or when
	/// <paramref name="significantDigits"/> is less than one.
	/// </exception>
	public static PreciseNumber AsinPi(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x == One)
		{
			return Half;
		}

		if (x == NegativeOne)
		{
			return -Half;
		}

		int working = significantDigits + TrigonometricGuardDigits;
		return Divide(Asin(x, working), PiTo(working), significantDigits);
	}

	/// <summary>
	/// Returns the arc cosine of a value, in half turns.
	/// </summary>
	/// <param name="x">The value, which must lie in <c>[-1, 1]</c>.</param>
	/// <returns><c>acos(x) / π</c>, in <c>[0, 1]</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> lies outside <c>[-1, 1]</c>.</exception>
	public static PreciseNumber AcosPi(PreciseNumber x) =>
		AcosPi(x, DefaultTrigonometricPrecision(x));

	/// <summary>
	/// Returns the arc cosine of a value, in half turns, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value, which must lie in <c>[-1, 1]</c>.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns><c>acos(x) / π</c>, in <c>[0, 1]</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="x"/> lies outside <c>[-1, 1]</c>, or when
	/// <paramref name="significantDigits"/> is less than one.
	/// </exception>
	public static PreciseNumber AcosPi(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x == One)
		{
			return Zero;
		}

		if (x == NegativeOne)
		{
			return One;
		}

		int working = significantDigits + TrigonometricGuardDigits;
		return Divide(Acos(x, working), PiTo(working), significantDigits);
	}

	/// <summary>
	/// Returns the arc tangent of a value, in half turns.
	/// </summary>
	/// <param name="x">The value.</param>
	/// <returns><c>atan(x) / π</c>, in <c>(-1/2, 1/2)</c>.</returns>
	public static PreciseNumber AtanPi(PreciseNumber x) =>
		AtanPi(x, DefaultTrigonometricPrecision(x));

	/// <summary>
	/// Returns the arc tangent of a value, in half turns, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns><c>atan(x) / π</c>, in <c>(-1/2, 1/2)</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	public static PreciseNumber AtanPi(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return Zero;
		}

		int working = significantDigits + TrigonometricGuardDigits;
		return Divide(Atan(x, working), PiTo(working), significantDigits);
	}

	/// <summary>
	/// Converts an angle in degrees to radians.
	/// </summary>
	/// <param name="degrees">The angle, in degrees.</param>
	/// <returns>The angle in radians.</returns>
	/// <remarks>
	/// <c>degrees · π / 180</c>, with π read as the correctly-rounded literal
	/// <see cref="PiTo(int)"/> rather than derived from any other constant. Produced to the
	/// significant digits of <paramref name="degrees"/>, and never fewer than
	/// <see cref="MinimumDivisionPrecision"/>.
	/// </remarks>
	public static PreciseNumber DegreesToRadians(PreciseNumber degrees) =>
		DegreesToRadians(degrees, DefaultTrigonometricPrecision(degrees));

	/// <summary>
	/// Converts an angle in degrees to radians, to a chosen number of significant digits.
	/// </summary>
	/// <param name="degrees">The angle, in degrees.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The angle in radians.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	public static PreciseNumber DegreesToRadians(PreciseNumber degrees, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);
		int working = significantDigits + TrigonometricGuardDigits;
		return Divide(Multiply(degrees, PiTo(working)), OneEighty, significantDigits);
	}

	/// <summary>
	/// Converts an angle in radians to degrees.
	/// </summary>
	/// <param name="radians">The angle, in radians.</param>
	/// <returns>The angle in degrees.</returns>
	/// <remarks>
	/// <c>radians · 180 / π</c>, with π read as the correctly-rounded literal
	/// <see cref="PiTo(int)"/> rather than derived from any other constant. Produced to the
	/// significant digits of <paramref name="radians"/>, and never fewer than
	/// <see cref="MinimumDivisionPrecision"/>.
	/// </remarks>
	public static PreciseNumber RadiansToDegrees(PreciseNumber radians) =>
		RadiansToDegrees(radians, DefaultTrigonometricPrecision(radians));

	/// <summary>
	/// Converts an angle in radians to degrees, to a chosen number of significant digits.
	/// </summary>
	/// <param name="radians">The angle, in radians.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The angle in degrees.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	public static PreciseNumber RadiansToDegrees(PreciseNumber radians, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);
		int working = significantDigits + TrigonometricGuardDigits;
		return Divide(Multiply(radians, OneEighty), PiTo(working), significantDigits);
	}

	/// <summary>
	/// Gets the significant digits a trigonometric function produces when the caller does not choose.
	/// </summary>
	/// <param name="value">The value being operated on.</param>
	/// <returns>The significant digits of <paramref name="value"/>, or <see cref="MinimumDivisionPrecision"/> if that is more.</returns>
	/// <remarks>
	/// The same rule <see cref="Divide(PreciseNumber, PreciseNumber)"/>, the roots and the
	/// exponentials follow, so that a constant carrying <see cref="ConstantPrecision"/> digits does
	/// not silently cap the expression at fifty.
	/// </remarks>
	private static int DefaultTrigonometricPrecision(PreciseNumber value) =>
		Math.Max(value.SignificantDigits, MinimumDivisionPrecision);

	/// <summary>
	/// Selects the sine and cosine of an angle from those of its reduced remainder and the quadrant
	/// the reduction removed.
	/// </summary>
	/// <param name="quadrant">The number of quarter turns the reduction subtracted.</param>
	/// <param name="sinRemainder">The sine of the remainder, in <c>[-π/4, π/4]</c>.</param>
	/// <param name="cosRemainder">The cosine of the remainder.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The sine and cosine of the original angle.</returns>
	/// <remarks>
	/// Adding a quarter turn rotates <c>(sin, cos)</c> to <c>(cos, -sin)</c>, so the quadrant modulo
	/// four names one of four sign-and-swap patterns.
	/// </remarks>
	private static (PreciseNumber Sin, PreciseNumber Cos) SelectOctant(
		BigInteger quadrant, PreciseNumber sinRemainder, PreciseNumber cosRemainder, int significantDigits)
	{
		int octant = (int)(((quadrant % 4) + 4) % 4);
		(PreciseNumber sin, PreciseNumber cos) = octant switch
		{
			0 => (sinRemainder, cosRemainder),
			1 => (cosRemainder, -sinRemainder),
			2 => (-sinRemainder, -cosRemainder),
			_ => (-cosRemainder, sinRemainder),
		};

		return (sin.ReduceSignificance(significantDigits), cos.ReduceSignificance(significantDigits));
	}

	/// <summary>
	/// Computes the sine and cosine of an angle already reduced into <c>[-π/4, π/4]</c>.
	/// </summary>
	/// <param name="angle">The reduced angle.</param>
	/// <param name="workingDigits">The significant digits to carry through the series.</param>
	/// <returns>The sine and cosine of <paramref name="angle"/>.</returns>
	/// <remarks>
	/// Halving the angle before the series and applying the double-angle identities afterwards trades
	/// a handful of multiplications for most of the terms. Each doubling compounds whatever relative
	/// error it is handed, so the series is carried one digit wider per halving.
	/// </remarks>
	private static (PreciseNumber Sin, PreciseNumber Cos) SmallAngleSinCos(PreciseNumber angle, int workingDigits)
	{
		if (angle.Significand.IsZero)
		{
			return (Zero, One);
		}

		int halvings = 0;
		PreciseNumber reduced = angle;
		while (halvings < MaximumTrigonometricHalvings && Abs(reduced) > SeriesArgumentLimit)
		{
			// Halving terminates, so this is exact whatever precision is asked of it.
			reduced = Divide(reduced, Two, workingDigits + MaximumTrigonometricHalvings + TrigonometricGuardDigits);
			halvings++;
		}

		int series = workingDigits + halvings + TrigonometricGuardDigits;
		(PreciseNumber sin, PreciseNumber cos) = SinCosSeries(reduced, series);

		for (int doubling = 0; doubling < halvings; doubling++)
		{
			PreciseNumber nextSin = Multiply(Two, Multiply(sin, cos)).ReduceSignificance(series);
			PreciseNumber nextCos = Subtract(Multiply(cos, cos), Multiply(sin, sin)).ReduceSignificance(series);
			sin = nextSin;
			cos = nextCos;
		}

		return (sin.ReduceSignificance(workingDigits), cos.ReduceSignificance(workingDigits));
	}

	/// <summary>
	/// Sums the sine and cosine series for a small argument.
	/// </summary>
	/// <param name="x">The argument, whose magnitude is at or below <see cref="SeriesArgumentLimit"/>.</param>
	/// <param name="workingDigits">The significant digits to carry through the sums.</param>
	/// <returns>The sine and cosine of <paramref name="x"/>.</returns>
	/// <exception cref="ArithmeticException">Thrown when a series does not converge.</exception>
	/// <remarks>
	/// <c>sin x = Σ (-1)ⁿ x^(2n+1) / (2n+1)!</c> and <c>cos x = Σ (-1)ⁿ x^(2n) / (2n)!</c>, summed
	/// together so the shared power of <c>x²</c> is formed once per term. Both stop as soon as their
	/// terms fall below the last digit being carried.
	/// </remarks>
	private static (PreciseNumber Sin, PreciseNumber Cos) SinCosSeries(PreciseNumber x, int workingDigits)
	{
		PreciseNumber negativeXSquared = -Multiply(x, x).ReduceSignificance(workingDigits);

		PreciseNumber sinTerm = x;
		PreciseNumber sinSum = x;
		PreciseNumber cosTerm = One;
		PreciseNumber cosSum = One;

		for (int k = 1; k <= SeriesIterationAllowance(workingDigits); k++)
		{
			// cos term k is the previous one times -x² / ((2k-1)(2k)); sin term k times -x² / ((2k)(2k+1)).
			cosTerm = Divide(Multiply(cosTerm, negativeXSquared), new(0, (long)((2 * k) - 1) * (2 * k)), workingDigits)
				.ReduceSignificance(workingDigits);
			sinTerm = Divide(Multiply(sinTerm, negativeXSquared), new(0, (long)(2 * k) * ((2 * k) + 1)), workingDigits)
				.ReduceSignificance(workingDigits);

			PreciseNumber nextCos = Add(cosSum, cosTerm).ReduceSignificance(workingDigits);
			PreciseNumber nextSin = Add(sinSum, sinTerm).ReduceSignificance(workingDigits);

			bool settled = nextCos == cosSum && nextSin == sinSum;
			bool exhausted = cosTerm.Significand.IsZero && sinTerm.Significand.IsZero;

			cosSum = nextCos;
			sinSum = nextSin;

			if (settled || exhausted)
			{
				return (sinSum, cosSum);
			}
		}

		throw new ArithmeticException(
			$"The trigonometric series did not converge to {workingDigits.ToString(InvariantCulture)} significant digits.");
	}

	/// <summary>
	/// Sums the arc tangent series for a small argument.
	/// </summary>
	/// <param name="z">The argument, whose magnitude is at or below <see cref="AtanReductionLimit"/>.</param>
	/// <param name="workingDigits">The significant digits to carry through the sum.</param>
	/// <returns><c>atan z</c>.</returns>
	/// <exception cref="ArithmeticException">Thrown when the series does not converge.</exception>
	/// <remarks>
	/// <c>atan z = z - z³/3 + z⁵/5 - …</c>. The terms alternate in sign, and the sum stops as soon as
	/// one falls below the last digit being carried.
	/// </remarks>
	private static PreciseNumber AtanSeries(PreciseNumber z, int workingDigits)
	{
		if (z.Significand.IsZero)
		{
			return Zero;
		}

		PreciseNumber negativeZSquared = -Multiply(z, z).ReduceSignificance(workingDigits);
		PreciseNumber term = z;
		PreciseNumber sum = z;

		for (int k = 1; k <= SeriesIterationAllowance(workingDigits); k++)
		{
			term = Multiply(term, negativeZSquared).ReduceSignificance(workingDigits);
			if (term.Significand.IsZero)
			{
				return sum;
			}

			PreciseNumber next = Add(sum, Divide(term, new(0, (2 * k) + 1), workingDigits))
				.ReduceSignificance(workingDigits);

			if (next == sum)
			{
				return sum;
			}

			sum = next;
		}

		throw new ArithmeticException(
			$"The arc tangent series did not converge to {workingDigits.ToString(InvariantCulture)} significant digits.");
	}
}
