namespace ChartNETDemo
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    public enum HeatmapScaleType
    {
        Linear,
        Logarithmic
    }

    public class HeatmapCell
    {
        public string X { get; set; } = "";
        public string Y { get; set; } = "";
        public double Value { get; set; }
    }

    /// <summary>
    /// Interaktionslogik für HeatmapChartControl.xaml
    /// </summary>
    public partial class HeatmapChartControl : UserControl
    {
        private const double LEFTMARGIN = 80;
        private const double RIGHTMARGIN = 20;
        private const double TOPMARGIN = 20;
        private const double BOTTOMMARGIN = 60;
        private const double LEGENDWIDTH = 20;
        private const double LEGENDMARGIN = 10;
        private const int COLORLEGENDMAJORTICKCOUNT = 5;
        private const int COLORLEGENDMINORTICKSPERSEGMENT = 4;

        private static readonly Brush TickNormalBrush = Brushes.Black;
        private static readonly Brush TickHoverBrush = Brushes.DarkOrange;
        private readonly List<(Rectangle Rect, double Value)> _heatmapRects = new();
        private readonly List<(Line Line, double Value, bool IsMajor)> _legendTicks = new();

        public HeatmapChartControl()
        {
            InitializeComponent();
            SizeChanged += (_, _) => Redraw();
        }

        #region DependencyProperties

        public IEnumerable<HeatmapCell> ItemSource
        {
            get => (IEnumerable<HeatmapCell>)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }

        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
                typeof(IEnumerable<HeatmapCell>),
                typeof(HeatmapChartControl),
                new PropertyMetadata(null, (_, __) => ((HeatmapChartControl)_).Redraw()));

        public Brush MinValueColor
        {
            get => (Brush)GetValue(MinValueColorProperty);
            set => SetValue(MinValueColorProperty, value);
        }

        public static readonly DependencyProperty MinValueColorProperty =
            DependencyProperty.Register(
                nameof(MinValueColor),
                typeof(Brush),
                typeof(HeatmapChartControl),
                new PropertyMetadata(Brushes.LightBlue, (_, __) => ((HeatmapChartControl)_).Redraw()));

        public Brush MaxValueColor
        {
            get => (Brush)GetValue(MaxValueColorProperty);
            set => SetValue(MaxValueColorProperty, value);
        }

        public static readonly DependencyProperty MaxValueColorProperty =
            DependencyProperty.Register(
                nameof(MaxValueColor),
                typeof(Brush),
                typeof(HeatmapChartControl),
                new PropertyMetadata(Brushes.DarkRed, (_, __) => ((HeatmapChartControl)_).Redraw()));

        #endregion

        #region Color Legend

        public bool ShowColorLegend
        {
            get => (bool)GetValue(ShowColorLegendProperty);
            set => SetValue(ShowColorLegendProperty, value);
        }

        public static readonly DependencyProperty ShowColorLegendProperty =
            DependencyProperty.Register(
                nameof(ShowColorLegend),
                typeof(bool),
                typeof(HeatmapChartControl),
                new PropertyMetadata(true, (_, __) => ((HeatmapChartControl)_).Redraw()));

        #endregion

        #region Color Legend Title

        public string ColorLegendTitle
        {
            get => (string)GetValue(ColorLegendTitleProperty);
            set => SetValue(ColorLegendTitleProperty, value);
        }

        public static readonly DependencyProperty ColorLegendTitleProperty =
            DependencyProperty.Register(
                nameof(ColorLegendTitle),
                typeof(string),
                typeof(HeatmapChartControl),
                new PropertyMetadata(string.Empty, (_, __) => ((HeatmapChartControl)_).Redraw()));

        public Brush ColorLegendTitleForeground
        {
            get => (Brush)GetValue(ColorLegendTitleForegroundProperty);
            set => SetValue(ColorLegendTitleForegroundProperty, value);
        }

        public static readonly DependencyProperty ColorLegendTitleForegroundProperty =
            DependencyProperty.Register(
                nameof(ColorLegendTitleForeground),
                typeof(Brush),
                typeof(HeatmapChartControl),
                new PropertyMetadata(Brushes.Black, (_, __) => ((HeatmapChartControl)_).Redraw()));

        public double ColorLegendTitleFontSize
        {
            get => (double)GetValue(ColorLegendTitleFontSizeProperty);
            set => SetValue(ColorLegendTitleFontSizeProperty, value);
        }

        public static readonly DependencyProperty ColorLegendTitleFontSizeProperty =
            DependencyProperty.Register(
                nameof(ColorLegendTitleFontSize),
                typeof(double),
                typeof(HeatmapChartControl),
                new PropertyMetadata(12.0, (_, __) => ((HeatmapChartControl)_).Redraw()));

        public TextAlignment ColorLegendTitleAlignment
        {
            get => (TextAlignment)GetValue(ColorLegendTitleAlignmentProperty);
            set => SetValue(ColorLegendTitleAlignmentProperty, value);
        }

        public static readonly DependencyProperty ColorLegendTitleAlignmentProperty =
            DependencyProperty.Register(
                nameof(ColorLegendTitleAlignment),
                typeof(TextAlignment),
                typeof(HeatmapChartControl),
                new PropertyMetadata(TextAlignment.Center, (_, __) => ((HeatmapChartControl)_).Redraw()));

        #endregion Color Legend Title

        #region Heatmap Scale

        public HeatmapScaleType HeatmapScale
        {
            get => (HeatmapScaleType)GetValue(HeatmapScaleProperty);
            set => SetValue(HeatmapScaleProperty, value);
        }

        public static readonly DependencyProperty HeatmapScaleProperty =
            DependencyProperty.Register(
                nameof(HeatmapScale),
                typeof(HeatmapScaleType),
                typeof(HeatmapChartControl),
                new PropertyMetadata(HeatmapScaleType.Linear, (_, __) => ((HeatmapChartControl)_).Redraw()));

        #endregion

        #region Minor Ticks

        public bool MinorTicks
        {
            get => (bool)GetValue(MinorTicksProperty);
            set => SetValue(MinorTicksProperty, value);
        }

        public static readonly DependencyProperty MinorTicksProperty =
            DependencyProperty.Register(
                nameof(MinorTicks),
                typeof(bool),
                typeof(HeatmapChartControl),
                new PropertyMetadata(false, (_, __) => ((HeatmapChartControl)_).Redraw()));

        #endregion Minor Ticks

        #region Rendering

        private void Redraw()
        {
            PART_Canvas.Children.Clear();
            this._heatmapRects.Clear();
            this._legendTicks.Clear();

            if (ItemSource == null || !ItemSource.Any())
            {
                return;
            }

            this.DrawHeatmap();
            this.DrawAxes();
            this.DrawAxisLabels();

            if (this.ShowColorLegend == true)
            {
                this.DrawColorLegend();
            }
        }

        private void DrawHeatmap()
        {
            var list = this.ItemSource.ToList();

            var xLabels = list.Select(c => c.X).Distinct().ToList();
            var yLabels = list.Select(c => c.Y).Distinct().ToList();

            double plotWidth = this.ActualWidth - LEFTMARGIN - RIGHTMARGIN;
            double plotHeight = this.ActualHeight - TOPMARGIN - BOTTOMMARGIN;

            if (plotWidth <= 0 || plotHeight <= 0)
            {
                return;
            }

            double cellWidth = plotWidth / xLabels.Count;
            double cellHeight = plotHeight / yLabels.Count;

            double min = list.Min(c => c.Value);
            double max = list.Max(c => c.Value);
            if (Math.Abs(max - min) < 0.0001)
            {
                max = min + 1;
            }

            foreach (var cell in list)
            {
                int xIndex = xLabels.IndexOf(cell.X);
                int yIndex = yLabels.IndexOf(cell.Y);

                double t = NormalizeValue(cell.Value, min, max);
                Brush fill = InterpolateBrush(MinValueColor, MaxValueColor, t);

                var rect = new Rectangle
                {
                    Width = cellWidth,
                    Height = cellHeight,
                    Fill = fill,
                    Stroke = Brushes.White,
                    StrokeThickness = 0.5,
                    ToolTip = $"{cell.X} / {cell.Y}\nWert: {cell.Value:0.##}"
                };

                rect.MouseEnter += (_, __) =>
                {
                    this.HighlightLegend(cell.Value);
                };

                rect.MouseLeave += (_, __) =>
                {
                    this.ResetLegendHighlight();
                };

                this._heatmapRects.Add((rect, cell.Value));
                Canvas.SetLeft(rect, LEFTMARGIN + xIndex * cellWidth);
                Canvas.SetTop(rect, TOPMARGIN + (yLabels.Count - 1 - yIndex) * cellHeight);
                this.PART_Canvas.Children.Add(rect);
            }
        }

        private void DrawAxes()
        {
            double plotHeight = ActualHeight - BOTTOMMARGIN;

            // Y-Achse
            this.PART_Canvas.Children.Add(new Line
            {
                X1 = LEFTMARGIN,
                X2 = LEFTMARGIN,
                Y1 = TOPMARGIN,
                Y2 = plotHeight,
                Stroke = Brushes.Black
            });

            // X-Achse
            this.PART_Canvas.Children.Add(new Line
            {
                X1 = LEFTMARGIN,
                X2 = ActualWidth - RIGHTMARGIN,
                Y1 = plotHeight,
                Y2 = plotHeight,
                Stroke = Brushes.Black
            });
        }

        private void DrawAxisLabels()
        {
            var list = this.ItemSource.ToList();
            var xLabels = list.Select(c => c.X).Distinct().ToList();
            var yLabels = list.Select(c => c.Y).Distinct().ToList();

            double plotWidth = ActualWidth - LEFTMARGIN - RIGHTMARGIN;
            double plotHeight = ActualHeight - TOPMARGIN - BOTTOMMARGIN;

            double cellWidth = plotWidth / xLabels.Count;
            double cellHeight = plotHeight / yLabels.Count;

            // X Labels
            for (int i = 0; i < xLabels.Count; i++)
            {
                var tb = new TextBlock { Text = xLabels[i], FontSize = 12 };
                tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Canvas.SetLeft(tb, LEFTMARGIN + i * cellWidth + cellWidth / 2 - tb.DesiredSize.Width / 2);
                Canvas.SetTop(tb, plotHeight + 5);
                this.PART_Canvas.Children.Add(tb);
            }

            // Y Labels
            for (int i = 0; i < yLabels.Count; i++)
            {
                var tb = new TextBlock { Text = yLabels[i], FontSize = 12 };
                tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Canvas.SetLeft(tb, LEFTMARGIN - tb.DesiredSize.Width - 10);
                Canvas.SetTop(tb, TOPMARGIN + (yLabels.Count - 1 - i) * cellHeight + cellHeight / 2 - tb.DesiredSize.Height / 2);

                this.PART_Canvas.Children.Add(tb);
            }
        }

        private void DrawColorLegend()
        {
            if (this.ItemSource == null)
            {
                return;
            }

            var list = this.ItemSource.ToList();
            if (list.Count == 0)
            {
                return;
            }

            double min = list.Min(c => c.Value);
            double max = list.Max(c => c.Value);
            if (Math.Abs(max - min) < 0.0001)
            {
                max = min + 1;
            }

            double plotHeight = ActualHeight - TOPMARGIN - BOTTOMMARGIN;
            if (plotHeight <= 0)
            {
                return;
            }

            double x = ActualWidth - RIGHTMARGIN + LEGENDMARGIN;
            double y = TOPMARGIN;

            // ---------- Titel ----------
            if (!string.IsNullOrWhiteSpace(this.ColorLegendTitle))
            {
                var title = new TextBlock
                {
                    Text = this.ColorLegendTitle,
                    FontSize = this.ColorLegendTitleFontSize,
                    Foreground = this.ColorLegendTitleForeground,
                    TextAlignment = this.ColorLegendTitleAlignment,
                    Width = LEGENDWIDTH + 60
                };

                title.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(title, x - (title.Width - LEGENDWIDTH) / 2);
                Canvas.SetTop(title, y - title.DesiredSize.Height - 6);
                this.PART_Canvas.Children.Add(title);
            }

            // ---------- Farbskala ----------
            int steps = 120;
            double stepHeight = plotHeight / steps;

            for (int i = 0; i < steps; i++)
            {
                double rawValue = min + (max - min) * (1.0 - i / (double)(steps - 1));
                double t = this.NormalizeValue(rawValue, min, max);
                Brush brush = InterpolateBrush(MinValueColor, MaxValueColor, t);

                var rect = new Rectangle
                {
                    Width = LEGENDWIDTH,
                    Height = stepHeight + 1,
                    Fill = brush
                };

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y + i * stepHeight);
                this.PART_Canvas.Children.Add(rect);
            }

            // ---------- Ticks ----------
            foreach (var tick in GetColorLegendTicks(min, max))
            {
                double t = this.NormalizeValue(tick.Value, min, max);
                double tickY = y + plotHeight - t * plotHeight;

                var tickLine = new Line
                {
                    X1 = x + LEGENDWIDTH,
                    X2 = x + LEGENDWIDTH + (tick.IsMajor ? 8 : 4),
                    Y1 = tickY,
                    Y2 = tickY,
                    Stroke = TickNormalBrush,
                    StrokeThickness = tick.IsMajor ? 1.4 : 0.8,
                    Cursor = Cursors.Hand
                };

                this.AttachTickHover(tickLine, tick.Value, tick.IsMajor ? this.FormatLegendValue(tick.Value) : null);
                this._legendTicks.Add((tickLine, tick.Value, tick.IsMajor));
                this.PART_Canvas.Children.Add(tickLine);

                if (tick.IsMajor == true)
                {
                    var label = new TextBlock
                    {
                        Text = FormatLegendValue(tick.Value),
                        FontSize = 11,
                        FontWeight = FontWeights.Bold
                    };

                    label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    Canvas.SetLeft(label, x + LEGENDWIDTH + 12);
                    Canvas.SetTop(label, tickY - label.DesiredSize.Height / 2);
                    this.PART_Canvas.Children.Add(label);
                }
                else
                {
                    if (this.MinorTicks == true)
                    {
                        var label = new TextBlock
                        {
                            Text = FormatLegendValue(tick.Value),
                            FontSize = 11,
                            FontWeight = FontWeights.Normal
                        };

                        label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                        Canvas.SetLeft(label, x + LEGENDWIDTH + 12);
                        Canvas.SetTop(label, tickY - label.DesiredSize.Height / 2);
                        this.PART_Canvas.Children.Add(label);
                    }
                }
            }
        }
        #endregion

        #region Helpers
        private void HighlightHeatmap(double centerValue)
        {
            if (_heatmapRects.Count == 0)
            {
                return;
            }

            // Toleranzbereich (10 % des Wertebereichs)
            double min = _heatmapRects.Min(r => r.Value);
            double max = _heatmapRects.Max(r => r.Value);
            double tolerance = (max - min) * 0.05;

            foreach (var (rect, value) in _heatmapRects)
            {
                if (Math.Abs(value - centerValue) <= tolerance)
                {
                    rect.Opacity = 1.0;
                    rect.Stroke = Brushes.Black;
                    rect.StrokeThickness = 1.5;
                }
                else
                {
                    rect.Opacity = 0.25;
                    rect.StrokeThickness = 0.5;
                }
            }
        }

        private void ResetHeatmapHighlight()
        {
            foreach (var (rect, _) in this._heatmapRects)
            {
                rect.Opacity = 1.0;
                rect.StrokeThickness = 0.5;
                rect.Stroke = Brushes.White;
            }
        }

        string FormatLegendValue(double value)
        {
            return HeatmapScale == HeatmapScaleType.Logarithmic
                ? value.ToString("0.###E+0", CultureInfo.CurrentCulture) : value.ToString("0.##", CultureInfo.CurrentCulture);
        }

        private double NormalizeValue(double value, double min, double max)
        {
            if (HeatmapScale == HeatmapScaleType.Linear)
            {
                return (value - min) / (max - min);
            }

            // ---------- Logarithmisch ----------
            // Schutz: Log benötigt positive Werte
            double safeMin = Math.Max(min, 1e-6);
            double safeValue = Math.Max(value, safeMin);

            double logMin = Math.Log10(safeMin);
            double logMax = Math.Log10(max);
            double logVal = Math.Log10(safeValue);

            return (logVal - logMin) / (logMax - logMin);
        }

        private static Brush InterpolateBrush(Brush min, Brush max, double t)
        {
            if (min is not SolidColorBrush a || max is not SolidColorBrush b)
            {
                return min;
            }

            byte r = (byte)(a.Color.R + (b.Color.R - a.Color.R) * t);
            byte g = (byte)(a.Color.G + (b.Color.G - a.Color.G) * t);
            byte bl = (byte)(a.Color.B + (b.Color.B - a.Color.B) * t);

            return new SolidColorBrush(Color.FromRgb(r, g, bl));
        }

        private IEnumerable<ColorLegendTick> GetColorLegendTicks(double min, double max)
        {
            if (HeatmapScale == HeatmapScaleType.Logarithmic)
            {
                double logMin = Math.Log10(Math.Max(min, 1e-6));
                double logMax = Math.Log10(max);

                for (int i = 0; i < COLORLEGENDMAJORTICKCOUNT; i++)
                {
                    double majorLog = logMin + i * (logMax - logMin) / (COLORLEGENDMAJORTICKCOUNT - 1);
                    double majorValue = Math.Pow(10, majorLog);

                    yield return new ColorLegendTick
                    {
                        Value = majorValue,
                        IsMajor = true
                    };

                    if (i < COLORLEGENDMAJORTICKCOUNT - 1)
                    {
                        double nextMajorLog = logMin + (i + 1) * (logMax - logMin) / (COLORLEGENDMAJORTICKCOUNT - 1);

                        for (int m = 1; m <= COLORLEGENDMINORTICKSPERSEGMENT; m++)
                        {
                            double minorLog = majorLog + m * (nextMajorLog - majorLog) / (COLORLEGENDMINORTICKSPERSEGMENT + 1);

                            yield return new ColorLegendTick
                            {
                                Value = Math.Pow(10, minorLog),
                                IsMajor = false
                            };
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < COLORLEGENDMAJORTICKCOUNT; i++)
                {
                    double majorValue = min + i * (max - min) / (COLORLEGENDMAJORTICKCOUNT - 1);

                    yield return new ColorLegendTick
                    {
                        Value = majorValue,
                        IsMajor = true
                    };

                    if (i < COLORLEGENDMAJORTICKCOUNT - 1)
                    {
                        double nextMajor = min + (i + 1) * (max - min) / (COLORLEGENDMAJORTICKCOUNT - 1);

                        for (int m = 1; m <= COLORLEGENDMINORTICKSPERSEGMENT; m++)
                        {
                            double minorValue = majorValue + m * (nextMajor - majorValue) / (COLORLEGENDMINORTICKSPERSEGMENT + 1);

                            yield return new ColorLegendTick
                            {
                                Value = minorValue,
                                IsMajor = false
                            };
                        }
                    }
                }
            }
        }

        private void AttachTickHover(Line tickLine, double value, string tooltipText)
        {
            tickLine.MouseEnter += (_, __) =>
            {
                tickLine.Stroke = TickHoverBrush;
                tickLine.StrokeThickness *= 1.6;

                this.HighlightHeatmap(value);

                if (string.IsNullOrWhiteSpace(tooltipText) == false)
                {
                    ToolTipService.SetToolTip(tickLine, tooltipText);
                }
            };

            tickLine.MouseLeave += (_, __) =>
            {
                tickLine.Stroke = TickNormalBrush;
                tickLine.StrokeThickness /= 1.6;

                this.ResetHeatmapHighlight();
                ToolTipService.SetToolTip(tickLine, null);
            };
        }

        private void HighlightLegend(double value)
        {
            if (this._legendTicks.Count == 0)
            {
                return;
            }

            double min = this._legendTicks.Min(t => t.Value);
            double max = this._legendTicks.Max(t => t.Value);
            double tolerance = (max - min) * 0.05;

            foreach (var (line, tickValue, isMajor) in this._legendTicks)
            {
                if (Math.Abs(tickValue - value) <= tolerance)
                {
                    line.Stroke = TickHoverBrush;
                    line.StrokeThickness = isMajor ? 2.2 : 1.4;
                }
                else
                {
                    line.Stroke = TickNormalBrush;
                    line.StrokeThickness = isMajor ? 1.4 : 0.8;
                }
            }
        }

        private void ResetLegendHighlight()
        {
            foreach (var (line, _, isMajor) in _legendTicks)
            {
                line.Stroke = TickNormalBrush;
                line.StrokeThickness = isMajor ? 1.4 : 0.8;
            }
        }

        #endregion

        private sealed class ColorLegendTick
        {
            public double Value { get; set; }
            public bool IsMajor { get; set; }
        }

        #region Export als PNG Image
        public void ExportToPng(string filePath, double dpi = 96)
        {
            const int LEFT = 50;
            const int HEIGHTMARGING = 40;

            if (this.ActualWidth <= 0 || this.ActualHeight <= 0)
            {
                return;
            }

            // Layout sicherstellen
            Size size = new Size((this.ActualWidth+ LEFT), (this.ActualHeight+ HEIGHTMARGING));
            this.Measure(size);
            this.Arrange(new Rect(size));
            this.UpdateLayout();

            var rtb = new RenderTargetBitmap(
                (int)((this.ActualWidth+ LEFT) * dpi / 96.0),
                (int)((this.ActualHeight + HEIGHTMARGING) * dpi / 96.0),
                dpi,
                dpi,
                PixelFormats.Pbgra32);

            rtb.Render(this);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
            encoder.Save(fs);
        }
         #endregion Export als PNG Image
    }
}