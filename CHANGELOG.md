## v2.6.12-pre.1 (prerelease)

Changes since v2.6.11:

- Bump Polyfill from 11.4.3 to 11.4.5 (dependabot[bot])

## v2.6.11 (patch)

Changes since v2.6.10:

- Split fixed-point formatting helpers and use Assert.Contains [patch] (Claude)
- Support the F, N, E, R and G\<n\> standard format strings [patch] (Claude)
- Throw OverflowException when Add/Subtract/Mod exponents are too far apart to align [patch] (Claude)

## v2.6.10 (patch)

Changes since v2.6.9:

- Stop ExpM1, LogP1 and the hyperbolics building million-digit sums for large arguments [patch] (Claude)
- Round a fractional Pow once, from a guarded width [patch] (Claude)

## v2.6.9 (patch)

Changes since v2.6.8:

- Honour AllowDecimalPoint, AllowExponent and AllowLeadingSign in Parse [patch] (Claude)

## v2.6.8 (patch)

Changes since v2.6.7:

- Root the reciprocal in integers for a negative-degree RootN [patch] (Claude)
- Check Tan's precision ceiling against the requested digits [patch] (Claude)
- Clamp default trig precision to what the reduction can deliver [patch] (Claude)

## v2.6.7 (patch)

Changes since v2.6.6:

- Move the integer-power path into IntegerPow to keep Pow's complexity down (Claude)
- Return ±1 for -1 raised to an integer too large for an int [patch] (Claude)

## v2.6.7-pre.1 (prerelease)

Changes since v2.6.6:

