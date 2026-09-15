# MPAndroidChart for .NET Android

A .NET for Android binding of [MPAndroidChart](https://github.com/PhilJay/MPAndroidChart) by Philipp Jahoda:
line, bar, horizontal bar, pie, scatter, candlestick, bubble, radar and combined charts with scaling, dragging,
animations, highlighting, custom formatters and markers.

Binding by Lucas Teixeira. Source and issues: https://github.com/Flash3001/MPAndroidChart.Xamarin

## Version 3.1.0.1

* Targets **net10.0-android** (.NET 10). Minimum Android API 21. No package dependencies.
* Binds MPAndroidChart **master @ 9c7275a0** (5 June 2025). Upstream has not tagged a release since v3.1.0
  (April 2019); this build carries the 63 commits made after it: `Fill` gradients for bars, pie highlight
  color, min/max axis label counts, `dataIndex` highlighting for combined charts, `ApproximatorN`, the
  restored `IValueFormatter` / `IAxisValueFormatter` interfaces and a number of bug fixes.
  Upstream diff: https://github.com/PhilJay/MPAndroidChart/compare/v3.1.0...9c7275a0
* IntelliSense comes from the library's Javadoc.

Looking for Xamarin.Android (MonoAndroid)? Use package 3.1.0.

## Install

```sh
dotnet add package MPAndroidChart
```

```xml
<PackageReference Include="MPAndroidChart" Version="3.1.0.1" />
```

## Quick start

Layouts keep the Java class names:

```xml
<com.github.mikephil.charting.charts.LineChart
    android:id="@+id/chart"
    android:layout_width="match_parent"
    android:layout_height="300dp" />
```

The Java packages `com.github.mikephil.charting.*` become the `MikePhil.Charting.*` namespaces
(`Charts`, `Data`, `Components`, `Formatter`, `Listener`, `Highlight`, `Util`, `Interfaces.Datasets`, …).

```csharp
using MikePhil.Charting.Charts;
using MikePhil.Charting.Components;
using MikePhil.Charting.Data;
using MikePhil.Charting.Formatter;
using MikePhil.Charting.Highlight;
using MikePhil.Charting.Listener;
using MikePhil.Charting.Util;

var chart = FindViewById<LineChart>(Resource.Id.chart)!;

var entries = Enumerable.Range(0, 12)
    .Select(i => new Entry(i, (float)(Math.Sin(i) * 10 + 20)))
    .ToList();

var set = new LineDataSet(entries, "sin") { LineWidth = 2f, ValueFormatter = new TwoDecimals() };
set.SetMode(LineDataSet.Mode.CubicBezier);
set.SetColors(ColorTemplate.MaterialColors.ToArray());
set.SetDrawFilled(true);

chart.Data = new LineData(set);
chart.XAxis.ValueFormatter = new IndexAxisValueFormatter(new[] { "Jan", "Feb", "Mar", "Apr" });
chart.Description.Text = "";
chart.SetOnChartValueSelectedListener(new Selection());
chart.AnimateX(800);

// Formatters and listeners are Java interfaces: implement them on a Java.Lang.Object subclass.
sealed class TwoDecimals : Java.Lang.Object, IValueFormatter
{
    public string GetFormattedValue(float value, Entry entry, int dataSetIndex, ViewPortHandler vph)
        => value.ToString("0.00");
}

sealed class Selection : Java.Lang.Object, IOnChartValueSelectedListenerSupport
{
    public void OnValueSelected(Entry e, Highlight h) { /* e.GetX(), e.GetY(), h.DataSetIndex */ }
    public void OnNothingSelected() { }
}
```

Bar charts with solid or gradient fills:

```csharp
var bars = new BarDataSet(barEntries, "bars")
{
    Fills = new List<Fill> { new Fill(Color.Red.ToArgb(), Color.Blue.ToArgb()), new Fill(Color.Green.ToArgb()) },
};
barChart.Data = new BarData(bars);
```

The MPAndroidChart wiki applies as is, with C# casing:
https://github.com/PhilJay/MPAndroidChart/wiki

## Upgrading from 3.1.0

The API differences are upstream's. The ones you are most likely to hit:

| 3.1.0                                                          | 3.1.0.1                                                                                  |
|----------------------------------------------------------------|------------------------------------------------------------------------------------------|
| `ValueFormatter` abstract class (`GetFormattedValue(float)`, `GetAxisLabel`, `GetBarLabel`, `GetPieLabel`, …) | Removed. Implement `IValueFormatter.GetFormattedValue(float, Entry, int, ViewPortHandler)` or `IAxisValueFormatter.GetFormattedValue(float, AxisBase)` on a `Java.Lang.Object` subclass. |
| `PercentFormatter(PieChart)`                                   | `PercentFormatter()` or `PercentFormatter(DecimalFormat)`                                 |
| `GradientColor`, `BaseDataSet.GradientColor` / `GradientColors` | `Fill` (`MikePhil.Charting.Util`), `BarDataSet.Fills`. `SetGradientColor(start, end)` still works. |
| `DataSet.Values`                                               | `DataSet.Entries` (`Values` remains as a deprecated alias)                                |
| `DataRenderer.DrawValue(Canvas, string, float, float, int)`    | `DrawValue(Canvas, IValueFormatter, float, Entry, int, float, float, int)`                |

New: `AxisBase.AxisMinLabels` / `AxisMaxLabels`, `YAxis.LabelXOffset`, `Chart.SetNoDataTextAlignment`,
`BarLineChartBase.SetClipDataToContent`, `PieDataSet.HighlightColor`, `Chart.HighlightValue(x, dataSetIndex, dataIndex)`.

## License

Apache License 2.0. Copyright 2016-2026 Lucas Teixeira & Philipp Jahoda.
