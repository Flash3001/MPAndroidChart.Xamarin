// Exercises the binding end to end: every chart type, the Fill API, the restored IValueFormatter /
// IAxisValueFormatter / IFillFormatter interfaces implemented from C#, the selection listener, the
// dataIndex HighlightValue overloads and the Chart.Data metadata fix. `dotnet build` is enough to catch
// binding regressions; run it on an emulator to see the charts.
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using MikePhil.Charting.Charts;
using MikePhil.Charting.Components;
using MikePhil.Charting.Data;
using MikePhil.Charting.Data.Filter;
using MikePhil.Charting.Formatter;
using MikePhil.Charting.Highlight;
using MikePhil.Charting.Interfaces.Dataprovider;
using MikePhil.Charting.Interfaces.Datasets;
using MikePhil.Charting.Listener;
using MikePhil.Charting.Util;

namespace SmokeTest;

[Activity(Label = "SmokeTest", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var scroll = new ScrollView(this);
        var stack = new LinearLayout(this) { Orientation = Orientation.Vertical };
        scroll.AddView(stack);
        SetContentView(scroll);

        var entries = Enumerable.Range(0, 12).Select(i => new Entry(i, (float)(Math.Sin(i) * 10 + 20))).ToList();
        var labels = Enumerable.Range(0, 12).Select(i => $"x{i}").ToArray();

        // Line: cubic, filled, custom formatters, listener, new axis knobs
        var line = new LineChart(this);
        var lineSet = new LineDataSet(entries, "sin") { LineWidth = 2f, FillFormatter = new HalfFillFormatter() };
        lineSet.SetMode(LineDataSet.Mode.CubicBezier);
        lineSet.SetColors(ColorTemplate.MaterialColors.ToArray());
        lineSet.SetDrawFilled(true);
        lineSet.ValueFormatter = new TwoDecimalsFormatter();
        line.Data = new LineData(lineSet);
        line.XAxis.ValueFormatter = new IndexAxisValueFormatter(labels);
        line.XAxis.AxisMinLabels = 3;
        line.XAxis.AxisMaxLabels = 8;
        line.AxisLeft.ValueFormatter = new HashAxisFormatter();
        line.AxisLeft.LabelXOffset = 4f;
        line.SetNoDataTextAlignment(Paint.Align.Center);
        line.SetClipDataToContent(false);
        line.SetOnChartValueSelectedListener(new SelectionListener(this));
        line.HighlightValue(3f, 0, 0);
        Add(stack, line);

        // Bar: Fill (solid + gradient) and the GradientColor-compatible setters
        var bar = new BarChart(this);
        var barSet = new BarDataSet(entries.Select(e => new BarEntry(e.GetX(), e.GetY())).ToList(), "bars")
        {
            Fills = new List<Fill> { new Fill(Color.Red.ToArgb(), Color.Blue.ToArgb()), new Fill(Color.Green.ToArgb()) },
        };
        barSet.SetGradientColor(Color.Cyan.ToArgb(), Color.Magenta.ToArgb());
        bar.Data = new BarData(barSet);
        var barData = (BarData)bar.Data;          // Chart.Data is ChartData (Metadata.xml), not Java.Lang.Object
        barData.BarWidth = 0.8f;
        Add(stack, bar);

        var hbar = new HorizontalBarChart(this);
        hbar.Data = new BarData(new BarDataSet(entries.Select(e => new BarEntry(e.GetX(), new[] { e.GetY(), e.GetY() / 2 })).ToList(), "stacked"));
        Add(stack, hbar);

        // Pie: PercentFormatter without the PieChart argument, nullable highlight color
        var pie = new PieChart(this);
        var pieSet = new PieDataSet(entries.Take(4).Select(e => new PieEntry(e.GetY(), $"slice {e.GetX()}")).ToList(), "pie")
        {
            HighlightColor = Java.Lang.Integer.ValueOf(Color.Yellow.ToArgb()),
            ValueFormatter = new PercentFormatter(),
        };
        pieSet.SetColors(ColorTemplate.JoyfulColors.ToArray());
        pie.Data = new PieData(pieSet);
        Add(stack, pie);

        var scatter = new ScatterChart(this);
        var scatterSet = new ScatterDataSet(entries, "scatter");
        scatterSet.SetScatterShape(ScatterChart.ScatterShape.Triangle);
        scatter.Data = new ScatterData(scatterSet);
        Add(stack, scatter);

        var candle = new CandleStickChart(this);
        candle.Data = new CandleData(new CandleDataSet(entries.Select(e => new CandleEntry(e.GetX(), e.GetY() + 5, e.GetY() - 5, e.GetY() + 2, e.GetY() - 2)).ToList(), "candles"));
        Add(stack, candle);

        var bubble = new BubbleChart(this);
        bubble.Data = new BubbleData(new BubbleDataSet(entries.Select(e => new BubbleEntry(e.GetX(), e.GetY(), e.GetY() / 10)).ToList(), "bubbles"));
        Add(stack, bubble);

        var radar = new RadarChart(this);
        radar.Data = new RadarData(new RadarDataSet(entries.Take(6).Select(e => new RadarEntry(e.GetY())).ToList(), "radar"));
        Add(stack, radar);

        // Combined: the dataIndex HighlightValue overloads
        var combined = new CombinedChart(this);
        var combinedData = new CombinedData();
        combinedData.SetData(new LineData(new LineDataSet(entries, "line")));
        combinedData.SetData(new BarData(new BarDataSet(entries.Select(e => new BarEntry(e.GetX(), e.GetY() / 2)).ToList(), "bar")));
        combined.Data = combinedData;
        combined.HighlightValue(2f, 0, 1);
        combined.HighlightValue(new Highlight(2f, 10f, 0, 1), callListener: false);
        Add(stack, combined);

        // Utils / data.filter additions
        var reduced = new ApproximatorN().ReduceWithDouglasPeucker(new[] { 0f, 0f, 1f, 1f, 2f, 0f, 3f, 1f }, 3);
        Console.WriteLine($"ApproximatorN kept {reduced.Length / 2} points; {Utils.ConvertDpToPixel(1f)} px per dp");
    }

    static void Add(LinearLayout stack, View chart) =>
        stack.AddView(chart, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 600));

    sealed class TwoDecimalsFormatter : Java.Lang.Object, IValueFormatter
    {
        public string GetFormattedValue(float value, Entry entry, int dataSetIndex, ViewPortHandler viewPortHandler) => value.ToString("0.00");
    }

    sealed class HashAxisFormatter : Java.Lang.Object, IAxisValueFormatter
    {
        public string GetFormattedValue(float value, AxisBase axis) => $"#{(int)value}";
    }

    sealed class HalfFillFormatter : Java.Lang.Object, IFillFormatter
    {
        public float GetFillLinePosition(ILineDataSet dataSet, ILineDataProvider dataProvider) => dataProvider.YChartMin / 2;
    }

    sealed class SelectionListener(Context context) : Java.Lang.Object, IOnChartValueSelectedListenerSupport
    {
        public void OnValueSelected(Entry e, Highlight h) => Toast.MakeText(context, $"x={e.GetX()} y={e.GetY()} dataSet={h.DataSetIndex}", ToastLength.Short)?.Show();
        public void OnNothingSelected() { }
    }
}
