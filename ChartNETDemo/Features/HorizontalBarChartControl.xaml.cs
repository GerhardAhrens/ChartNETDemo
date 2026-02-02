namespace ChartNETDemo
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    public class HorizontalBarChartPoint
    {
        public string Y { get; set; } = "";
        public double X { get; set; }
    }

    public class HorizontalBarSeries
    {
        public string Title { get; set; } = "";
        public Brush Fill { get; set; } = Brushes.SteelBlue;
        public IList<HorizontalBarChartPoint> Values { get; set; } = new List<HorizontalBarChartPoint>();
    }

    public partial class HorizontalBarChartControl : UserControl
    {
        private const double LEFTMARGIN = 60;
        private const double RIGHTMARGIN = 30;
        private const double TOPMARGIN = 20;
        private const double BOTTOMMARGIN = 60;

        public HorizontalBarChartControl()
        {
            this.InitializeComponent();
            this.SizeChanged += (_, _) => this.Redraw();
            this.Loaded += (_, _) => this.UpdateLegendLayout();
        }

        #region DependencyProperties

        public IEnumerable<HorizontalBarSeries> ItemSource
        {
            get => (IEnumerable<HorizontalBarSeries>)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }

        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
                typeof(IEnumerable<HorizontalBarSeries>),
                typeof(HorizontalBarChartControl),
                new PropertyMetadata(null, (_, __) => ((HorizontalBarChartControl)_).Redraw()));

        public bool ShowLegend
        {
            get => (bool)GetValue(ShowLegendProperty);
            set => SetValue(ShowLegendProperty, value);
        }

        public static readonly DependencyProperty ShowLegendProperty =
            DependencyProperty.Register(
                nameof(ShowLegend),
                typeof(bool),
                typeof(HorizontalBarChartControl),
                new PropertyMetadata(true, (_, __) => ((HorizontalBarChartControl)_).Redraw()));

        public LegendPosition LegendPosition
        {
            get => (LegendPosition)GetValue(LegendPositionProperty);
            set => SetValue(LegendPositionProperty, value);
        }

        public static readonly DependencyProperty LegendPositionProperty =
            DependencyProperty.Register(
                nameof(LegendPosition),
                typeof(LegendPosition),
                typeof(HorizontalBarChartControl),
                new PropertyMetadata(LegendPosition.Right, (_, __) =>
                {
                    var c = (HorizontalBarChartControl)_;
                    c.UpdateLegendLayout();
                }));

        public bool ShowGridLines
        {
            get => (bool)GetValue(ShowGridLinesProperty);
            set => SetValue(ShowGridLinesProperty, value);
        }

        public static readonly DependencyProperty ShowGridLinesProperty =
            DependencyProperty.Register(
                nameof(ShowGridLines),
                typeof(bool),
                typeof(HorizontalBarChartControl),
                new PropertyMetadata(true, (_, __) => ((HorizontalBarChartControl)_).Redraw()));

        #endregion

        #region Axis Titles

        public string XAxisTitle
        {
            get => (string)GetValue(XAxisTitleProperty);
            set => SetValue(XAxisTitleProperty, value);
        }

        public static readonly DependencyProperty XAxisTitleProperty =
            DependencyProperty.Register(
                nameof(XAxisTitle),
                typeof(string),
                typeof(HorizontalBarChartControl),
                new PropertyMetadata(string.Empty, (_, __) => ((HorizontalBarChartControl)_).Redraw()));

        public Brush XAxisTitleForeground
        {
            get => (Brush)GetValue(XAxisTitleForegroundProperty);
            set => SetValue(XAxisTitleForegroundProperty, value);
        }

        public static readonly DependencyProperty XAxisTitleForegroundProperty =
            DependencyProperty.Register(
                nameof(XAxisTitleForeground),
                typeof(Brush),
                typeof(HorizontalBarChartControl),
                new PropertyMetadata(Brushes.Black, (_, __) => ((HorizontalBarChartControl)_).Redraw()));

        public double XAxisTitleFontSize
        {
            get => (double)GetValue(XAxisTitleFontSizeProperty);
            set => SetValue(XAxisTitleFontSizeProperty, value);
        }

        public static readonly DependencyProperty XAxisTitleFontSizeProperty =
            DependencyProperty.Register(
                nameof(XAxisTitleFontSize),
                typeof(double),
                typeof(HorizontalBarChartControl),
                new PropertyMetadata(13.0, (_, __) => ((HorizontalBarChartControl)_).Redraw()));

        public TextAlignment XAxisTitleAlignment
        {
            get => (TextAlignment)GetValue(XAxisTitleAlignmentProperty);
            set => SetValue(XAxisTitleAlignmentProperty, value);
        }

        public static readonly DependencyProperty XAxisTitleAlignmentProperty =
            DependencyProperty.Register(
                nameof(XAxisTitleAlignment),
                typeof(TextAlignment),
                typeof(HorizontalBarChartControl),
                new PropertyMetadata(TextAlignment.Center, (_, __) => ((HorizontalBarChartControl)_).Redraw()));

        // ------------------------------------------------------------

        public string YAxisTitle
        {
            get => (string)GetValue(YAxisTitleProperty);
            set => SetValue(YAxisTitleProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleProperty =
            DependencyProperty.Register(
                nameof(YAxisTitle),
                typeof(string),
                typeof(HorizontalBarChartControl),
                new PropertyMetadata(string.Empty, (_, __) => ((HorizontalBarChartControl)_).Redraw()));

        public Brush YAxisTitleForeground
        {
            get => (Brush)GetValue(YAxisTitleForegroundProperty);
            set => SetValue(YAxisTitleForegroundProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleForegroundProperty =
            DependencyProperty.Register(
                nameof(YAxisTitleForeground),
                typeof(Brush),
                typeof(HorizontalBarChartControl),
                new PropertyMetadata(Brushes.Black, (_, __) => ((HorizontalBarChartControl)_).Redraw()));

        public double YAxisTitleFontSize
        {
            get => (double)GetValue(YAxisTitleFontSizeProperty);
            set => SetValue(YAxisTitleFontSizeProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleFontSizeProperty =
            DependencyProperty.Register(
                nameof(YAxisTitleFontSize),
                typeof(double),
                typeof(HorizontalBarChartControl),
                new PropertyMetadata(13.0, (_, __) => ((HorizontalBarChartControl)_).Redraw()));

        public TextAlignment YAxisTitleAlignment
        {
            get => (TextAlignment)GetValue(YAxisTitleAlignmentProperty);
            set => SetValue(YAxisTitleAlignmentProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleAlignmentProperty =
            DependencyProperty.Register(
                nameof(YAxisTitleAlignment),
                typeof(TextAlignment),
                typeof(HorizontalBarChartControl),
                new PropertyMetadata(TextAlignment.Center, (_, __) => ((HorizontalBarChartControl)_).Redraw()));

        #endregion Axis Titles

        #region Axis Scale Format

        public AxisScaleFormat XAxisScaleFormat
        {
            get => (AxisScaleFormat)GetValue(XAxisScaleFormatProperty);
            set => SetValue(XAxisScaleFormatProperty, value);
        }

        public static readonly DependencyProperty XAxisScaleFormatProperty =
            DependencyProperty.Register(
                nameof(XAxisScaleFormat),
                typeof(AxisScaleFormat),
                typeof(HorizontalBarChartControl),
                new PropertyMetadata(AxisScaleFormat.Number, (_, __) => ((HorizontalBarChartControl)_).Redraw()));

        #endregion Axis Scale Format

        #region Rendering

        private void Redraw()
        {
            this.PART_Canvas.Children.Clear();
            this.PART_Legend.Children.Clear();

            if (this.ItemSource == null || this.ItemSource.Any() == false || this.ActualWidth <= 0 || this.ActualHeight <= 0)
            {
                return;
            }

            this.DrawAxes();
            this.DrawBars();
            this.DrawYAxisLabels();
            this.DrawXAxisLabels();

            if (this.ShowLegend == true)
            {
                this.DrawLegend();
            }
        }

        private void DrawAxes()
        {
            double width = this.ActualWidth - RIGHTMARGIN;
            double height = this.ActualHeight - BOTTOMMARGIN;

            // X-Achse
            this.PART_Canvas.Children.Add(new Line
            {
                X1 = LEFTMARGIN,
                X2 = width,
                Y1 = height,
                Y2 = height,
                Stroke = Brushes.Black
            });

            // Y-Achse
            this.PART_Canvas.Children.Add(new Line
            {
                X1 = LEFTMARGIN,
                X2 = LEFTMARGIN,
                Y1 = TOPMARGIN,
                Y2 = height,
                Stroke = Brushes.Black
            });

            /* X-Achsentitel */
            if (string.IsNullOrEmpty(XAxisTitle) == false)
            {
                var xTitle = new TextBlock
                {
                    Text = XAxisTitle,
                    FontSize = XAxisTitleFontSize,
                    Foreground = XAxisTitleForeground,
                    TextAlignment = XAxisTitleAlignment
                };

                xTitle.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                double plotWidth = width - LEFTMARGIN;
                double x = LEFTMARGIN + plotWidth / 2 - xTitle.DesiredSize.Width / 2;
                double y = height + 30;

                Canvas.SetLeft(xTitle, x);
                Canvas.SetTop(xTitle, y);
                this.PART_Canvas.Children.Add(xTitle);
            }

            /* Y-Achsentitel */
            if (string.IsNullOrEmpty(YAxisTitle) == false)
            {
                var yTitle = new TextBlock
                {
                    Text = YAxisTitle,
                    FontSize = YAxisTitleFontSize,
                    Foreground = YAxisTitleForeground,
                    TextAlignment = YAxisTitleAlignment
                };

                yTitle.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                // vertikal
                yTitle.RenderTransform = new RotateTransform(-90);

                double plotHeight = height - TOPMARGIN;
                double y = TOPMARGIN + plotHeight / 2 + yTitle.DesiredSize.Width / 2;

                Canvas.SetLeft(yTitle, -15);
                Canvas.SetTop(yTitle, y);
                this.PART_Canvas.Children.Add(yTitle);
            }
        }

        private void DrawBars()
        {
            var seriesList = this.ItemSource.ToList();
            var categories = seriesList.First().Values.Select(v => v.Y).ToList();

            double plotWidth = ActualWidth - LEFTMARGIN - RIGHTMARGIN;
            double plotHeight = ActualHeight - TOPMARGIN - BOTTOMMARGIN;

            double maxStackValue = categories.Max(cat => seriesList.Sum(s => s.Values.FirstOrDefault(v => v.Y == cat)?.X ?? 0));

            int count = categories.Count;
            double rowHeight = plotHeight / count;
            double barHeight = rowHeight * 0.6;

            for (int i = 0; i < count; i++)
            {
                double stackBase = 0;

                foreach (var series in seriesList)
                {
                    var value = series.Values.FirstOrDefault(v => v.Y == categories[i]);
                    if (value == null || value.X <= 0)
                    {
                        continue;
                    }

                    double width = value.X / maxStackValue * plotWidth;
                    double x = LEFTMARGIN + stackBase / maxStackValue * plotWidth;
                    double y = TOPMARGIN + i * rowHeight + (rowHeight - barHeight) / 2;

                    double stackedTotal = seriesList.Sum(s => s.Values.FirstOrDefault(v => v.Y == categories[i])?.X ?? 0);

                    var rect = new Rectangle
                    {
                        Width = width,
                        Height = barHeight,
                        Fill = series.Fill,
                        ToolTip = new ToolTip
                        {
                            Content = BuildBarSegmentTooltip(series.Title, categories[i], value.X, stackedTotal)
                        }
                    };

                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                    this.PART_Canvas.Children.Add(rect);

                    stackBase += value.X;
                }
            }
        }

        private static string BuildBarSegmentTooltip(string seriesTitle, string category, double value, double stackedTotal)
        {
            double percent = ((stackedTotal > 0) ? value / stackedTotal : 0) * 100;

            return $"Serie: {seriesTitle}\nKategorie: {category}\nWert: {value:0.##}\nAnteil: {percent:0.#} %";
        }

        private void DrawYAxisLabels()
        {
            var categories = this.ItemSource.First().Values.Select(v => v.Y).ToList();
            double plotHeight = ActualHeight - TOPMARGIN - BOTTOMMARGIN;
            double rowHeight = plotHeight / categories.Count;

            for (int i = 0; i < categories.Count; i++)
            {
                var label = new TextBlock { Text = categories[i] };
                Canvas.SetLeft(label, 10);
                Canvas.SetTop(label, TOPMARGIN + i * rowHeight + rowHeight / 2 - 8);
                this.PART_Canvas.Children.Add(label);
            }
        }

        private void DrawXAxisLabels()
        {
            var seriesList = ItemSource.ToList();
            var categories = seriesList.First().Values.Select(v => v.Y).ToList();

            double plotWidth = ActualWidth - LEFTMARGIN - RIGHTMARGIN;
            double plotHeight = ActualHeight - TOPMARGIN - BOTTOMMARGIN;

            double maxStackValue = categories.Max(cat =>
                seriesList.Sum(s => s.Values.FirstOrDefault(v => v.Y == cat)?.X ?? 0));

            int ticks = 5;

            for (int i = 0; i <= ticks; i++)
            {
                double value = maxStackValue * i / ticks;
                double x = LEFTMARGIN + value / maxStackValue * plotWidth;

                if (ShowGridLines && i > 0)
                {
                    this.PART_Canvas.Children.Add(new Line
                    {
                        X1 = x,
                        X2 = x,
                        Y1 = TOPMARGIN,
                        Y2 = plotHeight + TOPMARGIN,
                        Stroke = Brushes.LightGray
                    });
                }

                var label = new TextBlock
                {
                    Text = FormatAxisValue(value),
                    FontSize = 11
                };

                Canvas.SetLeft(label, x - 10);
                Canvas.SetTop(label, plotHeight + TOPMARGIN + 5);
                this.PART_Canvas.Children.Add(label);
            }
        }

        private void DrawLegend()
        {
            foreach (var series in ItemSource)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(4) };

                row.Children.Add(new Rectangle
                {
                    Width = 14,
                    Height = 14,
                    Fill = series.Fill,
                    Stroke = Brushes.Black,
                    Margin = new Thickness(0, 0, 6, 0)
                });

                row.Children.Add(new TextBlock { Text = series.Title });
                this.PART_Legend.Children.Add(row);
            }
        }

        private void UpdateLegendLayout()
        {
            this.PART_RootGrid.RowDefinitions.Clear();
            this.PART_RootGrid.ColumnDefinitions.Clear();

            switch (this.LegendPosition)
            {
                case LegendPosition.Left:
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    Grid.SetColumn(this.PART_Legend, 0);
                    Grid.SetColumn(this.PART_Canvas, 1);
                    break;

                case LegendPosition.Right:
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    Grid.SetColumn(this.PART_Canvas, 0);
                    Grid.SetColumn(this.PART_Legend, 1);
                    break;

                case LegendPosition.Top:
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition());
                    Grid.SetRow(this.PART_Legend, 0);
                    Grid.SetRow(this.PART_Canvas, 1);
                    break;

                case LegendPosition.Bottom:
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition());
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    Grid.SetRow(this.PART_Canvas, 0);
                    Grid.SetRow(this.PART_Legend, 1);
                    break;
            }
        }

        private string FormatAxisValue(double value)
        {
            return XAxisScaleFormat switch
            {
                AxisScaleFormat.Number =>  value.ToString("0", CultureInfo.CurrentCulture),

                AxisScaleFormat.NumberK =>
                    value >= 1000
                        ? (value / 1000d).ToString("0.#", CultureInfo.CurrentCulture) + "k"
                        : value.ToString("0", CultureInfo.CurrentCulture),

                AxisScaleFormat.NumberM =>
                    value >= 1_000_000
                        ? (value / 1_000_000d).ToString("0.##", CultureInfo.CurrentCulture) + "M"
                        : value >= 1000
                            ? (value / 1000d).ToString("0.#", CultureInfo.CurrentCulture) + "k"
                            : value.ToString("0", CultureInfo.CurrentCulture),

                AxisScaleFormat.Percent => (value * 100).ToString("0.#", CultureInfo.CurrentCulture) + " %",

                _ => value.ToString("0", CultureInfo.CurrentCulture)
            };
        }

        #endregion

        #region Export als PNG Image
        public void ExportToPng(string filePath, double dpi = 96)
        {
            if (this.ActualWidth <= 0 || this.ActualHeight <= 0)
            {
                return;
            }

            // Layout sicherstellen
            Measure(new Size(this.ActualWidth, this.ActualHeight));
            Arrange(new Rect(new Size(this.ActualWidth, this.ActualHeight)));
            UpdateLayout();

            var rtb = new RenderTargetBitmap(
                (int)(this.ActualWidth * dpi / 96.0),
                (int)(this.ActualHeight * dpi / 96.0),
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
