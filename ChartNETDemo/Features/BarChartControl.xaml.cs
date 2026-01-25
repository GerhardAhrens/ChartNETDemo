namespace ChartNETDemo
{
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    public enum AxisScaleFormat
    {
        Number,     // 1234
        NumberK,    // 1.2k
        NumberM,    // 1.2M
        Percent     // 25 %
    }

    public enum LegendPosition
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public class BarChartPoint
    {
        public string X { get; set; } = "";
        public double Y { get; set; }
    }

    public class BarSeries
    {
        public string Title { get; set; } = "";
        public Brush Fill { get; set; } = Brushes.SteelBlue;
        public IList<BarChartPoint> Values { get; set; } = new List<BarChartPoint>();
    }

    /// <summary>
    /// Interaktionslogik für BarChartControl.xaml
    /// </summary>
    public partial class BarChartControl : UserControl
    {
        private const double LeftMargin = 60;
        private const double RightMargin = 20;
        private const double TopMargin = 20;
        private const double BottomMargin = 60;

        public BarChartControl()
        {
            InitializeComponent();
            this.SizeChanged += (_, _) =>
            {
                this.Redraw();
            };

            this.Loaded += (_, __) =>
            {
                this.UpdateLegendLayout();
                this.Redraw();
            };
        }

        #region X-Achsentitel + Gridlines

        public string XAxisTitle
        {
            get => (string)GetValue(XAxisTitleProperty);
            set => SetValue(XAxisTitleProperty, value);
        }

        public static readonly DependencyProperty XAxisTitleProperty =
            DependencyProperty.Register(
                nameof(XAxisTitle),
                typeof(string),
                typeof(BarChartControl),
                new PropertyMetadata(string.Empty, (_, __) => ((BarChartControl)_).Redraw()));

        public double XAxisTitleFontSize
        {
            get => (double)GetValue(XAxisTitleFontSizeProperty);
            set => SetValue(XAxisTitleFontSizeProperty, value);
        }

        public static readonly DependencyProperty XAxisTitleFontSizeProperty =
            DependencyProperty.Register(
                nameof(XAxisTitleFontSize),
                typeof(double),
                typeof(BarChartControl),
                new PropertyMetadata(12.0, (_, __) => ((BarChartControl)_).Redraw()));

        public Brush XAxisTitleForeground
        {
            get => (Brush)GetValue(XAxisTitleForegroundProperty);
            set => SetValue(XAxisTitleForegroundProperty, value);
        }

        public static readonly DependencyProperty XAxisTitleForegroundProperty =
            DependencyProperty.Register(
                nameof(XAxisTitleForeground),
                typeof(Brush),
                typeof(BarChartControl),
                new PropertyMetadata(Brushes.Black, (_, __) => ((BarChartControl)_).Redraw()));

        public bool ShowHorizontalGridLines
        {
            get => (bool)GetValue(ShowHorizontalGridLinesProperty);
            set => SetValue(ShowHorizontalGridLinesProperty, value);
        }

        public static readonly DependencyProperty ShowHorizontalGridLinesProperty =
            DependencyProperty.Register(
                nameof(ShowHorizontalGridLines),
                typeof(bool),
                typeof(BarChartControl),
                new PropertyMetadata(true, (_, __) => ((BarChartControl)_).Redraw()));

        #endregion

        #region DependencyProperty

        public bool ShowLegend
        {
            get => (bool)GetValue(ShowLegendProperty);
            set => SetValue(ShowLegendProperty, value);
        }

        public static readonly DependencyProperty ShowLegendProperty =
            DependencyProperty.Register(
                nameof(ShowLegend),
                typeof(bool),
                typeof(BarChartControl),
                new PropertyMetadata(true, (_, __) => ((BarChartControl)_).Redraw()));

        public LegendPosition LegendPosition
        {
            get => (LegendPosition)GetValue(LegendPositionProperty);
            set => SetValue(LegendPositionProperty, value);
        }

        public static readonly DependencyProperty LegendPositionProperty =
            DependencyProperty.Register(
                nameof(LegendPosition),
                typeof(LegendPosition),
                typeof(BarChartControl),
                new PropertyMetadata(LegendPosition.Right, (_, __) =>
                {
                    var c = (BarChartControl)_;
                    c.UpdateLegendLayout();
                }));

        public IEnumerable<BarSeries> Series
        {
            get => (IEnumerable<BarSeries>)GetValue(SeriesProperty);
            set => SetValue(SeriesProperty, value);
        }

        public static readonly DependencyProperty SeriesProperty =
            DependencyProperty.Register(
                nameof(Series),
                typeof(IEnumerable<BarSeries>),
                typeof(BarChartControl),
                new PropertyMetadata(null, (_, __) => ((BarChartControl)_).Redraw()));


        #endregion

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
                typeof(BarChartControl),
                new PropertyMetadata(AxisScaleFormat.Number, (_, __) => ((BarChartControl)_).Redraw()));

        #endregion

        #region Rendering

        private void Redraw()
        {
            this.PART_Canvas.Children.Clear();
            this.PART_Legend.Children.Clear();

            if (Series == null || !Series.Any() || ActualWidth <= 0 || ActualHeight <= 0)
            {
                return;
            }

            this.DrawYAxisWithLabels();
            this.DrawBars();
            this.DrawXAxisLabels();

            if (this.ShowLegend == true)
            {
                this.DrawLegend();
            }
        }

        private void DrawLegend()
        {
            foreach (var series in this.Series)
            {
                var row = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 4, 0, 4)
                };

                var colorBox = new Rectangle
                {
                    Width = 14,
                    Height = 14,
                    Fill = series.Fill,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5,
                    Margin = new Thickness(5, 0, 6, 0)
                };

                var text = new TextBlock
                {
                    Text = series.Title,
                    VerticalAlignment = VerticalAlignment.Center
                };

                row.Children.Add(colorBox);
                row.Children.Add(text);

                this.PART_Legend.Children.Add(row);
            }
        }

        private void DrawBars()
        {
            if (this.Series == null || this.Series.Any() == false)
            {
                return;
            }

            var seriesList = this.Series.ToList();

            // X-Kategorien aus erster Serie
            var categories = seriesList.First().Values.Select(v => v.X).ToList();
            int categoryCount = categories.Count;

            // Maximalwert für gestapelte Balken
            double maxStackValue = categories.Max(cat => seriesList.Sum(s => s.Values.FirstOrDefault(v => v.X == cat)?.Y ?? 0));

            double plotWidth = ActualWidth - LeftMargin - RightMargin;
            double plotHeight = ActualHeight - TopMargin - BottomMargin;

            double barWidth = plotWidth / categoryCount * 0.5;       // Balkenbreite
            double categoryStep = plotWidth / categoryCount;         // Abstand pro Kategorie

            for (int i = 0; i < categoryCount; i++)
            {
                double stackBase = 0;

                foreach (var series in seriesList)
                {
                    var value = series.Values.FirstOrDefault(v => v.X == categories[i]);
                    if (value == null || value.Y <= 0)
                    {
                        continue;
                    }

                    // Höhe proportional zur Y-Achse
                    double barHeight = value.Y / maxStackValue * plotHeight;

                    // X-Position für die Kategorie
                    double x = LeftMargin + i * categoryStep + (categoryStep - barWidth) / 2;

                    // Y-Position (von oben) unter Berücksichtigung der gestapelten Basis
                    double y = TopMargin + plotHeight - (stackBase + value.Y) / maxStackValue * plotHeight;

                    // Rechteck für Balken
                    var rect = new Rectangle
                    {
                        Width = barWidth,
                        Height = barHeight,
                        Fill = series.Fill
                    };

                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                    this.PART_Canvas.Children.Add(rect);

                    // Update gestapelte Basis
                    stackBase += value.Y;
                }
            }
        }

        private void DrawYAxisWithLabels()
        {
            if (this.Series == null || this.Series.Any() == false)
            {
                return;
            }

            var seriesList = this.Series.ToList();
            var categories = seriesList.First().Values.Select(v => v.X).ToList();
            if (categories.Count == 0)
            {
                return;
            }

            double plotHeight = ActualHeight - TopMargin - BottomMargin;

            // Maximalwert für gestapelte Balken
            double maxStackValue = categories.Max(cat => seriesList.Sum(s => s.Values.FirstOrDefault(v => v.X == cat)?.Y ?? 0));

            int tickCount = 5; // Anzahl der Y-Ticks

            for (int i = 0; i <= tickCount; i++)
            {
                double value = maxStackValue * i / tickCount;
                double y = TopMargin + plotHeight - (value / maxStackValue * plotHeight);

                // Tick Linie (kurzer Strich links der Achse)
                this.PART_Canvas.Children.Add(new Line
                {
                    X1 = LeftMargin - 5,
                    X2 = LeftMargin,
                    Y1 = y,
                    Y2 = y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                });

                // Label für Y-Achse

                var label = new TextBlock
                {
                    Text = FormatAxisValue(value),
                    FontSize = XAxisTitleFontSize,
                    Foreground = Brushes.Black,
                };

                Canvas.SetLeft(label, LeftMargin - 50); // links der Achse
                Canvas.SetTop(label, y - 8);
                this.PART_Canvas.Children.Add(label);

                // Horizontale Gridline (optional)
                if (ShowHorizontalGridLines && i > 0) // skip 0, sonst Achse überlagert
                {
                    PART_Canvas.Children.Add(new Line
                    {
                        X1 = LeftMargin,
                        X2 = ActualWidth - RightMargin,
                        Y1 = y,
                        Y2 = y,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1
                    });
                }
            }

            // Y-Achse Linie
            this.PART_Canvas.Children.Add(new Line
            {
                X1 = LeftMargin,
                X2 = LeftMargin,
                Y1 = TopMargin,
                Y2 = TopMargin + plotHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            });

            // Y-Achsentitel
            if (!string.IsNullOrEmpty(YAxisTitle))
            {
                var titleBlock = new TextBlock
                {
                    Text = this.YAxisTitle,
                    FontSize = this.YAxisTitleFontSize,
                    Foreground = this.YAxisTitleForeground,
                    TextAlignment = this.YAxisTitleAlignment
                };

                // Rotieren um -90° für vertikalen Titel
                titleBlock.RenderTransform = new RotateTransform(-90);

                // Position links neben Y-Achse (mittig entlang der Achse)
                double centerY = TopMargin + plotHeight / 2;

                Canvas.SetLeft(titleBlock, LeftMargin - 30); // links von Labels/Achse
                Canvas.SetTop(titleBlock, centerY - titleBlock.ActualHeight / 2);

                this.PART_Canvas.Children.Add(titleBlock);
            }
        }

        private void DrawXAxisLabels()
        {
            var categories = this.Series.First().Values.Select(v => v.X).ToList();
            int count = categories.Count;

            double plotWidth = ActualWidth - LeftMargin - RightMargin;
            double categoryStep = plotWidth / count;

            for (int i = 0; i < count; i++)
            {
                var text = new TextBlock
                {
                    Text = categories[i],
                    FontSize = YAxisTitleFontSize
                };

                text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                double x = LeftMargin + i * categoryStep + categoryStep / 2
                           - text.DesiredSize.Width / 2;

                Canvas.SetLeft(text, x);
                Canvas.SetTop(text, ActualHeight - BottomMargin + 5);

                this.PART_Canvas.Children.Add(text);
            }

            // X-Achsentitel
            if (!string.IsNullOrEmpty(this.XAxisTitle))
            {
                var titleBlock = new TextBlock
                {
                    Text = this.XAxisTitle,
                    FontSize = this.XAxisTitleFontSize,
                    Foreground = this.XAxisTitleForeground,
                    TextAlignment = TextAlignment.Center
                };

                titleBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                double x = LeftMargin + plotWidth / 2 - titleBlock.DesiredSize.Width / 2;
                double y = ActualHeight - BottomMargin + 25; // unter den Kategorie-Labels

                Canvas.SetLeft(titleBlock, x);
                Canvas.SetTop(titleBlock, y);
                this.PART_Canvas.Children.Add(titleBlock);
            }
        }

        private void UpdateLegendLayout()
        {
            if (PART_RootGrid == null || PART_Canvas == null || PART_Legend == null)
            {
                return;
            }

            this.PART_RootGrid.RowDefinitions.Clear();
            this.PART_RootGrid.ColumnDefinitions.Clear();

            // Reset positions
            Grid.SetRow(this.PART_Canvas, 0);
            Grid.SetColumn(this.PART_Canvas, 0);
            Grid.SetRow(this.PART_Legend, 0);
            Grid.SetColumn(this.PART_Legend, 0);

            switch (this.LegendPosition)
            {
                case LegendPosition.Left:
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition());

                    Grid.SetColumn(this.PART_Legend, 0);
                    Grid.SetColumn(this.PART_Canvas, 1);
                    this.PART_Legend.Orientation = Orientation.Vertical;
                    break;

                case LegendPosition.Right:
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition());

                    Grid.SetColumn(this.PART_Canvas, 0);
                    Grid.SetColumn(this.PART_Legend, 1);
                    this.PART_Legend.Orientation = Orientation.Vertical;
                    break;

                case LegendPosition.Top:
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    Grid.SetRow(this.PART_Legend, 0);
                    Grid.SetRow(this.PART_Canvas, 1);
                    this.PART_Legend.Orientation = Orientation.Horizontal;
                    break;

                case LegendPosition.Bottom:
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    Grid.SetRow(this.PART_Canvas, 0);
                    Grid.SetRow(this.PART_Legend, 1);
                    this.PART_Legend.Orientation = Orientation.Horizontal;
                    break;
            }
        }

        private string FormatAxisValue(double value)
        {
            return XAxisScaleFormat switch
            {
                AxisScaleFormat.Number =>
                    value.ToString("0", CultureInfo.CurrentCulture),

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

                AxisScaleFormat.Percent =>
                    (value * 100).ToString("0.#", CultureInfo.CurrentCulture) + " %",

                _ => value.ToString("0", CultureInfo.CurrentCulture)
            };
        }

        #endregion

        #region Y-Achsentitel Properties

        public string YAxisTitle
        {
            get => (string)GetValue(YAxisTitleProperty);
            set => SetValue(YAxisTitleProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleProperty =
            DependencyProperty.Register(
                nameof(YAxisTitle),
                typeof(string),
                typeof(BarChartControl),
                new PropertyMetadata(string.Empty, (_, __) => ((BarChartControl)_).Redraw()));

        public double YAxisTitleFontSize
        {
            get => (double)GetValue(YAxisTitleFontSizeProperty);
            set => SetValue(YAxisTitleFontSizeProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleFontSizeProperty =
            DependencyProperty.Register(
                nameof(YAxisTitleFontSize),
                typeof(double),
                typeof(BarChartControl),
                new PropertyMetadata(12.0, (_, __) => ((BarChartControl)_).Redraw()));

        public Brush YAxisTitleForeground
        {
            get => (Brush)GetValue(YAxisTitleForegroundProperty);
            set => SetValue(YAxisTitleForegroundProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleForegroundProperty =
            DependencyProperty.Register(
                nameof(YAxisTitleForeground),
                typeof(Brush),
                typeof(BarChartControl),
                new PropertyMetadata(Brushes.Black, (_, __) => ((BarChartControl)_).Redraw()));

        public TextAlignment YAxisTitleAlignment
        {
            get => (TextAlignment)GetValue(YAxisTitleAlignmentProperty);
            set => SetValue(YAxisTitleAlignmentProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleAlignmentProperty =
            DependencyProperty.Register(
                nameof(YAxisTitleAlignment),
                typeof(TextAlignment),
                typeof(BarChartControl),
                new PropertyMetadata(TextAlignment.Center, (_, __) => ((BarChartControl)_).Redraw()));

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
