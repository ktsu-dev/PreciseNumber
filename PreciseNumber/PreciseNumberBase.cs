// Ignore Spelling: Commonized

namespace ktsu.PreciseNumber;

using System.Diagnostics;
using System.Globalization;
using System.Numerics;

public record PreciseNumberBase
{
	protected const int Base10 = 10;

	/// <summary>
	/// Gets the exponent of the number.
	/// </summary>
	protected internal int Exponent { get; init; }

	/// <summary>
	/// Gets the significand of the number.
	/// </summary>
	protected internal BigInteger Significand { get; init; }

	/// <summary>
	/// Gets the number of significant digits in the number.
	/// </summary>
	protected internal int SignificantDigits { get; init; }

	protected internal static CultureInfo InvariantCulture { get; } = CultureInfo.InvariantCulture;

	/// <summary>
	/// Gets the value -1 for the type.
	/// </summary>
	public static PreciseNumberBase NegativeOne { get; } = CreateFromComponents<PreciseNumberBase>(0, -1);

	/// <inheritdoc/>
	public static PreciseNumberBase One { get; } = CreateFromComponents<PreciseNumberBase>(0, 1);

	/// <inheritdoc/>
	public static PreciseNumberBase Zero { get; } = new();

	private const int EExponent = -40;

	/// <summary>
	/// Gets the value of e for the type.
	/// </summary>
	public static PreciseNumberBase E { get; } = CreateFromComponents<PreciseNumberBase>(EExponent, BigInteger.Parse("27182818284590452353602874713526624977572", InvariantCulture));

	private const int PiExponent = -25;

	/// <summary>
	/// Gets the value of pi for the type.
	/// </summary>
	public static PreciseNumberBase Pi { get; } = CreateFromComponents<PreciseNumberBase>(PiExponent, BigInteger.Parse("31415926535897932384626433", InvariantCulture));

	private const int TauExponent = -24;

	/// <summary>
	/// Gets the value of tau for the type.
	/// </summary>
	public static PreciseNumberBase Tau { get; } = CreateFromComponents<PreciseNumberBase>(TauExponent, BigInteger.Parse("6283185307179586476925287", InvariantCulture));

	protected const int BinaryRadix = 2;

	/// <inheritdoc/>
	public static int Radix => BinaryRadix;

	/// <inheritdoc/>
	public static PreciseNumberBase AdditiveIdentity => Zero;

	/// <inheritdoc/>
	public static PreciseNumberBase MultiplicativeIdentity => One;

	public PreciseNumberBase()
	{
		Exponent = 0;
		Significand = 0;
		SignificantDigits = 0;
	}

	/// <summary>
	/// Counts the number of decimal digits in the current instance.
	/// </summary>
	/// <returns>The number of decimal digits in the current instance.</returns>
	internal int CountDecimalDigits() =>
		Exponent > 0
		? 0
		: int.Abs(Exponent);

	/// <summary>
	/// Gets a value indicating whether the current instance has infinite precision.
	/// </summary>
	internal bool HasInfinitePrecision =>
		Exponent == 0
		&& (Significand == BigInteger.One || Significand == BigInteger.Zero || Significand == BigInteger.MinusOne);

	public static bool IsCanonical(PreciseNumberBase value) => value.IsCanonical();

	public static bool IsComplexNumber(PreciseNumberBase value) => value.IsComplexNumber();

	public static bool IsEvenInteger(PreciseNumberBase value) => value.IsEvenInteger();

	public static bool IsFinite(PreciseNumberBase value) => value.IsFinite();

	public static bool IsImaginaryNumber(PreciseNumberBase value) => value.IsImaginaryNumber();

	public static bool IsInfinity(PreciseNumberBase value) => value.IsInfinity();

	public static bool IsInteger(PreciseNumberBase value) => value.IsInteger();

	public static bool IsNaN(PreciseNumberBase value) => value.IsNaN();

	public static bool IsNegative(PreciseNumberBase value) => value.IsNegative();

	public static bool IsNegativeInfinity(PreciseNumberBase value) => value.IsNegativeInfinity();
	public static bool IsNormal(PreciseNumberBase value) => value.IsNormal();

	public static bool IsOddInteger(PreciseNumberBase value) => value.IsOddInteger();

