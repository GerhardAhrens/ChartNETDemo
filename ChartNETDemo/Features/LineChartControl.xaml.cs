namespace ChartNETDemo
{
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    public class ChartLine
    {
        public string Title { get; set; }
        public IList<ChartPoint> Values { get; set; } = new List<ChartPoint>();
        public Brush Stroke { get; set; } = Brushes.Blue;
        public double StrokeThickness { get; set; } = 2;
    }

    public class ChartPoint
    {
        public string Category { get; set; }   // X-Achse
        public double Value { get; set; }       // Y-Achse

        public string X { get; set; }      // z.B. "2021"
        public double Y { get; set; }       // z.B. 30
        public double PosX { get; set; }
        public double PosY { get; set; }
    }

    public class LineSeries
    {
        public string Title { get; set; }
        public Brush Stroke { get; set; } = Brushes.Blue;
        public double StrokeThickness { get; set; } = 2;
        public IList<ChartPoint> Values { get; set; } = new List<ChartPoint>();
    }

    public enum AxisTitleAlignment
    {
        Start,
        Center,
        End
    }

    /// <summary>
    /// Interaktionslogik für LineChartUC.xaml
    /// </summary>
    public partial class LineChartControl : UserControl
    {
        // Plot-Ränder
        private const double LeftMargin = 30;
        private const double BottomMargin = 35;
        private const double TopMargin = 10;
        private const double RightMargin = 10;
        private const double PointRadius = 6;
        private readonly List<Ellipse> _dataPoints = new();

        public LineChartControl()
        {
            this.InitializeComponent();
            SizeChanged += (_, _) => this.Redraw();
        }

        #region Dependency Properties

        public ObservableCollection<ChartLine> ItemSource
        {
            get => (ObservableCollection<ChartLine>)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }

        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
                typeof(ObservableCollection<ChartLine>),
                typeof(LineChartControl),
                new PropertyMetadata(null, OnChartPropertyChanged));

        public int HorizontalGridLineCount
        {
            get => (int)GetValue(HorizontalGridLineCountProperty);
            set => SetValue(HorizontalGridLineCountProperty, value);
        }

        public static readonly DependencyProperty HorizontalGridLineCountProperty =
            DependencyProperty.Register(
                nameof(HorizontalGridLineCount),
                typeof(int),
                typeof(LineChartControl),
                new PropertyMetadata(5, OnChartPropertyChanged));

        public int VerticalGridLineCount
        {
            get => (int)GetValue(VerticalGridLineCountProperty);
            set => SetValue(VerticalGridLineCountProperty, value);
        }

        public static readonly DependencyProperty VerticalGridLineCountProperty =
            DependencyProperty.Register(
                nameof(VerticalGridLineCount),
                typeof(int),
                typeof(LineChartControl),
                new PropertyMetadata(5, OnChartPropertyChanged));

        public Brush GridLineBrush
        {
            get => (Brush)GetValue(GridLineBrushProperty);
            set => SetValue(GridLineBrushProperty, value);
        }

        public static readonly DependencyProperty GridLineBrushProperty =
            DependencyProperty.Register(
                nameof(GridLineBrush),
                typeof(Brush),
                typeof(LineChartControl),
                new PropertyMetadata(Brushes.LightGray, OnChartPropertyChanged));

        public double GridLineThickness
        {
            get => (double)GetValue(GridLineThicknessProperty);
            set => SetValue(GridLineThicknessProperty, value);
        }

        public static readonly DependencyProperty GridLineThicknessProperty =
            DependencyProperty.Register(
                nameof(GridLineThickness),
                typeof(double),
                typeof(LineChartControl),
                new PropertyMetadata(1.0, OnChartPropertyChanged));

        private static void OnChartPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            ((LineChartControl)d).Redraw();
        }


        public string XAxisTitle
        {
            get => (string)GetValue(XAxisTitleProperty);
            set => SetValue(XAxisTitleProperty, value);
        }

        public static readonly DependencyProperty XAxisTitleProperty =
            DependencyProperty.Register(
                nameof(XAxisTitle),
                typeof(string),
                typeof(LineChartControl),
                new PropertyMetadata(string.Empty, OnChartPropertyChanged));

        public Brush XAxisTitleBrush
        {
            get => (Brush)GetValue(XAxisTitleBrushProperty);
            set => SetValue(XAxisTitleBrushProperty, value);
        }

        public static readonly DependencyProperty XAxisTitleBrushProperty =
            DependencyProperty.Register(
                nameof(XAxisTitleBrush),
                typeof(Brush),
                typeof(LineChartControl),
                new PropertyMetadata(Brushes.Black, OnChartPropertyChanged));

        public double XAxisTitleFontSize
        {
            get => (double)GetValue(XAxisTitleFontSizeProperty);
            set => SetValue(XAxisTitleFontSizeProperty, value);
        }

        public static readonly DependencyProperty XAxisTitleFontSizeProperty =
            DependencyProperty.Register(
                nameof(XAxisTitleFontSize),
                typeof(double),
                typeof(LineChartControl),
                new PropertyMetadata(13.0, OnChartPropertyChanged));

        public AxisTitleAlignment XAxisTitleAlignment
        {
            get => (AxisTitleAlignment)GetValue(XAxisTitleAlignmentProperty);
            set => SetValue(XAxisTitleAlignmentProperty, value);
        }

        public static readonly DependencyProperty XAxisTitleAlignmentProperty =
            DependencyProperty.Register(
                nameof(XAxisTitleAlignment),
                typeof(AxisTitleAlignment),
                typeof(LineChartControl),
                new PropertyMetadata(AxisTitleAlignment.Center, OnChartPropertyChanged));


        public string YAxisTitle
        {
            get => (string)GetValue(YAxisTitleProperty);
            set => SetValue(YAxisTitleProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleProperty =
            DependencyProperty.Register(
                nameof(YAxisTitle),
                typeof(string),
                typeof(LineChartControl),
                new PropertyMetadata(string.Empty, OnChartPropertyChanged));

        public Brush YAxisTitleBrush
        {
            get => (Brush)GetValue(YAxisTitleBrushProperty);
            set => SetValue(YAxisTitleBrushProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleBrushProperty =
            DependencyProperty.Register(
                nameof(YAxisTitleBrush),
                typeof(Brush),
                typeof(LineChartControl),
                new PropertyMetadata(Brushes.Black, OnChartPropertyChanged));

        public double YAxisTitleFontSize
        {
            get => (double)GetValue(YAxisTitleFontSizeProperty);
            set => SetValue(YAxisTitleFontSizeProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleFontSizeProperty =
            DependencyProperty.Register(
                nameof(YAxisTitleFontSize),
                typeof(double),
                typeof(LineChartControl),
                new PropertyMetadata(13.0, OnChartPropertyChanged));

        public AxisTitleAlignment YAxisTitleAlignment
        {
            get => (AxisTitleAlignment)GetValue(YAxisTitleAlignmentProperty);
            set => SetValue(YAxisTitleAlignmentProperty, value);
        }

        public static readonly DependencyProperty YAxisTitleAlignmentProperty =
            DependencyProperty.Register(
                nameof(YAxisTitleAlignment),
                typeof(AxisTitleAlignment),
                typeof(LineChartControl),
                new PropertyMetadata(AxisTitleAlignment.Center, OnChartPropertyChanged));

        #endregion

        private void ChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Redraw();
        }

        private void Redraw()
        {
            this.ChartCanvas.Children.Clear();

            if (this.ItemSource == null || this.ItemSource.Count == 0)
            {
                return;
            }

            double width = this.ChartCanvas.ActualWidth;
            double height = this.ChartCanvas.ActualHeight;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            double plotWidth = width - LeftMargin - RightMargin;
            double plotHeight = height - TopMargin - BottomMargin;

            if (plotWidth <= 0 || plotHeight <= 0)
            {
                return;
            }

            this.DrawGridLines(plotWidth, plotHeight);
            this.DrawAxes(plotWidth, plotHeight);
            this.DrawYAxisLabels(plotHeight);
            this.DrawXAxisLabels(plotWidth, plotHeight);
            this.DrawAxisTitles(plotWidth, plotHeight);
            this.DrawLines(plotWidth, plotHeight);
        }

        #region Drawing

        private void DrawAxisTitles(double plotWidth, double plotHeight)
        {
            this.DrawXAxisTitle(plotWidth, plotHeight);
            this.DrawYAxisTitle(plotHeight);
        }

        private void DrawXAxisTitle(double plotWidth, double plotHeight)
        {
            if (string.IsNullOrWhiteSpace(this.XAxisTitle))
            {
                return;
            }

            var text = new TextBlock
            {
                Text = this.XAxisTitle,
                Foreground = this.XAxisTitleBrush,
                FontSize = this.XAxisTitleFontSize,
                FontWeight = FontWeights.SemiBold
            };

            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            double x = XAxisTitleAlignment switch
            {
                AxisTitleAlignment.Start => LeftMargin,
                AxisTitleAlignment.End => LeftMargin + plotWidth - text.DesiredSize.Width,
                _ => LeftMargin + plotWidth / 2 - text.DesiredSize.Width / 2
            };

            Canvas.SetLeft(text, x);
            Canvas.SetTop(text, TopMargin + plotHeight + BottomMargin - text.DesiredSize.Height);

            this.ChartCanvas.Children.Add(text);
        }

        private void DrawYAxisTitle(double plotHeight)
        {
            if (string.IsNullOrWhiteSpace(this.YAxisTitle))
            {
                return;
            }

            var text = new TextBlock
            {
                Text = this.YAxisTitle,
                Foreground = YAxisTitleBrush,
                FontSize = YAxisTitleFontSize,
                FontWeight = FontWeights.SemiBold,
                LayoutTransform = new RotateTransform(-90)
            };

            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            double y = YAxisTitleAlignment switch
            {
                AxisTitleAlignment.Start => TopMargin + plotHeight - text.DesiredSize.Width,
                AxisTitleAlignment.End => TopMargin,
                _ => TopMargin + plotHeight / 2 - text.DesiredSize.Width / 2
            };

            Canvas.SetLeft(text, -20); // links außerhalb
            Canvas.SetTop(text, y);

            this.ChartCanvas.Children.Add(text);
        }

        private void DrawAxes(double plotWidth, double plotHeight)
        {
            // Y-Achse
            this.ChartCanvas.Children.Add(new Line
            {
                X1 = LeftMargin,
                X2 = LeftMargin,
                Y1 = TopMargin,
                Y2 = TopMargin + plotHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            });

            // X-Achse
            this.ChartCanvas.Children.Add(new Line
            {
                X1 = LeftMargin,
                X2 = LeftMargin + plotWidth,
                Y1 = TopMargin + plotHeight,
                Y2 = TopMargin + plotHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            });
        }

        private void DrawGridLines(double plotWidth, double plotHeight)
        {
            double left = LeftMargin;
            double top = TopMargin;

            // Horizontal
            for (int i = 0; i <= HorizontalGridLineCount; i++)
            {
                double y = top + i * plotHeight / HorizontalGridLineCount;

                this.ChartCanvas.Children.Add(new Line
                {
                    X1 = left,
                    X2 = left + plotWidth,
                    Y1 = y,
                    Y2 = y,
                    Stroke = GridLineBrush,
                    StrokeThickness = GridLineThickness
                });
            }

            /* Vertikal (kategorial, gleichmäßig) */
            for (int i = 0; i <= VerticalGridLineCount; i++)
            {
                double x = left + i * plotWidth / VerticalGridLineCount;

                this.ChartCanvas.Children.Add(new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = top,
                    Y2 = top + plotHeight,
                    Stroke = GridLineBrush,
                    StrokeThickness = GridLineThickness
                });
            }
        }

        private void DrawYAxisLabels(double plotHeight)
        {
            double min = ItemSource.SelectMany(l => l.Values).Min(p => p.Value);
            double max = ItemSource.SelectMany(l => l.Values).Max(p => p.Value);

            for (int i = 0; i <= HorizontalGridLineCount; i++)
            {
                double value = max - i * (max - min) / HorizontalGridLineCount;
                double y = TopMargin + i * plotHeight / HorizontalGridLineCount;

                var tb = new TextBlock
                {
                    Text = value.ToString("0.##", CultureInfo.CurrentCulture),
                    FontSize = 11
                };

                Canvas.SetLeft(tb, 5);
                Canvas.SetTop(tb, y - 8);

                this.ChartCanvas.Children.Add(tb);
            }
        }

        private void DrawXAxisLabels(double plotWidth, double plotHeight)
        {
            var referenceLine = ItemSource.FirstOrDefault();
            if (referenceLine == null || referenceLine.Values.Count < 2)
            {
                return;
            }

            int count = referenceLine.Values.Count;
            double step = plotWidth / (count - 1);

            for (int i = 0; i < count; i++)
            {
                double x = LeftMargin + i * step;

                var tb = new TextBlock
                {
                    Text = referenceLine.Values[i].Category,
                    FontSize = 11,
                    TextAlignment = TextAlignment.Center
                };

                Canvas.SetLeft(tb, x - 20);
                Canvas.SetTop(tb, TopMargin + plotHeight + 5);

                this.ChartCanvas.Children.Add(tb);
            }
        }

        private void DrawLines(double plotWidth, double plotHeight)
        {
            double min = this.ItemSource.SelectMany(l => l.Values).Min(p => p.Value);
            double max = this.ItemSource.SelectMany(l => l.Values).Max(p => p.Value);

            if (Math.Abs(max - min) < double.Epsilon)
            {
                return;
            }

            foreach (var line in this.ItemSource)
            {
                if (line.Values.Count < 2)
                {
                    continue;
                }

                var polyline = new Polyline
                {
                    Stroke = line.Stroke,
                    StrokeThickness = line.StrokeThickness
                };

                for (int i = 0; i < line.Values.Count; i++)
                {
                    double x = LeftMargin + i * plotWidth / (line.Values.Count - 1);
                    double y = TopMargin + plotHeight - (line.Values[i].Value - min) / (max - min) * plotHeight;

                    ChartPoint cp = new ChartPoint()
                    {
                        Category = line.Values[i].Category,
                        Value = line.Values[i].Value,
                        PosX = x,
                        PosY = y
                    };

                    var dataPoint = new Ellipse
                    {
                        Width = PointRadius * 2,
                        Height = PointRadius * 2,
                        Fill = Brushes.DarkGreen,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Tag = cp,
                    };

                    polyline.Points.Add(new Point(x, y));
                    _dataPoints.Add(dataPoint);
                    this.ChartCanvas.Children.Add(dataPoint);
                }

                this.ChartCanvas.Children.Add(polyline);
            }

            this.UpdateLayoutPositions();
        }

        private void UpdateLayoutPositions()
        {
            var linePoint = this.ChartCanvas.Children.OfType<Polyline>().SelectMany(pl => pl.Points).ToList();
            if (linePoint != null)
            {
                for (int i = 0; i < linePoint.Count; i++)
                {
                    Ellipse dp = _dataPoints[i];
                    ChartPoint p = (ChartPoint)_dataPoints[i].Tag;
                    Canvas.SetLeft(dp, p.PosX - PointRadius);
                    Canvas.SetTop(dp, p.PosY - PointRadius);
                    //dp.ToolTip = $"Kategorie: {p.Category}\nWert: {p.Value}";
                    ToolTipService.SetInitialShowDelay(dp, 100);
                    ToolTipService.SetShowDuration(dp, 1500);
                    ToolTipService.SetToolTip(dp, $"Kategorie: {p.Category}\nWert: {p.Value}");
                }
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
            Size size = new Size(this.ActualWidth, this.ActualHeight);
            this.Measure(size);
            this.Arrange(new Rect(size));
            this.UpdateLayout();

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