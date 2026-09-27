## v2.6.3 (patch)

Changes since v2.6.2:

- Throw OverflowException when Multiply or Divide leaves the int exponent range ([@Claude](https://github.com/Claude))
- Throw ArgumentException from Clamp when min is greater than max ([@Claude](https://github.com/Claude))
- Report Radix as 10, since the type is base 10 ([@Claude](https://github.com/Claude))
- Move zero-to-a-power into its own helper to keep Pow's complexity down [patch] ([@Claude](https://github.com/Claude))
- Replace the nested ternaries in MaxMagnitude and MinMagnitude with an if [patch] ([@Claude](https://github.com/Claude))
- Drop the Debug.Assert that rejects decimals like 10.0m [patch] ([@Claude](https://github.com/Claude))
- Throw DivideByZeroException for zero to a negative power [patch] ([@Claude](https://github.com/Claude))
- Break MaxMagnitude and MinMagnitude ties by sign, as int and double do [patch] ([@Claude](https://github.com/Claude))
- Round to negative decimal places correctly for integers ([@Claude](https://github.com/Claude))
- Count trailing zeros in IsEvenInteger and IsOddInteger ([@Claude](https://github.com/Claude))