	public static bool IsPositive(PreciseNumberBase value) => value.IsPositive();

	public static bool IsPositiveInfinity(PreciseNumberBase value) => value.IsPositiveInfinity();

	public static bool IsRealNumber(PreciseNumberBase value) => value.IsRealNumber();

	public static bool IsSubnormal(PreciseNumberBase value) => value.IsSubnormal();

	public static bool IsZero(PreciseNumberBase value) => value.IsZero();

	public static PreciseNumberBase Abs(PreciseNumberBase value) => value.Abs();

	/// <summary>
	/// Returns the larger of two numbers.
	/// </summary>
	/// <param name="x">The first number.</param>
	/// <param name="y">The second number.</param>
	/// <returns>The larger of the two numbers.</returns>
	public static PreciseNumberBase Max(PreciseNumberBase x, PreciseNumberBase y) => x.Max(y);

	/// <summary>
	/// Returns the smaller of two numbers.
	/// </summary>
	/// <param name="x">The first number.</param>
	/// <param name="y">The second number.</param>
	/// <returns>The smaller of the two numbers.</returns>
	public static PreciseNumberBase Min(PreciseNumberBase x, PreciseNumberBase y) => x.Min(y);

	/// <summary>
	/// Clamps a number to the specified minimum and maximum values.
	/// </summary>
	/// <param name="value">The number to clamp.</param>
	/// <param name="min">The minimum value.</param>
	/// <param name="max">The maximum value.</param>
	/// <returns>The clamped number.</returns>
	public static PreciseNumberBase Clamp<T1, T2>(PreciseNumberBase value, T1 min, T2 max)
		where T1 : INumber<T1>
		where T2 : INumber<T2>
		=> value.Clamp(min, max);

	/// <summary>
	/// Rounds a number to the specified number of decimal digits.
	/// </summary>
	/// <param name="value">The number to round.</param>
	/// <param name="decimalDigits">The number of decimal digits to round to.</param>
	/// <returns>The rounded number.</returns>
	public static PreciseNumberBase Round(PreciseNumberBase value, int decimalDigits) => value.Round(decimalDigits);

