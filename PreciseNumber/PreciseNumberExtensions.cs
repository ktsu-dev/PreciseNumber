// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

/// <summary>
/// Provides extension methods for converting numbers to <see cref="PreciseNumber"/>.
/// </summary>
public static class PreciseNumberExtensions
{
	/// <summary>
	/// How a numeric type should be converted to a <see cref="PreciseNumber"/>.
	/// </summary>
	private enum NumberKind
	{
		Unsupported,
		PreciseNumber,
		Integer,
		FloatingPoint,
	}

	/// <summary>
	/// Caches the conversion strategy for a numeric type so the interface probing below only ever
	/// runs once per type rather than once per conversion.
	/// </summary>
	private static class KindOf<TInput>
		where TInput : INumber<TInput>
	{
		internal static readonly NumberKind Kind = ClassifyType(typeof(TInput));
	}

	/// <summary>
	/// Caches the conversion strategy for runtime types that do not match their static type, such as
	/// a type derived from one that already implements <see cref="INumber{TSelf}"/>.
	/// </summary>
	private static readonly ConcurrentDictionary<Type, NumberKind> RuntimeKinds = new();

	private static NumberKind ClassifyType(Type type)
	{
		if (type == typeof(PreciseNumber) || type.IsSubclassOf(typeof(PreciseNumber)))
		{
			return NumberKind.PreciseNumber;
		}

		Type[] interfaces = type.GetInterfaces();

		if (Array.Exists(interfaces, i => i.Name.StartsWith("IBinaryInteger", StringComparison.Ordinal)))
		{
			return NumberKind.Integer;
		}

		return Array.Exists(interfaces, i => i.Name.StartsWith("IFloatingPoint", StringComparison.Ordinal))
			? NumberKind.FloatingPoint
			: NumberKind.Unsupported;
	}

	private static NumberKind ClassifyInput<TInput>(TInput input)
		where TInput : INumber<TInput>
	{
		NumberKind kind = KindOf<TInput>.Kind;

		// Reference types can be passed as a base type, in which case the runtime type is what
		// decides. Value types always match their static type, so this never boxes for them.
		return kind == NumberKind.Unsupported && !typeof(TInput).IsValueType
			? RuntimeKinds.GetOrAdd(input.GetType(), static t => ClassifyType(t))
			: kind;
	}

	/// <summary>
	/// Converts the input number to a <see cref="PreciseNumber"/>.
	/// </summary>
	/// <typeparam name="TInput">The type of the input number.</typeparam>
	/// <param name="input">The input number to convert.</param>
	/// <returns>The converted <see cref="PreciseNumber"/>.</returns>
	/// <exception cref="NotSupportedException">Thrown when the conversion cannot be performed because the input type is not supported.</exception>
	public static PreciseNumber ToPreciseNumber<TInput>(this TInput input)
		where TInput : INumber<TInput>
	{
		// if TInput is already a PreciseNumber then just return it
		if (input is PreciseNumber alreadyPrecise)
		{
			return alreadyPrecise;
		}

		return TryCreate(input, out PreciseNumber? preciseNumber)
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
	internal static bool TryCreate<TInput>([NotNullWhen(true)] TInput input, [MaybeNullWhen(false)][NotNullWhen(true)] out PreciseNumber? preciseNumber)
		where TInput : INumber<TInput>
	{
		if (input is PreciseNumber alreadyPrecise)
		{
			preciseNumber = alreadyPrecise;
			return true;
		}

		switch (ClassifyInput(input))
		{
			case NumberKind.Integer:
				preciseNumber = PreciseNumber.CreateFromInteger(input);
				return true;

			case NumberKind.FloatingPoint:
				preciseNumber = PreciseNumber.CreateFromFloatingPoint(input);
				return true;

			default:
				preciseNumber = null;
				return false;
		}
	}
}
