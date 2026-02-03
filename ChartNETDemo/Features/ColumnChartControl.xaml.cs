namespace ChartNETDemo
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    public class ColumnChartPoint
    {
        public string X { get; set; } = "";
        public double Y { get; set; }
    }

    public class ColumnChartSeries
    {
        public string Title { get; set; } = "";
        public Brush Fill { get; set; } = Brushes.SteelBlue;
        public IList<ColumnChartPoint> Values { get; set; } = new List<ColumnChartPoint>();
    }

    /// <summary>
    /// Interaktionslogik für ColumnChartControl.xaml
    /// </summary>
    public partial class ColumnChartControl : UserControl
    {
        private const double LEFTMARGIN = 60;
        private const double RIGHTMARGIN = 20;
        private const double TOPMARGIN = 20;
        private const double BOTTOMMARGIN = 60;
        private const int YAXISTICKCOUNT = 5;
        private const double YAXISLABELOFFSET = 10;

        public ColumnChartControl()
        {
            this.InitializeComponent();
            this.SizeChanged += (_, _) => this.Redraw();
        }

        #region ItemSource

        public IEnumerable<ColumnChartSeries> ItemSource
        {
            get => (IEnumerable<ColumnChartSeries>)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }

        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
                typeof(IEnumerable<ColumnChartSeries>),
                typeof(ColumnChartControl),
                new PropertyMetadata(null, (_, __) => ((ColumnChartControl)_).Redraw()));

        #endregion ItemSource

        #region Legend Properties

        public bool ShowLegend
        {
            get => (bool)GetValue(ShowLegendProperty);
            set => SetValue(ShowLegendProperty, value);
        }

        public static readonly DependencyProperty ShowLegendProperty =
            DependencyProperty.Register(
                nameof(ShowLegend),
                typeof(bool),
                typeof(ColumnChartControl),
                new PropertyMetadata(true, (_, __) => ((ColumnChartControl)_).Redraw()));

        public LegendPosition LegendPosition
        {
            get => (LegendPosition)GetValue(LegendPositionProperty);
            set => SetValue(LegendPositionProperty, value);
        }

        public static readonly DependencyProperty LegendPositionProperty =
            DependencyProperty.Register(
                nameof(LegendPosition),
                typeof(LegendPosition),
                typeof(ColumnChartControl),
                new PropertyMetadata(LegendPosition.Right, (_, __) =>
                {
                    var c = (ColumnChartControl)_;
                    c.UpdateLegendLayout();
                }));

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
                typeof(ColumnChartControl),
                new PropertyMetadata(string.Empty, (_, __) => ((ColumnChartControl)_).Redraw()));

        public Brush XAxisTitleForeground
        {
            get => (Brush)GetValue(XAxisTitleForegroundProperty);
            set => SetValue(XAxisTitleForegroundProperty, value);
        }

        public static readonly DependencyProperty XAxisTitleForegroundProperty =
            DependencyProperty.Register(
                nameof(XAxisTitleForeground),
                typeof(Brush),
                typeof(ColumnChartControl),
                new PropertyMetadata(Brushes.Black, (_, __) => ((ColumnChartControl)_).Redraw()));

        public double XAxisTitleFontSize
        {
            get => (double)GetValue(XAxisTitleFontSizeProperty);
            set => SetValue(XAxisTitleFontSizeProperty, value);
        }

        public static readonly DependencyProperty XAxisTitleFontSizeProperty =
            DependencyProperty.Register(
                nameof(XAxisTitleFontSize),
                typeof(double),
                typeof(ColumnChartControl),
                new PropertyMetadata(13.0, (_, __) => ((ColumnChartControl)_).Redraw()));

        public TextAlignment XAxisTitleAlignment
        {
            get => (TextAlignment)GetValue(XAxisTitleAlignmentProperty);
            set => SetValue(XAxisTitleAlignmentProperty, value);
        }

        public static readonly DependencyProperty XAxisTitleAlignmentProperty =
            DependencyProperty.Register(
                nameof(XAxisTitleAlignment),
                typeof(TextAlignment),
                typeof(ColumnChartControl),
                new PropertyMetadata(TextAlignment.Center, (_, __) => ((ColumnChartControl)_).Redraw()));

        // ------------------------------------------------------

        public string YAxisTitle
        {
            get => (string)GetValue(YAxisTitleProperty);
            set => SetValue(YAxisTitleProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleProperty =
            DependencyProperty.Register(
                nameof(YAxisTitle),
                typeof(string),
                typeof(ColumnChartControl),
                new PropertyMetadata(string.Empty, (_, __) => ((ColumnChartControl)_).Redraw()));

        public Brush YAxisTitleForeground
        {
            get => (Brush)GetValue(YAxisTitleForegroundProperty);
            set => SetValue(YAxisTitleForegroundProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleForegroundProperty =
            DependencyProperty.Register(
                nameof(YAxisTitleForeground),
                typeof(Brush),
                typeof(ColumnChartControl),
                new PropertyMetadata(Brushes.Black, (_, __) => ((ColumnChartControl)_).Redraw()));

        public double YAxisTitleFontSize
        {
            get => (double)GetValue(YAxisTitleFontSizeProperty);
            set => SetValue(YAxisTitleFontSizeProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleFontSizeProperty =
            DependencyProperty.Register(
                nameof(YAxisTitleFontSize),
                typeof(double),
                typeof(ColumnChartControl),
                new PropertyMetadata(13.0, (_, __) => ((ColumnChartControl)_).Redraw()));

        public TextAlignment YAxisTitleAlignment
        {
            get => (TextAlignment)GetValue(YAxisTitleAlignmentProperty);
            set => SetValue(YAxisTitleAlignmentProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleAlignmentProperty =
            DependencyProperty.Register(
                nameof(YAxisTitleAlignment),
                typeof(TextAlignment),
                typeof(ColumnChartControl),
                new PropertyMetadata(TextAlignment.Center, (_, __) => ((ColumnChartControl)_).Redraw()));

        #endregion

        #region Axis Scale Format
        public AxisScaleFormat YAxisScaleFormat
        {
            get => (AxisScaleFormat)GetValue(YAxisScaleFormatProperty);
            set => SetValue(YAxisScaleFormatProperty, value);
        }

        public static readonly DependencyProperty YAxisScaleFormatProperty =
            DependencyProperty.Register(
                nameof(YAxisScaleFormat),
                typeof(AxisScaleFormat),
                typeof(ColumnChartControl),
                new PropertyMetadata(AxisScaleFormat.Number, (_, __) => ((ColumnChartControl)_).Redraw()));

        #endregion

        #region Rendering

        private void Redraw()
        {
            this.PART_Canvas.Children.Clear();
            this.PART_Legend.Children.Clear();

            if (this.ItemSource == null || this.ItemSource.Any() == false)
            {
                return;
            }

            this.DrawAxes();
            this.DrawYAxisWithLabelsAndGridlines();
            this.DrawColumns();
            this.DrawXAxisLabels();

            if (this.ShowLegend == true)
            {
                this.DrawLegend();
            }
        }

        private void DrawAxes()
        {
            double plotHeight = this.ActualHeight - BOTTOMMARGIN;
            double plotWidth = this.ActualWidth - LEFTMARGIN - RIGHTMARGIN;

            /* Y-Achse Linie */
            PART_Canvas.Children.Add(new Line
            {
                X1 = LEFTMARGIN,
                X2 = LEFTMARGIN,
                Y1 = TOPMARGIN,
                Y2 = plotHeight,
                Stroke = Brushes.Black
            });

            /* X-Achse Linie */
            this.PART_Canvas.Children.Add(new Line
            {
                X1 = LEFTMARGIN,
                X2 = LEFTMARGIN + plotWidth,
                Y1 = plotHeight,
                Y2 = plotHeight,
                Stroke = Brushes.Black
            });

            /* X-Achsentitel */
            if (string.IsNullOrWhiteSpace(this.XAxisTitle) == false)
            {
                var xTitle = new TextBlock
                {
                    Text = this.XAxisTitle,
                    FontSize = this.XAxisTitleFontSize,
                    Foreground = this.XAxisTitleForeground,
                    TextAlignment = this.XAxisTitleAlignment
                };

                xTitle.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Canvas.SetLeft(xTitle, LEFTMARGIN + plotWidth / 2 - xTitle.DesiredSize.Width / 2);
                Canvas.SetTop(xTitle,plotHeight + 20);

                this.PART_Canvas.Children.Add(xTitle);
            }

            /* Y-Achsentitel */
            if (string.IsNullOrWhiteSpace(this.YAxisTitle) == false)
            {
                var yTitle = new TextBlock
                {
                    Text = this.YAxisTitle,
                    FontSize = this.YAxisTitleFontSize,
                    Foreground = this.YAxisTitleForeground,
                    TextAlignment = this.YAxisTitleAlignment,
                    RenderTransform = new RotateTransform(-90)
                };

                yTitle.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Canvas.SetLeft(yTitle, 20);
                Canvas.SetTop(yTitle, TOPMARGIN + (plotHeight - TOPMARGIN) / 2 + yTitle.DesiredSize.Width / 2);

                this.PART_Canvas.Children.Add(yTitle);
            }
        }

        private void DrawColumns()
        {
            var seriesList = this.ItemSource.ToList();
            if (seriesList.Count == 0)
            {
                return;
            }

            var categories = seriesList.First().Values.Select(v => v.X).ToList();
            int categoryCount = categories.Count;
            int seriesCount = seriesList.Count;

            if (categoryCount == 0 || seriesCount == 0)
            {
                return;
            }

            double plotWidth = this.ActualWidth - LEFTMARGIN - RIGHTMARGIN;
            double plotHeight = this.ActualHeight - TOPMARGIN - BOTTOMMARGIN;

            // ❗ WICHTIG: Platz prüfen
            if (plotWidth <= 0 || plotHeight <= 0)
            {
                return;
            }

            double maxValue = seriesList.Max(s => s.Values.Max(v => v.Y));
            if (maxValue <= 0)
            {
                return;
            }

            double categoryStep = plotWidth / categoryCount;

            // ❗ Mindestabstände erzwingen
            double groupWidth = Math.Max(categoryStep * 0.8, seriesCount * 2);
            double columnWidth = Math.Max(groupWidth / seriesCount, 2);

            for (int i = 0; i < categoryCount; i++)
            {
                for (int s = 0; s < seriesCount; s++)
                {
                    var series = seriesList[s];
                    double value = series.Values[i].Y;
                    if (value <= 0)
                        continue;

                    double height = value / maxValue * plotHeight;
                    double x = LEFTMARGIN + i * categoryStep + (categoryStep - groupWidth) / 2 + s * columnWidth;
                    double y = TOPMARGIN + plotHeight - height;

                    var rect = new Rectangle
                    {
                        Width = columnWidth - 1,
                        Height = height,
                        Fill = series.Fill,
                        ToolTip = new ToolTip
                        {
                            Content = "Tooltip"
                        }
                    };

                    ToolTipService.SetInitialShowDelay(rect, 100);
                    ToolTipService.SetShowDuration(rect, 1500);

                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                    this.PART_Canvas.Children.Add(rect);
                }
            }
        }

        private void DrawXAxisLabels()
        {
            var categories = this.ItemSource.First().Values.Select(v => v.X).ToList();
            if (categories == null || categories.Count == 0)
            {
                return;
            }

            double plotWidth = this.ActualWidth - LEFTMARGIN - RIGHTMARGIN;
            double categoryStep = plotWidth / categories.Count;

            for (int i = 0; i < categories.Count; i++)
            {
                var label = new TextBlock
                {
                    Text = categories[i],
                    FontSize = 12
                };

                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Canvas.SetLeft(label, LEFTMARGIN + i * categoryStep + categoryStep / 2 - label.DesiredSize.Width / 2);
                Canvas.SetTop(label, this.ActualHeight - BOTTOMMARGIN + 5);
                this.PART_Canvas.Children.Add(label);
            }
        }

        private void DrawYAxisWithLabelsAndGridlines()
        {
            var seriesList = ItemSource.ToList();
            if (seriesList.Count == 0)
            {
                return;
            }

            double plotHeight = this.ActualHeight - TOPMARGIN - BOTTOMMARGIN;
            double plotWidth = this.ActualWidth - LEFTMARGIN - RIGHTMARGIN;

            if (plotHeight <= 0 || plotWidth <= 0)
            {
                return;
            }

            double maxValue = seriesList.Max(s => s.Values.Max(v => v.Y));
            if (maxValue <= 0)
            {
                return;
            }

            for (int i = 0; i <= YAXISTICKCOUNT; i++)
            {
                double value = maxValue * i / YAXISTICKCOUNT;
                double y = TOPMARGIN + plotHeight - (value / maxValue) * plotHeight;

                /* Gridline */
                if (i > 0)
                {
                    PART_Canvas.Children.Add(new Line
                    {
                        X1 = LEFTMARGIN,
                        X2 = LEFTMARGIN + plotWidth,
                        Y1 = y,
                        Y2 = y,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1
                    });
                }

                /* Tick */
                PART_Canvas.Children.Add(new Line
                {
                    X1 = LEFTMARGIN - 5,
                    X2 = LEFTMARGIN,
                    Y1 = y,
                    Y2 = y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                });

                /* Label */
                var label = new TextBlock
                {
                    Text = FormatYAxisValue(value),
                    FontSize = 11
                };

                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Canvas.SetLeft(label, LEFTMARGIN - label.DesiredSize.Width - YAXISLABELOFFSET);
                Canvas.SetTop(label, y - label.DesiredSize.Height / 2);

                this.PART_Canvas.Children.Add(label);
            }

            /* Y-Achsenlinie */
            this.PART_Canvas.Children.Add(new Line
            {
                X1 = LEFTMARGIN,
                X2 = LEFTMARGIN,
                Y1 = TOPMARGIN,
                Y2 = TOPMARGIN + plotHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            });
        }

        private void DrawLegend()
        {
            this.PART_Legend.Children.Clear();

            foreach (var series in this.ItemSource)
            {
                var row = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(4)
                };

                row.Children.Add(new Rectangle
                {
                    Width = 14,
                    Height = 14,
                    Fill = series.Fill,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5,
                    Margin = new Thickness(0, 0, 6, 0)
                });

                row.Children.Add(new TextBlock
                {
                    Text = series.Title,
                    VerticalAlignment = VerticalAlignment.Center
                });

                this.PART_Legend.Children.Add(row);
            }
        }

        private void UpdateLegendLayout()
        {
            if (this.PART_RootGrid == null || this.PART_Canvas == null || this.PART_Legend == null)
            {
                return;
            }

            this.PART_RootGrid.RowDefinitions.Clear();
            this.PART_RootGrid.ColumnDefinitions.Clear();

            /* Reset */
            Grid.SetRow(this.PART_Canvas, 0);
            Grid.SetColumn(this.PART_Canvas, 0);
            Grid.SetRow(this.PART_Legend, 0);
            Grid.SetColumn(this.PART_Legend, 0);

            switch (LegendPosition)
            {
                case LegendPosition.Left:
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    Grid.SetColumn(this.PART_Legend, 0);
                    Grid.SetColumn(this.PART_Canvas, 1);
                    this.PART_Legend.Orientation = Orientation.Vertical;
                    break;

                case LegendPosition.Right:
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    Grid.SetColumn(this.PART_Canvas, 0);
                    Grid.SetColumn(this.PART_Legend, 1);
                    this.PART_Legend.Orientation = Orientation.Vertical;
                    break;

                case LegendPosition.Top:
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition());

                    Grid.SetRow(this.PART_Legend, 0);
                    Grid.SetRow(this.PART_Canvas, 1);
                    this.PART_Legend.Orientation = Orientation.Horizontal;
                    break;

                case LegendPosition.Bottom:
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition());
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    Grid.SetRow(this.PART_Canvas, 0);
                    Grid.SetRow(this.PART_Legend, 1);
                    this.PART_Legend.Orientation = Orientation.Horizontal;
                    break;
            }
        }

        private string FormatYAxisValue(double value)
        {
            return YAxisScaleFormat switch
            {
                AxisScaleFormat.Number => value.ToString("0", CultureInfo.CurrentCulture),
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

                _ => value.ToString("0",CultureInfo.CurrentCulture)
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
