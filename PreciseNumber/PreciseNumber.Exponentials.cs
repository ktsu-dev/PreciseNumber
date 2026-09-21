// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber;

using System;
using System.Numerics;

/// <summary>
/// Exponentials, logarithms and powers, none of which route through <see cref="double"/>.
/// </summary>
/// <remarks>
/// The base-10 representation does half the work. A value is <c>significand × 10^exponent</c>, so
/// splitting it into a mantissa in <c>[1, 10)</c> and a decimal exponent costs nothing: the exponent
/// contributes <c>e · ln 10</c> to a logarithm by one multiplication against a stored constant, and
/// consumes a whole factor of <c>10^k</c> in an exponential by shifting the exponent field. Only the
/// mantissa needs a series, and it is always centred on one before it gets there.
/// <para>
/// Precision follows <see cref="Divide(PreciseNumber, PreciseNumber)"/> and the roots: a result
/// carries the significant digits of its argument, and never fewer than
/// <see cref="MinimumDivisionPrecision"/>. Every function has an overload taking that count, and
/// every series runs at <see cref="ExponentialGuardDigits"/> digits beyond it so the digit the final
/// rounding decision is made on is itself correct.
/// </para>
/// <para>
/// <see cref="PreciseNumber"/> has no NaN and no infinity, so the logarithm of zero or of a negative
/// value throws where a <see cref="double"/> would quietly return one of those and carry on, the
/// same way <see cref="Sqrt(PreciseNumber)"/> does.
/// </para>
/// </remarks>
public readonly partial record struct PreciseNumber
	: IExponentialFunctions<PreciseNumber>,
	  ILogarithmicFunctions<PreciseNumber>,
	  IPowerFunctions<PreciseNumber>
{
	/// <summary>
	/// Digits computed past the ones the caller asked for, so that the digit the final rounding
	/// decision is made on is itself correct.
	/// </summary>
	/// <remarks>
	/// Wider than the two a root needs. A root makes one rounding decision at the end; a series makes
	/// one per term, and the exponential squares its result back up to eight times, each of which
	/// doubles whatever relative error it was handed.
	/// </remarks>
	private const int ExponentialGuardDigits = 10;

	/// <summary>
	/// Times the exponential series may halve its argument before summing.
	/// </summary>
	/// <remarks>
	/// The argument reaching the series is at most <c>ln(10) / 2</c>, so seven halvings always bring
	/// it under <see cref="SeriesArgumentLimit"/>; the eighth is slack.
	/// </remarks>
	private const int MaximumHalvings = 8;

	/// <summary>
	/// Integer digits allowed in the argument of an exponential before it is rejected as overflowing.
	/// </summary>
	/// <remarks>
	/// The power of ten an exponential factors out lives in the <see cref="Exponent"/> field, so it
	/// has to fit an <see cref="int"/>. This bound only keeps the intermediate arithmetic sane; the
	/// <see cref="int"/> range itself is checked exactly once the power of ten is known.
	/// </remarks>
	private const int ExponentialArgumentDigitLimit = 20;

	/// <summary>The message carried by the exception thrown for the logarithm of a non-positive value.</summary>
	private const string NonPositiveLogarithmMessage = "A logarithm is only defined for a positive value.";

	/// <summary>The message carried by the exception thrown by <c>LogP1</c> below negative one.</summary>
	private const string LogP1DomainMessage = "LogP1 is only defined for a value greater than negative one.";

	/// <summary>The message carried by the exception thrown for a fractional power of a negative value.</summary>
	private const string NegativeBaseMessage = "A negative value has no real power with a fractional exponent.";

	/// <summary>Two, as the divisor of the halving step and the multiplier of the atanh series.</summary>
	private static PreciseNumber Two { get; } = new(0, 2);

	/// <summary>Ten, as the bound the mantissa of a logarithm is centred against.</summary>
	private static PreciseNumber Ten { get; } = new(1, 1);

	/// <summary>One half, added before flooring to round to the nearest integer.</summary>
	private static PreciseNumber Half { get; } = new(-1, 5);

	/// <summary>
	/// The largest power of ten an exponential may factor into the <see cref="Exponent"/> field.
	/// </summary>
	/// <remarks>
	/// Bounded symmetrically, so the one extra value an <see cref="int"/> holds below zero is given
	/// up. An exponent at the very edge of the range is unusable for anything that follows anyway.
	/// </remarks>
	private static BigInteger MaximumExponentShift { get; } = int.MaxValue;

	/// <summary>
	/// The magnitude at or below which a series is summed directly rather than reduced first.
	/// </summary>
	/// <remarks>
	/// One half. Below it the <c>M1</c> and <c>P1</c> variants have no cancellation to avoid and the
	/// series is short; above it the range reduction is worth more than the series it replaces.
	/// </remarks>
	private static PreciseNumber DirectSeriesLimit { get; } = Half;

	/// <summary>
	/// The magnitude the exponential series halves its argument down to before summing.
	/// </summary>
	/// <remarks>
	/// One sixty-fourth. Each term then shrinks by at least a further factor of <c>64n</c>, so fifty
	/// digits take about twenty terms instead of about fifty.
	/// </remarks>
	private static PreciseNumber SeriesArgumentLimit { get; } = new(-6, 15625);

	/// <summary>
	/// Returns the natural logarithm of a value.
	/// </summary>
	/// <param name="x">The value to take the logarithm of, which must be positive.</param>
	/// <returns>The natural logarithm of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> is zero or negative.</exception>
	/// <remarks>
	/// Produced to the significant digits of <paramref name="x"/>, and never fewer than
	/// <see cref="MinimumDivisionPrecision"/>. Use <see cref="Log(PreciseNumber, int)"/> to choose
	/// that precision. <c>ln 1</c> is exactly zero.
	/// </remarks>
	public static PreciseNumber Log(PreciseNumber x) =>
		Log(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns the natural logarithm of a value, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value to take the logarithm of, which must be positive.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The natural logarithm of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="x"/> is zero or negative, or when <paramref name="significantDigits"/>
	/// is less than one.
	/// </exception>
	/// <remarks>
	/// <c>ln(m · 10^k) = ln m + k · ln 10</c>, with <c>ln 10</c> read from
	/// <see cref="Ln10"/> rather than computed. The mantissa is centred on
	/// <c>[1/√10, √10)</c> and fed to the atanh series, whose argument is then never larger than
	/// about <c>0.52</c>.
	/// </remarks>
	public static PreciseNumber Log(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.Sign <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(x), x, NonPositiveLogarithmMessage);
		}

		if (x.IsUnit)
		{
			return Zero;
		}

		int working = significantDigits + ExponentialGuardDigits;
		(PreciseNumber mantissa, long decimalExponent) = SplitAroundRootTen(x);
		PreciseNumber result = LogMantissa(mantissa, working);

		if (decimalExponent != 0)
		{
			// The exponent's own digits are consumed by the product, so the constant has to be read
			// wider than the answer is wanted.
			int exponentDigits = CountDigits(new BigInteger(decimalExponent));
			result = Add(result, Multiply(new(0, decimalExponent), Ln10To(working + exponentDigits)));
		}

		return result.ReduceSignificance(significantDigits);
	}

	/// <summary>
	/// Returns the logarithm of a value in a chosen base.
	/// </summary>
	/// <param name="x">The value to take the logarithm of, which must be positive.</param>
	/// <param name="newBase">The base of the logarithm, which must be positive and not one.</param>
	/// <returns>The logarithm of <paramref name="x"/> in base <paramref name="newBase"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when either argument is zero or negative.</exception>
	/// <exception cref="DivideByZeroException">Thrown when <paramref name="newBase"/> is one, whose logarithm is zero.</exception>
	public static PreciseNumber Log(PreciseNumber x, PreciseNumber newBase) =>
		Log(x, newBase, Math.Max(DefaultExponentialPrecision(x), DefaultExponentialPrecision(newBase)));

	/// <summary>
	/// Returns the logarithm of a value in a chosen base, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value to take the logarithm of, which must be positive.</param>
	/// <param name="newBase">The base of the logarithm, which must be positive and not one.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The logarithm of <paramref name="x"/> in base <paramref name="newBase"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when either value is zero or negative, or when <paramref name="significantDigits"/> is
	/// less than one.
	/// </exception>
	/// <exception cref="DivideByZeroException">Thrown when <paramref name="newBase"/> is one, whose logarithm is zero.</exception>
	public static PreciseNumber Log(PreciseNumber x, PreciseNumber newBase, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);
		int working = significantDigits + ExponentialGuardDigits;
		return Divide(Log(x, working), Log(newBase, working), significantDigits);
	}

	/// <summary>
	/// Returns the base-2 logarithm of a value.
	/// </summary>
	/// <param name="x">The value to take the logarithm of, which must be positive.</param>
	/// <returns>The base-2 logarithm of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> is zero or negative.</exception>
	/// <remarks>
	/// Unlike <see cref="Log10(PreciseNumber)"/>, no part of this is free in a base-10
	/// representation, so an exact power of two still arrives through a series and is correct to the
	/// digits asked for rather than exact.
	/// </remarks>
	public static PreciseNumber Log2(PreciseNumber x) =>
		Log2(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns the base-2 logarithm of a value, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value to take the logarithm of, which must be positive.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The base-2 logarithm of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="x"/> is zero or negative, or when <paramref name="significantDigits"/>
	/// is less than one.
	/// </exception>
	public static PreciseNumber Log2(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);
		int working = significantDigits + ExponentialGuardDigits;
		return Divide(Log(x, working), Ln2To(working), significantDigits);
	}

	/// <summary>
	/// Returns the base-10 logarithm of a value.
	/// </summary>
	/// <param name="x">The value to take the logarithm of, which must be positive.</param>
	/// <returns>The base-10 logarithm of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> is zero or negative.</exception>
	/// <remarks>
	/// Does not route through the natural logarithm for the part of the answer the representation
	/// already holds. <c>log10(m · 10^k) = k + log10 m</c>, so an exact power of ten returns its own
	/// exponent exactly, with no series run at all, and everything else pays for one mantissa.
	/// </remarks>
	public static PreciseNumber Log10(PreciseNumber x) =>
		Log10(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns the base-10 logarithm of a value, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value to take the logarithm of, which must be positive.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The base-10 logarithm of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="x"/> is zero or negative, or when <paramref name="significantDigits"/>
	/// is less than one.
	/// </exception>
	public static PreciseNumber Log10(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.Sign <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(x), x, NonPositiveLogarithmMessage);
		}

		(PreciseNumber mantissa, long decimalExponent) = SplitAroundRootTen(x);
		PreciseNumber wholePart = new(0, decimalExponent);

		// A power of ten is entirely exponent, so there is nothing left for a series to do.
		if (mantissa.IsUnit)
		{
			return wholePart;
		}

		int working = significantDigits + ExponentialGuardDigits;
		PreciseNumber fraction = Divide(LogMantissa(mantissa, working), Ln10To(working), working);
		return Add(wholePart, fraction).ReduceSignificance(significantDigits);
	}

	/// <summary>
	/// Returns the natural logarithm of one plus a value.
	/// </summary>
	/// <param name="x">The value to add to one, which must be greater than negative one.</param>
	/// <returns><c>ln(1 + x)</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> is negative one or less.</exception>
	/// <remarks>
	/// Near zero this is computed as <c>2 · atanh(x / (x + 2))</c>, which never forms <c>1 + x</c>
	/// and so keeps every digit of a small argument. Computing it as <c>Log(One + x)</c> would throw
	/// away precisely the precision this function exists to preserve:
	/// <c>LogP1(1e-30)</c> is <c>1e-30</c>, not zero.
	/// </remarks>
	public static PreciseNumber LogP1(PreciseNumber x) =>
		LogP1(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns the natural logarithm of one plus a value, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value to add to one, which must be greater than negative one.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns><c>ln(1 + x)</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="x"/> is negative one or less, or when
	/// <paramref name="significantDigits"/> is less than one.
	/// </exception>
	public static PreciseNumber LogP1(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return Zero;
		}

		if (x <= NegativeOne)
		{
			throw new ArgumentOutOfRangeException(nameof(x), x, LogP1DomainMessage);
		}

		// Away from zero there is nothing to cancel, so the general logarithm is both simpler and
		// better conditioned than an atanh argument approaching one.
		if (Abs(x) > DirectSeriesLimit)
		{
			return Log(Add(One, x), significantDigits);
		}

		int working = significantDigits + ExponentialGuardDigits;
		PreciseNumber z = Divide(x, Add(x, Two), working);
		return Multiply(Two, AtanhSeries(z, working)).ReduceSignificance(significantDigits);
	}

	/// <summary>
	/// Returns the base-2 logarithm of one plus a value.
	/// </summary>
	/// <param name="x">The value to add to one, which must be greater than negative one.</param>
	/// <returns><c>log2(1 + x)</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> is negative one or less.</exception>
	public static PreciseNumber Log2P1(PreciseNumber x) =>
		Log2P1(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns the base-2 logarithm of one plus a value, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value to add to one, which must be greater than negative one.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns><c>log2(1 + x)</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="x"/> is negative one or less, or when
	/// <paramref name="significantDigits"/> is less than one.
	/// </exception>
	public static PreciseNumber Log2P1(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return Zero;
		}

		int working = significantDigits + ExponentialGuardDigits;
		return Divide(LogP1(x, working), Ln2To(working), significantDigits);
	}

	/// <summary>
	/// Returns the base-10 logarithm of one plus a value.
	/// </summary>
	/// <param name="x">The value to add to one, which must be greater than negative one.</param>
	/// <returns><c>log10(1 + x)</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> is negative one or less.</exception>
	public static PreciseNumber Log10P1(PreciseNumber x) =>
		Log10P1(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns the base-10 logarithm of one plus a value, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value to add to one, which must be greater than negative one.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns><c>log10(1 + x)</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="x"/> is negative one or less, or when
	/// <paramref name="significantDigits"/> is less than one.
	/// </exception>
	public static PreciseNumber Log10P1(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return Zero;
		}

		int working = significantDigits + ExponentialGuardDigits;
		return Divide(LogP1(x, working), Ln10To(working), significantDigits);
	}

	/// <summary>
	/// Returns e raised to a power.
	/// </summary>
	/// <param name="x">The power to raise e to.</param>
	/// <returns><c>e^x</c>.</returns>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	/// <remarks>
	/// Produced to the significant digits of <paramref name="x"/>, and never fewer than
	/// <see cref="MinimumDivisionPrecision"/>. Use <see cref="Exp(PreciseNumber, int)"/> to choose
	/// that precision.
	/// <para>
	/// <c>e</c> itself is returned at the full <see cref="ConstantPrecision"/> it is stored to,
	/// rather than capped at the fifty a one-digit argument would otherwise ask for.
	/// </para>
	/// </remarks>
	public static PreciseNumber Exp(PreciseNumber x) =>
		x.IsUnit ? E : Exp(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns e raised to a power, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The power to raise e to.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns><c>e^x</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	/// <remarks>
	/// <c>exp(x) = 10^k · exp(r)</c> with <c>k = round(x / ln 10)</c>, so the whole power of ten is a
	/// shift of the <see cref="Exponent"/> field and the series only ever sees an <c>r</c> no larger
	/// than <c>ln(10) / 2</c>, halved further until it is under
	/// <see cref="SeriesArgumentLimit"/> and squared back afterwards.
	/// </remarks>
	public static PreciseNumber Exp(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return One;
		}

		// e itself is stored, so there is no reason to rediscover it a term at a time.
		if (x.IsUnit)
		{
			return ETo(significantDigits);
		}

		RequireExponentialArgumentInRange(x);

		int working = significantDigits + ExponentialGuardDigits;
		BigInteger powerOfTen = RoundToNearestInteger(Divide(x, Ln10To(working), working));
		int shift = ToExponentShift(powerOfTen);

		// Subtracting k · ln 10 cancels the integer digits of x, so the constant and the series both
		// have to be carried that much wider than the answer is wanted.
		int consumed = CountDigits(powerOfTen);
		PreciseNumber remainder = Subtract(x, Multiply(new(0, powerOfTen), Ln10To(working + consumed)));
		PreciseNumber series = ExpReduced(remainder, working + consumed);
		return ShiftExponent(series, shift).ReduceSignificance(significantDigits);
	}

	/// <summary>
	/// Returns e raised to a power, minus one.
	/// </summary>
	/// <param name="x">The power to raise e to.</param>
	/// <returns><c>e^x - 1</c>.</returns>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	/// <remarks>
	/// Near zero this is the exponential series with its leading one omitted rather than
	/// <c>Exp(x) - 1</c>, which would cancel away exactly the digits the function exists to keep:
	/// <c>ExpM1(1e-30)</c> is <c>1e-30</c>, not zero.
	/// </remarks>
	public static PreciseNumber ExpM1(PreciseNumber x) =>
		ExpM1(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns e raised to a power, minus one, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The power to raise e to.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns><c>e^x - 1</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	public static PreciseNumber ExpM1(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return Zero;
		}

		// Away from zero the subtraction cancels nothing worth keeping, and the range reduction
		// inside Exp is worth more than a series run on an unreduced argument.
		if (Abs(x) > DirectSeriesLimit)
		{
			int wide = significantDigits + ExponentialGuardDigits;
			return Subtract(Exp(x, wide), One).ReduceSignificance(significantDigits);
		}

		return ExpSeriesWithoutLeadingOne(x, significantDigits + ExponentialGuardDigits)
			.ReduceSignificance(significantDigits);
	}

	/// <summary>
	/// Returns two raised to a power.
	/// </summary>
	/// <param name="x">The power to raise two to.</param>
	/// <returns><c>2^x</c>.</returns>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	public static PreciseNumber Exp2(PreciseNumber x) =>
		Exp2(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns two raised to a power, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The power to raise two to.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns><c>2^x</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	/// <remarks>
	/// An integer power of two is produced by repeated squaring, which is exact. Anything else goes
	/// through <c>exp(x · ln 2)</c>.
	/// </remarks>
	public static PreciseNumber Exp2(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return One;
		}

		if (IsInteger(x))
		{
			return Two.Pow(x);
		}

		return ExpOfProductWithConstant(x, Ln2To(significantDigits + ExponentialGuardDigits), significantDigits);
	}

	/// <summary>
	/// Returns two raised to a power, minus one.
	/// </summary>
	/// <param name="x">The power to raise two to.</param>
	/// <returns><c>2^x - 1</c>.</returns>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	public static PreciseNumber Exp2M1(PreciseNumber x) =>
		Exp2M1(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns two raised to a power, minus one, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The power to raise two to.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns><c>2^x - 1</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	/// <remarks>Routed through <see cref="ExpM1(PreciseNumber, int)"/>, so a small argument keeps its digits.</remarks>
	public static PreciseNumber Exp2M1(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return Zero;
		}

		int working = significantDigits + ExponentialGuardDigits;
		return ExpM1(Multiply(x, Ln2To(working + IntegerDigitCount(x))), significantDigits);
	}

	/// <summary>
	/// Returns ten raised to a power.
	/// </summary>
	/// <param name="x">The power to raise ten to.</param>
	/// <returns><c>10^x</c>.</returns>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	/// <remarks>
	/// Does not route through the natural logarithm. An integer power of ten is an exponent and
	/// nothing else — <c>Exp10(50)</c> is one significand and a shift, with no series run at all —
	/// and a fractional power pays only for its fractional part.
	/// </remarks>
	public static PreciseNumber Exp10(PreciseNumber x) =>
		Exp10(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns ten raised to a power, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The power to raise ten to.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns><c>10^x</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	public static PreciseNumber Exp10(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return One;
		}

		RequireExponentialArgumentInRange(x);

		// Split off the whole power of ten, which the exponent field carries for free. What is left
		// is in [0, 1), so an integer argument never reaches a series at all.
		BigInteger wholePart = FloorToInteger(x);
		int shift = ToExponentShift(wholePart);
		PreciseNumber fraction = Subtract(x, new(0, wholePart));

		int working = significantDigits + ExponentialGuardDigits;
		PreciseNumber series = fraction.Significand.IsZero
			? One
			: Exp(Multiply(fraction, Ln10To(working)), working);

		return ShiftExponent(series, shift).ReduceSignificance(significantDigits);
	}

	/// <summary>
	/// Returns ten raised to a power, minus one.
	/// </summary>
	/// <param name="x">The power to raise ten to.</param>
	/// <returns><c>10^x - 1</c>.</returns>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	public static PreciseNumber Exp10M1(PreciseNumber x) =>
		Exp10M1(x, DefaultExponentialPrecision(x));

	/// <summary>
	/// Returns ten raised to a power, minus one, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The power to raise ten to.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns><c>10^x - 1</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	/// <remarks>Routed through <see cref="ExpM1(PreciseNumber, int)"/>, so a small argument keeps its digits.</remarks>
	public static PreciseNumber Exp10M1(PreciseNumber x, int significantDigits)
	{
		RequireSignificantDigits(significantDigits);

		if (x.Significand.IsZero)
		{
			return Zero;
		}

		int working = significantDigits + ExponentialGuardDigits;
		return ExpM1(Multiply(x, Ln10To(working + IntegerDigitCount(x))), significantDigits);
	}

	/// <summary>
	/// Returns one value raised to the power of another.
	/// </summary>
	/// <param name="x">The base.</param>
	/// <param name="y">The exponent.</param>
	/// <returns><c>x^y</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> is negative and <paramref name="y"/> is not an integer.</exception>
	/// <exception cref="OverflowException">Thrown when the result needs an exponent outside the range of an <see cref="int"/>.</exception>
	/// <remarks>
	/// An integer exponent is exact, by repeated squaring. Anything else is
	/// <c>exp(y · ln x)</c>, carried wide enough that the digits the exponential's range reduction
	/// consumes are digits it was given rather than digits it invents.
	/// </remarks>
	public static PreciseNumber Pow(PreciseNumber x, PreciseNumber y) =>
		x.Pow(y);

	/// <summary>
	/// Computes <c>exp(y · ln x)</c> for a positive base and a non-integer exponent.
	/// </summary>
	/// <param name="x">The base, which must be positive.</param>
	/// <param name="y">The exponent.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns><c>x^y</c>.</returns>
	/// <remarks>
	/// The exponential factors out <c>10^k</c>, and forming <c>r</c> cancels every integer digit of
	/// <c>y · ln x</c>. Those digits therefore have to be present in the logarithm before the
	/// exponential asks for them, which is what the second, wider pass buys.
	/// </remarks>
	private static PreciseNumber FractionalPow(PreciseNumber x, PreciseNumber y, int significantDigits)
	{
		int working = significantDigits + ExponentialGuardDigits;
		PreciseNumber product = Multiply(y, Log(x, working));
		int consumed = IntegerDigitCount(product);

		if (consumed > 0)
		{
			product = Multiply(y, Log(x, working + consumed));
		}

		return Exp(product, significantDigits + consumed).ReduceSignificance(significantDigits);
	}

	/// <summary>
	/// Gets the significant digits an exponential or logarithm produces when the caller does not choose.
	/// </summary>
	/// <param name="value">The value being operated on.</param>
	/// <returns>The significant digits of <paramref name="value"/>, or <see cref="MinimumDivisionPrecision"/> if that is more.</returns>
	/// <remarks>
	/// The same rule <see cref="Divide(PreciseNumber, PreciseNumber)"/> and the roots follow, so that
	/// a constant carrying <see cref="ConstantPrecision"/> digits does not silently cap the
	/// expression at fifty.
	/// </remarks>
	private static int DefaultExponentialPrecision(PreciseNumber value) =>
		Math.Max(value.SignificantDigits, MinimumDivisionPrecision);

	/// <summary>
	/// Throws when a caller asks for fewer than one significant digit.
	/// </summary>
	/// <param name="significantDigits">The requested significant digits.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	private static void RequireSignificantDigits(int significantDigits)
	{
		if (significantDigits < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(significantDigits), significantDigits, "At least one significant digit is required.");
		}
	}

	/// <summary>
	/// Throws when an exponential's argument is so large that its result cannot be represented.
	/// </summary>
	/// <param name="x">The argument of the exponential.</param>
	/// <exception cref="OverflowException">Thrown when <paramref name="x"/> has more integer digits than any representable result could need.</exception>
	/// <remarks>
	/// The power of ten an exponential factors out has to fit the <see cref="Exponent"/> field, which
	/// bounds the argument long before this does. Rejecting the absurd cases up front keeps the
	/// intermediate arithmetic — in particular the power of ten a floor divides by — small enough to
	/// be worth computing.
	/// </remarks>
	private static void RequireExponentialArgumentInRange(PreciseNumber x)
	{
		if (IntegerDigitCount(x) > ExponentialArgumentDigitLimit)
		{
			throw new OverflowException(
				$"An exponential of a value with {IntegerDigitCount(x).ToString(InvariantCulture)} integer digits needs an exponent outside the range of an int.");
		}
	}

	/// <summary>
	/// Narrows a power of ten to the exponent shift that carries it.
	/// </summary>
	/// <param name="powerOfTen">The power of ten to carry in the exponent.</param>
	/// <returns>The same value as an <see cref="int"/>.</returns>
	/// <exception cref="OverflowException">Thrown when the power of ten does not fit an <see cref="int"/>.</exception>
	private static int ToExponentShift(BigInteger powerOfTen) =>
		BigInteger.Abs(powerOfTen) > MaximumExponentShift
		? throw new OverflowException(
			$"A result scaled by 10^{powerOfTen.ToString(InvariantCulture)} needs an exponent outside the range of an int.")
		: (int)powerOfTen;

	/// <summary>
	/// Multiplies a value by a power of ten by moving its exponent rather than its digits.
	/// </summary>
	/// <param name="value">The value to scale.</param>
	/// <param name="shift">The power of ten to scale by.</param>
	/// <returns><paramref name="value"/> multiplied by <c>10^<paramref name="shift"/></c>.</returns>
	/// <exception cref="OverflowException">Thrown when the shifted exponent does not fit an <see cref="int"/>.</exception>
	private static PreciseNumber ShiftExponent(PreciseNumber value, int shift)
	{
		if (shift == 0 || value.Significand.IsZero)
		{
			return value;
		}

		long shifted = (long)value.Exponent + shift;
		return shifted is < int.MinValue or > int.MaxValue
			? throw new OverflowException(
				$"A result scaled by 10^{shift.ToString(InvariantCulture)} needs an exponent outside the range of an int.")
			: new((int)shifted, value.Significand);
	}

	/// <summary>
	/// Counts the digits a value carries ahead of its decimal point.
	/// </summary>
	/// <param name="value">The value to measure.</param>
	/// <returns>The number of integer digits, or zero when the value is less than one in magnitude.</returns>
	private static int IntegerDigitCount(PreciseNumber value)
	{
		if (value.Significand.IsZero)
		{
			return 0;
		}

		long decimalExponent = (long)value.Exponent + value.SignificantDigits - 1;
		return decimalExponent < 0 ? 0 : (int)Math.Min(decimalExponent + 1, int.MaxValue);
	}

	/// <summary>
	/// Splits a positive value into a mantissa centred on one and the power of ten it was scaled by.
	/// </summary>
	/// <param name="value">The value to split, which must be positive.</param>
	/// <returns>A mantissa in <c>[1/√10, √10)</c> and the exponent such that their product is <paramref name="value"/>.</returns>
	/// <remarks>
	/// The representation already holds a mantissa in <c>[1, 10)</c>, so the split costs no
	/// arithmetic at all. Centring it further halves the worst-case series argument, and the test for
	/// it is exact: <c>m > √10</c> exactly when <c>m² > 10</c>, and squaring is exact.
	/// </remarks>
	private static (PreciseNumber Mantissa, long DecimalExponent) SplitAroundRootTen(PreciseNumber value)
	{
		int digits = value.SignificantDigits;
		PreciseNumber mantissa = new(-(digits - 1), value.Significand);
		long decimalExponent = (long)value.Exponent + digits - 1;

		if (Multiply(mantissa, mantissa) > Ten)
		{
			mantissa = new(mantissa.Exponent - 1, mantissa.Significand);
			decimalExponent++;
		}

		return (mantissa, decimalExponent);
	}

	/// <summary>
	/// Computes the natural logarithm of a mantissa centred on one.
	/// </summary>
	/// <param name="mantissa">The mantissa, in <c>[1/√10, √10)</c>.</param>
	/// <param name="workingDigits">The significant digits to carry through the series.</param>
	/// <returns>The natural logarithm of <paramref name="mantissa"/>.</returns>
	/// <remarks>
	/// <c>ln m = 2 · atanh((m - 1) / (m + 1))</c>. Over the centred interval the argument never
	/// exceeds about <c>0.52</c>, so each term of the series gains a little over half a digit.
	/// </remarks>
	private static PreciseNumber LogMantissa(PreciseNumber mantissa, int workingDigits)
	{
		if (mantissa.IsUnit)
		{
			return Zero;
		}

		PreciseNumber z = Divide(Subtract(mantissa, One), Add(mantissa, One), workingDigits);
		return Multiply(Two, AtanhSeries(z, workingDigits));
	}

	/// <summary>
	/// Sums the inverse hyperbolic tangent series.
	/// </summary>
	/// <param name="z">The argument, whose magnitude must be below one.</param>
	/// <param name="workingDigits">The significant digits to carry through the sum.</param>
	/// <returns><c>atanh z</c>.</returns>
	/// <exception cref="ArithmeticException">Thrown when the series does not converge.</exception>
	/// <remarks>
	/// <c>atanh z = z + z³/3 + z⁵/5 + …</c>. Every term is positive when <c>z</c> is, and shares its
	/// sign otherwise, so nothing cancels and the sum stops as soon as a term falls below the last
	/// digit being carried.
	/// </remarks>
	private static PreciseNumber AtanhSeries(PreciseNumber z, int workingDigits)
	{
		if (z.Significand.IsZero)
		{
			return Zero;
		}

		PreciseNumber zSquared = Multiply(z, z).ReduceSignificance(workingDigits);
		PreciseNumber term = z;
		PreciseNumber sum = z;

		for (int denominator = 3; denominator <= SeriesIterationAllowance(workingDigits); denominator += 2)
		{
			term = Multiply(term, zSquared).ReduceSignificance(workingDigits);
			if (term.Significand.IsZero)
			{
				return sum;
			}

			PreciseNumber next = Add(sum, Divide(term, new(0, denominator), workingDigits))
				.ReduceSignificance(workingDigits);

			if (next == sum)
			{
				return sum;
			}

			sum = next;
		}

		throw new ArithmeticException(
			$"The logarithm series did not converge to {workingDigits.ToString(InvariantCulture)} significant digits.");
	}

	/// <summary>
	/// Computes the exponential of a value already reduced below <c>ln(10) / 2</c>.
	/// </summary>
	/// <param name="r">The reduced argument.</param>
	/// <param name="workingDigits">The significant digits to carry through the series.</param>
	/// <returns><c>e^r</c>.</returns>
	/// <remarks>
	/// Halving the argument before the series and squaring the result back afterwards trades a
	/// handful of multiplications for most of the terms. Each squaring doubles whatever relative
	/// error it is handed, so the sum is carried one digit wider per halving.
	/// </remarks>
	private static PreciseNumber ExpReduced(PreciseNumber r, int workingDigits)
	{
		if (r.Significand.IsZero)
		{
			return One;
		}

		int halvings = 0;
		PreciseNumber reduced = r;

		while (halvings < MaximumHalvings && Abs(reduced) > SeriesArgumentLimit)
		{
			// Halving terminates, so this is exact whatever precision is asked of it.
			reduced = Divide(reduced, Two, workingDigits);
			halvings++;
		}

		int series = workingDigits + halvings;
		PreciseNumber result = Add(One, ExpSeriesWithoutLeadingOne(reduced, series));

		for (int squaring = 0; squaring < halvings; squaring++)
		{
			result = Multiply(result, result).ReduceSignificance(series);
		}

		return result;
	}

	/// <summary>
	/// Sums the exponential series with its leading one omitted.
	/// </summary>
	/// <param name="x">The argument.</param>
	/// <param name="workingDigits">The significant digits to carry through the sum.</param>
	/// <returns><c>e^x - 1</c>.</returns>
	/// <exception cref="ArithmeticException">Thrown when the series does not converge.</exception>
	/// <remarks>
	/// <c>Σ xⁿ/n!</c> from one. Omitting the leading term is what keeps a small argument: the sum is
	/// of the order of <paramref name="x"/> itself, so carrying it to
	/// <paramref name="workingDigits"/> significant digits keeps them relative to <c>x</c> rather
	/// than relative to one.
	/// </remarks>
	private static PreciseNumber ExpSeriesWithoutLeadingOne(PreciseNumber x, int workingDigits)
	{
		if (x.Significand.IsZero)
		{
			return Zero;
		}

		PreciseNumber term = x;
		PreciseNumber sum = x;

		for (int n = 2; n <= SeriesIterationAllowance(workingDigits); n++)
		{
			term = Divide(Multiply(term, x), new(0, n), workingDigits);
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
			$"The exponential series did not converge to {workingDigits.ToString(InvariantCulture)} significant digits.");
	}

	/// <summary>
	/// Computes the exponential of a value scaled by a stored constant.
	/// </summary>
	/// <param name="x">The value to scale.</param>
	/// <param name="constant">The logarithm of the base being raised.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns><c>e^(x · constant)</c>.</returns>
	private static PreciseNumber ExpOfProductWithConstant(PreciseNumber x, PreciseNumber constant, int significantDigits) =>
		Exp(Multiply(x, constant), significantDigits);

	/// <summary>
	/// Gets the terms a series is allowed before it is called non-convergent.
	/// </summary>
	/// <param name="workingDigits">The significant digits being carried.</param>
	/// <returns>The largest loop counter the series may reach.</returns>
	/// <remarks>
	/// The slowest series here is the atanh one at the edge of its centred interval, where the
	/// argument squares to about <c>0.27</c> and each term is therefore worth a little over half a
	/// digit. Four terms per digit leaves that a wide margin, and the constant covers the short sums
	/// where a digit count of one would otherwise allow almost no terms at all.
	/// </remarks>
	private static int SeriesIterationAllowance(int workingDigits) =>
		(workingDigits * 4) + 64;

	/// <summary>
	/// Rounds a value to the nearest integer, half away from negative infinity.
	/// </summary>
	/// <param name="value">The value to round.</param>
	/// <returns>The nearest integer to <paramref name="value"/>.</returns>
	private static BigInteger RoundToNearestInteger(PreciseNumber value) =>
		FloorToInteger(Add(value, Half));

	/// <summary>
	/// Takes the largest integer no greater than a value.
	/// </summary>
	/// <param name="value">The value to floor.</param>
	/// <returns>The floor of <paramref name="value"/>.</returns>
	private static BigInteger FloorToInteger(PreciseNumber value)
	{
		if (value.Significand.IsZero)
		{
			return BigInteger.Zero;
		}

		if (value.Exponent >= 0)
		{
			return value.Significand * Pow10(value.Exponent);
		}

		BigInteger quotient = BigInteger.DivRem(value.Significand, Pow10(-value.Exponent), out BigInteger remainder);

		// BigInteger division truncates towards zero, so a negative value with anything left over
		// has been rounded the wrong way for a floor.
		return remainder.Sign < 0 ? quotient - BigInteger.One : quotient;
	}
}
