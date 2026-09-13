// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber;

using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

/// <summary>
/// Represents a precise number.
/// </summary>
[DebuggerDisplay("{Significand}e{Exponent}")]
public record PreciseNumber
	: INumber<PreciseNumber>
{
	private const int Base10 = 10;

	/// <summary>
	/// Largest character buffer that is taken from the stack before falling back to the array pool.
	/// </summary>
	private const int MaxStackAllocChars = 256;

	/// <summary>
	/// Number of powers of ten pre-computed when the type is first used.
	/// </summary>
	private const int Pow10InitialCacheSize = 128;

	/// <summary>
	/// Ceiling on the power of ten cache. Beyond this, powers are computed per call rather than
	/// retained, so that one extreme exponent cannot leave a large cache behind.
	/// </summary>
	private const int Pow10MaxCacheSize = 1024;

	/// <summary>
	/// log10(2), used to derive a decimal digit count from a binary bit length.
	/// </summary>
	private const double Log10Of2 = 0.3010299956639812;

	/// <summary>
	/// The message carried by the <see cref="FormatException"/> that parsing throws.
	/// </summary>
	private const string InvalidFormatMessage = "Input string was not in a correct format.";

	/// <summary>
	/// Pre-computed powers of ten, grown on demand. Declared before any other static state so
	/// that the static constants below can rely on it while they are being initialized.
	/// </summary>
	/// <remarks>
	/// Growing replaces the array rather than filling the existing one. A <see cref="BigInteger"/>
	/// is a multi-field struct, so writing one into a shared array is not atomic and a concurrent
	/// reader could observe it half written. Publishing an already populated array through a
	/// single reference assignment cannot tear, and two threads growing at once simply build two
	/// correct arrays, one of which wins.
	/// </remarks>
	private static BigInteger[] pow10Cache = BuildPow10Cache(Pow10InitialCacheSize, []);

	/// <summary>
	/// Builds a power of ten cache of the given size, reusing the entries already computed.
	/// </summary>
	/// <param name="size">The number of powers the new cache should hold.</param>
	/// <param name="existing">The entries to carry over, which must be a prefix of the new cache.</param>
	/// <returns>The populated cache.</returns>
	private static BigInteger[] BuildPow10Cache(int size, BigInteger[] existing)
	{
		BigInteger[] cache = new BigInteger[size];
		existing.CopyTo(cache, 0);

		BigInteger value = existing.Length == 0 ? BigInteger.One : existing[^1] * Base10;
		for (int i = existing.Length; i < size; i++)
		{
			cache[i] = value;
			value *= Base10;
		}

		return cache;
	}

	/// <summary>
	/// Raises ten to the specified non-negative power, serving it from a cache.
	/// </summary>
	/// <param name="exponent">The power to raise ten to.</param>
	/// <returns>Ten raised to <paramref name="exponent"/>.</returns>
	internal static BigInteger Pow10(int exponent)
	{
		BigInteger[] cache = pow10Cache;
		return (uint)exponent < (uint)cache.Length
			? cache[exponent]
			: GrowCacheAndGetPow10(exponent, cache);
	}

	/// <summary>
	/// Extends the power of ten cache to cover an exponent it does not yet reach.
	/// </summary>
	/// <param name="exponent">The power to raise ten to.</param>
	/// <param name="current">The cache as it was read by the caller.</param>
	/// <returns>Ten raised to <paramref name="exponent"/>.</returns>
	/// <remarks>
	/// Every digit count and every exponent alignment needs a power of ten, so a value wider than
	/// the cache would otherwise pay for a fresh <see cref="BigInteger.Pow"/> on every single
	/// operation. That produced a cliff at the cache boundary rather than a gradual slope.
	/// </remarks>
	private static BigInteger GrowCacheAndGetPow10(int exponent, BigInteger[] current)
	{
		if (exponent is < 0 or > Pow10MaxCacheSize)
		{
			return BigInteger.Pow(Base10, exponent);
		}

		int size = Math.Min(Math.Max(current.Length * 2, exponent + 1), Pow10MaxCacheSize + 1);
		BigInteger[] grown = BuildPow10Cache(size, current);
		pow10Cache = grown;
		return grown[exponent];
	}

	/// <summary>
	/// Counts the decimal digits in the absolute value of a <see cref="BigInteger"/>.
	/// </summary>
	/// <param name="value">The value to count the digits of.</param>
	/// <returns>The number of decimal digits, or zero when <paramref name="value"/> is zero.</returns>
	/// <remarks>
	/// Derives an estimate from the bit length in constant time and corrects it with at most a
	/// couple of comparisons, rather than dividing the value down one digit at a time.
	/// </remarks>
	internal static int CountDigits(BigInteger value)
	{
		if (value.IsZero)
		{
			return 0;
		}

		BigInteger magnitude = BigInteger.Abs(value);
		long bitLength = magnitude.GetBitLength();

		// 2^(bitLength - 1) <= magnitude, so this never overestimates the digit count.
		int digits = (int)((bitLength - 1) * Log10Of2) + 1;

		while (digits > 1 && magnitude < Pow10(digits - 1))
		{
			digits--;
		}

		while (magnitude >= Pow10(digits))
		{
			digits++;
		}

		return digits;
	}

	/// <summary>
	/// Counts how many trailing decimal zeros a value has, up to a known upper bound.
	/// </summary>
	/// <param name="value">The value to inspect. Must not be zero.</param>
	/// <param name="maxZeros">An upper bound on the number of trailing zeros.</param>
	/// <returns>The number of trailing decimal zeros.</returns>
	/// <remarks>
	/// Binary searches on divisibility so the cost is logarithmic in the digit count instead of
	/// linear, which matters for significands with many digits.
	/// </remarks>
	private static int CountTrailingZeros(BigInteger value, int maxZeros)
	{
		if (maxZeros <= 0 || !(value % Base10).IsZero)
		{
			return 0;
		}

		int low = 1;
		int high = maxZeros;
		while (low < high)
		{
			int middle = low + ((high - low + 1) / 2);
			if ((value % Pow10(middle)).IsZero)
			{
				low = middle;
			}
			else
			{
				high = middle - 1;
			}
		}

		return low;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PreciseNumber"/> record by copying the values from an existing instance.
	/// </summary>
	/// <param name="original">The <see cref="PreciseNumber"/> instance to copy.</param>
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="original"/> is <c>null</c>.</exception>
	public PreciseNumber(PreciseNumber original)
	{
		Ensure.NotNull(original);
		Exponent = original.Exponent;
		Significand = original.Significand;
		SignificantDigits = original.SignificantDigits;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PreciseNumber"/> record.
	/// </summary>
	/// <param name="exponent">The exponent of the number.</param>
	/// <param name="significand">The significand of the number.</param>
	protected internal PreciseNumber(int exponent, BigInteger significand)
		: this(exponent, significand, true)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="PreciseNumber"/> record.
	/// </summary>
	/// <param name="exponent">The exponent of the number.</param>
	/// <param name="significand">The significand of the number.</param>
	/// <param name="sanitize">If true, trailing zeros in the significand will be removed.</param>
	protected internal PreciseNumber(int exponent, BigInteger significand, bool sanitize)
	{
		if (significand.IsZero)
		{
			Exponent = sanitize ? 0 : exponent;
			Significand = BigInteger.Zero;
			SignificantDigits = 0;
			return;
		}

		int significantDigits = CountDigits(significand);

		if (sanitize)
		{
			// The leading digit is non-zero, so at most significantDigits - 1 zeros can trail.
			int trailingZeros = CountTrailingZeros(significand, significantDigits - 1);
			if (trailingZeros > 0)
			{
				significand /= Pow10(trailingZeros);
				exponent += trailingZeros;
				significantDigits -= trailingZeros;
			}
		}

		SignificantDigits = significantDigits;
		Exponent = exponent;
		Significand = significand;
	}

	/// <summary>
	/// Gets the value -1 for the type.
	/// </summary>
	public static PreciseNumber NegativeOne { get; } = new(0, -1);

	/// <inheritdoc/>
	public static PreciseNumber One { get; } = new(0, 1);

	/// <inheritdoc/>
	public static PreciseNumber Zero { get; } = new(0, 0);

	private const int EExponent = -40;

	/// <summary>
	/// Gets the value of e for the type.
	/// </summary>
	public static PreciseNumber E { get; } = new(EExponent, BigInteger.Parse("27182818284590452353602874713526624977572", InvariantCulture));

	private const int PiExponent = -25;

	/// <summary>
	/// Gets the value of pi for the type.
	/// </summary>
	public static PreciseNumber Pi { get; } = new(PiExponent, BigInteger.Parse("31415926535897932384626433", InvariantCulture));

	private const int TauExponent = -24;

	/// <summary>
	/// Gets the value of tau for the type.
	/// </summary>
	public static PreciseNumber Tau { get; } = new(TauExponent, BigInteger.Parse("6283185307179586476925287", InvariantCulture));

	/// <summary>
	/// Gets the exponent of the number.
	/// </summary>
	public int Exponent { get; }

	/// <summary>
	/// Gets the significand of the number.
	/// </summary>
	public BigInteger Significand { get; }

	/// <summary>
	/// Gets the number of significant digits in the number.
	/// </summary>
	public int SignificantDigits { get; }

	/// <summary>
	/// Gets the invariant culture information.
	/// </summary>
	protected internal static CultureInfo InvariantCulture { get; } = CultureInfo.InvariantCulture;

	private const int BinaryRadix = 2;

	/// <inheritdoc/>
	public static int Radix => BinaryRadix;

	/// <inheritdoc/>
	public static PreciseNumber AdditiveIdentity => Zero;

	/// <inheritdoc/>
	public static PreciseNumber MultiplicativeIdentity => One;

	/// <inheritdoc/>
	public virtual bool Equals(PreciseNumber? other) =>
		other is not null && Equal(this, other);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Exponent, Significand);

	/// <inheritdoc/>
	public override string ToString() => ToString(this, null, null);

	/// <inheritdoc/>
	public string ToString(IFormatProvider? formatProvider) => ToString(this, null, formatProvider);

	/// <inheritdoc/>
	public string ToString(string format) => ToString(this, format, null);

	/// <summary>
	/// Converts the current instance to its equivalent string representation using the specified format and format provider.
	/// </summary>
	/// <param name="number">The <see cref="PreciseNumber"/> number to convert.</param>
	/// <param name="format">A numeric format string.</param>
	/// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
	/// <returns>A string representation of the current instance.</returns>
	public static string ToString(PreciseNumber number, string? format, IFormatProvider? formatProvider)
	{
		Ensure.NotNull(number);

		NumberFormatInfo numberFormat = NumberFormatInfo.GetInstance(formatProvider ?? InvariantCulture);

		// Digits, plus the padding zeros implied by the exponent, plus the sign, the decimal
		// separator and a possible leading "0".
		int desiredAlloc = number.SignificantDigits
			+ int.Abs(number.Exponent)
			+ numberFormat.NegativeSign.Length
			+ numberFormat.NumberDecimalSeparator.Length
			+ 1;

		char[]? rentedBuffer = desiredAlloc > MaxStackAllocChars ? ArrayPool<char>.Shared.Rent(desiredAlloc) : null;
		Span<char> stackBuffer = stackalloc char[MaxStackAllocChars];
		Span<char> buffer = rentedBuffer is null ? stackBuffer : rentedBuffer.AsSpan();

		try
		{
			return number.TryFormat(buffer, out int charsWritten, format.AsSpan(), formatProvider)
				? buffer[..charsWritten].ToString()
				: string.Empty;
		}
		finally
		{
			if (rentedBuffer is not null)
			{
				ArrayPool<char>.Shared.Return(rentedBuffer);
			}
		}
	}

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? formatProvider) => ToString(this, format, formatProvider);

	/// <summary>
	/// Returns the absolute value of the current instance.
	/// </summary>
	/// <returns>The absolute value of the current instance.</returns>
	public PreciseNumber Abs() => Abs(this);

	/// <summary>
	/// Rounds the current instance to the specified number of decimal digits.
	/// </summary>
	/// <param name="decimalDigits">The number of decimal digits to round to.</param>
	/// <returns>A new instance of <see cref="PreciseNumber"/> rounded to the specified number of decimal digits.</returns>
	public PreciseNumber Round(int decimalDigits)
	{
		int currentDecimalDigits = CountDecimalDigits();
		int decimalDifference = int.Abs(decimalDigits - currentDecimalDigits);
		if (currentDecimalDigits > decimalDigits && decimalDifference > 0)
		{
			BigInteger roundingFactor = BigInteger.CopySign(CreateRepeatingDigits(5, decimalDifference), Significand);
			BigInteger newSignificand = (Significand + roundingFactor) / Pow10(decimalDifference);
			int newExponent = Exponent - int.CopySign(decimalDifference, Exponent);
			return new PreciseNumber(newExponent, newSignificand);
		}

		return this;
	}

	/// <summary>
	/// Clamps the specified value between the minimum and maximum values.
	/// </summary>
	/// <typeparam name="TNumber">The type of the value to clamp.</typeparam>
	/// <param name="min">The minimum value.</param>
	/// <param name="max">The maximum value.</param>
	/// <returns>The clamped value.</returns>
	public PreciseNumber Clamp<TNumber>(TNumber min, TNumber max)
		where TNumber : INumber<TNumber>
	{
		PreciseNumber sigMin = min.ToPreciseNumber();
		PreciseNumber sigMax = max.ToPreciseNumber();
		PreciseNumber clampedToMax = this > sigMax ? sigMax : this;
		return this < sigMin ? sigMin : clampedToMax;
	}

	internal static PreciseNumber CreateFromComponents(int exponent, BigInteger significand) =>
		new(exponent, significand);

	internal static PreciseNumber CreateFromComponents(int exponent, BigInteger significand, bool sanitize) =>
		new(exponent, significand, sanitize);

	/// <summary>
	/// Creates a <see cref="PreciseNumber"/> from a floating point value.
	/// </summary>
	/// <typeparam name="TFloat">The type of the floating point value.</typeparam>
	/// <param name="input">The floating point value.</param>
	/// <returns>A <see cref="PreciseNumber"/> representing the floating point value.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when the input is infinite or NaN.</exception>
	internal static PreciseNumber CreateFromFloatingPoint<TFloat>(TFloat input)
		where TFloat : INumber<TFloat>
	{
		if (TFloat.IsInfinity(input))
		{
			throw new ArgumentOutOfRangeException(nameof(input), "Infinite values are not supported");
		}
		else if (TFloat.IsNaN(input))
		{
			throw new ArgumentOutOfRangeException(nameof(input), "NaN values are not supported");
		}

		AssertDoesImplementGenericInterface(typeof(TFloat), typeof(IFloatingPoint<>));

		if (TFloat.IsZero(input))
		{
			return Zero;
		}
		else if (input == TFloat.One)
		{
			return One;
		}
		else if (input == -TFloat.One)
		{
			return NegativeOne;
		}

		return CreatePreciseNumberFromNonSpecialFloat(input);

	}

	private static PreciseNumber CreatePreciseNumberFromNonSpecialFloat<TFloat>(TFloat input)
		where TFloat : INumber<TFloat>
	{
		string format = GetStringFormatForFloatType<TFloat>();

		Span<char> rendered = stackalloc char[MaxStackAllocChars];
		return input.TryFormat(rendered, out int renderedLength, format.AsSpan(), InvariantCulture)
			? ParseRenderedFloat(rendered[..renderedLength])
			: ParseRenderedFloat(input.ToString(format, InvariantCulture).AsSpan());
	}

	/// <summary>
	/// Converts the round-trippable text of a floating point value into a <see cref="PreciseNumber"/>.
	/// </summary>
	/// <param name="text">The rendered value, optionally in scientific notation.</param>
	/// <returns>A <see cref="PreciseNumber"/> with the same value.</returns>
	private static PreciseNumber ParseRenderedFloat(ReadOnlySpan<char> text)
	{
		int exponentValue = 0;
		int exponentIndex = text.IndexOfAny('E', 'e');
		if (exponentIndex >= 0)
		{
			exponentValue = int.Parse(text[(exponentIndex + 1)..], NumberStyles.Integer, InvariantCulture);
			text = text[..exponentIndex];
		}

		bool isInteger = !text.Contains('.');

		while (text.Length > 2 && text[^1] == '0')
		{
			text = text[..^1];
			if (isInteger)
			{
				++exponentValue;
			}
		}

		int decimalIndex = text.IndexOf('.');
		ReadOnlySpan<char> integerComponent = decimalIndex < 0 ? text : text[..decimalIndex];
		ReadOnlySpan<char> fractionalComponent = decimalIndex < 0 ? "0".AsSpan() : text[(decimalIndex + 1)..];
		exponentValue -= fractionalComponent.Length;

		Debug.Assert(fractionalComponent.Length != 0 || integerComponent.TrimStart("-").Length == 1, $"Unexpected format: {text}");

		int digitLength = integerComponent.Length + fractionalComponent.Length;
		char[]? rentedDigits = digitLength > MaxStackAllocChars ? ArrayPool<char>.Shared.Rent(digitLength) : null;
		Span<char> stackDigits = stackalloc char[MaxStackAllocChars];
		Span<char> digits = rentedDigits is null ? stackDigits : rentedDigits.AsSpan();

		try
		{
			integerComponent.CopyTo(digits);
			fractionalComponent.CopyTo(digits[integerComponent.Length..]);

			return new(exponentValue, BigInteger.Parse(digits[..digitLength], NumberStyles.Integer, InvariantCulture));
		}
		finally
		{
			if (rentedDigits is not null)
			{
				ArrayPool<char>.Shared.Return(rentedDigits);
			}
		}
	}

	internal static string GetStringFormatForFloatType<TFloat>()
		where TFloat : INumber<TFloat>
	{
		return typeof(TFloat) switch
		{
			_ when typeof(TFloat) == typeof(float) => "E7",
			_ when typeof(TFloat) == typeof(double) => "E15",
			_ => "R",
		};
	}

	/// <summary>
	/// Creates a <see cref="PreciseNumber"/> from an integer value.
	/// </summary>
	/// <typeparam name="TInteger">The type of the integer value.</typeparam>
	/// <param name="input">The integer value.</param>
	/// <returns>A <see cref="PreciseNumber"/> representing the integer value.</returns>
	internal static PreciseNumber CreateFromInteger<TInteger>(TInteger input)
		where TInteger : INumber<TInteger>
	{
		AssertDoesImplementGenericInterface(typeof(TInteger), typeof(IBinaryInteger<>));

		bool isOne = input == TInteger.One;
		bool isNegativeOne = TInteger.IsNegative(input) && input == -TInteger.One;
		bool isZero = TInteger.IsZero(input);

		if (isZero)
		{
			return Zero;
		}

		if (isOne)
		{
			return One;
		}

		if (isNegativeOne)
		{
			return NegativeOne;
		}

		// The constructor sanitizes trailing zeros, so there is no need to do it again here.
		return new(0, BigInteger.CreateChecked(input));
	}

	/// <summary>
	/// Creates a repeating digit sequence of a specified length.
	/// </summary>
	/// <param name="digit">The digit to repeat.</param>
	/// <param name="numberOfRepeats">The number of times to repeat the digit.</param>
	/// <returns>A <see cref="BigInteger"/> representing the repeating digit sequence.</returns>
	internal static BigInteger CreateRepeatingDigits(int digit, int numberOfRepeats)
	{
		if (numberOfRepeats <= 0)
		{
			return 0;
		}

		// digit * (10^n - 1) / 9 is the repunit of length n scaled by the digit.
		return digit * (Pow10(numberOfRepeats) - BigInteger.One) / 9;
	}

	/// <summary>
	/// Gets a value indicating whether the current instance is exactly one in canonical form.
	/// </summary>
	private bool IsUnit => Exponent == 0 && Significand.IsOne;

	/// <summary>
	/// Gets a value indicating whether the current instance has infinite precision.
	/// </summary>
	internal bool HasInfinitePrecision =>
		Exponent == 0
		&& (Significand == BigInteger.One || Significand == BigInteger.Zero || Significand == BigInteger.MinusOne);

	/// <summary>
	/// Gets the lower of the decimal digit counts of two numbers.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	/// <returns>The lower of the decimal digit counts of the two numbers.</returns>
	protected internal static int LowestDecimalDigits(PreciseNumber left, PreciseNumber right)
	{
		Ensure.NotNull(left);
		Ensure.NotNull(right);

		int leftDecimalDigits = left.CountDecimalDigits();
		int rightDecimalDigits = right.CountDecimalDigits();

		leftDecimalDigits = left.HasInfinitePrecision ? rightDecimalDigits : leftDecimalDigits;
		rightDecimalDigits = right.HasInfinitePrecision ? leftDecimalDigits : rightDecimalDigits;

		return leftDecimalDigits < rightDecimalDigits
			? leftDecimalDigits
			: rightDecimalDigits;
	}

	/// <summary>
	/// Gets the lower of the significant digit counts of two numbers.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	/// <returns>The lower of the significant digit counts of the two numbers.</returns>
	protected internal static int LowestSignificantDigits(PreciseNumber left, PreciseNumber right)
	{
		Ensure.NotNull(left);
		Ensure.NotNull(right);

		int leftSignificantDigits = left.SignificantDigits;
		int rightSignificantDigits = right.SignificantDigits;

		leftSignificantDigits = left.HasInfinitePrecision ? rightSignificantDigits : leftSignificantDigits;
		rightSignificantDigits = right.HasInfinitePrecision ? leftSignificantDigits : rightSignificantDigits;

		return leftSignificantDigits < rightSignificantDigits
		? leftSignificantDigits
		: rightSignificantDigits;
	}

	/// <summary>
	/// Counts the number of decimal digits in the current instance.
	/// </summary>
	/// <returns>The number of decimal digits in the current instance.</returns>
	protected internal int CountDecimalDigits() =>
		Exponent > 0
		? 0
		: int.Abs(Exponent);

	/// <summary>
	/// Reduces the significance of the current instance to a specified number of significant digits.
	/// </summary>
	/// <param name="significantDigits">The number of significant digits to reduce to.</param>
	/// <returns>A new instance of <see cref="PreciseNumber"/> reduced to the specified number of significant digits.</returns>
	public PreciseNumber ReduceSignificance(int significantDigits)
	{
		int significantDifference = significantDigits < SignificantDigits
			? SignificantDigits - significantDigits
			: 0;

		if (significantDifference == 0)
		{
			return this;
		}

		int newExponent = Exponent == 0
			? significantDifference
			: Exponent + significantDifference;
		BigInteger roundingFactor = BigInteger.CopySign(CreateRepeatingDigits(5, significantDifference), Significand);
		BigInteger newSignificand = (Significand + roundingFactor) / Pow10(significantDifference);
		return new(newExponent, newSignificand);
	}

	/// <summary>
	/// Adjusts the exponents of two <see cref="PreciseNumber"/> instances to a common exponent.
	/// </summary>
	/// <param name="left">The left <see cref="PreciseNumber"/> instance.</param>
	/// <param name="right">The right <see cref="PreciseNumber"/> instance.</param>
	/// <returns>A tuple containing the commonized <see cref="PreciseNumber"/> instances.</returns>
	protected internal static (PreciseNumber, PreciseNumber) MakeCommonized(PreciseNumber left, PreciseNumber right)
	{
		(PreciseNumber commonLeft, PreciseNumber commonRight, int _) = MakeCommonizedWithExponent(left, right);
		return (commonLeft, commonRight);
	}

	/// <summary>
	/// Adjusts the exponents of two <see cref="PreciseNumber"/> instances to a common exponent and returns the common exponent.
	/// </summary>
	/// <param name="left">The left <see cref="PreciseNumber"/> instance.</param>
	/// <param name="right">The right <see cref="PreciseNumber"/> instance.</param>
	/// <returns>
	/// A tuple containing the commonized <see cref="PreciseNumber"/> instances and the common exponent.
	/// </returns>
	protected internal static (PreciseNumber, PreciseNumber, int) MakeCommonizedWithExponent(PreciseNumber left, PreciseNumber right)
	{
		Ensure.NotNull(left);
		Ensure.NotNull(right);

		int smallestExponent = left.Exponent < right.Exponent ? left.Exponent : right.Exponent;
		int exponentDifferenceLeft = Math.Abs(left.Exponent - smallestExponent);
		int exponentDifferenceRight = Math.Abs(right.Exponent - smallestExponent);
		BigInteger newSignificandLeft = left.Significand * BigInteger.Pow(Base10, exponentDifferenceLeft);
		BigInteger newSignificandRight = right.Significand * BigInteger.Pow(Base10, exponentDifferenceRight);

		return (new(smallestExponent, newSignificandLeft, sanitize: false),
			new(smallestExponent, newSignificandRight, sanitize: false),
			smallestExponent);
	}

	/// <summary>
	/// Scales the significands of two numbers to a common exponent without allocating
	/// intermediate <see cref="PreciseNumber"/> instances.
	/// </summary>
	/// <param name="left">The left number.</param>
	/// <param name="right">The right number.</param>
	/// <returns>The scaled significands and the exponent they share.</returns>
	private static (BigInteger Left, BigInteger Right, int Exponent) CommonizeSignificands(PreciseNumber left, PreciseNumber right)
	{
		Ensure.NotNull(left);
		Ensure.NotNull(right);

		int leftExponent = left.Exponent;
		int rightExponent = right.Exponent;

		if (leftExponent == rightExponent)
		{
			return (left.Significand, right.Significand, leftExponent);
		}

		return leftExponent > rightExponent
			? (left.Significand * Pow10(leftExponent - rightExponent), right.Significand, rightExponent)
			: (left.Significand, right.Significand * Pow10(rightExponent - leftExponent), leftExponent);
	}

	/// <summary>
	/// Orders two numbers, returning a negative value, zero, or a positive value.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	/// <returns>A negative value if <paramref name="left"/> is smaller, zero if the two are equal, otherwise a positive value.</returns>
	/// <remarks>
	/// This is the single primitive behind every comparison operator. It short circuits on sign and
	/// on decimal magnitude so that significands only have to be scaled when the two numbers occupy
	/// the same decade.
	/// </remarks>
	private static int Compare(PreciseNumber left, PreciseNumber right)
	{
		Ensure.NotNull(left);
		Ensure.NotNull(right);

		int leftSign = left.Significand.Sign;
		int rightSign = right.Significand.Sign;

		if (leftSign != rightSign)
		{
			return leftSign < rightSign ? -1 : 1;
		}

		if (leftSign == 0)
		{
			return 0;
		}

		// A value lies in [10^(exponent + digits - 1), 10^(exponent + digits)), so a strictly larger
		// decimal magnitude always implies a strictly larger absolute value.
		long leftMagnitude = (long)left.Exponent + left.SignificantDigits;
		long rightMagnitude = (long)right.Exponent + right.SignificantDigits;

		if (leftMagnitude != rightMagnitude)
		{
			int magnitudeOrder = leftMagnitude < rightMagnitude ? -1 : 1;
			return leftSign < 0 ? -magnitudeOrder : magnitudeOrder;
		}

		(BigInteger commonLeft, BigInteger commonRight, _) = CommonizeSignificands(left, right);
		return BigInteger.Compare(commonLeft, commonRight);
	}

	/// <inheritdoc/>
	public int CompareTo(PreciseNumber? other) =>
		other is null ? 1 : Compare(this, other);

	/// <summary>
	/// Compares the current instance with another number of a specified type.
	/// </summary>
	/// <typeparam name="TNumber">The type of the number to compare with. Must implement <see cref="INumber{TNumber}"/>.</typeparam>
	/// <param name="obj">The number to compare with the current instance.</param>
	/// <returns>
	/// A value that indicates the relative order of the objects being compared.
	/// The return value has these meanings:
	/// <list type="table">
	/// <item>
	/// <term>Less than zero</term>
	/// <description>The current instance is less than <paramref name="obj"/>.</description>
	/// </item>
	/// <item>
	/// <term>Zero</term>
	/// <description>The current instance is equal to <paramref name="obj"/>.</description>
	/// </item>
	/// <item>
	/// <term>Greater than zero</term>
	/// <description>The current instance is greater than <paramref name="obj"/>.</description>
	/// </item>
	/// </list>
	/// </returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is <c>null</c>.</exception>
	public int CompareTo<TNumber>(INumber<TNumber>? obj)
		where TNumber : INumber<TNumber>
	{
		if (obj is null)
		{
			return 1;
		}

		PreciseNumber other = ((TNumber)obj).ToPreciseNumber();
		return CompareTo(other);
	}

	/// <inheritdoc/>
	public int CompareTo(object? obj)
	{
		return obj is PreciseNumber preciseNumber
			? CompareTo(preciseNumber)
			: throw new NotSupportedException();
	}

	/// <summary>
	/// Compares the current instance with another number.
	/// </summary>
	/// <typeparam name="TInput">The type of the other number.</typeparam>
	/// <param name="other">The number to compare with the current instance.</param>
	/// <returns>A value indicating whether the current instance is less than, equal to, or greater than the other number.</returns>
	public int CompareTo<TInput>(TInput other)
		where TInput : INumber<TInput>
	{
		if (other is null)
		{
			return 1;
		}

		return Compare(this, other.ToPreciseNumber());
	}

	/// <inheritdoc/>
	public static PreciseNumber Abs(PreciseNumber value)
	{
		Ensure.NotNull(value);
		return value.Significand.Sign < 0 ? -value : value;
	}

	/// <inheritdoc/>
	public static bool IsCanonical(PreciseNumber value) => true;

	/// <inheritdoc/>
	public static bool IsComplexNumber(PreciseNumber value) => !IsRealNumber(value);

	/// <inheritdoc/>
	public static bool IsEvenInteger(PreciseNumber value) => IsInteger(value) && value.Significand.IsEven;

	/// <inheritdoc/>
	public static bool IsFinite(PreciseNumber value) => true;

	/// <inheritdoc/>
	public static bool IsImaginaryNumber(PreciseNumber value) => !IsRealNumber(value);

	/// <inheritdoc/>
	public static bool IsInfinity(PreciseNumber value) => !IsFinite(value);

	/// <inheritdoc/>
	public static bool IsInteger(PreciseNumber value)
	{
		Ensure.NotNull(value);
		return value.Exponent >= 0;
	}

	/// <inheritdoc/>
	public static bool IsNaN(PreciseNumber value) => false;

	/// <inheritdoc/>
	public static bool IsNegative(PreciseNumber value) => !IsPositive(value);

	/// <inheritdoc/>
	public static bool IsNegativeInfinity(PreciseNumber value) => IsInfinity(value) && IsNegative(value);

	/// <summary>
	/// Determines whether the specified value is normal.
	/// </summary>
	/// <param name="value">The PreciseNumber.</param>
	/// <returns><c>true</c> if the specified value is normal; otherwise, <c>false</c>.</returns>
	public static bool IsNormal(PreciseNumber value) => true;

	/// <inheritdoc/>
	public static bool IsOddInteger(PreciseNumber value) => IsInteger(value) && !value.Significand.IsEven;

	/// <inheritdoc/>
	public static bool IsPositive(PreciseNumber value)
	{
		Ensure.NotNull(value);
		return value.Significand >= 0;
	}

	/// <inheritdoc/>
	public static bool IsPositiveInfinity(PreciseNumber value) => IsInfinity(value) && IsPositive(value);

	/// <inheritdoc/>
	public static bool IsRealNumber(PreciseNumber value) => true;

	/// <inheritdoc/>
	public static bool IsSubnormal(PreciseNumber value) => !IsNormal(value);

	/// <inheritdoc/>
	public static bool IsZero(PreciseNumber value)
	{
		Ensure.NotNull(value);
		return value.Significand == 0;
	}

	/// <inheritdoc/>
	public static PreciseNumber MaxMagnitude(PreciseNumber x, PreciseNumber y)
	{
		Ensure.NotNull(x);
		Ensure.NotNull(y);

		return x.Abs() >= y.Abs() ? x : y;
	}

	/// <inheritdoc/>
	public static PreciseNumber MaxMagnitudeNumber(PreciseNumber x, PreciseNumber y) => MaxMagnitude(x, y);

	/// <inheritdoc/>
	public static PreciseNumber MinMagnitude(PreciseNumber x, PreciseNumber y)
	{
		Ensure.NotNull(x);
		Ensure.NotNull(y);

		return x.Abs() <= y.Abs() ? x : y;
	}

	/// <inheritdoc/>
	public static PreciseNumber MinMagnitudeNumber(PreciseNumber x, PreciseNumber y) => MinMagnitude(x, y);

	/// <inheritdoc/>
	public static PreciseNumber Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
	{
		if (s.IsEmpty)
		{
			throw new FormatException(InvalidFormatMessage);
		}

		if (s.Length == 1 && s[0] == '0')
		{
			return Zero;
		}

		bool isNegative = s[0] == '-';
		int startIndex = isNegative ? 1 : 0;

		// Collect the digits first and hand them to BigInteger in one go. Accumulating with
		// significand = significand * 10 + digit costs a full BigInteger multiply per character.
		char[]? rentedDigits = s.Length > MaxStackAllocChars ? ArrayPool<char>.Shared.Rent(s.Length) : null;
		Span<char> stackDigits = stackalloc char[MaxStackAllocChars];
		Span<char> digits = rentedDigits is null ? stackDigits : rentedDigits.AsSpan();

		try
		{
			int digitCount = 0;
			int exponent = 0;
			bool hasDecimal = false;
			int decimalDigits = 0;

			for (int i = startIndex; i < s.Length; i++)
			{
				char c = s[i];
				if (c == '.')
				{
					if (hasDecimal)
					{
						throw new FormatException(InvalidFormatMessage);
					}

					hasDecimal = true;
					continue;
				}

				if (c is 'e' or 'E')
				{
					exponent = int.Parse(s[(i + 1)..], InvariantCulture);
					break;
				}

				if (c is < '0' or > '9')
				{
					throw new FormatException(InvalidFormatMessage);
				}

				if (hasDecimal)
				{
					decimalDigits++;
				}

				digits[digitCount++] = c;
			}

			if (digitCount == 0)
			{
				throw new FormatException(InvalidFormatMessage);
			}

			BigInteger significand = BigInteger.Parse(digits[..digitCount], NumberStyles.None, InvariantCulture);

			exponent -= decimalDigits;

			if (isNegative)
			{
				significand = -significand;
			}

			return new(exponent, significand);
		}
		finally
		{
			if (rentedDigits is not null)
			{
				ArrayPool<char>.Shared.Return(rentedDigits);
			}
		}
	}

	/// <inheritdoc/>
	public static PreciseNumber Parse(string s, NumberStyles style, IFormatProvider? provider) =>
		Parse(s.AsSpan(), style, provider);

	/// <inheritdoc/>
	public static PreciseNumber Parse(string s, IFormatProvider? provider) =>
		Parse(s, NumberStyles.Any, provider);

	/// <inheritdoc/>
	public static PreciseNumber Parse(ReadOnlySpan<char> s, IFormatProvider? provider) =>
		Parse(s, NumberStyles.Any, provider);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)][NotNullWhen(true)] out PreciseNumber result)
	{
		try
		{
			result = Parse(s, style, provider);
			return true;
		}
		catch (FormatException)
		{
			result = default;
			return false;
		}
	}

	/// <inheritdoc/>
	public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [NotNullWhen(true)] out PreciseNumber? result) =>
		TryParse(s.AsSpan(), style, provider, out result);

	/// <inheritdoc/>
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [NotNullWhen(true)] out PreciseNumber? result) =>
		TryParse(s.AsSpan(), NumberStyles.Any, provider, out result);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [NotNullWhen(true)] out PreciseNumber? result) =>
		TryParse(s, NumberStyles.Any, provider, out result);

	/// <inheritdoc/>
	public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
	{
		if (!format.IsEmpty && !format.Equals("G", StringComparison.OrdinalIgnoreCase))
		{
			throw new FormatException();
		}

		if (Significand.IsZero)
		{
			charsWritten = 0;
			if (destination.IsEmpty)
			{
				return false;
			}

			destination[0] = '0';
			charsWritten = 1;
			return true;
		}

		NumberFormatInfo numberFormat = NumberFormatInfo.GetInstance(provider ?? InvariantCulture);

		int digitCount = SignificantDigits;
		char[]? rentedDigits = digitCount > MaxStackAllocChars ? ArrayPool<char>.Shared.Rent(digitCount) : null;
		Span<char> stackDigits = stackalloc char[MaxStackAllocChars];
		Span<char> digitBuffer = rentedDigits is null ? stackDigits : rentedDigits.AsSpan();

		try
		{
			if (!BigInteger.Abs(Significand).TryFormat(digitBuffer, out int digitsWritten, default, InvariantCulture))
			{
				charsWritten = 0;
				return false;
			}

			return TryWriteDigits(destination, digitBuffer[..digitsWritten], numberFormat, out charsWritten);
		}
		finally
		{
			if (rentedDigits is not null)
			{
				ArrayPool<char>.Shared.Return(rentedDigits);
			}
		}
	}

	/// <summary>
	/// Places the already rendered significand digits into <paramref name="destination"/>, inserting the
	/// sign, padding zeros and decimal separator required by this number's exponent.
	/// </summary>
	private bool TryWriteDigits(Span<char> destination, ReadOnlySpan<char> digits, NumberFormatInfo numberFormat, out int charsWritten)
	{
		charsWritten = 0;

		ReadOnlySpan<char> sign = default;
		if (Significand.Sign < 0)
		{
			sign = numberFormat.NegativeSign;
		}

		if (Exponent >= 0)
		{
			int wholeLength = sign.Length + digits.Length + Exponent;
			if (destination.Length < wholeLength)
			{
				return false;
			}

			sign.CopyTo(destination);
			digits.CopyTo(destination[sign.Length..]);
			destination.Slice(sign.Length + digits.Length, Exponent).Fill('0');
			charsWritten = wholeLength;
			return true;
		}

		ReadOnlySpan<char> separator = numberFormat.NumberDecimalSeparator;
		int fractionalDigits = -Exponent;
		int integralDigits = digits.Length - fractionalDigits;

		// When the exponent consumes every digit the integral part is a single "0" and the
		// fractional part is padded out to the full width with leading zeros.
		int integralLength = integralDigits > 0 ? integralDigits : 1;
		int required = sign.Length + integralLength + separator.Length + fractionalDigits;

		if (destination.Length < required)
		{
			return false;
		}

		int position = 0;
		sign.CopyTo(destination);
		position += sign.Length;

		if (integralDigits > 0)
		{
			digits[..integralDigits].CopyTo(destination[position..]);
			position += integralDigits;
			separator.CopyTo(destination[position..]);
			position += separator.Length;
			digits[integralDigits..].CopyTo(destination[position..]);
		}
		else
		{
			destination[position++] = '0';
			separator.CopyTo(destination[position..]);
			position += separator.Length;
			destination.Slice(position, fractionalDigits - digits.Length).Fill('0');
			position += fractionalDigits - digits.Length;
			digits.CopyTo(destination[position..]);
		}

		charsWritten = required;
		return true;
	}

	/// <inheritdoc/>
	public static bool TryConvertFromChecked<TOther>(TOther value, out PreciseNumber result)
		where TOther : INumberBase<TOther>
		=> throw new NotSupportedException();

	/// <inheritdoc/>
	public static bool TryConvertFromSaturating<TOther>(TOther value, out PreciseNumber result)
		where TOther : INumberBase<TOther>
		=> throw new NotSupportedException();

	/// <inheritdoc/>
	public static bool TryConvertFromTruncating<TOther>(TOther value, out PreciseNumber result)
		where TOther : INumberBase<TOther>
		=> throw new NotSupportedException();

	/// <inheritdoc/>
	public static bool TryConvertToChecked<TOther>(PreciseNumber value, out TOther result)
		where TOther : INumberBase<TOther>
		=> throw new NotSupportedException();

	/// <inheritdoc/>
	public static bool TryConvertToSaturating<TOther>(PreciseNumber value, out TOther result)
		where TOther : INumberBase<TOther>
		=> throw new NotSupportedException();

	/// <inheritdoc/>
	public static bool TryConvertToTruncating<TOther>(PreciseNumber value, out TOther result)
		where TOther : INumberBase<TOther>
		=> throw new NotSupportedException();

	/// <summary>
	/// Asserts that the exponents of two numbers match.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	protected internal static void AssertExponentsMatch(PreciseNumber left, PreciseNumber right)
	{
		Ensure.NotNull(left);
		Ensure.NotNull(right);

		Debug.Assert(left.Exponent == right.Exponent, $"{nameof(AssertExponentsMatch)}: {left.Exponent} == {right.Exponent}");
	}

	/// <summary>
	/// Negates a number.
	/// </summary>
	/// <param name="value">The number to negate.</param>
	/// <returns>The negated number.</returns>
	public static PreciseNumber Negate(PreciseNumber value)
	{
		Ensure.NotNull(value);
		return value.Significand.IsZero
			? value
			: new(value.Exponent, -value.Significand);
	}

	/// <summary>
	/// Subtracts one number from another.
	/// </summary>
	/// <param name="left">The number to subtract from.</param>
	/// <param name="right">The number to subtract.</param>
	/// <returns>The result of the subtraction.</returns>
	public static PreciseNumber Subtract(PreciseNumber left, PreciseNumber right)
	{
		(BigInteger commonLeft, BigInteger commonRight, int commonExponent) = CommonizeSignificands(left, right);
		return new PreciseNumber(commonExponent, commonLeft - commonRight);
	}

	/// <summary>
	/// Adds two numbers.
	/// </summary>
	/// <param name="left">The first number to add.</param>
	/// <param name="right">The second number to add.</param>
	/// <returns>The result of the addition.</returns>
	public static PreciseNumber Add(PreciseNumber left, PreciseNumber right)
	{
		(BigInteger commonLeft, BigInteger commonRight, int commonExponent) = CommonizeSignificands(left, right);
		return new PreciseNumber(commonExponent, commonLeft + commonRight);
	}

	/// <summary>
	/// Multiplies two numbers.
	/// </summary>
	/// <param name="left">The first number to multiply.</param>
	/// <param name="right">The second number to multiply.</param>
	/// <returns>The result of the multiplication.</returns>
	public static PreciseNumber Multiply(PreciseNumber left, PreciseNumber right)
	{
		Ensure.NotNull(left);
		Ensure.NotNull(right);

		if (left.Significand.IsZero || right.Significand.IsZero)
		{
			return Zero;
		}
		else if (left.IsUnit)
		{
			return right;
		}
		else if (right.IsUnit)
		{
			return left;
		}

		// (l * 10^el) * (r * 10^er) == (l * r) * 10^(el + er), so there is no need to scale the
		// operands to a common exponent first; doing so only inflates both significands.
		return new PreciseNumber(left.Exponent + right.Exponent, left.Significand * right.Significand);
	}

	/// <summary>
	/// Divides one number by another.
	/// </summary>
	/// <param name="left">The number to divide.</param>
	/// <param name="right">The number to divide by.</param>
	/// <returns>The result of the division.</returns>
	public static PreciseNumber Divide(PreciseNumber left, PreciseNumber right)
	{
		Ensure.NotNull(left);
		Ensure.NotNull(right);

		if (right.Significand.IsZero)
		{
			throw new DivideByZeroException();
		}

		if (Compare(left, right) == 0)
		{
			return One;
		}

		(BigInteger commonLeft, BigInteger commonRight, _) = CommonizeSignificands(left, right);

		BigInteger integerComponent = BigInteger.DivRem(commonLeft, commonRight, out BigInteger remainder);

		// The common power of ten cancels between the remainder and the divisor, so it is left out
		// entirely; including it would overflow to infinity for large exponents.
		double fractionalComponent = double.CreateTruncating(remainder) / double.CreateTruncating(commonRight);

		return new PreciseNumber(0, integerComponent) + fractionalComponent.ToPreciseNumber();
	}

	/// <summary>
	/// Computes the modulus of two numbers.
	/// </summary>
	/// <param name="left">The number to divide.</param>
	/// <param name="right">The number to divide by.</param>
	/// <returns>The modulus of the two numbers.</returns>
	public static PreciseNumber Mod(PreciseNumber left, PreciseNumber right)
	{
		Ensure.NotNull(left);
		Ensure.NotNull(right);

		if (right.Significand.IsZero)
		{
			throw new DivideByZeroException();
		}

		if (Compare(left, right) == 0)
		{
			return Zero;
		}

		(BigInteger commonLeft, BigInteger commonRight, int commonExponent) = CommonizeSignificands(left, right);

		return new PreciseNumber(commonExponent, BigInteger.Remainder(commonLeft, commonRight));
	}

	/// <summary>
	/// Increments the specified <see cref="PreciseNumber"/> by one.
	/// </summary>
	/// <param name="value">The <see cref="PreciseNumber"/> to increment.</param>
	/// <returns>A new <see cref="PreciseNumber"/> that is the result of incrementing the specified value by one.</returns>
	public static PreciseNumber Increment(PreciseNumber value) =>
	value + One;

	/// <summary>
	/// Decrements the specified <see cref="PreciseNumber"/> by one.
	/// </summary>
	/// <param name="value">The <see cref="PreciseNumber"/> to decrement.</param>
	/// <returns>A new <see cref="PreciseNumber"/> that is the result of decrementing the specified value by one.</returns>
	public static PreciseNumber Decrement(PreciseNumber value) =>
	value - One;

	/// <summary>
	/// Returns the unary plus of a number.
	/// </summary>
	/// <param name="value">The number.</param>
	/// <returns>The unary plus of the number.</returns>
	public static PreciseNumber Plus(PreciseNumber value) =>
		value;

	/// <summary>
	/// Determines whether one number is greater than another.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	/// <returns><c>true</c> if the first number is greater than the second; otherwise, <c>false</c>.</returns>
	public static bool GreaterThan(PreciseNumber left, PreciseNumber right) =>
		Compare(left, right) > 0;

	/// <summary>
	/// Determines whether one number is greater than or equal to another.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	/// <returns><c>true</c> if the first number is greater than or equal to the second; otherwise, <c>false</c>.</returns>
	public static bool GreaterThanOrEqual(PreciseNumber left, PreciseNumber right) =>
		Compare(left, right) >= 0;

	/// <summary>
	/// Determines whether one number is less than another.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	/// <returns><c>true</c> if the first number is less than the second; otherwise, <c>false</c>.</returns>
	public static bool LessThan(PreciseNumber left, PreciseNumber right) =>
		Compare(left, right) < 0;

	/// <summary>
	/// Determines whether one number is less than or equal to another.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	/// <returns><c>true</c> if the first number is less than or equal to the second; otherwise, <c>false</c>.</returns>
	public static bool LessThanOrEqual(PreciseNumber left, PreciseNumber right) =>
		Compare(left, right) <= 0;

	/// <summary>
	/// Determines whether two numbers are equal.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	/// <returns><c>true</c> if the two numbers are equal; otherwise, <c>false</c>.</returns>
	public static bool Equal(PreciseNumber left, PreciseNumber right) =>
		Compare(left, right) == 0;

	/// <summary>
	/// Determines whether two numbers are not equal.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	/// <returns><c>true</c> if the two numbers are not equal; otherwise, <c>false</c>.</returns>
	public static bool NotEqual(PreciseNumber left, PreciseNumber right) =>
		Compare(left, right) != 0;

	/// <summary>
	/// Returns the larger of two numbers.
	/// </summary>
	/// <param name="x">The first number.</param>
	/// <param name="y">The second number.</param>
	/// <returns>The larger of the two numbers.</returns>
	public static PreciseNumber Max(PreciseNumber x, PreciseNumber y) => x > y ? x : y;

	/// <summary>
	/// Returns the smaller of two numbers.
	/// </summary>
	/// <param name="x">The first number.</param>
	/// <param name="y">The second number.</param>
	/// <returns>The smaller of the two numbers.</returns>
	public static PreciseNumber Min(PreciseNumber x, PreciseNumber y) => x < y ? x : y;

	/// <summary>
	/// Clamps a number to the specified minimum and maximum values.
	/// </summary>
	/// <param name="value">The number to clamp.</param>
	/// <param name="min">The minimum value.</param>
	/// <param name="max">The maximum value.</param>
	/// <returns>The clamped number.</returns>
	public static PreciseNumber Clamp(PreciseNumber value, PreciseNumber min, PreciseNumber max)
	{
		Ensure.NotNull(value);
		return value.Clamp(min, max);
	}

	/// <summary>
	/// Rounds a number to the specified number of decimal digits.
	/// </summary>
	/// <param name="value">The number to round.</param>
	/// <param name="decimalDigits">The number of decimal digits to round to.</param>
	/// <returns>The rounded number.</returns>
	public static PreciseNumber Round(PreciseNumber value, int decimalDigits)
	{
		Ensure.NotNull(value);
		return value.Round(decimalDigits);
	}

	/// <summary>
	/// Returns the square of the current number.
	/// </summary>
	/// <returns>A new instance of <see cref="PreciseNumber"/> that is the square of the current instance.</returns>
	public PreciseNumber Squared() => this * this;

	/// <summary>
	/// Returns the cube of the current number.
	/// </summary>
	/// <returns>A new instance of <see cref="PreciseNumber"/> that is the cube of the current instance.</returns>
	public PreciseNumber Cubed() => Squared() * this;

	/// <summary>
	/// Returns the result of raising the current number to the specified power.
	/// </summary>
	/// <param name="power">The power to raise the number to.</param>
	/// <returns>A new instance of <see cref="PreciseNumber"/> that is the result of raising the current instance to the specified power.</returns>
	public PreciseNumber Pow(PreciseNumber power)
	{
		Ensure.NotNull(power);

		if (power.Significand.IsZero)
		{
			return One;
		}
		else if (Significand.IsZero)
		{
			return Zero;
		}
		else if (IsUnit)
		{
			return One;
		}

		if (IsInteger(power))
		{
			// Exponentiation by squaring: O(log n) multiplications instead of O(n).
			PreciseNumber result = One;
			PreciseNumber factor = this;

			for (int remaining = power.Abs().To<int>(); remaining > 0; remaining >>= 1)
			{
				if ((remaining & 1) != 0)
				{
					result *= factor;
				}

				if (remaining > 1)
				{
					factor = factor.Squared();
				}
			}

			return power.Significand.Sign < 0 ? One / result : result;
		}

		// Use logarithm and exponential to support decimal powers
		double logValue = Math.Log(To<double>());
		return Math.Exp(logValue * power.To<double>()).ToPreciseNumber();
	}

	/// <summary>
	/// Returns the result of raising e to the specified power.
	/// </summary>
	/// <param name="power">The power to raise e to.</param>
	/// <returns>A new instance of <see cref="PreciseNumber"/> that is the result of raising e to the specified power.</returns>
	public static PreciseNumber Exp(PreciseNumber power)
	{
		Ensure.NotNull(power);

		if (power.Significand.IsZero)
		{
			return One;
		}
		else if (power.IsUnit)
		{
			return E;
		}

		return Math.Exp(power.To<double>()).ToPreciseNumber();
	}

	/// <inheritdoc/>
	public static PreciseNumber operator -(PreciseNumber value) =>
		Negate(value);

	/// <inheritdoc/>
	public static PreciseNumber operator -(PreciseNumber left, PreciseNumber right) =>
		Subtract(left, right);

	/// <inheritdoc/>
	public static PreciseNumber operator *(PreciseNumber left, PreciseNumber right) =>
		Multiply(left, right);

	/// <inheritdoc/>
	public static PreciseNumber operator /(PreciseNumber left, PreciseNumber right) =>
		Divide(left, right);

	/// <inheritdoc/>
	public static PreciseNumber operator +(PreciseNumber value) =>
		Plus(value);

	/// <inheritdoc/>
	public static PreciseNumber operator +(PreciseNumber left, PreciseNumber right) =>
		Add(left, right);

	/// <inheritdoc/>
	public static bool operator >(PreciseNumber left, PreciseNumber right) =>
		GreaterThan(left, right);

	/// <inheritdoc/>
	public static bool operator <(PreciseNumber left, PreciseNumber right) =>
		LessThan(left, right);

	/// <inheritdoc/>
	public static bool operator >=(PreciseNumber left, PreciseNumber right) =>
		GreaterThanOrEqual(left, right);

	/// <inheritdoc/>
	public static bool operator <=(PreciseNumber left, PreciseNumber right) =>
		LessThanOrEqual(left, right);

	/// <inheritdoc/>
	public static PreciseNumber operator %(PreciseNumber left, PreciseNumber right) =>
		Mod(left, right);

	/// <inheritdoc/>
	public static PreciseNumber operator --(PreciseNumber value) =>
		Decrement(value);

	/// <inheritdoc/>
	public static PreciseNumber operator ++(PreciseNumber value) =>
		Increment(value);

	/// <summary>
	/// Caches the <see cref="PreciseNumber"/> copy constructor of a derived type so that
	/// <see cref="As{TOutput}"/> only reflects over each type once.
	/// </summary>
	private static class CopyConstructorOf<TOutput>
		where TOutput : PreciseNumber
	{
		internal static readonly System.Reflection.ConstructorInfo? Constructor =
			typeof(TOutput).GetConstructor([typeof(PreciseNumber)]);
	}

	/// <summary>
	/// Asserts that a type implements a specified generic interface.
	/// </summary>
	/// <param name="type">The type to check.</param>
	/// <param name="genericInterface">The generic interface to check for.</param>
	/// <exception cref="ArgumentException">Thrown when the specified type does not implement the generic interface.</exception>
	internal static void AssertDoesImplementGenericInterface(Type type, Type genericInterface) =>
		Debug.Assert(DoesImplementGenericInterface(type, genericInterface), $"{type.Name} does not implement {genericInterface.Name}");

	/// <summary>
	/// Determines whether a type implements a specified generic interface.
	/// </summary>
	/// <param name="type">The type to check.</param>
	/// <param name="genericInterface">The generic interface to check for.</param>
	/// <returns><c>true</c> if the type implements the generic interface; otherwise, <c>false</c>.</returns>
	/// <exception cref="ArgumentException">Thrown when the specified type is not a valid generic interface.</exception>
	internal static bool DoesImplementGenericInterface(Type type, Type genericInterface)
	{
		bool genericInterfaceIsValid = genericInterface.IsInterface && genericInterface.IsGenericType;

		return genericInterfaceIsValid
			? Array.Exists(type.GetInterfaces(), x => x.IsGenericType && x.GetGenericTypeDefinition() == genericInterface)
			: throw new ArgumentException($"{genericInterface.Name} is not a generic interface");
	}

	/// <summary>
	/// Converts the current number to the specified numeric type.
	/// </summary>
	/// <typeparam name="TOutput">The type to convert to. Must implement <see cref="INumber{TOutput}"/>.</typeparam>
	/// <returns>The converted value of the number as type <typeparamref name="TOutput"/>.</returns>
	/// <exception cref="OverflowException">
	/// Thrown if the conversion cannot be performed. This may occur if the target type cannot represent
	/// the value of the number.
	/// </exception>
	public TOutput To<TOutput>()
		where TOutput : INumber<TOutput> =>
		typeof(TOutput) == typeof(PreciseNumber)
		? (TOutput)(object)this
		: TOutput.CreateChecked(Significand) * TOutput.CreateChecked(Math.Pow(Base10, Exponent));

	/// <summary>
	/// Converts the current instance to the specified derived type of <see cref="PreciseNumber"/>.
	/// </summary>
	/// <typeparam name="TOutput">The type to convert to. Must derive from <see cref="PreciseNumber"/>.</typeparam>
	/// <returns>
	/// An instance of type <typeparamref name="TOutput"/> representing the current instance.
	/// </returns>
	/// <exception cref="NotSupportedException">
	/// Thrown if the conversion cannot be performed. This may occur if the target type does not have a constructor
	/// that accepts a <see cref="PreciseNumber"/> as a parameter.
	/// </exception>
	public TOutput As<TOutput>()
	where TOutput : PreciseNumber
	{
		if (typeof(TOutput) == typeof(PreciseNumber))
		{
			return (TOutput)(object)this;
		}

		System.Reflection.ConstructorInfo? constructor = CopyConstructorOf<TOutput>.Constructor;
		return (TOutput)(constructor?.Invoke([this]) ??
		throw new NotSupportedException($"Cannot convert {GetType()} to {typeof(TOutput)}"));
	}
}
