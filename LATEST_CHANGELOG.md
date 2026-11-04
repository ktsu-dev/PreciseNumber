## v2.6.12 (patch)

Changes since v2.6.11:

- Honour AllowParentheses, AllowTrailingSign and AllowCurrencySymbol in Parse [patch] (#184) (matt-edmondson)
- Round Log in a base, Log2, Log2P1 and Log10P1 to the digits asked for [patch] (#183) (matt-edmondson)
- Trim only ASCII white space under AllowLeadingWhite and AllowTrailingWhite [patch] (#182) (matt-edmondson)
- Merge remote-tracking branch 'origin/main' into fix/174-typed-space-group-separator (Claude)
- Return Zero from the overflow catch instead of leaving it empty (Claude)
- Parse values and zeros at the edges of the exponent range [patch] (Claude)
- Reject hex and binary NumberStyles in Parse and TryParse [patch] (Claude)
- Let a typed space match a no-break-space separator in Parse [patch] (Claude)
- Convert NFloat in CreateChecked, CreateSaturating, CreateTruncating and To() [patch] (Claude)

