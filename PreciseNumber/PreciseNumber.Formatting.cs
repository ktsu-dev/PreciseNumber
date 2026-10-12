// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber;

using System;
using System.Globalization;
using System.Numerics;
using System.Text;

/// <summary>
/// The standard numeric format strings other than the general one: fixed point (<c>F</c>), number
/// (<c>N</c>), scientific (<c>E</c>), round trip (<c>R</c>) and general with a precision (<c>G</c><i>n</i>).
/// </summary>
/// <remarks>
/// Each specifier means what it means for <see cref="decimal"/>, worked on the exact digits rather
/// than through a binary type, and rounds half away from zero like <see cref="Round(int)"/>. Custom
/// format strings such as <c>0.00</c> are not supported and throw a <see cref="FormatException"/>
/// that names the ones that are.
/// </remarks>
public readonly partial record struct PreciseNumber
{
	/// <summary>
	/// The largest precision a standard format string accepts, matching the runtime's own limit.
	/// </summary>
	private const int MaxFormatPrecision = 999_999_999;

	/// <summary>
	/// The default number of fractional digits <c>E</c> writes when no precision is given.
	/// </summary>
	private const int DefaultScientificPrecision = 6;

	/// <summary>
	/// The fewest digits <c>E</c> writes for the exponent, matching the runtime's own types.
	/// </summary>
	private const int MinScientificExponentDigits = 3;

	/// <summary>
	/// Returns whether a format string asks for the general, shortest exact rendering that
	/// <see cref="TryFormat"/> writes directly: empty, <c>G</c>, <c>G0</c> or <c>R</c>.
	/// </summary>
	/// <param name="format">The format string.</param>
	/// <returns><see langword="true"/> when the format is the plain general one.</returns>
	private static bool IsPlainGeneralFormat(ReadOnlySpan<char> format) =>
		format.IsEmpty
		|| format.Equals("G", StringComparison.OrdinalIgnoreCase)
		|| format.Equals("G0", StringComparison.OrdinalIgnoreCase)
		|| format.Equals("R", StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Renders this number with a standard format string other than the plain general one.
	/// </summary>
	/// <param name="format">The format string.</param>
	/// <param name="numberFormat">The culture's number formatting information.</param>
	/// <returns>The rendered text.</returns>
	/// <exception cref="FormatException">Thrown when the format string is not one of the supported standard formats.</exception>
	/// <exception cref="OverflowException">Thrown when the text would be longer than any string can hold.</exception>
	private string FormatStandard(ReadOnlySpan<char> format, NumberFormatInfo numberFormat)
	{
		if (!TryParseStandardFormat(format, out char specifier, out int? precision))
		{
			throw new FormatException(
				$"Format string '{format}' is not supported. PreciseNumber supports the standard formats G, R, F, N and E, each optionally followed by a precision such as F2.");
		}

		return char.ToUpperInvariant(specifier) switch
		{
			'F' => FormatFixedPoint(precision ?? numberFormat.NumberDecimalDigits, numberFormat, grouped: false),
			'N' => FormatFixedPoint(precision ?? numberFormat.NumberDecimalDigits, numberFormat, grouped: true),
			'E' => FormatScientific(precision ?? DefaultScientificPrecision, specifier, numberFormat),
			_ => FormatGeneralWithPrecision(precision ?? 0, numberFormat),
		};
	}

	/// <summary>
	/// Splits a standard format string into its specifier and optional precision.
	/// </summary>
	/// <param name="format">The format string.</param>
	/// <param name="specifier">The specifier letter, as written.</param>
	/// <param name="precision">The precision, or <see langword="null"/> when none was given.</param>
	/// <returns><see langword="true"/> when the format is one of the supported standard formats.</returns>
	private static bool TryParseStandardFormat(ReadOnlySpan<char> format, out char specifier, out int? precision)
	{
		specifier = default;
		precision = null;
		if (format.IsEmpty || char.ToUpperInvariant(format[0]) is not ('F' or 'N' or 'E' or 'G' or 'R'))
		{
			return false;
		}

		specifier = format[0];
		ReadOnlySpan<char> digits = format[1..];
		if (digits.IsEmpty)
		{
			return true;
		}

		// R takes no precision, as with the runtime's own types.
		if (char.ToUpperInvariant(specifier) == 'R'
			|| !int.TryParse(digits, NumberStyles.None, InvariantCulture, out int parsed)
			|| parsed > MaxFormatPrecision)
		{
			return false;
		}

		precision = parsed;
		return true;
	}

	/// <summary>
	/// Renders the <c>F</c> and <c>N</c> formats: rounded to <paramref name="precision"/> fractional
	/// digits and padded out to exactly that many.
	/// </summary>
	/// <param name="precision">The number of fractional digits.</param>
	/// <param name="numberFormat">The culture's number formatting information.</param>
	/// <param name="grouped">Whether to group the integral digits and apply the culture's negative pattern, as <c>N</c> does.</param>
	/// <returns>The rendered text.</returns>
	private string FormatFixedPoint(int precision, NumberFormatInfo numberFormat, bool grouped)
	{
		PreciseNumber rounded = Round(precision);
		string digits = BigInteger.Abs(rounded.Significand).ToString(InvariantCulture);

		// Rounding leaves at most precision fractional digits, so the exponent is no lower than
		// -precision and the fraction only ever needs padding on the right.
		long integralLength;
		long fractionalLength;
		if (rounded.Significand.IsZero || rounded.Exponent >= 0)
		{
			integralLength = rounded.Significand.IsZero ? 1 : digits.Length + (long)rounded.Exponent;
			fractionalLength = 0;
		}
		else
		{
			fractionalLength = -(long)rounded.Exponent;
			integralLength = long.Max(digits.Length - fractionalLength, 1);
		}

		string decimalSeparator = numberFormat.NumberDecimalSeparator;
		long bound = (integralLength * (1 + numberFormat.NumberGroupSeparator.Length))
			+ precision
			+ decimalSeparator.Length
			+ numberFormat.NegativeSign.Length
			+ 4;
		ThrowIfLongerThanAnyString(bound);

		StringBuilder integral = new((int)integralLength);
		if (rounded.Significand.IsZero)
		{
			_ = integral.Append('0');
		}
		else if (rounded.Exponent >= 0)
		{
			_ = integral.Append(digits).Append('0', rounded.Exponent);
		}
		else if (digits.Length > fractionalLength)
		{
			_ = integral.Append(digits, 0, (int)(digits.Length - fractionalLength));
		}
		else
		{
			_ = integral.Append('0');
		}

		StringBuilder body = new((int)bound);
		if (grouped)
		{
			AppendGrouped(body, integral.ToString(), numberFormat);
		}
		else
		{
			_ = body.Append(integral);
		}

		if (precision > 0)
		{
			_ = body.Append(decimalSeparator);
			if (fractionalLength > 0)
			{
				int written = (int)long.Min(digits.Length, fractionalLength);
				_ = body.Append('0', (int)(fractionalLength - written))
					.Append(digits, digits.Length - written, written);
			}

			_ = body.Append('0', (int)(precision - fractionalLength));
		}

		bool negative = rounded.Significand.Sign < 0;
		if (!negative)
		{
			return body.ToString();
		}

		if (!grouped)
		{
			return numberFormat.NegativeSign + body;
		}

		return numberFormat.NumberNegativePattern switch
		{
			0 => $"({body})",
			2 => $"{numberFormat.NegativeSign} {body}",
			3 => $"{body}{numberFormat.NegativeSign}",
			4 => $"{body} {numberFormat.NegativeSign}",
			_ => numberFormat.NegativeSign + body,
		};
	}

	/// <summary>
	/// Appends integral digits with the culture's group separator between groups.
	/// </summary>
	/// <param name="builder">The builder to append to.</param>
	/// <param name="integral">The integral digits, most significant first.</param>
	/// <param name="numberFormat">The culture's number formatting information.</param>
	private static void AppendGrouped(StringBuilder builder, string integral, NumberFormatInfo numberFormat)
	{
		int[] groupSizes = numberFormat.NumberGroupSizes;
		string separator = numberFormat.NumberGroupSeparator;

		// Group boundaries are counted from the right. The last size repeats, and a size of zero
		// leaves everything to its left in one group.
		Span<int> boundaries = integral.Length <= 256 ? stackalloc int[integral.Length] : new int[integral.Length];
		int boundaryCount = 0;
		int position = integral.Length;
		int sizeIndex = 0;
		while (groupSizes.Length > 0)
		{
			int size = groupSizes[sizeIndex];
			if (size <= 0 || position - size <= 0)
			{
				break;
			}

			position -= size;
			boundaries[boundaryCount++] = position;
			sizeIndex = int.Min(sizeIndex + 1, groupSizes.Length - 1);
		}

		int start = 0;
		for (int i = boundaryCount - 1; i >= 0; i--)
		{
			_ = builder.Append(integral, start, boundaries[i] - start).Append(separator);
			start = boundaries[i];
		}

		_ = builder.Append(integral, start, integral.Length - start);
	}

	/// <summary>
	/// Renders the <c>E</c> format: one digit before the separator, <paramref name="precision"/>
	/// after it, then the exponent with a sign and at least three digits.
	/// </summary>
	/// <param name="precision">The number of fractional digits.</param>
	/// <param name="specifier">The specifier as written, whose case the exponent marker follows.</param>
	/// <param name="numberFormat">The culture's number formatting information.</param>
	/// <returns>The rendered text.</returns>
	private string FormatScientific(int precision, char specifier, NumberFormatInfo numberFormat)
	{
		long bound = (long)precision
			+ numberFormat.NegativeSign.Length
			+ numberFormat.NumberDecimalSeparator.Length
			+ numberFormat.PositiveSign.Length
			+ 16;
		ThrowIfLongerThanAnyString(bound);

		string digits;
		long exponent;
		if (Significand.IsZero)
		{
			digits = "0";
			exponent = 0;
		}
		else
		{
			BigInteger magnitude = BigInteger.Abs(Significand);
			exponent = (long)Exponent + SignificantDigits - 1;
			int kept = precision + 1;
			if (SignificantDigits > kept)
			{
				magnitude = DropDigitsRoundingHalfAwayFromZero(magnitude, SignificantDigits - kept);
			}

			digits = magnitude.ToString(InvariantCulture);

			// Rounding 9.99… up carries into a new leading digit, a one followed only by zeros, which
			// moves the exponent and leaves one digit more than was kept.
			if (digits.Length > kept)
			{
				digits = digits[..kept];
				exponent++;
			}
		}

		StringBuilder builder = new((int)bound);
		if (Significand.Sign < 0)
		{
			_ = builder.Append(numberFormat.NegativeSign);
		}

		_ = builder.Append(digits[0]);
		if (precision > 0)
		{
			_ = builder.Append(numberFormat.NumberDecimalSeparator)
				.Append(digits, 1, digits.Length - 1)
				.Append('0', precision - (digits.Length - 1));
		}

		_ = builder.Append(char.IsLower(specifier) ? 'e' : 'E')
			.Append(exponent < 0 ? numberFormat.NegativeSign : numberFormat.PositiveSign)
			.Append(long.Abs(exponent).ToString(InvariantCulture).PadLeft(MinScientificExponentDigits, '0'));
		return builder.ToString();
	}

	/// <summary>
	/// Renders the <c>G</c> format with a precision: reduced to that many significant digits, then
	/// written like the plain general format.
	/// </summary>
	/// <param name="precision">The number of significant digits, where zero keeps them all.</param>
	/// <param name="numberFormat">The culture's number formatting information.</param>
	/// <returns>The rendered text.</returns>
	private string FormatGeneralWithPrecision(int precision, NumberFormatInfo numberFormat)
	{
		PreciseNumber reduced = precision > 0 ? ReduceSignificance(precision) : this;
		return ToString(reduced, null, numberFormat);
	}

	/// <summary>
	/// Throws when a rendering would be longer than any string can hold.
	/// </summary>
	/// <param name="length">An upper bound on the rendering's length.</param>
	/// <exception cref="OverflowException">Thrown when <paramref name="length"/> exceeds <see cref="Array.MaxLength"/>.</exception>
	private static void ThrowIfLongerThanAnyString(long length)
	{
		if (length > Array.MaxLength)
		{
			throw new OverflowException("The formatted text would be longer than a string can hold.");
		}
	}
}
