namespace ktsu.PreciseNumber;

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
		bool success = typeof(TInput) == typeof(PreciseNumberBase) || typeof(TInput).IsSubclassOf(typeof(PreciseNumberBase));

		if (success)
		{
			preciseNumber = ((PreciseNumberBase)input).As<PreciseNumber>();
		}
		else
		{
			PreciseNumberBase preciseNumberBase;
			success = PreciseNumberBase.TryCreate((TInput)input, out preciseNumberBase!);
			preciseNumber = preciseNumberBase.As<PreciseNumber>();
		}

		return success
			? preciseNumber
			: throw new NotSupportedException();
	}
}