	/// <summary>
	/// Returns the result of raising e to the specified power.
	/// </summary>
	/// <param name="power">The power to raise e to.</param>
	/// <returns>A new instance of <see cref="PreciseNumber"/> that is the result of raising e to the specified power.</returns>
	public static PreciseNumberBase Exp(PreciseNumberBase power)
	{
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
		typeof(TOutput).IsSubclassOf(typeof(PreciseNumberBase)) || typeof(TOutput) == typeof(PreciseNumberBase)
		? (TOutput)(object)this
		: TOutput.CreateChecked(Significand) * TOutput.CreateChecked(Math.Pow(Base10, Exponent));

	/// <summary>
	/// Returns the square of the current number.
	/// </summary>
	/// <returns>A new instance of <see cref="PreciseNumber"/> that is the square of the current instance.</returns>
	public PreciseNumberBase Squared() => Multiply(this);

	/// <summary>
	/// Returns the cube of the current number.
	/// </summary>
	/// <returns>A new instance of <see cref="PreciseNumber"/> that is the cube of the current instance.</returns>
	public PreciseNumberBase Cubed() => Squared().Multiply(this);

	/// <summary>
	/// Returns the result of raising the current number to the specified power.
	/// </summary>
	/// <param name="power">The power to raise the number to.</param>
	/// <returns>A new instance of <see cref="PreciseNumber"/> that is the result of raising the current instance to the specified power.</returns>
	public PreciseNumberBase Pow<TNumber>(TNumber power)
		where TNumber : INumber<TNumber>
	{
		var precisePower = power.ToPreciseNumber();
		if (precisePower == Zero)
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

		if (IsInteger(precisePower))
		{
			var result = this;
			int absPower = precisePower.Abs().To<int>();

			for (int i = 1; i < absPower; i++)
			{
				result = result.Multiply(this);
			}

			return precisePower.LessThan(Zero) ? One.Divide(result) : result;
		}

		// Use logarithm and exponential to support decimal powers
		double logValue = Math.Log(To<double>());
		return Math.Exp(logValue * precisePower.To<double>()).ToPreciseNumber();
	}

	/// <summary>
	/// Returns the absolute value of the current instance.
	/// </summary>
	/// <returns>The absolute value of the current instance.</returns>
	public virtual PreciseNumberBase Abs() => Significand < 0 ? Negate() : this;

	/// <summary>
	/// Rounds the current instance to the specified number of decimal digits.
	/// </summary>
	/// <param name="decimalDigits">The number of decimal digits to round to.</param>
	/// <returns>A new instance of <see cref="PreciseNumber"/> rounded to the specified number of decimal digits.</returns>
	public virtual PreciseNumberBase Round(int decimalDigits)
	{
		int currentDecimalDigits = CountDecimalDigits();
		int decimalDifference = int.Abs(decimalDigits - currentDecimalDigits);
		if (currentDecimalDigits > decimalDigits && decimalDifference > 0)
		{
			var roundingFactor = BigInteger.CopySign(CreateRepeatingDigits(5, decimalDifference), Significand);
			var newSignificand = (Significand + roundingFactor) / BigInteger.Pow(Base10, decimalDifference);
			int newExponent = Exponent - int.CopySign(decimalDifference, Exponent);
			return CreateFromComponents<PreciseNumberBase>(newExponent, newSignificand);
		}

		return this;
	}

	public virtual PreciseNumberBase Clamp<T1, T2>(T1 min, T2 max)
		where T1 : INumber<T1>
		where T2 : INumber<T2>
	{
		var preciseMin = min.ToPreciseNumber();
		var preciseMax = max.ToPreciseNumber();
		var clampedToMax = GreaterThan(preciseMax) ? preciseMax : this;
		return LessThan(preciseMin) ? preciseMin : clampedToMax;
	}

	public virtual PreciseNumberBase Max(PreciseNumberBase other) => GreaterThan(other) ? this : other;
	public virtual PreciseNumberBase Min(PreciseNumberBase other) => LessThan(other) ? this : other;

	public virtual bool Equals(PreciseNumberBase? other) => EqualTo(other);

	/// <summary>
	/// Adjusts the exponents of two <see cref="PreciseNumber"/> instances to a common exponent.
	/// </summary>
	/// <param name="left">The left <see cref="PreciseNumber"/> instance.</param>
	/// <param name="right">The right <see cref="PreciseNumber"/> instance.</param>
	/// <returns>A tuple containing the commonized <see cref="PreciseNumber"/> instances.</returns>
	protected internal static (TNumber, TNumber) MakeCommonized<TNumber>(TNumber left, TNumber right)
		where TNumber : PreciseNumberBase, new()
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
	protected internal static (TNumber, TNumber, int) MakeCommonizedWithExponent<TNumber>(TNumber left, TNumber right)
		where TNumber : PreciseNumberBase, new()
	{
		int smallestExponent = left.Exponent < right.Exponent ? left.Exponent : right.Exponent;
		int exponentDifferenceLeft = Math.Abs(left.Exponent - smallestExponent);
		int exponentDifferenceRight = Math.Abs(right.Exponent - smallestExponent);
		var newSignificandLeft = left.Significand * BigInteger.Pow(Base10, exponentDifferenceLeft);
		var newSignificandRight = right.Significand * BigInteger.Pow(Base10, exponentDifferenceRight);

		return (CreateFromComponents<TNumber>(smallestExponent, newSignificandLeft, sanitize: false),
			CreateFromComponents<TNumber>(smallestExponent, newSignificandRight, sanitize: false),
			smallestExponent);
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
	/// Gets the lower of the decimal digit counts of two numbers.
	/// </summary>
	/// <param name="left">The first number.</param>
	/// <param name="right">The second number.</param>
	/// <returns>The lower of the decimal digit counts of the two numbers.</returns>
	protected internal static int LowestDecimalDigits(PreciseNumberBase left, PreciseNumberBase right)
	{
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
	protected internal static int LowestSignificantDigits(PreciseNumberBase left, PreciseNumberBase right)
	{
		int leftSignificantDigits = left.SignificantDigits;
		int rightSignificantDigits = right.SignificantDigits;

		leftSignificantDigits = left.HasInfinitePrecision ? rightSignificantDigits : leftSignificantDigits;
		rightSignificantDigits = right.HasInfinitePrecision ? leftSignificantDigits : rightSignificantDigits;

		return leftSignificantDigits < rightSignificantDigits
		? leftSignificantDigits
		: rightSignificantDigits;
	}

	public virtual PreciseNumberBase Add(PreciseNumberBase other)
	{
		var left = this;
		var right = other;

		if (left == Zero)
		{
			return right;
		}
		else if (right == Zero)
		{
			return left;
		}

		var (commonLeft, commonRight) = MakeCommonized(left, right);
		return CreateFromComponents<PreciseNumberBase>(commonLeft.Exponent, commonLeft.Significand + commonRight.Significand);
	}

	public virtual PreciseNumberBase Subtract(PreciseNumberBase other)
	{
		var left = this;
		var right = other;

		if (left == Zero)
		{
			return right.Negate();
		}
		else if (right == Zero)
		{
			return left;
		}

		var (commonLeft, commonRight) = MakeCommonized(left, right);
		return CreateFromComponents<PreciseNumberBase>(commonLeft.Exponent, commonLeft.Significand - commonRight.Significand);
	}

	public virtual PreciseNumberBase Multiply(PreciseNumberBase other)
	{
		var left = this;
		var right = other;

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

		var (commonLeft, commonRight) = MakeCommonized(left, right);
		return CreateFromComponents<PreciseNumberBase>(commonLeft.Exponent + commonRight.Exponent, commonLeft.Significand * commonRight.Significand);
	}

	public virtual PreciseNumberBase Divide(PreciseNumberBase other)
	{
		var left = this;
		var right = other;

		if (right == Zero)
		{
			throw new DivideByZeroException();
		}

		if (left == right)
		{
			return One;
		}

		var (commonLeft, commonRight, commonExponent) = MakeCommonizedWithExponent(left, right);

		var integerComponent = commonLeft.Significand / commonRight.Significand;
		double remainder = double.CreateTruncating(commonLeft.Significand - (integerComponent * commonRight.Significand)) * double.Pow(Base10, commonExponent);
		double fractionalComponent = remainder / (double.CreateTruncating(commonRight.Significand) * double.Pow(Base10, commonExponent));

		return CreateFromComponents<PreciseNumberBase>(0, integerComponent).Add(fractionalComponent.ToPreciseNumber());
	}

	public virtual PreciseNumberBase Negate() => CreateFromComponents<PreciseNumberBase>(Exponent, -Significand);

	public virtual PreciseNumberBase UnaryPlus() => this;

	public virtual PreciseNumberBase Modulo(PreciseNumberBase other)
	{
		var left = this;
		var right = other;

		if (right == Zero)
		{
			throw new DivideByZeroException();
		}

		if (left == right)
		{
			return Zero;
		}

		var (commonLeft, commonRight, commonExponent) = MakeCommonizedWithExponent(left, right);
		var integerComponent = commonLeft.Significand / commonRight.Significand;
		var remainder = commonLeft.Significand - (integerComponent * commonRight.Significand);

		return CreateFromComponents<PreciseNumberBase>(commonExponent, remainder);
	}

	public virtual PreciseNumberBase Increment() => Add(One);
	public virtual PreciseNumberBase Decrement() => Subtract(One);

	public virtual PreciseNumberBase MaxMagnitude(PreciseNumberBase other) => Abs().GreaterThanOrEqualTo(other.Abs()) ? this : other;

	/// <inheritdoc/>
	public virtual PreciseNumberBase MaxMagnitudeNumber(PreciseNumberBase other) => MaxMagnitude(other);

	/// <inheritdoc/>
	public virtual PreciseNumberBase MinMagnitude(PreciseNumberBase other) => Abs().LessThanOrEqualTo(other.Abs()) ? this : other;

	/// <inheritdoc/>
	public virtual PreciseNumberBase MinMagnitudeNumber(PreciseNumberBase other) => MinMagnitude(other);

	public virtual bool EqualTo(PreciseNumberBase? other)
	{
		if (other is null)
		{
			return false;
		}

		var (commonLeft, commonRight) = MakeCommonized(this, other);
		return commonLeft.Significand == commonRight.Significand;
	}

	public virtual bool NotEqualTo(PreciseNumberBase? other)
	{
		if (other is null)
		{
			return false;
		}

		var (commonLeft, commonRight) = MakeCommonized(this, other);
		return commonLeft.Significand != commonRight.Significand;
	}

	public virtual bool GreaterThan(PreciseNumberBase other)
	{
		var (commonLeft, commonRight) = MakeCommonized(this, other);
		return commonLeft.Significand > commonRight.Significand;
	}

	public virtual bool LessThan(PreciseNumberBase other)
	{
		var (commonLeft, commonRight) = MakeCommonized(this, other);
		return commonLeft.Significand < commonRight.Significand;
	}

	public virtual bool GreaterThanOrEqualTo(PreciseNumberBase other)
	{
		var (commonLeft, commonRight) = MakeCommonized(this, other);
		return commonLeft.Significand >= commonRight.Significand;
	}

	public virtual bool LessThanOrEqualTo(PreciseNumberBase other)
	{
		var (commonLeft, commonRight) = MakeCommonized(this, other);
		return commonLeft.Significand <= commonRight.Significand;
	}

	public virtual bool IsZero() => Significand == 0;
	public virtual bool IsOne() => Significand == 1 && Exponent == 0;
	public virtual bool IsNegative() => Significand < 0;
	public virtual bool IsPositive() => Significand > 0;
	public virtual bool IsInteger() => Exponent >= 0;
	public virtual bool IsEvenInteger() => Exponent > 0 || (Exponent == 0 && Significand.IsEven);
	public virtual bool IsOddInteger() => Exponent == 0 && !Significand.IsEven;
	public virtual bool IsNaN() => false;
	public virtual bool IsInfinity() => false;
	public virtual bool IsPositiveInfinity() => false;
	public virtual bool IsNegativeInfinity() => false;
	public virtual bool IsFinite() => true;
	public virtual bool IsNormal() => Significand != 0;
	public virtual bool IsSubnormal() => false;
	public virtual bool IsComplexNumber() => false;
	public virtual bool IsRealNumber() => true;
	public virtual bool IsImaginaryNumber() => false;
	public virtual bool IsCanonical() => true;

	/// <inheritdoc/>
	public static PreciseNumberBase operator -(PreciseNumberBase value) =>
		value.Negate();

	/// <inheritdoc/>
	public static PreciseNumberBase operator -(PreciseNumberBase left, PreciseNumberBase right) =>
		left.Subtract(right);

	/// <inheritdoc/>
	public static PreciseNumberBase operator *(PreciseNumberBase left, PreciseNumberBase right) =>
		left.Multiply(right);

	/// <inheritdoc/>
	public static PreciseNumberBase operator /(PreciseNumberBase left, PreciseNumberBase right) =>
		left.Divide(right);

	/// <inheritdoc/>
	public static PreciseNumberBase operator +(PreciseNumberBase value) =>
		value.UnaryPlus();

	/// <inheritdoc/>
	public static PreciseNumberBase operator +(PreciseNumberBase left, PreciseNumberBase right) =>
		left.Add(right);

	/// <inheritdoc/>
	public static bool operator >(PreciseNumberBase left, PreciseNumberBase right) =>
		left.GreaterThan(right);

	/// <inheritdoc/>
	public static bool operator <(PreciseNumberBase left, PreciseNumberBase right) =>
		left.LessThan(right);

	/// <inheritdoc/>
	public static bool operator >=(PreciseNumberBase left, PreciseNumberBase right) =>
		left.GreaterThanOrEqualTo(right);

	/// <inheritdoc/>
	public static bool operator <=(PreciseNumberBase left, PreciseNumberBase right) =>
		left.LessThanOrEqualTo(right);

	/// <inheritdoc/>
	public static PreciseNumberBase operator %(PreciseNumberBase left, PreciseNumberBase right) =>
		left.Modulo(right);

	/// <inheritdoc/>
	public static PreciseNumberBase operator --(PreciseNumberBase value) =>
		value.Decrement();

	/// <inheritdoc/>
	public static PreciseNumberBase operator ++(PreciseNumberBase value) =>
		value.Increment();

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
	public static string ToString(PreciseNumberBase number, string? format, IFormatProvider? formatProvider)
	{
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
	/// Creates a <see cref="PreciseNumber"/> from a floating point value.
	/// </summary>
	/// <typeparam name="TFloat">The type of the floating point value.</typeparam>
	/// <param name="input">The floating point value.</param>
	/// <returns>A <see cref="PreciseNumber"/> representing the floating point value.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when the input is infinite or NaN.</exception>
	internal static PreciseNumberBase CreateFromFloatingPoint<TFloat>(TFloat input)
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

	private static PreciseNumberBase CreatePreciseNumberFromNonSpecialFloat<TFloat>(TFloat input)
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

		return CreateFromComponents<PreciseNumberBase>(exponentValue, significandValue);
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
	internal static PreciseNumberBase CreateFromInteger<TInteger>(TInteger input)
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

		return CreateFromComponents<PreciseNumberBase>(exponentValue, significandValue);
	}

	/// <summary>
	/// Reduces the significance of the current instance to a specified number of significant digits.
	/// </summary>
	/// <param name="significantDigits">The number of significant digits to reduce to.</param>
	/// <returns>A new instance of <see cref="PreciseNumber"/> reduced to the specified number of significant digits.</returns>
	public virtual PreciseNumberBase ReduceSignificance(int significantDigits)
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
		return CreateFromComponents<PreciseNumberBase>(newExponent, newSignificand);
	}

	public virtual int CompareTo(PreciseNumberBase? other)
	{
		if (other is null)
		{
			return 1;
		}

		int greaterOrEqual = GreaterThan(other) ? 1 : 0;
		return LessThan(other) ? -1 : greaterOrEqual;
	}

	/// <inheritdoc/>
	public virtual int CompareTo(object? obj)
	{
		return obj is PreciseNumberBase preciseNumber
			? CompareTo(preciseNumber)
			: throw new NotSupportedException();
	}

	/// <summary>
	/// Compares the current instance with another number.
	/// </summary>
	/// <typeparam name="TInput">The type of the other number.</typeparam>
	/// <param name="other">The number to compare with the current instance.</param>
	/// <returns>A value indicating whether the current instance is less than, equal to, or greater than the other number.</returns>
	public virtual int CompareTo<TInput>(TInput other)
		where TInput : INumber<TInput>
	{
		var significantOther = other.ToPreciseNumber();
		int greaterOrEqual = this > significantOther ? 1 : 0;
		return this < significantOther ? -1 : greaterOrEqual;
	}

	public static int Compare(PreciseNumberBase? left, PreciseNumberBase? right) => left?.CompareTo(right) ?? -1;

	/// <inheritdoc/>
	public static PreciseNumberBase MaxMagnitude(PreciseNumberBase x, PreciseNumberBase y) => x.MaxMagnitude(y);

	/// <inheritdoc/>
	public static PreciseNumberBase MaxMagnitudeNumber(PreciseNumberBase x, PreciseNumberBase y) => x.MaxMagnitudeNumber(y);

	/// <inheritdoc/>
	public static PreciseNumberBase MinMagnitude(PreciseNumberBase x, PreciseNumberBase y) => x.MinMagnitude(y);

	/// <inheritdoc/>
	public static PreciseNumberBase MinMagnitudeNumber(PreciseNumberBase x, PreciseNumberBase y) => x.MinMagnitudeNumber(y);

	/// <inheritdoc/>
	public static PreciseNumberBase Parse(ReadOnlySpan<char> s, NumberStyles _, IFormatProvider? __)
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

		return CreateFromComponents<PreciseNumberBase>(exponent, significand);
	}

	/// <inheritdoc/>
	public static PreciseNumberBase Parse(string s, NumberStyles style, IFormatProvider? provider) =>
		Parse(s.AsSpan(), style, provider);

	/// <inheritdoc/>
	public static PreciseNumberBase Parse(string s, IFormatProvider? provider) =>
		Parse(s, NumberStyles.Any, provider);

	/// <inheritdoc/>
	public static PreciseNumberBase Parse(ReadOnlySpan<char> s, IFormatProvider? provider) =>
		Parse(s, NumberStyles.Any, provider);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out PreciseNumberBase result)
	{
		try
		{
			result = Parse(s, style, provider);
			return true;
		}
		catch (FormatException)
		{
			result = new();
			return false;
		}

	}

