# ktsu.PreciseNumber

A high-precision numeric type for .NET that provides arbitrary precision arithmetic with a focus on accuracy. By combining the scale benefits of scientific notation with the precision of `BigInteger`, this library offers reliable and accurate mathematical operations where standard floating point types fall short.

[![License](https://img.shields.io/github/license/ktsu-dev/PreciseNumber.svg?label=License&logo=nuget)](LICENSE.md)

[![NuGet Version](https://img.shields.io/nuget/v/ktsu.PreciseNumber?label=Stable&logo=nuget)](https://nuget.org/packages/ktsu.PreciseNumber)
[![NuGet Version](https://img.shields.io/nuget/vpre/ktsu.PreciseNumber?label=Latest&logo=nuget)](https://nuget.org/packages/ktsu.PreciseNumber)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ktsu.PreciseNumber?label=Downloads&logo=nuget)](https://nuget.org/packages/ktsu.PreciseNumber)

[![GitHub commit activity](https://img.shields.io/github/commit-activity/m/ktsu-dev/PreciseNumber?label=Commits&logo=github)](https://github.com/ktsu-dev/PreciseNumber/commits/main)
[![GitHub contributors](https://img.shields.io/github/contributors/ktsu-dev/PreciseNumber?label=Contributors&logo=github)](https://github.com/ktsu-dev/PreciseNumber/graphs/contributors)
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/ktsu-dev/PreciseNumber/dotnet.yml?label=Build&logo=github)](https://github.com/ktsu-dev/PreciseNumber/actions)

## Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
  - [Installation](#installation)
  - [Requirements](#requirements)
  - [Quick Usage](#quick-usage)
    - [Basic Example](#basic-example)
    - [Common Operations](#common-operations)
    - [When to Use PreciseNumber](#when-to-use-precisenumber)
- [Advanced Usage](#advanced-usage)
  - [Type Conversions](#type-conversions)
  - [Mathematical Functions](#mathematical-functions)
  - [Parsing and Formatting](#parsing-and-formatting)
    - [Parsing from Strings](#parsing-from-strings)
    - [String Formatting](#string-formatting)
  - [Comparison with Built-in Types](#comparison-with-built-in-types)
    - [PreciseNumber vs. double/float](#precisenumber-vs-doublefloat)
    - [PreciseNumber vs. decimal](#precisenumber-vs-decimal)
    - [PreciseNumber vs. BigInteger](#precisenumber-vs-biginteger)
- [Technical Details](#technical-details)
  - [Internal Representation](#internal-representation)
  - [Precision Control](#precision-control)
  - [Limitations](#limitations)
  - [API Reference](#api-reference)
    - [PreciseNumber Class](#precisenumber-class)
    - [PreciseNumberExtensions Class](#precisenumberextensions-class)
- [License](#license)
- [Contributing](#contributing)
- [Acknowledgements](#acknowledgements)

## Features

- **Arbitrary Precision**: Based on `BigInteger` for the significand, allowing numbers of unlimited size.
- **Scientific Notation**: Uses an exponent and significand (the coefficient or mantissa in scientific notation) model similar to scientific notation.
- **Lossless Arithmetic**: Preserves precision during calculations with no rounding errors.
- **Full .NET Integration**: Implements `INumber<T>` interface for seamless integration with .NET's numeric ecosystem.
- **Comprehensive Mathematical Support**: Includes advanced mathematical functions like exponential operations (Pow, Exp, Squared, Cubed), constant values (Pi, E, Tau) with high precision, absolute value operations, and specialized numerical checks (isOdd, isEven, etc.)—all with arbitrary precision.
- **Balanced Performance**: The design prioritizes accuracy and precision while maintaining reasonable performance. For calculations where extreme precision matters more than raw speed, PreciseNumber delivers excellent results, though built-in numeric types remain faster for standard precision needs.

# Getting Started

## Installation

To install PreciseNumber, you can use the .NET CLI:

```sh
dotnet add package ktsu.PreciseNumber
```

Or you can use the NuGet Package Manager in Visual Studio by searching for `ktsu.PreciseNumber`.

## Requirements

This library requires .NET 8.0 or later.

## Quick Usage

### Basic Example

```csharp
using System.Numerics;
using ktsu.PreciseNumber;

// Create PreciseNumber from various types
var precise1 = 123.456.ToPreciseNumber();
var precise2 = BigInteger.Parse("1234567890").ToPreciseNumber();

// Perform calculation with high precision
var result = precise1 * precise2 / 7.89.ToPreciseNumber();
Console.WriteLine(result); // Displays accurate result with no floating point errors
```

### Common Operations

Create and perform operations with precise numbers:

```csharp
using ktsu.PreciseNumber;

// Create PreciseNumbers from various numeric types
var a = 123.456.ToPreciseNumber();
var b = 2.ToPreciseNumber();

// Basic arithmetic operations
var sum = a + b;        // 125.456
var difference = a - b; // 121.456
var product = a * b;    // 246.912
var quotient = a / b;   // 61.728

// Comparison
bool isGreater = a > b; // true
```

### When to Use PreciseNumber

PreciseNumber is ideal for:
- **Financial calculations** where exact precision is required beyond what decimal offers
- **Scientific computing** involving very large or small numbers with many significant digits
- **Cryptography** applications requiring arbitrary precision arithmetic
- **Mathematical algorithms** where rounding errors would accumulate and affect results

For everyday calculations where standard precision is sufficient, built-in types like `int`, `double`, or `decimal` will offer better performance.

# Advanced Usage

## Type Conversions

The library provides seamless round-trip conversions between standard numeric types and PreciseNumber using extension methods:

```csharp
using System.Numerics;
using ktsu.PreciseNumber;

// Convert FROM standard types TO PreciseNumber
int originalInt = 42;
double originalDouble = 3.14159;
decimal originalDecimal = 1234.5678m;
BigInteger originalBigInt = BigInteger.Parse("123456789012345678901234567890");

// Convert using the ToPreciseNumber() extension method
var preciseInt = originalInt.ToPreciseNumber();
var preciseDouble = originalDouble.ToPreciseNumber();
var preciseDecimal = originalDecimal.ToPreciseNumber();
var preciseBigInt = originalBigInt.ToPreciseNumber();

// Perform precise calculations if needed
preciseInt *= 10;
preciseDouble += PreciseNumber.Pi;

// Convert back FROM PreciseNumber TO standard types using To<T>()
int roundTripInt = preciseInt.To<int>();                  // 420
double roundTripDouble = preciseDouble.To<double>();      // ~6.28318
decimal roundTripDecimal = preciseDecimal.To<decimal>();  // 1234.5678
BigInteger roundTripBigInt = preciseBigInt.To<BigInteger>(); // 123456789012345678901234567890

// Verify round-trip conversion (for values that weren't modified)
Console.WriteLine(originalDecimal == roundTripDecimal);   // True
Console.WriteLine(originalBigInt == roundTripBigInt);     // True
```

### Mathematical Functions

PreciseNumber supports a wide range of mathematical operations:

```csharp
using ktsu.PreciseNumber;

var number = 2.5.ToPreciseNumber();

// Exponentiation
var squared = number.Squared();  // 6.25
var cubed = number.Cubed();      // 15.625
var toThe4th = number.Pow(4.ToPreciseNumber());  // 39.0625

// Constants
var pi = PreciseNumber.Pi;
var e = PreciseNumber.E;

// Exponential function
var expValue = PreciseNumber.Exp(1.ToPreciseNumber()); // e^1 = e

// Rounding and precision control
var roundedValue = number.Round(1);  // 2.5 (already at 1 decimal place)
var reducedValue = number.ReduceSignificance(1); // 3 (reduced to 1 significant digit)

// Min, Max, Abs, and Clamp
var absValue = (-5).ToPreciseNumber().Abs();  // 5
var maxValue = PreciseNumber.Max(2.ToPreciseNumber(), 3.ToPreciseNumber()); // 3
var minValue = PreciseNumber.Min(2.ToPreciseNumber(), 3.ToPreciseNumber()); // 2
var clampedValue = 10.ToPreciseNumber().Clamp(0, 5); // 5 (clamped to maximum)
```

## Parsing and Formatting

### Parsing from Strings

```csharp
using System.Globalization;
using System.Numerics;
using ktsu.PreciseNumber;

// Parse from string using various formats
var number1 = PreciseNumber.Parse("123.456", CultureInfo.InvariantCulture);
var number2 = PreciseNumber.Parse("1.23E4", NumberStyles.Any, CultureInfo.InvariantCulture);

// Try parsing with error handling
if (PreciseNumber.TryParse("456.789", out var result))
{
    Console.WriteLine($"Parsed successfully: {result}");
}
```

### String Formatting

Convert PreciseNumber to string:

```csharp
using ktsu.PreciseNumber;

var number = 123.456.ToPreciseNumber();
string formatted = number.ToString(); // "123.456"
```

## Comparison with Built-in Types

### PreciseNumber vs. double/float

**Advantages of PreciseNumber:**
- **No Rounding Errors**: Unlike floating-point types, PreciseNumber doesn't suffer from binary representation issues (e.g., 0.1 + 0.2 ≠ 0.3 in floating point)
- **Arbitrary Precision**: Not limited to 15-17 significant digits (double) or 6-9 significant digits (float)
- **Consistent Results**: Mathematical operations produce identical results regardless of magnitude
- **No Special Values**: PreciseNumber doesn't have NaN or Infinity values that can propagate through calculations

```csharp
// Double arithmetic issue
double a = 0.1;
double b = 0.2;
Console.WriteLine(a + b == 0.3);  // False (equals 0.30000000000000004)

// PreciseNumber solves this
var pa = 0.1.ToPreciseNumber();
var pb = 0.2.ToPreciseNumber();
Console.WriteLine((pa + pb) == 0.3.ToPreciseNumber());  // True (exactly 0.3)
```

### PreciseNumber vs. decimal

**Advantages of PreciseNumber:**
- **Unlimited Range**: Not constrained by decimal's ±7.9E±28 range
- **Unlimited Precision**: Decimal is limited to 28-29 significant digits
- **Scientific Operations**: Better suited for scientific calculations requiring extreme precision
- **More Flexible Format**: Exponent-significand model makes it suitable for both very large and very small numbers

```csharp
// Decimal range/precision limitations
decimal largeDecimal = 1.0m;
for (int i = 0; i < 30; i++)
    largeDecimal *= 10; // Will throw OverflowException

// PreciseNumber handles this easily
var largePrecise = PreciseNumber.One;
for (int i = 0; i < 1000; i++)
    largePrecise *= 10; // Works fine with arbitrary large values
```

### PreciseNumber vs. BigInteger

**Advantages of PreciseNumber:**
- **Decimal Point Support**: Represents both integer and fractional parts while BigInteger only handles integers
- **Scientific Notation**: More convenient for very large or small numbers with fraction components
- **Mathematical Constants**: Built-in support for constants like Pi and E with high precision

# Technical Details

## Internal Representation

PreciseNumber stores values in the form: `significand × 10^exponent`

- **Significand**: A `BigInteger` that contains all the significant digits
- **Exponent**: An `int` that determines the decimal place

This representation allows for:
- Exact representation of integers of any size
- High precision for decimal values
- Accurate arithmetic without floating-point errors

## Precision Control

You can control precision using:

- **Round()**: Rounds to a specific number of decimal places
- **ReduceSignificance()**: Reduces to a specific number of significant digits

## Limitations

- Operations that inherently require approximation (like certain roots or logarithms) fall back to `double` precision for calculation
- Conversion to standard types may throw `OverflowException` if the value is too large

## API Reference

### PreciseNumber Class

- **Constants**: `Zero`, `One`, `NegativeOne`, `Pi`, `E`, `Tau`
- **Arithmetic**: `+`, `-`, `*`, `/`, `%`, `++`, `--`
- **Comparison**: `==`, `!=`, `<`, `>`, `<=`, `>=`
- **Functions**: `Abs()`, `Round()`, `Clamp()`, `Squared()`, `Cubed()`, `Pow()`, `Exp()`
- **Utility**: `ToString()`, `Parse()`, `TryParse()`, `To<T>()`

### PreciseNumberExtensions Class

- **Conversion**: `ToPreciseNumber<T>()` extension method for any `INumber<T>`

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any improvements or bug fixes.

## Acknowledgements

Thanks to the .NET community and ktsu.dev contributors for their support.
