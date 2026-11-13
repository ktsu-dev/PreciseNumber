## v2.6.12 (patch)

Changes since v2.6.11:

- Route Log in a base's domain errors through DomainError [patch] (#205) (matt-edmondson)
- Reject a NUL inside the exponent in Parse [patch] (#206) (matt-edmondson)
- Read a currency symbol outside the parentheses in Parse [patch] (#207) (matt-edmondson)
- Read white space around a sign or currency symbol as the BCL does [patch] (#191) (matt-edmondson)
- Render a domain error's argument in E notation, so its message stays short [patch] (#192) (matt-edmondson)
- Ignore a run of trailing NULs in Parse, as decimal and double do [patch] (#194) (matt-edmondson)
- Name newBase, not x, when Log is given a base outside its domain [patch] (#197) (matt-edmondson)
- Read the number separators as well as the currency ones under AllowCurrencySymbol [patch] (#198) (matt-edmondson)
- Convert an INumber type that converts itself to PreciseNumber in ToPreciseNumber [patch] (#199) (matt-edmondson)
- Honour AllowParentheses, AllowTrailingSign and AllowCurrencySymbol in Parse [patch] (#184) (matt-edmondson)
- Round Log in a base, Log2, Log2P1 and Log10P1 to the digits asked for [patch] (#183) (matt-edmondson)
- Trim only ASCII white space under AllowLeadingWhite and AllowTrailingWhite [patch] (#182) (matt-edmondson)
- Merge remote-tracking branch 'origin/main' into fix/174-typed-space-group-separator (Claude)
- Return Zero from the overflow catch instead of leaving it empty (Claude)
- Parse values and zeros at the edges of the exponent range [patch] (Claude)
- Reject hex and binary NumberStyles in Parse and TryParse [patch] (Claude)
- Let a typed space match a no-break-space separator in Parse [patch] (Claude)
- Convert NFloat in CreateChecked, CreateSaturating, CreateTruncating and To() [patch] (Claude)

