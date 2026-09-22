// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber;

using System;
using System.Numerics;

/// <summary>
/// Hyperbolic functions and their inverses, none of which route through <see cref="double"/>.
/// </summary>
/// <remarks>
/// Every one of these is the exponential or the logarithm wearing a different name, so none of them
/// carries a series of its own except where the identity it is named for would cancel away the
/// answer. The definitions are reached through <see cref="Exp(PreciseNumber, int)"/>,
/// <see cref="ExpM1(PreciseNumber, int)"/>, <see cref="LogP1(PreciseNumber, int)"/> and
/// <see cref="Sqrt(PreciseNumber, int)"/> rather than restated.
/// <para>
/// Precision follows the exponentials: a result carries the significant digits of its argument, and
/// never fewer than <see cref="MinimumDivisionPrecision"/>. Every function has an overload taking
/// that count, and the work is carried <see cref="ExponentialGuardDigits"/> digits beyond it.
/// </para>
/// <para>
/// Three of these need their textbook form rearranged, and which three is worth being exact about,
/// because the usual floating-point reasoning does not transfer. Addition, subtraction and
/// multiplication are <em>exact</em> here, so a difference of two nearly equal values loses nothing
/// by itself. What loses digits is cancelling against a value that has <em>already</em> been rounded
/// to a working width — which is what <see cref="Exp(PreciseNumber, int)"/>,
/// <see cref="Log(PreciseNumber, int)"/>, <see cref="Sqrt(PreciseNumber, int)"/> and
/// <see cref="Divide(PreciseNumber, PreciseNumber, int)"/> all return.
/// </para>
/// <para>
/// That is the test each form has to pass. <c>sinh x = (e^x - e^-x) / 2</c> fails it: both
/// exponentials come back rounded to the working width and then cancel down to something of the
/// order of <c>x</c>, so a <c>1e-30</c> argument keeps about thirty of the fifty digits asked for.
/// So does <c>asinh x = ln(x + √(x² + 1))</c>, where the root arrives rounded, and
/// <c>atanh x = ½ ln((1 + x)/(1 - x))</c>, where the quotient does. Near zero
/// <see cref="Sinh(PreciseNumber, int)"/> therefore sums its own series, and both inverses subtract
/// their one analytically and hand the remainder to <c>LogP1</c>. <c>Sinh(1e-30)</c> is
/// <c>1e-30</c>, not zero, and so are <c>Asinh</c> and <c>Atanh</c> of the same.
/// </para>
/// <para>
/// <see cref="Cosh(PreciseNumber, int)"/>, <see cref="Tanh(PreciseNumber, int)"/> and
/// <see cref="Acosh(PreciseNumber, int)"/> pass it as written, and none of them is rearranged for
/// precision. <c>Cosh</c> sums two positive terms and has nothing to cancel at all; the other two
/// are written the way they are for reasons that are not about lost digits, and each gives its own
/// under its own remarks.
/// </para>
/// <para>
/// <see cref="PreciseNumber"/> has no NaN and no infinity, so <c>Acosh</c> below one and
/// <c>Atanh</c> at or beyond one throw where a <see cref="double"/> would quietly return one of
/// those and carry on, the same way <see cref="Log(PreciseNumber)"/> does.
/// </para>
/// </remarks>
public readonly partial record struct PreciseNumber
	: IHyperbolicFunctions<PreciseNumber>
{
	/// <summary>The message carried by the exception thrown by <c>Acosh</c> below one.</summary>
	private const string AcoshDomainMessage = "Acosh is only defined for a value of at least one.";

	/// <summary>The message carried by the exception thrown by <c>Atanh</c> at or beyond one.</summary>
	private const string AtanhDomainMessage = "Atanh is only defined for a value strictly between negative one and one.";

	/// <summary>
	/// Returns the hyperbolic sine of a value.
	/// </summary>
	/// <param name="x">The value.</param>
	/// <returns>The hyperbolic sine of <paramref name="x"/>.</returns>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	/// <remarks>
	/// Produced to the significant digits of <paramref name="x"/>, and never fewer than
	/// <see cref="MinimumDivisionPrecision"/>. Use <see cref="Sinh(PreciseNumber, int)"/> to choose
	/// that precision. <c>sinh 0</c> is exactly zero.
	/// </remarks>
	public static PreciseNumber Sinh(PreciseNumber x) =>
		Sinh(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns the hyperbolic sine of a value, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The hyperbolic sine of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	/// <remarks>
	/// Near zero this is <c>Σ x^(2n+1)/(2n+1)!</c> rather than <c>(e^x - e^-x) / 2</c>, because that
	/// difference cancels away exactly the digits the function is being asked for: both terms
	/// approach one while their difference approaches <c>2x</c>, so a <c>1e-30</c> argument would
	/// lose thirty digits before the halving. Away from zero the two exponentials differ by enough
	/// that the identity costs nothing, and one reciprocal is cheaper than a second series.
	/// </remarks>
	public static PreciseNumber Sinh(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return Zero;
		}

		int working = significantDigits + ExponentialGuardDigits;

		if (Abs(x) <= DirectSeriesLimit)
		{
			return SinhSeries(x, working).ReduceSignificance(significantDigits);
		}

		PreciseNumber raised = Exp(x, working);
		PreciseNumber lowered = Divide(One, raised, working);
		return Divide(Subtract(raised, lowered), Two, working).ReduceSignificance(significantDigits);
	}

	/// <summary>
	/// Returns the hyperbolic cosine of a value.
	/// </summary>
	/// <param name="x">The value.</param>
	/// <returns>The hyperbolic cosine of <paramref name="x"/>.</returns>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	/// <remarks>
	/// Produced to the significant digits of <paramref name="x"/>, and never fewer than
	/// <see cref="MinimumDivisionPrecision"/>. Use <see cref="Cosh(PreciseNumber, int)"/> to choose
	/// that precision. <c>cosh 0</c> is exactly one.
	/// </remarks>
	public static PreciseNumber Cosh(PreciseNumber x) =>
		Cosh(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns the hyperbolic cosine of a value, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The hyperbolic cosine of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	/// <remarks>
	/// <c>(e^x + e^-x) / 2</c> everywhere, with no series of its own and no small-argument case. This
	/// is a sum of two positive terms, so unlike <see cref="Sinh(PreciseNumber, int)"/> there is
	/// nothing for it to cancel against: the answer approaches one as the argument approaches zero,
	/// which is where the precision is wanted relative to, and the identity delivers it there.
	/// </remarks>
	public static PreciseNumber Cosh(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return One;
		}

		int working = significantDigits + ExponentialGuardDigits;
		PreciseNumber raised = Exp(x, working);
		PreciseNumber lowered = Divide(One, raised, working);
		return Divide(Add(raised, lowered), Two, working).ReduceSignificance(significantDigits);
	}

	/// <summary>
	/// Returns the hyperbolic tangent of a value.
	/// </summary>
	/// <param name="x">The value.</param>
	/// <returns>The hyperbolic tangent of <paramref name="x"/>, in <c>(-1, 1)</c>.</returns>
	/// <remarks>
	/// Produced to the significant digits of <paramref name="x"/>, and never fewer than
	/// <see cref="MinimumDivisionPrecision"/>. Use <see cref="Tanh(PreciseNumber, int)"/> to choose
	/// that precision. <c>tanh 0</c> is exactly zero.
	/// </remarks>
	public static PreciseNumber Tanh(PreciseNumber x) =>
		Tanh(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns the hyperbolic tangent of a value, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The hyperbolic tangent of <paramref name="x"/>, in <c>(-1, 1)</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <remarks>
	/// <c>tanh x = -t / (2 + t)</c> with <c>t = expm1(-2x)</c>, which is the quotient
	/// <c>(1 - e^-2x) / (1 + e^-2x)</c> with the numerator's subtraction done by <c>ExpM1</c> rather
	/// than against a literal one.
	/// <para>
	/// The part that earns its keep is the sign of the exponent, not the <c>ExpM1</c>. Taking the
	/// exponential of <em>minus</em> twice the magnitude means it decays towards zero for a large
	/// argument instead of growing, so this form saturates where a <c>tanh</c> written the obvious
	/// way round would need <c>e^2x</c> and overflow. Past the point where <c>e^-2|x|</c> falls below
	/// the requested precision the answer is <c>±1</c> to every digit asked for, and is returned
	/// without taking an exponential that would only confirm it.
	/// </para>
	/// <para>
	/// The <c>ExpM1</c> is the smaller point: <c>1 - e^-2x</c> would in fact hold its digits here,
	/// because the subtraction is exact and nothing has been rounded before it. Reaching for
	/// <c>ExpM1</c> costs nothing and keeps the numerator from depending on that, but unlike
	/// <see cref="Sinh(PreciseNumber, int)"/> it is not repairing a real loss.
	/// </para>
	/// </remarks>
	public static PreciseNumber Tanh(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return Zero;
		}

		int working = significantDigits + ExponentialGuardDigits;

		if (IsBeyondTanhSaturation(x, working))
		{
			return x.Significand.Sign > 0 ? One : -One;
		}

		PreciseNumber shifted = ExpM1(Multiply(new(0, -2), x), working);
		return Divide(-shifted, Add(Two, shifted), working).ReduceSignificance(significantDigits);
	}

	/// <summary>
	/// Returns the inverse hyperbolic sine of a value.
	/// </summary>
	/// <param name="x">The value.</param>
	/// <returns>The value whose hyperbolic sine is <paramref name="x"/>.</returns>
	/// <remarks>
	/// Produced to the significant digits of <paramref name="x"/>, and never fewer than
	/// <see cref="MinimumDivisionPrecision"/>. Use <see cref="Asinh(PreciseNumber, int)"/> to choose
	/// that precision. <c>asinh 0</c> is exactly zero.
	/// </remarks>
	public static PreciseNumber Asinh(PreciseNumber x) =>
		Asinh(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns the inverse hyperbolic sine of a value, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The value whose hyperbolic sine is <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <remarks>
	/// <c>asinh x = ln(x + √(x² + 1))</c>, rearranged so the one is subtracted analytically:
	/// <c>√(1 + x²) - 1</c> is <c>x² / (1 + √(1 + x²))</c>, so the whole logarithm is
	/// <c>LogP1( x + x² / (1 + √(1 + x²)) )</c>. Written the first way, a small argument asks for the
	/// logarithm of a number indistinguishable from one at the precision it was given; written this
	/// way, the argument handed to <c>LogP1</c> is of the order of <c>x</c> itself.
	/// <para>
	/// The function is odd, and is evaluated on the magnitude with the sign restored afterwards.
	/// That is not only economy: <c>x + √(x² + 1)</c> for a large negative <c>x</c> is a difference
	/// of two nearly equal numbers, and taking the magnitude first is what avoids it.
	/// </para>
	/// </remarks>
	public static PreciseNumber Asinh(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return Zero;
		}

		int working = significantDigits + ExponentialGuardDigits;
		PreciseNumber magnitude = Abs(x);
		PreciseNumber square = Multiply(magnitude, magnitude);
		PreciseNumber root = Sqrt(Add(One, square), working);
		PreciseNumber excess = Divide(square, Add(One, root), working);
		PreciseNumber result = LogP1(Add(magnitude, excess), significantDigits);

		return x.Significand.Sign > 0 ? result : -result;
	}

	/// <summary>
	/// Returns the inverse hyperbolic cosine of a value.
	/// </summary>
	/// <param name="x">The value, which must be at least one.</param>
	/// <returns>The non-negative value whose hyperbolic cosine is <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> is less than one.</exception>
	/// <remarks>
	/// Produced to the significant digits of <paramref name="x"/>, and never fewer than
	/// <see cref="MinimumDivisionPrecision"/>. Use <see cref="Acosh(PreciseNumber, int)"/> to choose
	/// that precision. <c>acosh 1</c> is exactly zero.
	/// </remarks>
	public static PreciseNumber Acosh(PreciseNumber x) =>
		Acosh(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns the inverse hyperbolic cosine of a value, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value, which must be at least one.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The non-negative value whose hyperbolic cosine is <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="x"/> is less than one, or when <paramref name="significantDigits"/>
	/// is less than one.
	/// </exception>
	/// <remarks>
	/// <c>acosh x = ln(x + √(x² - 1))</c>, rearranged as
	/// <c>LogP1( (x - 1) + √((x - 1)(x + 1)) )</c>.
	/// <para>
	/// In fixed-precision arithmetic that rearrangement is a precision fix, because <c>x² - 1</c>
	/// just above one cancels the digits it is about to take the root of. Here it is not: squaring is
	/// exact and so is the subtraction, so the unfactored form keeps its digits too. What the
	/// factored form saves is width. <c>x²</c> has twice the significand of <c>x</c>, and an argument
	/// a hundred digits wide would carry a two-hundred-digit intermediate into the root for a result
	/// wanted at fifty — which on a type whose digits live in a <see cref="BigInteger"/> is paid for
	/// in allocation. Factoring the difference of squares also keeps the result independent of
	/// exactness rather than resting on it.
	/// </para>
	/// <para>
	/// <c>acosh</c> of a value below one is a value this type cannot represent, and throws rather
	/// than returning something wrong.
	/// </para>
	/// </remarks>
	public static PreciseNumber Acosh(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x < One)
		{
			throw new ArgumentOutOfRangeException(nameof(x), x, AcoshDomainMessage);
		}

		if (x.IsUnit)
		{
			return Zero;
		}

		int working = significantDigits + ExponentialGuardDigits;
		PreciseNumber below = Subtract(x, One);
		PreciseNumber root = Sqrt(Multiply(below, Add(x, One)), working);
		return LogP1(Add(below, root), significantDigits);
	}

	/// <summary>
	/// Returns the inverse hyperbolic tangent of a value.
	/// </summary>
	/// <param name="x">The value, which must lie strictly between negative one and one.</param>
	/// <returns>The value whose hyperbolic tangent is <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when the magnitude of <paramref name="x"/> is at least one.</exception>
	/// <remarks>
	/// Produced to the significant digits of <paramref name="x"/>, and never fewer than
	/// <see cref="MinimumDivisionPrecision"/>. Use <see cref="Atanh(PreciseNumber, int)"/> to choose
	/// that precision. <c>atanh 0</c> is exactly zero.
	/// </remarks>
	public static PreciseNumber Atanh(PreciseNumber x) =>
		Atanh(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns the inverse hyperbolic tangent of a value, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value, which must lie strictly between negative one and one.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The value whose hyperbolic tangent is <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when the magnitude of <paramref name="x"/> is at least one, or when
	/// <paramref name="significantDigits"/> is less than one.
	/// </exception>
	/// <remarks>
	/// <c>atanh x = ½ ln((1 + x)/(1 - x))</c>, written as <c>½ LogP1( 2x / (1 - x) )</c> because
	/// <c>(1 + x)/(1 - x)</c> is <c>1 + 2x/(1 - x)</c> and the quotient approaches one near zero.
	/// The rearranged argument approaches <c>2x</c> instead, which is what <c>LogP1</c> is for.
	/// <para>
	/// At <c>±1</c> the value is unbounded, and <see cref="PreciseNumber"/> has no infinity, so the
	/// endpoints throw along with everything past them.
	/// </para>
	/// </remarks>
	public static PreciseNumber Atanh(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (Abs(x) >= One)
		{
			throw new ArgumentOutOfRangeException(nameof(x), x, AtanhDomainMessage);
		}

		if (x.Significand.IsZero)
		{
			return Zero;
		}

		int working = significantDigits + ExponentialGuardDigits;
		PreciseNumber ratio = Divide(Multiply(Two, x), Subtract(One, x), working);
		return Divide(LogP1(ratio, working), Two, working).ReduceSignificance(significantDigits);
	}

	/// <summary>
	/// Sums the hyperbolic sine series.
	/// </summary>
	/// <param name="x">The argument.</param>
	/// <param name="workingDigits">The significant digits to carry through the sum.</param>
	/// <returns><c>sinh x</c>.</returns>
	/// <exception cref="ArithmeticException">Thrown when the series does not converge.</exception>
	/// <remarks>
	/// <c>Σ x^(2n+1)/(2n+1)!</c> from zero, each term built from its predecessor by multiplying in
	/// <c>x²</c> and dividing by <c>2n(2n + 1)</c>. The leading term is <c>x</c> itself, so the sum
	/// is of the order of <c>x</c> and its digits are kept relative to <c>x</c> rather than to one —
	/// the same reason <see cref="ExpSeriesWithoutLeadingOne"/> omits its leading one.
	/// <para>
	/// Only called with an argument no larger than <see cref="DirectSeriesLimit"/>, where the
	/// factorial outruns the power immediately and the sum is a handful of terms.
	/// </para>
	/// </remarks>
	private static PreciseNumber SinhSeries(PreciseNumber x, int workingDigits)
	{
		if (x.Significand.IsZero)
		{
			return Zero;
		}

		PreciseNumber square = Multiply(x, x).ReduceSignificance(workingDigits);
		PreciseNumber term = x;
		PreciseNumber sum = x;

		for (int n = 1; n <= SeriesIterationAllowance(workingDigits); n++)
		{
			// The divisor outgrows an int well before the allowance does, so it is formed as a long.
			long even = 2L * n;
			term = Divide(Multiply(term, square), new(0, even * (even + 1)), workingDigits);
			if (term.Significand.IsZero)
			{
				return sum;
			}

			PreciseNumber next = Add(sum, term).ReduceSignificance(workingDigits);
			if (next == sum)
			{
				return sum;
			}

			sum = next;
		}

		throw new ArithmeticException(
			$"The hyperbolic sine series did not converge to {workingDigits.ToString(InvariantCulture)} significant digits.");
	}

	/// <summary>
	/// Reports whether a hyperbolic tangent has saturated at the requested precision.
	/// </summary>
	/// <param name="x">The argument.</param>
	/// <param name="workingDigits">The significant digits being carried.</param>
	/// <returns><see langword="true"/> when <c>tanh</c> of <paramref name="x"/> is <c>±1</c> to every digit asked for.</returns>
	/// <remarks>
	/// <c>tanh</c> differs from <c>±1</c> by about <c>2·e^-2|x|</c>, so once <c>2|x|</c> passes
	/// <c>(workingDigits + 1) · ln 10</c> the difference is below the last digit being carried. The
	/// threshold only has to be recognised, not resolved, so the constant behind it is read at the
	/// stored precision rather than at the working one.
	/// </remarks>
	private static bool IsBeyondTanhSaturation(PreciseNumber x, int workingDigits) =>
		Multiply(Two, Abs(x)) > Multiply(new(0, workingDigits + 1), Ln10To(MinimumDivisionPrecision));
}
