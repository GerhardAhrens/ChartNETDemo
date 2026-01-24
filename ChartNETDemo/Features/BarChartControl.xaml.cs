namespace ChartNETDemo
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;

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
            this.SizeChanged += (_, _) => Redraw();

            this.Loaded += (_, __) =>
            {
                UpdateLegendLayout();
                Redraw();
            };
        }

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

        public int YAxisTickCount
        {
            get => (int)GetValue(YAxisTickCountProperty);
            set => SetValue(YAxisTickCountProperty, value);
        }

        public static readonly DependencyProperty YAxisTickCountProperty =
            DependencyProperty.Register(
                nameof(YAxisTickCount),
                typeof(int),
                typeof(BarChartControl),
                new PropertyMetadata(5, (_, __) => ((BarChartControl)_).Redraw()));

        public double? YAxisMaximum
        {
            get => (double?)GetValue(YAxisMaximumProperty);
            set => SetValue(YAxisMaximumProperty, value);
        }

        public static readonly DependencyProperty YAxisMaximumProperty =
            DependencyProperty.Register(
                nameof(YAxisMaximum),
                typeof(double?),
                typeof(BarChartControl),
                new PropertyMetadata(null, (_, __) => ((BarChartControl)_).Redraw()));

        #endregion

        #region Rendering

        private void Redraw()
        {
            PART_Canvas.Children.Clear();
            PART_Legend.Children.Clear();

            if (Series == null || !Series.Any() || ActualWidth <= 0 || ActualHeight <= 0)
                return;

            DrawAxes();
            DrawBars();
            DrawXAxisLabels();

            if (ShowLegend)
                DrawLegend();
        }

        private void DrawLegend()
        {
            foreach (var series in Series)
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

                PART_Legend.Children.Add(row);
            }
        }

        private void DrawAxes()
        {
            // Y-Achse
            PART_Canvas.Children.Add(new Line
            {
                X1 = LeftMargin,
                Y1 = TopMargin,
                X2 = LeftMargin,
                Y2 = ActualHeight - BottomMargin,
                Stroke = Brushes.Black
            });

            // X-Achse
            PART_Canvas.Children.Add(new Line
            {
                X1 = LeftMargin,
                Y1 = ActualHeight - BottomMargin,
                X2 = ActualWidth - RightMargin,
                Y2 = ActualHeight - BottomMargin,
                Stroke = Brushes.Black
            });
        }

        private void DrawBars()
        {
            var seriesList = Series.ToList();

            // X-Kategorien aus erster Serie
            var categories = seriesList.First().Values.Select(v => v.X).ToList();
            int categoryCount = categories.Count;

            // Maximalwert für gestapelte Balken
            double maxStackValue = categories.Max(cat =>
                seriesList.Sum(s => s.Values.FirstOrDefault(v => v.X == cat)?.Y ?? 0));

            double plotWidth = ActualWidth - LeftMargin - RightMargin;
            double plotHeight = ActualHeight - TopMargin - BottomMargin;

            double barWidth = plotWidth / categoryCount * 0.5;
            double categoryStep = plotWidth / categoryCount;

            for (int i = 0; i < categoryCount; i++)
            {
                double stackBase = 0;

                foreach (var series in seriesList)
                {
                    var value = series.Values.FirstOrDefault(v => v.X == categories[i]);
                    if (value == null || value.Y <= 0)
                        continue;

                    double barHeight = value.Y / maxStackValue * plotHeight;

                    double x = LeftMargin + i * categoryStep + (categoryStep - barWidth) / 2;
                    double y = TopMargin + plotHeight
                               - (stackBase + value.Y) / maxStackValue * plotHeight;

                    var rect = new Rectangle
                    {
                        Width = barWidth,
                        Height = barHeight,
                        Fill = series.Fill
                    };

                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                    PART_Canvas.Children.Add(rect);

                    stackBase += value.Y;
                }
            }
        }

        private void DrawXAxisLabels()
        {
            var categories = Series.First().Values.Select(v => v.X).ToList();
            int count = categories.Count;

            double plotWidth = ActualWidth - LeftMargin - RightMargin;
            double categoryStep = plotWidth / count;

            for (int i = 0; i < count; i++)
            {
                var text = new TextBlock
                {
                    Text = categories[i],
                    FontSize = 12
                };

                text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                double x = LeftMargin + i * categoryStep + categoryStep / 2
                           - text.DesiredSize.Width / 2;

                Canvas.SetLeft(text, x);
                Canvas.SetTop(text, ActualHeight - BottomMargin + 5);

                PART_Canvas.Children.Add(text);
            }
        }

        private void UpdateLegendLayout()
        {
            if (PART_RootGrid == null || PART_Canvas == null || PART_Legend == null)
                return;

            PART_RootGrid.RowDefinitions.Clear();
            PART_RootGrid.ColumnDefinitions.Clear();

            // Reset positions
            Grid.SetRow(PART_Canvas, 0);
            Grid.SetColumn(PART_Canvas, 0);
            Grid.SetRow(PART_Legend, 0);
            Grid.SetColumn(PART_Legend, 0);

            switch (LegendPosition)
            {
                case LegendPosition.Left:
                    PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    PART_RootGrid.RowDefinitions.Add(new RowDefinition());

                    Grid.SetColumn(PART_Legend, 0);
                    Grid.SetColumn(PART_Canvas, 1);
                    PART_Legend.Orientation = Orientation.Vertical;
                    break;

                case LegendPosition.Right:
                    PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    PART_RootGrid.RowDefinitions.Add(new RowDefinition());

                    Grid.SetColumn(PART_Canvas, 0);
                    Grid.SetColumn(PART_Legend, 1);
                    PART_Legend.Orientation = Orientation.Vertical;
                    break;

                case LegendPosition.Top:
                    PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                    PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    Grid.SetRow(PART_Legend, 0);
                    Grid.SetRow(PART_Canvas, 1);
                    PART_Legend.Orientation = Orientation.Horizontal;
                    break;

                case LegendPosition.Bottom:
                    PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    Grid.SetRow(PART_Canvas, 0);
                    Grid.SetRow(PART_Legend, 1);
                    PART_Legend.Orientation = Orientation.Horizontal;
                    break;
            }
        }
        #endregion
    }
}