- Exercise StringBuilder paths that actually reach TryFormat ([@Copilot](https://github.com/Copilot))
- Throw OverflowException from TryFormat when no string can hold the text ([@Copilot](https://github.com/Copilot))
- Initial plan ([@Copilot](https://github.com/Copilot))

## v2.6.6 (patch)

Changes since v2.6.5:

- Cover AsinPi, AcosPi and TanPi on their rounding paths (Claude)
- Round exact quotients of approximations to the requested digits [patch] (Claude)
- Widen ln 2 by x's integer digits in Exp2 [patch] (Claude)

## v2.6.5 (patch)

Changes since v2.6.4:

- Move CI onto the shared ci-shared.yml pipeline (Claude)

## v2.6.4 (patch)

Changes since v2.6.3:

- Parse with the provider's decimal separator and signs [patch] (Claude)
- Reject a precision below one in ReduceSignificance [patch] (Claude)
- Report zero as not normal, and nothing as subnormal [patch] (Claude)

## v2.6.3 (patch)

Changes since v2.6.2:

- Throw OverflowException when Multiply or Divide leaves the int exponent range (Claude)
- Throw ArgumentException from Clamp when min is greater than max (Claude)
- Report Radix as 10, since the type is base 10 (Claude)
- Move zero-to-a-power into its own helper to keep Pow's complexity down [patch] (Claude)
- Replace the nested ternaries in MaxMagnitude and MinMagnitude with an if [patch] (Claude)
- Drop the Debug.Assert that rejects decimals like 10.0m [patch] (Claude)
- Throw DivideByZeroException for zero to a negative power [patch] (Claude)
- Break MaxMagnitude and MinMagnitude ties by sign, as int and double do [patch] (Claude)
- Round to negative decimal places correctly for integers (Claude)
- Count trailing zeros in IsEvenInteger and IsOddInteger (Claude)

## v2.6.2 (patch)

Changes since v2.6.1:

- Hash the sanitized form, so Equals and GetHashCode agree [patch] (Claude)
- Reduce by the GCD before testing whether a quotient terminates [patch] (Claude)

## v2.6.1 (patch)

Changes since v2.6.0:

- Bump the ktsu group with 9 updates (dependabot[bot])

## v2.6.0 (minor)

Changes since v2.5.0:

- Merge remote-tracking branch 'origin/main' into claude/precisenumber-78-hyperbolic-functions (Claude)
- Refuse a sine whose reduction needs more of pi than exists [patch] (Claude)
- Measure a baseline ref in the same job as the head [patch] (matt-edmondson)
- Assert comparisons with the comparison asserts [patch] (matt-edmondson)
- Implement IHyperbolicFunctions\<PreciseNumber\> [minor] (matt-edmondson)

## v2.5.0 (minor)

Changes since v2.4.0:

- Implement ITrigonometricFunctions\<PreciseNumber\>, plus Atan2 [minor] (Claude)

## v2.4.0 (minor)

Changes since v2.3.0:

- Project the test sweeps instead of remapping the iteration variable [patch] (matt-edmondson)
- Implement IExponentialFunctions, ILogarithmicFunctions and IPowerFunctions [minor] (matt-edmondson)

## v2.3.1 (patch)

Changes since v2.3.0:

- Bump the ktsu group with 9 updates (dependabot[bot])

## v2.3.0 (minor)

Changes since v2.2.0:

- Implement IRootFunctions\<PreciseNumber\> with Sqrt, Cbrt, RootN and Hypot [minor] (Claude)
- Measure this type against a bare double, and chart it per release [patch] (Claude)

## v2.2.1 (patch)

Changes since v2.2.0:

- Measure this type against a bare double, and chart it per release [patch] (Claude)

## v2.2.0 (minor)

Changes since v2.1.0:

- Use Assert.IsGreaterThanOrEqualTo for the precision floor [patch] (Claude)
- Carry Pi, Tau and E at 150 correctly rounded digits, and add Ln2 and Ln10 [minor] (Claude)

## v2.1.0 (minor)

Changes since v2.0.0:

- Seed the chart with every release that can be measured the same way [patch] (Claude)
- Map the reports to parsed documents rather than reparsing in the body [patch] (Claude)
- Write the benchmark history tool in C# rather than Python [patch] (Claude)
- Chart performance per release, and show it in the README [minor] (Claude)
- Gate Dependabot auto-merge on CI actually being green (Claude)
- fix: do the formatting length arithmetic in long [patch] (Claude)
- Write benchmark results where the workflow looks for them [patch] (Claude)
- [patch] Fix conversion edge cases found in review (matt-edmondson)
- ci: adopt the consolidated .NET workflow [patch] (Claude)

## v2.0.5 (patch)

Changes since v2.0.4:

- Gate Dependabot auto-merge on CI actually being green (Claude)

## v2.0.4 (patch)

Changes since v2.0.3:

- fix: do the formatting length arithmetic in long [patch] (Claude)

## v2.0.3 (patch)

Changes since v2.0.2:

- Write benchmark results where the workflow looks for them [patch] (Claude)

## v2.0.2 (patch)

Changes since v2.0.1:

- [patch] Fix conversion edge cases found in review (matt-edmondson)

## v2.0.1 (patch)

Changes since v2.0.0:

- ci: adopt the consolidated .NET workflow [patch] (Claude)

## v2.0.0 (major)

Changes since v1.9.0:

- [major] Make PreciseNumber a value type (matt-edmondson)

## v1.9.0 (minor)

Changes since v1.8.0:

- Divide exactly, or to a chosen precision (Claude)

## v1.8.0 (minor)

Changes since v1.7.0:

- Fix script injection in the benchmark workflow (Claude)
- Grow the power of ten cache on demand (Claude)
- Add a BenchmarkDotNet suite for the library's hot paths (Claude)
- Optimize hot paths: comparisons, multiply, pow, digit counting, formatting (Claude)
- ci: make the SonarQube quality gate opt in [patch] (matt-edmondson)
- ci: adopt the unified dotnet workflow [patch] (matt-edmondson)
- chore: store icon.png in LFS as .gitattributes declares (matt-edmondson)
- docs: scope build badge to the default branch (matt-edmondson)
- docs: correct README, DESCRIPTION and TAGS metadata (matt-edmondson)
- Stop Update SDKs failing when there is nothing to update (matt-edmondson)
- Fix build against ktsu.Sdk 2.27.0 (matt-edmondson)
- Sync .github\workflows\dotnet.yml (KtsuTools)
- Sync .editorconfig (KtsuTools)
- Sync global.json (KtsuTools)
- Sync .github\workflows\dotnet.yml (KtsuTools)
- chore: update ktsu.Sdk to 2.21.1 [patch] (matt-edmondson)
- chore: remove SourceLink package versions from central props (matt-edmondson)
- chore: remove SourceLink package references (matt-edmondson)
- Add TAGS.md with NuGet package tags (matt-edmondson)
- Sync .github\workflows\dotnet.yml (KtsuTools)
- Refactor CI pipeline to clone KtsuBuild at latest tag and streamline release logic (matt-edmondson)
- Remove legacy build scripts (matt-edmondson)
- Remove .github\workflows\project.yml (matt-edmondson)
- Refactor input handling in PreciseNumber creation methods (matt-edmondson)
- migrate to dotnet 10 (matt-edmondson)
- Update sdk (matt-edmondson)
- Update configuration files and scripts for improved build and test processes (matt-edmondson)
- Remove obsolete build configuration files and update PreciseNumber class for improved variable declarations and copyright information. (matt-edmondson)
- Update README and project files (matt-edmondson)

## v1.7.36 (patch)

Changes since v1.7.35:

- Bump Polyfill from 11.2.0 to 11.3.0 (dependabot[bot])

## v1.7.35 (patch)

No significant changes detected since v1.7.34.

## v1.7.34 (patch)

Changes since v1.7.33:

- ci: make the SonarQube quality gate opt in [patch] (matt-edmondson)
- ci: adopt the unified dotnet workflow [patch] (matt-edmondson)

## v1.7.33 (patch)

Changes since v1.7.32:

- Bump the ktsu group with 9 updates (dependabot[bot])

## v1.7.32 (patch)

Changes since v1.7.31:

- Bump the ktsu group with 9 updates (dependabot[bot])

## v1.7.31 (patch)

Changes since v1.7.30:

- Bump the ktsu group with 9 updates (dependabot[bot])

## v1.7.30 (patch)

Changes since v1.7.29:

- chore: store icon.png in LFS as .gitattributes declares (matt-edmondson)
- docs: scope build badge to the default branch (matt-edmondson)
- docs: correct README, DESCRIPTION and TAGS metadata (matt-edmondson)

## v1.7.29 (patch)

Changes since v1.7.28:

- Stop Update SDKs failing when there is nothing to update (matt-edmondson)

## v1.7.28 (patch)

Changes since v1.7.27:

- Fix build against ktsu.Sdk 2.27.0 (matt-edmondson)
- Sync .github\workflows\dotnet.yml (KtsuTools)
- Sync .editorconfig (KtsuTools)
- Sync global.json (KtsuTools)
- Sync .github\workflows\dotnet.yml (KtsuTools)

## v1.7.27 (patch)

Changes since v1.7.26:

- chore: update ktsu.Sdk to 2.21.1 [patch] (matt-edmondson)

## v1.7.26 (patch)

Changes since v1.7.25:

- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync global.json (ktsu[bot])

## v1.7.25 (patch)

Changes since v1.7.24:

- Bump the ktsu group with 8 updates (dependabot[bot])

## v1.7.24 (patch)

Changes since v1.7.23:

- Sync .github\workflows\update-sdks.yml (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync .github\workflows\dependabot-merge.yml (ktsu[bot])
- Sync .gitattributes (ktsu[bot])

## v1.7.23 (patch)

Changes since v1.7.22:

- Bump MSTest.Sdk from 4.3.2 to 4.3.3 (dependabot[bot])

## v1.7.22 (patch)

Changes since v1.7.21:

- Bump the ktsu group with 8 updates (dependabot[bot])

## v1.7.21 (patch)

Changes since v1.7.20:

- Bump Polyfill from 11.0.0 to 11.0.1 (dependabot[bot])

## v1.7.20 (patch)

Changes since v1.7.19:

- Bump Polyfill from 10.11.2 to 11.0.0 (dependabot[bot])

## v1.7.19 (patch)

Changes since v1.7.18:

- Bump MSTest.Sdk from 4.3.0 to 4.3.2 (dependabot[bot])

## v1.7.18 (patch)

Changes since v1.7.17:

- Bump MSTest.Sdk from 4.2.3 to 4.3.0 (dependabot[bot])

## v1.7.17 (patch)

Changes since v1.7.16:

- Bump Polyfill from 10.11.1 to 10.11.2 (dependabot[bot])

## v1.7.16 (patch)

Changes since v1.7.15:

- Bump the ktsu group with 8 updates (dependabot[bot])

## v1.7.15 (patch)

Changes since v1.7.14:

- Bump Polyfill from 10.11.0 to 10.11.1 (dependabot[bot])
- Bump the ktsu group with 8 updates (dependabot[bot])

## v1.7.14 (patch)

Changes since v1.7.13:

- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync global.json (ktsu[bot])

## v1.7.13 (patch)

Changes since v1.7.12:

- chore: remove SourceLink package versions from central props (matt-edmondson)
- chore: remove SourceLink package references (matt-edmondson)

## v1.7.12 (patch)

Changes since v1.7.11:

- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync .github\dependabot.yml (ktsu[bot])
- Sync icon.png (ktsu[bot])
- Sync .gitignore (ktsu[bot])
- Sync .gitattributes (ktsu[bot])
- Sync global.json (ktsu[bot])
- Bump Polyfill from 10.10.0 to 10.11.0 (dependabot[bot])

## v1.7.11 (patch)

Changes since v1.7.10:

- Bump the ktsu group with 8 updates (dependabot[bot])

## v1.7.10 (patch)

Changes since v1.7.9:

- Bump Polyfill from 10.8.1 to 10.10.0 (dependabot[bot])
- Bump the ktsu group with 8 updates (dependabot[bot])

## v1.7.9 (patch)

Changes since v1.7.8:

- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync global.json (ktsu[bot])

## v1.7.8 (patch)

Changes since v1.7.7:

- Add TAGS.md with NuGet package tags (matt-edmondson)
- Sync .github\workflows\dotnet.yml (KtsuTools)

## v1.7.8-pre.1 (prerelease)

Changes since v1.7.7:

- Sync .github\workflows\dotnet.yml (KtsuTools)

## v1.7.7 (patch)

Changes since v1.7.6:

- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])

## v1.7.7-pre.1 (prerelease)

Changes since v1.7.6:

- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])

## v1.7.6 (patch)

Changes since v1.7.5:

- Refactor CI pipeline to clone KtsuBuild at latest tag and streamline release logic (matt-edmondson)
- Remove legacy build scripts (matt-edmondson)

## v1.7.6-pre.6 (prerelease)

Changes since v1.7.6-pre.5:

- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync scripts\update-winget-manifests.ps1 (ktsu[bot])

## v1.7.6-pre.5 (prerelease)

Changes since v1.7.6-pre.4:

- Sync scripts\update-winget-manifests.ps1 (ktsu[bot])
- Sync scripts\PSBuild.psm1 (ktsu[bot])
- Sync global.json (ktsu[bot])

## v1.7.6-pre.4 (prerelease)

Changes since v1.7.6-pre.3:

- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync scripts\PSBuild.psm1 (ktsu[bot])
- Sync scripts\PSBuild.psm1 (ktsu[bot])
- Sync global.json (ktsu[bot])

## v1.7.6-pre.3 (prerelease)

Changes since v1.7.6-pre.2:

- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync global.json (ktsu[bot])
- Sync COPYRIGHT.md (ktsu[bot])

## v1.7.6-pre.2 (prerelease)

Changes since v1.7.6-pre.1:

- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync scripts\PSBuild.psm1 (ktsu[bot])

## v1.7.6-pre.1 (prerelease)

Changes since v1.7.5:

- Sync scripts\update-winget-manifests.ps1 (ktsu[bot])
- Sync global.json (ktsu[bot])
- Sync COPYRIGHT.md (ktsu[bot])

## v1.7.5 (patch)

Changes since v1.7.4:

- Remove .github\workflows\project.yml (matt-edmondson)

## v1.7.4 (patch)

Changes since v1.7.3:

- Refactor input handling in PreciseNumber creation methods (matt-edmondson)

## v1.7.3 (patch)

Changes since v1.7.2:

- migrate to dotnet 10 (matt-edmondson)

## v1.7.3-pre.4 (prerelease)

Changes since v1.7.3-pre.3:

- Sync scripts\update-winget-manifests.ps1 (ktsu[bot])

## v1.7.3-pre.3 (prerelease)

Changes since v1.7.3-pre.2:

- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])

## v1.7.3-pre.2 (prerelease)

Changes since v1.7.3-pre.1:

- Sync scripts\PSBuild.psm1 (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])

