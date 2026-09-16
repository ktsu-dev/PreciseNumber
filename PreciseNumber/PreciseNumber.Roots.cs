// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber;

using System;
using System.Numerics;

/// <summary>
/// Square, cube and n-th roots, and the hypotenuse built on them.
/// </summary>
/// <remarks>
/// Every root is taken on the significand as an integer rather than on the number as a whole. A
/// value is <c>significand × 10^exponent</c>, so scaling the significand by a power of ten until the
/// exponent divides by the degree splits the problem in two: the exponent is rooted by division
/// alone, and the significand is rooted by integer Newton, which terminates on an exact answer
/// instead of on a tolerance. A value whose root is exact therefore comes back exact, however many
/// digits were asked for, the same way <see cref="Divide(PreciseNumber, PreciseNumber)"/> is exact
/// when a quotient terminates.
/// </remarks>
public readonly partial record struct PreciseNumber
	: IRootFunctions<PreciseNumber>
{
	/// <summary>
	/// Digits computed past the ones the caller asked for, so that the digit the final rounding
	/// decision is made on is itself correct.
	/// </summary>
	private const int RootGuardDigits = 2;

	/// <summary>
	/// Iterations allowed to a root beyond its degree before it is called non-convergent.
	/// </summary>
	/// <remarks>
	/// The descent below starts at most twice the root and is strictly decreasing, so it cannot
	/// loop. The allowance grows with the degree because the first phase closes a fixed fraction of
	/// the gap per step, and that fraction is <c>1/n</c>; measured over degrees up to 2000 the
	/// worst case needs four iterations more than the degree.
	/// </remarks>
	private const int RootIterationAllowance = 64;

	/// <summary>
	/// The message carried by the <see cref="ArgumentOutOfRangeException"/> thrown for an even root
	/// of a negative value.
	/// </summary>
	private const string NegativeRootMessage = "A negative value has no real root of an even degree.";

	/// <summary>
	/// Returns the square root of a value.
	/// </summary>
	/// <param name="x">The value to take the square root of.</param>
	/// <returns>The positive square root of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> is negative.</exception>
	/// <remarks>
	/// A perfect square roots exactly. Anything else is produced to the significant digits of
	/// <paramref name="x"/>, and never fewer than <see cref="MinimumDivisionPrecision"/>, matching
	/// <see cref="Divide(PreciseNumber, PreciseNumber)"/>. Use
	/// <see cref="Sqrt(PreciseNumber, int)"/> to choose that precision.
	/// <para>
	/// <see cref="PreciseNumber"/> has no NaN, so a negative value throws where a
	/// <see cref="double"/> would quietly return NaN and carry on. An algorithm ported from
	/// <see cref="double"/> that relies on that has to test the sign itself.
	/// </para>
	/// </remarks>
	public static PreciseNumber Sqrt(PreciseNumber x) =>
		Sqrt(x, DefaultRootPrecision(x));

	/// <summary>
	/// Returns the square root of a value, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value to take the square root of.</param>
	/// <param name="significantDigits">
	/// The number of significant digits to produce. A root that is exact is exact regardless of this
	/// value.
	/// </param>
	/// <returns>The positive square root of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="x"/> is negative, or when <paramref name="significantDigits"/> is
	/// less than one.
	/// </exception>
	public static PreciseNumber Sqrt(PreciseNumber x, int significantDigits) =>
		RootN(x, 2, significantDigits);

	/// <summary>
	/// Returns the cube root of a value.
	/// </summary>
	/// <param name="x">The value to take the cube root of.</param>
	/// <returns>The cube root of <paramref name="x"/>, which carries its sign.</returns>
	/// <remarks>
	/// A perfect cube roots exactly. Anything else is produced to the significant digits of
	/// <paramref name="x"/>, and never fewer than <see cref="MinimumDivisionPrecision"/>. The cube
	/// root of a negative value is real, so it is returned rather than rejected.
	/// </remarks>
	public static PreciseNumber Cbrt(PreciseNumber x) =>
		RootN(x, 3);

	/// <summary>
	/// Returns the cube root of a value, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value to take the cube root of.</param>
	/// <param name="significantDigits">
	/// The number of significant digits to produce. A root that is exact is exact regardless of this
	/// value.
	/// </param>
	/// <returns>The cube root of <paramref name="x"/>, which carries its sign.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	public static PreciseNumber Cbrt(PreciseNumber x, int significantDigits) =>
		RootN(x, 3, significantDigits);

	/// <summary>
	/// Returns the n-th root of a value.
	/// </summary>
	/// <param name="x">The value to take the root of.</param>
	/// <param name="n">The degree of the root.</param>
	/// <returns>The n-th root of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="n"/> is zero or <see cref="int.MinValue"/>, or when
	/// <paramref name="x"/> is negative and <paramref name="n"/> is even.
	/// </exception>
	/// <exception cref="DivideByZeroException">Thrown when <paramref name="x"/> is zero and <paramref name="n"/> is negative.</exception>
	/// <remarks>
	/// A root that is exact is produced exactly. Anything else is produced to the significant digits
	/// of <paramref name="x"/>, and never fewer than <see cref="MinimumDivisionPrecision"/>.
	/// </remarks>
	public static PreciseNumber RootN(PreciseNumber x, int n) =>
		RootN(x, n, DefaultRootPrecision(x));

	/// <summary>
	/// Returns the n-th root of a value, to a chosen number of significant digits.
	/// </summary>
	/// <param name="x">The value to take the root of.</param>
	/// <param name="n">The degree of the root.</param>
	/// <param name="significantDigits">
	/// The number of significant digits to produce. A root that is exact is exact regardless of this
	/// value.
	/// </param>
	/// <returns>The n-th root of <paramref name="x"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <paramref name="n"/> is zero or <see cref="int.MinValue"/>, when
	/// <paramref name="x"/> is negative and <paramref name="n"/> is even, or when
	/// <paramref name="significantDigits"/> is less than one.
	/// </exception>
	/// <exception cref="DivideByZeroException">Thrown when <paramref name="x"/> is zero and <paramref name="n"/> is negative.</exception>
	/// <exception cref="ArithmeticException">Thrown when the root does not converge.</exception>
	public static PreciseNumber RootN(PreciseNumber x, int n, int significantDigits)
	{
		if (significantDigits < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(significantDigits), significantDigits, "At least one significant digit is required.");
		}

		// A degree of int.MinValue has no negation that fits an int, so the reciprocal below cannot
		// express it. Zero asks for a root that is not a number at all.
		if (n is 0 or int.MinValue)
		{
			throw new ArgumentOutOfRangeException(nameof(n), n, "The degree of a root must be a non-zero value other than int.MinValue.");
		}

		if (n < 0)
		{
			return x.Significand.IsZero
				? throw new DivideByZeroException()
				: Divide(One, RootN(x, -n, significantDigits), significantDigits);
		}

		if (x.Significand.IsZero || n == 1)
		{
			return x;
		}

		if (x.Significand.Sign > 0)
		{
			return PositiveRootN(x, n, significantDigits);
		}

		// An odd root of a negative value is real, so the sign comes out and goes back on.
		return int.IsEvenInteger(n)
			? throw new ArgumentOutOfRangeException(nameof(x), x, NegativeRootMessage)
			: -PositiveRootN(-x, n, significantDigits);
	}

	/// <summary>
	/// Returns the length of the hypotenuse of a right triangle with the given side lengths.
	/// </summary>
	/// <param name="x">The length of one side.</param>
	/// <param name="y">The length of the other side.</param>
	/// <returns>The square root of <c>x² + y²</c>.</returns>
	/// <remarks>
	/// Computed directly, without the scaling a <see cref="double"/> implementation needs. That
	/// scaling exists to keep <c>x²</c> inside a fixed exponent range, and a
	/// <see cref="PreciseNumber"/> has no such range: squaring and adding are both exact, so the
	/// value handed to the square root is the exact sum whatever the magnitudes involved.
	/// </remarks>
	public static PreciseNumber Hypot(PreciseNumber x, PreciseNumber y) =>
		Hypot(x, y, Math.Max(DefaultRootPrecision(x), DefaultRootPrecision(y)));

	/// <summary>
	/// Returns the length of the hypotenuse of a right triangle with the given side lengths, to a
	/// chosen number of significant digits.
	/// </summary>
	/// <param name="x">The length of one side.</param>
	/// <param name="y">The length of the other side.</param>
	/// <param name="significantDigits">
	/// The number of significant digits to produce. A result that is exact is exact regardless of
	/// this value.
	/// </param>
	/// <returns>The square root of <c>x² + y²</c>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="significantDigits"/> is less than one.</exception>
	public static PreciseNumber Hypot(PreciseNumber x, PreciseNumber y, int significantDigits) =>
		Sqrt(Add(Multiply(x, x), Multiply(y, y)), significantDigits);

	/// <summary>
	/// Gets the significant digits a root produces when the caller does not choose.
	/// </summary>
	/// <param name="value">The value being rooted.</param>
	/// <returns>The significant digits of <paramref name="value"/>, or <see cref="MinimumDivisionPrecision"/> if that is more.</returns>
	/// <remarks>
	/// The same rule <see cref="Divide(PreciseNumber, PreciseNumber)"/> follows, so that rooting a
	/// constant carrying <see cref="ConstantPrecision"/> digits does not silently cap the expression
	/// at fifty.
	/// </remarks>
	private static int DefaultRootPrecision(PreciseNumber value) =>
		Math.Max(value.SignificantDigits, MinimumDivisionPrecision);

	/// <summary>
	/// Computes the n-th root of a positive value.
	/// </summary>
	/// <param name="value">The value to take the root of, which must be positive.</param>
	/// <param name="n">The degree of the root, which must be at least two.</param>
	/// <param name="significantDigits">The number of significant digits to produce.</param>
	/// <returns>The n-th root of <paramref name="value"/>.</returns>
	/// <exception cref="OverflowException">Thrown when the root needs a scale or an exponent wider than an <see cref="int"/>.</exception>
	private static PreciseNumber PositiveRootN(PreciseNumber value, int n, int significantDigits)
	{
		// root(s · 10^(e - k)) == root(s · 10^k') · 10^((e - k) / n) once n divides e - k, so
		// scaling the significand until that holds leaves an integer root and an exact exponent.
		int digits = CountDigits(value.Significand);
		long wanted = (long)n * (significantDigits + RootGuardDigits);
		long scale = Math.Max(wanted - digits, 0);

		// Raise the scale to the next value that leaves an exponent the degree divides.
		scale += (((value.Exponent - scale) % n) + n) % n;

		long rootExponent = (value.Exponent - scale) / n;
		if (scale > int.MaxValue || rootExponent is < int.MinValue or > int.MaxValue)
		{
			throw new OverflowException(
				$"A root of degree {n.ToString(InvariantCulture)} to {significantDigits.ToString(InvariantCulture)} significant digits needs an exponent outside the range of an int.");
		}

		BigInteger scaled = scale > 0 ? value.Significand * Pow10((int)scale) : value.Significand;
		BigInteger root = IntegerRootN(scaled, n);
		PreciseNumber result = new((int)rootExponent, root);

		// An exact root has every digit of its own, so rounding it to the requested precision would
		// throw away an answer that is already right.
		return BigInteger.Pow(root, n) == scaled
			? result
			: result.ReduceSignificance(significantDigits);
	}

	/// <summary>
	/// Computes the largest integer whose n-th power does not exceed a value.
	/// </summary>
	/// <param name="value">The value to take the root of, which must be positive.</param>
	/// <param name="n">The degree of the root, which must be at least two.</param>
	/// <returns>The floor of the n-th root of <paramref name="value"/>.</returns>
	/// <exception cref="ArithmeticException">Thrown when the iteration does not converge.</exception>
	/// <remarks>
	/// Newton on integers, <c>x ← ((n-1)·x + v / x^(n-1)) / n</c>. Started above the root it is
	/// strictly decreasing and stops at the floor of the root exactly, so there is no tolerance to
	/// pick and no working precision to carry: every digit it returns is correct.
	/// </remarks>
	private static BigInteger IntegerRootN(BigInteger value, int n)
	{
		if (value.IsZero || value.IsOne)
		{
			return value;
		}

		// value < 2^bits, so 2^ceil(bits / n) is above the root and within a factor of two of it.
		int power = (int)((value.GetBitLength() + n - 1) / n);
		BigInteger estimate = BigInteger.One << power;

		for (int iteration = 0; iteration <= RootIterationAllowance + n; iteration++)
		{
			BigInteger next = (((n - 1) * estimate) + (value / BigInteger.Pow(estimate, n - 1))) / n;
			if (next >= estimate)
			{
				return estimate;
			}

			estimate = next;
		}

		throw new ArithmeticException(
			$"The root of degree {n.ToString(InvariantCulture)} of a {CountDigits(value).ToString(InvariantCulture)} digit value did not converge.");
	}
}
