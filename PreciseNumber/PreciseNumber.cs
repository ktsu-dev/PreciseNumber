[assembly: CLSCompliant(true)]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
namespace ktsu.PreciseNumber;

using System;
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
		if (sanitize)
		{
			if (significand == 0)
			{
				Exponent = 0;
				Significand = 0;
				SignificantDigits = 0;
				return;
			}

			// remove trailing zeros
			while (significand != 0 && significand % Base10 == 0)
			{
				significand /= Base10;
				exponent++;
			}
		}

		// count digits
		int significantDigits = 0;
		var number = significand;
		while (number != 0)
		{
			significantDigits++;
			number /= Base10;
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
		ArgumentNullException.ThrowIfNull(number);

		int desiredAlloc = int.Abs(number.Exponent) + number.SignificantDigits + 2; // +2 is for negative symbol and decimal symbol
		int stackAlloc = Math.Min(desiredAlloc, 128);
		Span<char> buffer = stackAlloc == desiredAlloc
			? stackalloc char[stackAlloc]
			: new char[desiredAlloc];

		return number.TryFormat(buffer, out int charsWritten, format.AsSpan(), formatProvider)
			? buffer[..charsWritten].ToString()
			: string.Empty;
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
			var roundingFactor = BigInteger.CopySign(CreateRepeatingDigits(5, decimalDifference), Significand);
			var newSignificand = (Significand + roundingFactor) / BigInteger.Pow(Base10, decimalDifference);
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
		var sigMin = min.ToPreciseNumber();
		var sigMax = max.ToPreciseNumber();
		var clampedToMax = this > sigMax ? sigMax : this;
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
		ArgumentNullException.ThrowIfNull(input);

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
		string significandString = input.ToString(format, InvariantCulture).ToUpperInvariant();
		var significandSpan = significandString.AsSpan();

		int exponentValue = 0;
		if (significandString.Contains('E', StringComparison.OrdinalIgnoreCase))
		{
			string[] expComponents = significandString.Split('E');
			Debug.Assert(expComponents.Length == 2, $"Unexpected format: {significandString}");
			significandSpan = expComponents[0].AsSpan();
			exponentValue = int.Parse(expComponents[1], InvariantCulture);
		}

		bool isInteger = !significandSpan.Contains('.');

		while (significandSpan.Length > 2 && significandSpan[^1] == '0')
		{
			significandSpan = significandSpan[..^1];
			if (isInteger)
			{
				++exponentValue;
			}
		}

		string[] components = significandSpan.ToString().Split('.');
		Debug.Assert(components.Length <= 2, $"Invalid format: {significandSpan}");

		var integerComponent = components[0].AsSpan();
		var fractionalComponent = components.Length == 2 ? components[1].AsSpan() : "0".AsSpan();
		int fractionalLength = fractionalComponent.Length;
		exponentValue -= fractionalLength;

		Debug.Assert(fractionalLength != 0 || integerComponent.TrimStart("-").Length == 1, $"Unexpected format: {integerComponent}.{fractionalComponent}");

		string significandStrWithoutDecimal = $"{integerComponent}{fractionalComponent}";
		var significandValue = BigInteger.Parse(significandStrWithoutDecimal, InvariantCulture);

		return new(exponentValue, significandValue);
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
		ArgumentNullException.ThrowIfNull(input);
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

		int exponentValue = 0;
		var significandValue = BigInteger.CreateChecked(input);
		while (significandValue != 0 && significandValue % Base10 == 0)
		{
			significandValue /= Base10;
			exponentValue++;
		}

		return new(exponentValue, significandValue);
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

		BigInteger repeatingDigit = digit;
		for (int i = 1; i < numberOfRepeats; i++)
		{
			repeatingDigit = (repeatingDigit * Base10) + digit;
		}

		return repeatingDigit;
	}

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
		ArgumentNullException.ThrowIfNull(left);
		ArgumentNullException.ThrowIfNull(right);

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
		ArgumentNullException.ThrowIfNull(left);
		ArgumentNullException.ThrowIfNull(right);

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
		var roundingFactor = BigInteger.CopySign(CreateRepeatingDigits(5, significantDifference), Significand);
		var newSignificand = (Significand + roundingFactor) / BigInteger.Pow(Base10, significantDifference);
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
		var (commonLeft, commonRight, _) = MakeCommonizedWithExponent(left, right);
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
		ArgumentNullException.ThrowIfNull(left);
		ArgumentNullException.ThrowIfNull(right);

		int smallestExponent = left.Exponent < right.Exponent ? left.Exponent : right.Exponent;
		int exponentDifferenceLeft = Math.Abs(left.Exponent - smallestExponent);
		int exponentDifferenceRight = Math.Abs(right.Exponent - smallestExponent);
		var newSignificandLeft = left.Significand * BigInteger.Pow(Base10, exponentDifferenceLeft);
		var newSignificandRight = right.Significand * BigInteger.Pow(Base10, exponentDifferenceRight);

		return (new(smallestExponent, newSignificandLeft, sanitize: false),
			new(smallestExponent, newSignificandRight, sanitize: false),
			smallestExponent);
	}

	/// <inheritdoc/>
	public int CompareTo(PreciseNumber? other)
	{
		if (other is null)
		{
			return 1;
		}

		int greaterOrEqual = this > other ? 1 : 0;
		return this < other ? -1 : greaterOrEqual;
	}

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

		var other = obj.ToPreciseNumber();
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

		var significantOther = other.ToPreciseNumber();
		int greaterOrEqual = this > significantOther ? 1 : 0;
		return this < significantOther ? -1 : greaterOrEqual;
	}

	/// <inheritdoc/>
	public static PreciseNumber Abs(PreciseNumber value)
	{
		ArgumentNullException.ThrowIfNull(value);
		return value.Significand < 0 ? -value : value;
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
		ArgumentNullException.ThrowIfNull(value);
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
		ArgumentNullException.ThrowIfNull(value);
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
		ArgumentNullException.ThrowIfNull(value);
		return value.Significand == 0;
	}

	/// <inheritdoc/>
	public static PreciseNumber MaxMagnitude(PreciseNumber x, PreciseNumber y)
	{
		ArgumentNullException.ThrowIfNull(x);
		ArgumentNullException.ThrowIfNull(y);

		return x.Abs() >= y.Abs() ? x : y;
	}

	/// <inheritdoc/>
	public static PreciseNumber MaxMagnitudeNumber(PreciseNumber x, PreciseNumber y) => MaxMagnitude(x, y);

	/// <inheritdoc/>
	public static PreciseNumber MinMagnitude(PreciseNumber x, PreciseNumber y)
	{
		ArgumentNullException.ThrowIfNull(x);
		ArgumentNullException.ThrowIfNull(y);

		return x.Abs() <= y.Abs() ? x : y;
	}

	/// <inheritdoc/>
	public static PreciseNumber MinMagnitudeNumber(PreciseNumber x, PreciseNumber y) => MinMagnitude(x, y);

	/// <inheritdoc/>
	public static PreciseNumber Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
	{
		if (s.IsEmpty)
		{
			throw new FormatException("Input string was not in a correct format.");
		}

		if (s.Length == 1 && s[0] == '0')
		{
			return Zero;
		}

		bool isNegative = s[0] == '-';
		int startIndex = isNegative ? 1 : 0;
		int exponent = 0;
		BigInteger significand = 0;
		bool hasDecimal = false;
		int decimalDigits = 0;

		for (int i = startIndex; i < s.Length; i++)
		{
			char c = s[i];
			if (c == '.')
			{
				if (hasDecimal)
				{
					throw new FormatException("Input string was not in a correct format.");
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
				throw new FormatException("Input string was not in a correct format.");
			}

			if (hasDecimal)
			{
				decimalDigits++;
			}

			significand = (significand * Base10) + (c - '0');
		}

		exponent -= decimalDigits;

		if (isNegative)
		{
			significand = -significand;
		}

		return new(exponent, significand);
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
		int requiredLength = SignificantDigits + Exponent + 2;

		if (destination.Length < requiredLength)
		{
			charsWritten = 0;
			return false;
		}

		if (!format.IsEmpty && !format.Equals("G", StringComparison.OrdinalIgnoreCase))
		{
			throw new FormatException();
		}

		destination.Clear();

		string output = FormatOutput(provider);

		bool success = output.TryCopyTo(destination);
		charsWritten = success ? output.Length : 0;
		return success;
	}

	private string FormatOutput(IFormatProvider? provider)
	{
		if (this == Zero)
		{
			return "0";
		}
		else if (this == One)
		{
			return "1";
		}
		else if (this == NegativeOne)
		{
			return $"{NumberFormatInfo.GetInstance(provider).NegativeSign}1";
		}

		provider ??= InvariantCulture;
		var numberFormat = NumberFormatInfo.GetInstance(provider);
		string sign = Significand < 0 ? numberFormat.NegativeSign : string.Empty;
		string significandStr = BigInteger.Abs(Significand).ToString(InvariantCulture);

		if (Exponent == 0)
		{
			return $"{sign}{significandStr}";
		}
		else if (Exponent > 0)
		{
			return $"{sign}{significandStr}{new string('0', Exponent)}";
		}

		return FormatNegativeExponent(sign, significandStr, numberFormat);
	}

	private string FormatNegativeExponent(string sign, string significandStr, NumberFormatInfo numberFormat)
	{
		int absExponent = -Exponent;
		string integralComponent = absExponent >= significandStr.Length ? "0" : significandStr[..^absExponent];
		string fractionalComponent = absExponent >= significandStr.Length
			? $"{new string('0', absExponent - significandStr.Length)}{BigInteger.Abs(Significand)}"
			: significandStr[^absExponent..];

		return $"{sign}{integralComponent}{numberFormat.NumberDecimalSeparator}{fractionalComponent}";
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
		ArgumentNullException.ThrowIfNull(left);
		ArgumentNullException.ThrowIfNull(right);

		Debug.Assert(left.Exponent == right.Exponent, $"{nameof(AssertExponentsMatch)}: {left.Exponent} == {right.Exponent}");
	}

	/// <summary>
	/// Negates a number.
	/// </summary>
	/// <param name="value">The number to negate.</param>
	/// <returns>The negated number.</returns>
	public static PreciseNumber Negate(PreciseNumber value)
	{
		ArgumentNullException.ThrowIfNull(value);
		return value == Zero
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
		var (commonLeft, commonRight, commonExponent) = MakeCommonizedWithExponent(left, right);
		AssertExponentsMatch(commonLeft, commonRight);

		var newSignificand = commonLeft.Significand - commonRight.Significand;
		return new PreciseNumber(commonExponent, newSignificand);
	}

	/// <summary>
	/// Adds two numbers.
	/// </summary>
	/// <param name="left">The first number to add.</param>
	/// <param name="right">The second number to add.</param>
	/// <returns>The result of the addition.</returns>
	public static PreciseNumber Add(PreciseNumber left, PreciseNumber right)
	{
		var (commonLeft, commonRight, commonExponent) = MakeCommonizedWithExponent(left, right);
		AssertExponentsMatch(commonLeft, commonRight);

		var newSignificand = commonLeft.Significand + commonRight.Significand;
		return new PreciseNumber(commonExponent, newSignificand);
	}

	/// <summary>
	/// Multiplies two numbers.
	/// </summary>
	/// <param name="left">The first number to multiply.</param>
	/// <param name="right">The second number to multiply.</param>
	/// <returns>The result of the multiplication.</returns>
	public static PreciseNumber Multiply(PreciseNumber left, PreciseNumber right)
	{
		if (left == Zero || right == Zero)
		{
			return Zero;
		}
		else if (left == One)
		{
			return right;
		}
		else if (right == One)
		{
			return left;
		}

		var (commonLeft, commonRight, commonExponent) = MakeCommonizedWithExponent(left, right);
		AssertExponentsMatch(commonLeft, commonRight);

		var newSignificand = commonLeft.Significand * commonRight.Significand;
		return new PreciseNumber(commonExponent + commonExponent, newSignificand);
	}

	/// <summary>
	/// Divides one number by another.
	/// </summary>
	/// <param name="left">The number to divide.</param>
	/// <param name="right">The number to divide by.</param>
	/// <returns>The result of the division.</returns>
	public static PreciseNumber Divide(PreciseNumber left, PreciseNumber right)
	{
		if (right == Zero)
		{
			throw new DivideByZeroException();
		}

		if (left == right)
		{
			return One;
		}

		var (commonLeft, commonRight, commonExponent) = MakeCommonizedWithExponent(left, right);
		AssertExponentsMatch(commonLeft, commonRight);

		var integerComponent = commonLeft.Significand / commonRight.Significand;
		double remainder = double.CreateTruncating(commonLeft.Significand - (integerComponent * commonRight.Significand)) * double.Pow(Base10, commonExponent);
		double fractionalComponent = remainder / (double.CreateTruncating(commonRight.Significand) * double.Pow(Base10, commonExponent));

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
		if (right == Zero)
		{
			throw new DivideByZeroException();
		}

		if (left == right)
		{
			return Zero;
		}

		var (commonLeft, commonRight, commonExponent) = MakeCommonizedWithExponent(left, right);
		AssertExponentsMatch(commonLeft, commonRight);

		var integerComponent = commonLeft.Significand / commonRight.Significand;
		var remainder = commonLeft.Significand - (integerComponent * commonRight.Significand);

		return new PreciseNumber(commonExponent, remainder);
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
	public static bool GreaterThan(PreciseNumber left, PreciseNumber right)
	{
		var (commonLeft, commonRight) = MakeCommonized(left, right);
		AssertExponentsMatch(commonLeft, commonRight);
		return commonLeft.Significand > commonRight.Significand;
	}

	/// <summary>
	/// Determines whether one number is greater than or equal to another.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	/// <returns><c>true</c> if the first number is greater than or equal to the second; otherwise, <c>false</c>.</returns>
	public static bool GreaterThanOrEqual(PreciseNumber left, PreciseNumber right)
	{
		var (commonLeft, commonRight) = MakeCommonized(left, right);
		AssertExponentsMatch(commonLeft, commonRight);
		return commonLeft.Significand >= commonRight.Significand;
	}

	/// <summary>
	/// Determines whether one number is less than another.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	/// <returns><c>true</c> if the first number is less than the second; otherwise, <c>false</c>.</returns>
	public static bool LessThan(PreciseNumber left, PreciseNumber right)
	{
		var (commonLeft, commonRight) = MakeCommonized(left, right);
		AssertExponentsMatch(commonLeft, commonRight);
		return commonLeft.Significand < commonRight.Significand;
	}

	/// <summary>
	/// Determines whether one number is less than or equal to another.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	/// <returns><c>true</c> if the first number is less than or equal to the second; otherwise, <c>false</c>.</returns>
	public static bool LessThanOrEqual(PreciseNumber left, PreciseNumber right)
	{
		var (commonLeft, commonRight) = MakeCommonized(left, right);
		AssertExponentsMatch(commonLeft, commonRight);
		return commonLeft.Significand <= commonRight.Significand;
	}

	/// <summary>
	/// Determines whether two numbers are equal.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	/// <returns><c>true</c> if the two numbers are equal; otherwise, <c>false</c>.</returns>
	public static bool Equal(PreciseNumber left, PreciseNumber right)
	{
		var (commonLeft, commonRight) = MakeCommonized(left, right);
		AssertExponentsMatch(commonLeft, commonRight);
		return commonLeft.Significand == commonRight.Significand;
	}

	/// <summary>
	/// Determines whether two numbers are not equal.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	/// <returns><c>true</c> if the two numbers are not equal; otherwise, <c>false</c>.</returns>
	public static bool NotEqual(PreciseNumber left, PreciseNumber right)
	{
		var (commonLeft, commonRight) = MakeCommonized(left, right);
		AssertExponentsMatch(commonLeft, commonRight);
		return commonLeft.Significand != commonRight.Significand;
	}

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
		ArgumentNullException.ThrowIfNull(value);
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
		ArgumentNullException.ThrowIfNull(value);
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
		if (power == Zero)
		{
			return One;
		}
		else if (this == Zero)
		{
			return Zero;
		}
		else if (this == One)
		{
			return One;
		}

		if (IsInteger(power))
		{
			var result = this;
			int absPower = power.Abs().To<int>();

			for (int i = 1; i < absPower; i++)
			{
				result *= this;
			}

			return power < Zero ? One / result : result;
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
		ArgumentNullException.ThrowIfNull(power);

		if (power == Zero)
		{
			return One;
		}
		else if (power == One)
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
		typeof(TOutput) == typeof(PreciseNumber) || typeof(TOutput).IsSubclassOf(typeof(PreciseNumber))
		? (TOutput)(object)this
		: TOutput.CreateChecked(Significand) * TOutput.CreateChecked(Math.Pow(Base10, Exponent));
}