## v1.7.3-pre.1 (prerelease)

Changes since v1.7.2:

- Sync scripts\PSBuild.psm1 (ktsu[bot])
- Sync scripts\update-winget-manifests.ps1 (ktsu[bot])
- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync .gitattributes (ktsu[bot])
- Sync .runsettings (ktsu[bot])

## v1.7.2 (patch)

Changes since v1.7.1:

- Update sdk (matt-edmondson)
- Update configuration files and scripts for improved build and test processes (matt-edmondson)

## v1.7.2-pre.17 (prerelease)

Changes since v1.7.2-pre.16:

- Merge remote-tracking branch 'refs/remotes/origin/main' (ktsu[bot])
- Sync scripts\PSBuild.psm1 (ktsu[bot])
- Sync .editorconfig (ktsu[bot])
- Sync .gitattributes (ktsu[bot])
- Sync .gitignore (ktsu[bot])
- Sync .mailmap (ktsu[bot])
- Sync .runsettings (ktsu[bot])

## v1.7.2-pre.16 (prerelease)

Changes since v1.7.2-pre.15:

- Sync scripts\PSBuild.psm1 (ktsu[bot])
- Sync .editorconfig (ktsu[bot])
- Sync .gitattributes (ktsu[bot])
- Sync .gitignore (ktsu[bot])
- Sync .mailmap (ktsu[bot])
- Sync .runsettings (ktsu[bot])

