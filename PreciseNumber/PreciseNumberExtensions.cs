namespace ktsu.io.PreciseNumber;

using System.Diagnostics.CodeAnalysis;
using System.Numerics;

/// <summary>
/// Provides extension methods for converting numbers to <see cref="PreciseNumber"/>.
/// </summary>
public static class PreciseNumberExtensions
{
	/// <summary>
	/// Converts the input number to a <see cref="PreciseNumber"/>.
	/// </summary>
	/// <typeparam name="TInput">The type of the input number.</typeparam>
	/// <param name="input">The input number to convert.</param>
	/// <returns>The converted <see cref="PreciseNumber"/>.</returns>
	public static PreciseNumber ToPreciseNumber<TInput>(this INumber<TInput> input)
		where TInput : INumber<TInput>
	{
		// if TInput is already a PreciseNumber then just return it
		PreciseNumber preciseNumber;
		bool success = typeof(TInput) == typeof(PreciseNumber);

		if (success)
		{
			preciseNumber = (PreciseNumber)(object)input;
		}
		else
		{
			success = TryCreate((TInput)input, out preciseNumber!);
		}

		return success
			? preciseNumber
			: throw new NotSupportedException();
	}

	/// <summary>
	/// Tries to create a <see cref="PreciseNumber"/> from the input.
	/// </summary>
	/// <typeparam name="TInput">The type of the input number.</typeparam>
	/// <param name="input">The input number to create a <see cref="PreciseNumber"/> from.</param>
	/// <param name="preciseNumber">The created <see cref="PreciseNumber"/> if successful, otherwise null.</param>
	/// <returns>True if the creation was successful, otherwise false.</returns>
	internal static bool TryCreate<TInput>([NotNullWhen(true)] TInput input, [MaybeNullWhen(false)] out PreciseNumber preciseNumber)
		where TInput : INumber<TInput>
	{
		var type = typeof(TInput);
		if (Array.Exists(type.GetInterfaces(), i => i.Name.StartsWith("IBinaryInteger", StringComparison.Ordinal)))
		{
			preciseNumber = PreciseNumber.CreateFromInteger(input);
			return true;
		}

		if (Array.Exists(type.GetInterfaces(), i => i.Name.StartsWith("IFloatingPoint", StringComparison.Ordinal)))
		{
			preciseNumber = PreciseNumber.CreateFromFloatingPoint(input);
			return true;
		}

		preciseNumber = default;
		return false;
	}
}
