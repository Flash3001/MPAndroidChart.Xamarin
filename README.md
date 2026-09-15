# MPAndroidChart.Xamarin.Android

A .NET for Android binding by Lucas Teixeira for MPAndroidChart by Philipp Jahoda.
https://github.com/PhilJay/MPAndroidChart

## MPAndroidChart master @ 9c7275a0 (package 3.1.0.1)

* Upstream's last tagged release is still **v3.1.0** (April 2019), which is what package 3.1.0 binds. This
  package binds **master at 9c7275a0** (2025-06-05), the 63 commits made after the tag: the 2020 "catching
  up to iOS" batch (`Fill` gradients, pie highlight color, min/max axis label counts, `clipDataToContent`,
  no-data text alignment, `dataIndex` highlighting for combined charts, `ApproximatorN`), the revert that
  brought the `IValueFormatter` / `IAxisValueFormatter` interfaces back, and assorted bug fixes.
  Full upstream diff: https://github.com/PhilJay/MPAndroidChart/compare/v3.1.0...9c7275a0
* Targets **net10.0-android** (.NET 10 SDK, Android workload 36.1). `SupportedOSPlatformVersion` is 21, the
  lowest .NET 10 supports; MPChartLib itself is compiled against API 28 with `minSdkVersion 14` and has no
  runtime dependencies (`androidx.annotation` is compile-time only).
* The AAR is embedded in the assembly the .NET way (`AndroidLibrary`), the assembly is `IsTrimmable`, and the
  C# doc comments come from the Java sources jar (`JavaSourceJar`) instead of the old `JavaDocs/` folder.
* Namespaces are unchanged: `com.github.mikephil.charting.*` → `MikePhil.Charting.*`.

### API changes you will hit when upgrading from 3.1.0

These are upstream's changes, the binding just follows them:

| 3.1.0 binding                                                   | 3.1.0.1 binding                                                                                  |
|-----------------------------------------------------------------|--------------------------------------------------------------------------------------------------|
| `ValueFormatter` abstract class (`GetFormattedValue(float)`, `GetAxisLabel`, `GetBarLabel`, `GetPieLabel`, …) | Removed. `IValueFormatter.GetFormattedValue(float value, Entry entry, int dataSetIndex, ViewPortHandler vph)` and `IAxisValueFormatter.GetFormattedValue(float value, AxisBase axis)` are back (the 3.0.x shape). Implement them on a `Java.Lang.Object` subclass. |
| `IDataSet.ValueFormatter` / `AxisBase.ValueFormatter` typed `ValueFormatter` | Typed `IValueFormatter` / `IAxisValueFormatter`. `DefaultValueFormatter`, `LargeValueFormatter`, `PercentFormatter`, `StackedValueFormatter`, `IndexAxisValueFormatter` implement the interfaces. |
| `PercentFormatter(PieChart)`                                    | `PercentFormatter()` / `PercentFormatter(DecimalFormat)`                                        |
| `GradientColor`, `BaseDataSet.GradientColor`, `GradientColors`, `GetGradientColor(int)` | `MikePhil.Charting.Util.Fill` (solid, gradient or `Drawable`), `BarDataSet.Fills` / `SetFills`, `GetFill(int)`. `GradientColor` now derives from `Fill`; `SetGradientColor(start, end)` and `SetGradientColors(IList<Fill>)` still exist. |
| `DataSet.Values`                                                | `DataSet.Entries` (`Values` is kept as a deprecated alias)                                      |
| `DataRenderer.DrawValue(Canvas, string, float, float, int)` (abstract) | `DrawValue(Canvas, IValueFormatter, float, Entry, int, float, float, int)`                |
| `Chart.HighlightValue(x, y, dataSetIndex[, callListener])`      | Plus `HighlightValue(x[, y], dataSetIndex, dataIndex[, callListener])` and `Highlight(x, y, dataSetIndex, dataIndex)` for combined charts |

New since 3.1.0: `AxisBase.AxisMinLabels` / `AxisMaxLabels`, `YAxis.LabelXOffset`, `Chart.SetNoDataTextAlignment`,
`BarLineChartBase.SetClipDataToContent`, `PieDataSet.HighlightColor`, `PieDataSet.SetUseValueColorForLine`,
`DataSet.Entries`, `ApproximatorN`.

```csharp
var entries = Enumerable.Range(0, 8).Select(i => new Entry(i, (float)(Math.Sin(i) * 10 + 20))).ToList();
var set = new LineDataSet(entries, "sin") { ValueFormatter = new TwoDecimals() };   // : Java.Lang.Object, IValueFormatter
set.SetMode(LineDataSet.Mode.CubicBezier);
set.SetColors(ColorTemplate.MaterialColors.ToArray());
chart.Data = new LineData(set);
chart.XAxis.ValueFormatter = new IndexAxisValueFormatter(new[] { "a", "b", "c" });
chart.SetOnChartValueSelectedListener(new MyListener());                            // : Java.Lang.Object, IOnChartValueSelectedListenerSupport

var bars = new BarDataSet(barEntries, "bars") { Fills = new List<Fill> { new Fill(start, end), new Fill(solid) } };
```

`tools/SmokeTest/MainActivity.cs` has a longer version of the above covering every chart type.

## Building the binding

Requirements: .NET 10 SDK with the `android` workload and a JDK (11 or newer; the workload's configured JDK
is used). `global.json` pins the .NET SDK to 10.0.302.

```sh
dotnet build -c Release          # produces bin/Release/MPAndroidChart.3.1.0.1.nupkg
```

`NUGET.md` is the readme shown on nuget.org (install, quick start, upgrade notes); this file is for the repo.

`Transforms/Metadata.xml` carries the hand tweaks: the `MikePhil.*` namespaces, the `*Support` names for the
listener interfaces, `Chart.Data` typed as `ChartData` (Java declares it as the generic `T`), the `IDataSet`
generic overloads that the generator cannot bind, `Chart.CalculateOffsets` made public, and `ILineDataSet.getMode`
kept as `GetMode()` / `SetMode()` because `Mode` is also the nested enum. `Additions/` supplies the two members
the generator leaves abstract: `BarBuffer.Feed(Java.Lang.Object)` and `EntryXComparator.Compare(Object, Object)`.

## Regenerating the binding for a new upstream commit

1. `tools/build-aar.sh [ref]` clones PhilJay/MPAndroidChart at `ref` (default `master`), builds `MPChartLib`
   with Gradle (upstream still uses AGP 7.0.4 / Gradle 7.2, so the script picks JDK 11 and needs the
   `android-28` platform installed) and copies `MPChartLib-release.aar` and `MPChartLib-sources.jar` into `Jars/`.
2. Bump `<Version>` and `<PackageReleaseNotes>` in `MPAndroidChart.csproj`, then `dotnet build -c Release`.
   Metadata rules that no longer match anything show up as `BG8A00` / `BG8A04` warnings; delete them.
3. `dotnet build tools/SmokeTest` must compile (it uses the formatter interfaces, `Fill`, the listener and the
   `Chart.Data` fix). `dotnet build tools/SmokeTest -t:Run` installs and starts it on the connected emulator
   or device and shows every chart type.

## License

Copyright 2016-2026 Lucas Teixeira & Philipp Jahoda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
