namespace ChartNETDemo
{
    using System.Collections.Generic;
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
        private const double LeftMargin = 60;
        private const double RightMargin = 20;
        private const double TopMargin = 20;
        private const double BottomMargin = 60;
        private const int YAxisTickCount = 5;
        private const double YAxisLabelOffset = 10;

        public ColumnChartControl()
        {
            InitializeComponent();
            SizeChanged += (_, _) => Redraw();
        }

        #region DependencyProperty

        public IEnumerable<ColumnChartSeries> Series
        {
            get => (IEnumerable<ColumnChartSeries>)GetValue(SeriesProperty);
            set => SetValue(SeriesProperty, value);
        }

        public static readonly DependencyProperty SeriesProperty =
            DependencyProperty.Register(
                nameof(Series),
                typeof(IEnumerable<ColumnChartSeries>),
                typeof(ColumnChartControl),
                new PropertyMetadata(null, (_, __) => ((ColumnChartControl)_).Redraw()));

        #endregion

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

        #region Rendering

        private void Redraw()
        {
            PART_Canvas.Children.Clear();
            PART_Legend.Children.Clear();

            if (Series == null || !Series.Any())
                return;

            DrawAxes();
            DrawYAxisWithLabelsAndGridlines();
            DrawColumns();
            DrawXAxisLabels();

            if (ShowLegend)
                DrawLegend();
        }

        private void DrawAxes()
        {
            double h = ActualHeight - BottomMargin;

            // Y-Achse
            PART_Canvas.Children.Add(new Line
            {
                X1 = LeftMargin,
                X2 = LeftMargin,
                Y1 = TopMargin,
                Y2 = h,
                Stroke = Brushes.Black
            });

            // X-Achse
            PART_Canvas.Children.Add(new Line
            {
                X1 = LeftMargin,
                X2 = ActualWidth - RightMargin,
                Y1 = h,
                Y2 = h,
                Stroke = Brushes.Black
            });
        }

        private void DrawColumns()
        {
            var seriesList = Series.ToList();
            if (!seriesList.Any())
                return;

            var categories = seriesList.First().Values.Select(v => v.X).ToList();
            int categoryCount = categories.Count;
            int seriesCount = seriesList.Count;

            if (categoryCount == 0 || seriesCount == 0)
                return;

            double plotWidth = ActualWidth - LeftMargin - RightMargin;
            double plotHeight = ActualHeight - TopMargin - BottomMargin;

            // ❗ WICHTIG: Platz prüfen
            if (plotWidth <= 0 || plotHeight <= 0)
                return;

            double maxValue = seriesList.Max(s => s.Values.Max(v => v.Y));
            if (maxValue <= 0)
                return;

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

                    double x =
                        LeftMargin +
                        i * categoryStep +
                        (categoryStep - groupWidth) / 2 +
                        s * columnWidth;

                    double y =
                        TopMargin +
                        plotHeight -
                        height;

                    var rect = new Rectangle
                    {
                        Width = columnWidth - 1,
                        Height = height,
                        Fill = series.Fill
                    };

                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                    PART_Canvas.Children.Add(rect);
                }
            }
        }

        private void DrawXAxisLabels()
        {
            var categories = Series.First().Values.Select(v => v.X).ToList();

            double plotWidth = ActualWidth - LeftMargin - RightMargin;
            double categoryStep = plotWidth / categories.Count;

            for (int i = 0; i < categories.Count; i++)
            {
                var label = new TextBlock
                {
                    Text = categories[i],
                    FontSize = 12
                };

                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Canvas.SetLeft(label,
                    LeftMargin +
                    i * categoryStep +
                    categoryStep / 2 -
                    label.DesiredSize.Width / 2);

                Canvas.SetTop(label, ActualHeight - BottomMargin + 5);
                PART_Canvas.Children.Add(label);
            }
        }

        private void DrawYAxisWithLabelsAndGridlines()
        {
            var seriesList = Series.ToList();
            if (!seriesList.Any())
                return;

            double plotHeight = ActualHeight - TopMargin - BottomMargin;
            double plotWidth = ActualWidth - LeftMargin - RightMargin;

            if (plotHeight <= 0 || plotWidth <= 0)
                return;

            double maxValue = seriesList.Max(s => s.Values.Max(v => v.Y));
            if (maxValue <= 0)
                return;

            for (int i = 0; i <= YAxisTickCount; i++)
            {
                double value = maxValue * i / YAxisTickCount;
                double y =
                    TopMargin +
                    plotHeight -
                    (value / maxValue) * plotHeight;

                // ───── Gridline ─────
                if (i > 0)
                {
                    PART_Canvas.Children.Add(new Line
                    {
                        X1 = LeftMargin,
                        X2 = LeftMargin + plotWidth,
                        Y1 = y,
                        Y2 = y,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1
                    });
                }

                // ───── Tick ─────
                PART_Canvas.Children.Add(new Line
                {
                    X1 = LeftMargin - 5,
                    X2 = LeftMargin,
                    Y1 = y,
                    Y2 = y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                });

                // ───── Label ─────
                var label = new TextBlock
                {
                    Text = value.ToString("0"),
                    FontSize = 11
                };

                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Canvas.SetLeft(label,
                    LeftMargin - label.DesiredSize.Width - YAxisLabelOffset);

                Canvas.SetTop(label,
                    y - label.DesiredSize.Height / 2);

                PART_Canvas.Children.Add(label);
            }

            // ───── Y-Achsenlinie ─────
            PART_Canvas.Children.Add(new Line
            {
                X1 = LeftMargin,
                X2 = LeftMargin,
                Y1 = TopMargin,
                Y2 = TopMargin + plotHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            });
        }

        private void DrawLegend()
        {
            PART_Legend.Children.Clear();

            foreach (var series in Series)
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

                PART_Legend.Children.Add(row);
            }
        }

        private void UpdateLegendLayout()
        {
            if (PART_RootGrid == null || PART_Canvas == null || PART_Legend == null)
                return;

            PART_RootGrid.RowDefinitions.Clear();
            PART_RootGrid.ColumnDefinitions.Clear();

            // Reset
            Grid.SetRow(PART_Canvas, 0);
            Grid.SetColumn(PART_Canvas, 0);
            Grid.SetRow(PART_Legend, 0);
            Grid.SetColumn(PART_Legend, 0);

            switch (LegendPosition)
            {
                case LegendPosition.Left:
                    PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    Grid.SetColumn(PART_Legend, 0);
                    Grid.SetColumn(PART_Canvas, 1);
                    PART_Legend.Orientation = Orientation.Vertical;
                    break;

                case LegendPosition.Right:
                    PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    Grid.SetColumn(PART_Canvas, 0);
                    Grid.SetColumn(PART_Legend, 1);
                    PART_Legend.Orientation = Orientation.Vertical;
                    break;

                case LegendPosition.Top:
                    PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    PART_RootGrid.RowDefinitions.Add(new RowDefinition());

                    Grid.SetRow(PART_Legend, 0);
                    Grid.SetRow(PART_Canvas, 1);
                    PART_Legend.Orientation = Orientation.Horizontal;
                    break;

                case LegendPosition.Bottom:
                    PART_RootGrid.RowDefinitions.Add(new RowDefinition());
                    PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    Grid.SetRow(PART_Canvas, 0);
                    Grid.SetRow(PART_Legend, 1);
                    PART_Legend.Orientation = Orientation.Horizontal;
                    break;
            }
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