	/// <inheritdoc/>
	public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out PreciseNumberBase result) =>
		TryParse(s.AsSpan(), style, provider, out result);

	/// <inheritdoc/>
	public static bool TryParse(string? s, IFormatProvider? provider, out PreciseNumberBase result) =>
		TryParse(s.AsSpan(), NumberStyles.Any, provider, out result);

	/// <inheritdoc/>
	public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out PreciseNumberBase result) =>
		TryParse(s, NumberStyles.Any, provider, out result);

	public static bool TryFormat(PreciseNumberBase value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
		value.TryFormat(destination, out charsWritten, format, provider);

	/// <inheritdoc/>
	public virtual bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
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
	public static bool TryConvertFromChecked<TOther>(TOther value, out PreciseNumberBase result)
		where TOther : INumberBase<TOther>
		=> throw new NotSupportedException();

	/// <inheritdoc/>
	public static bool TryConvertFromSaturating<TOther>(TOther value, out PreciseNumberBase result)
		where TOther : INumberBase<TOther>
		=> throw new NotSupportedException();

	/// <inheritdoc/>
	public static bool TryConvertFromTruncating<TOther>(TOther value, out PreciseNumberBase result)
		where TOther : INumberBase<TOther>
		=> throw new NotSupportedException();

	/// <inheritdoc/>
	public static bool TryConvertToChecked<TOther>(PreciseNumberBase value, out TOther result)
		where TOther : INumberBase<TOther>
		=> throw new NotSupportedException();

	/// <inheritdoc/>
	public static bool TryConvertToSaturating<TOther>(PreciseNumberBase value, out TOther result)
		where TOther : INumberBase<TOther>
		=> throw new NotSupportedException();

	/// <inheritdoc/>
	public static bool TryConvertToTruncating<TOther>(PreciseNumberBase value, out TOther result)
		where TOther : INumberBase<TOther>
		=> throw new NotSupportedException();

	/// <summary>
	/// Tries to create a <see cref="PreciseNumber"/> from the input.
	/// </summary>
	/// <typeparam name="TInput">The type of the input number.</typeparam>
	/// <param name="input">The input number to create a <see cref="PreciseNumber"/> from.</param>
	/// <param name="preciseNumber">The created <see cref="PreciseNumber"/> if successful, otherwise null.</param>
	/// <returns>True if the creation was successful, otherwise false.</returns>
	internal static bool TryCreate<TInput>(TInput input, out PreciseNumberBase preciseNumber)
		where TInput : INumber<TInput>
	{
		var type = typeof(TInput);
		if (Array.Exists(type.GetInterfaces(), i => i.Name.StartsWith("IBinaryInteger", StringComparison.Ordinal)))
		{
			preciseNumber = CreateFromInteger(input);
			return true;
		}

		if (Array.Exists(type.GetInterfaces(), i => i.Name.StartsWith("IFloatingPoint", StringComparison.Ordinal)))
		{
			preciseNumber = CreateFromFloatingPoint(input);
			return true;
		}

		preciseNumber = new();
		return false;
	}

	internal TDerived As<TDerived>()
	where TDerived : PreciseNumberBase, new()
	=> CreateFromComponents<TDerived>(Exponent, Significand);

	internal static TDerived As<TDerived>(PreciseNumberBase input)
		where TDerived : PreciseNumberBase, new()
		=> input.As<TDerived>();

	internal static TDerived CreateFromComponents<TDerived>(int exponent, BigInteger significand)
		where TDerived : PreciseNumberBase, new()
		=> CreateFromComponents<TDerived>(exponent, significand, sanitize: true);

	internal static TDerived CreateFromComponents<TDerived>(int exponent, BigInteger significand, bool sanitize)
		where TDerived : PreciseNumberBase, new()
	{
		if (sanitize)
		{
			if (significand == 0)
			{
				return new()
				{
					Exponent = 0,
					Significand = 0,
					SignificantDigits = 0,
				};
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

		return new()
		{
			Exponent = exponent,
			Significand = significand,
			SignificantDigits = significantDigits,
		};
	}
}