## v1.7.2-pre.15 (prerelease)

No significant changes detected since v1.7.2-pre.14.

## v1.7.2-pre.14 (prerelease)

No significant changes detected since v1.7.2-pre.13.

## v1.7.2-pre.13 (prerelease)

No significant changes detected since v1.7.2-pre.12.

## v1.7.2-pre.12 (prerelease)

No significant changes detected since v1.7.2-pre.11.

## v1.7.2-pre.11 (prerelease)

No significant changes detected since v1.7.2-pre.10.

## v1.7.2-pre.10 (prerelease)

No significant changes detected since v1.7.2-pre.9.

## v1.7.2-pre.9 (prerelease)

No significant changes detected since v1.7.2-pre.8.

## v1.7.2-pre.8 (prerelease)

No significant changes detected since v1.7.2-pre.7.

## v1.7.2-pre.7 (prerelease)

No significant changes detected since v1.7.2-pre.6.

## v1.7.2-pre.6 (prerelease)

No significant changes detected since v1.7.2-pre.5.

## v1.7.2-pre.5 (prerelease)

No significant changes detected since v1.7.2-pre.4.

## v1.7.2-pre.4 (prerelease)

No significant changes detected since v1.7.2-pre.3.

## v1.7.2-pre.3 (prerelease)

No significant changes detected since v1.7.2-pre.2.

## v1.7.2-pre.2 (prerelease)

No significant changes detected since v1.7.2-pre.1.

## v1.7.2-pre.1 (prerelease)

Changes since v1.7.1:

- Sync scripts\PSBuild.psm1 (ktsu[bot])

## v1.7.1 (patch)

Changes since v1.7.0:

- Remove obsolete build configuration files and update PreciseNumber class for improved variable declarations and copyright information. (matt-edmondson)
- Update README and project files (matt-edmondson)

## v1.7.1-pre.1 (prerelease)

Changes since v1.7.0:

- Sync .github\workflows\dotnet.yml (ktsu[bot])
- Sync .editorconfig (ktsu[bot])
- Sync .runsettings (ktsu[bot])

## v1.7.0 (minor)

Changes since v1.6.0:

- Enhance PreciseNumber derived classes (matt-edmondson)

## v1.6.0 (minor)

Changes since v1.5.0:

- Enhance type check in PreciseNumber conversion method (matt-edmondson)

## v1.5.0 (minor)

Changes since v1.4.0:

- Improve type checking in ToPreciseNumber and TryCreate (matt-edmondson)

## v1.4.0 (minor)

Changes since v1.3.0:

- Update TryParse methods to use nullable PreciseNumber (matt-edmondson)

## v1.3.0 (minor)

Changes since v1.2.0:

- Enhance PreciseNumber functionality and tests (matt-edmondson)

## v1.2.0 (minor)

Changes since v1.1.0:

- Update PreciseNumber properties to public access (matt-edmondson)
- Added documentation in README.md (matt-edmondson)

## v1.1.2-pre.1 (prerelease)

Changes since v1.1.1:

- Sync .editorconfig (ktsu[bot])

## v1.1.1 (patch)

Changes since v1.1.0:

- Added documentation in README.md (matt-edmondson)

## v1.1.0 (minor)

Changes since v1.0.0-alpha.4:

- Fix code style violations and add more test coverage (matt-edmondson)
- Add LICENSE template (matt-edmondson)
- Add automation scripts for metadata management and versioning (matt-edmondson)
- Renamed metadata files (matt-edmondson)
- Replace LICENSE file with LICENSE.md and update copyright information (matt-edmondson)
- Update PreciseNumber.sln with new test project path (matt-edmondson)
- Rename Tests dir to PreciseNumber.Test (matt-edmondson)
- Add PreciseNumber.Test project and update solution structure (matt-edmondson)
- Update license file format and content (matt-edmondson)
- Update namespace to ktsu.PreciseNumber (matt-edmondson)
- Change ReduceSignificance to public (matt-edmondson)

## v1.0.0-alpha.4 (prerelease)

Changes since v1.0.0-alpha.3:

- 1.0.0-alpha.4 (matt-edmondson)
- Add new PreciseNumber constructor overload (matt-edmondson)
- Fix accessibility for tests (matt-edmondson)

## v1.0.0-alpha.3 (prerelease)

- 1.0.0-alpha.3 (matt-edmondson)
- Enhance PreciseNumber subclassing support (matt-edmondson)
- Update description (matt-edmondson)
- 1.0.0-alpha.2 (matt-edmondson)
- Refactor and enhance PreciseNumber class (matt-edmondson)
- Add .runsettings and update build props (matt-edmondson)
- Add project files. (matt-edmondson)

