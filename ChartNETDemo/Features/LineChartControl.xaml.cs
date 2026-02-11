namespace ChartNETDemo
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    [DebuggerDisplay("Titel: {this.Title}; Anzahl: {this.Values.Count}")]
    public class ChartLine : INotifyPropertyChanged
    {
        private string _Title;

        public string Title
        {
            get { return this._Title; }
            set
            {
                if (this._Title != value)
                {
                    this._Title = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private IList<ChartPoint> _Values = new List<ChartPoint>();

        public IList<ChartPoint> Values
        {
            get { return this._Values; }
            set
            {
                if (this._Values != value)
                {
                    this._Values = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Brush _Stroke = Brushes.Blue;

        public Brush Stroke
        {
            get { return this._Stroke; }
            set
            {
                if (this._Stroke != value)
                {
                    this._Stroke = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private double _StrokeThickness = 2;

        public double StrokeThickness
        {
            get { return this._StrokeThickness; }
            set
            {
                if (this._StrokeThickness != value)
                {
                    this._StrokeThickness = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    [DebuggerDisplay("Category: {this.Category}; Value: {this.Value}")]
    public class ChartPoint : INotifyPropertyChanged
    {
        /// <summary>
        /// Category; X-Achse, Legende
        /// </summary>
        private string _Category;

        public string Category
        {
            get { return this._Category; }
            set
            {
                if (this._Category != value)
                {
                    this._Category = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Werte, Y-Achse, Legende
        /// </summary>
        private double _Value;

        public double Value
        {
            get { return this._Value; }
            set
            {
                if (this._Value != value)
                {
                    this._Value = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// z.B. "2021"
        /// </summary>
        private string _X;

        public string X
        {
            get { return this._X; }
            set
            {
                if (this._X != value)
                {
                    this._X = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// z.B. 30
        /// </summary>
        private double _Y;

        public double Y
        {
            get { return this._Y; }
            set
            {
                if (this._Y != value)
                {
                    this._Y = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private double _PosX;

        public double PosX
        {
            get { return this._PosX; }
            set
            {
                if (this._PosX != value)
                {
                    this._PosX = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private double _PosY;

        public double PosY
        {
            get { return this._PosY; }
            set
            {
                if (this._PosY != value)
                {
                    this._PosY = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
        private const double LEFTMARGIN = 30;
        private const double BOTTOMMARGIN = 35;
        private const double TOPMARGIN = 10;
        private const double RIGHTMARGIN = 10;
        private const double POINTRADIUS = 6;
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

            double plotWidth = width - LEFTMARGIN - RIGHTMARGIN;
            double plotHeight = height - TOPMARGIN - BOTTOMMARGIN;

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
                AxisTitleAlignment.Start => LEFTMARGIN,
                AxisTitleAlignment.End => LEFTMARGIN + plotWidth - text.DesiredSize.Width,
                _ => LEFTMARGIN + plotWidth / 2 - text.DesiredSize.Width / 2
            };

            Canvas.SetLeft(text, x);
            Canvas.SetTop(text, TOPMARGIN + plotHeight + BOTTOMMARGIN - text.DesiredSize.Height);

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
                AxisTitleAlignment.Start => TOPMARGIN + plotHeight - text.DesiredSize.Width,
                AxisTitleAlignment.End => TOPMARGIN,
                _ => TOPMARGIN + plotHeight / 2 - text.DesiredSize.Width / 2
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
                X1 = LEFTMARGIN,
                X2 = LEFTMARGIN,
                Y1 = TOPMARGIN,
                Y2 = TOPMARGIN + plotHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            });

            // X-Achse
            this.ChartCanvas.Children.Add(new Line
            {
                X1 = LEFTMARGIN,
                X2 = LEFTMARGIN + plotWidth,
                Y1 = TOPMARGIN + plotHeight,
                Y2 = TOPMARGIN + plotHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            });
        }

        private void DrawGridLines(double plotWidth, double plotHeight)
        {
            double left = LEFTMARGIN;
            double top = TOPMARGIN;

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
                double y = TOPMARGIN + i * plotHeight / HorizontalGridLineCount;

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
                double x = LEFTMARGIN + i * step;

                var tb = new TextBlock
                {
                    Text = referenceLine.Values[i].Category,
                    FontSize = 11,
                    TextAlignment = TextAlignment.Center
                };

                Canvas.SetLeft(tb, x - 20);
                Canvas.SetTop(tb, TOPMARGIN + plotHeight + 5);

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
                    double x = LEFTMARGIN + i * plotWidth / (line.Values.Count - 1);
                    double y = TOPMARGIN + plotHeight - (line.Values[i].Value - min) / (max - min) * plotHeight;

                    ChartPoint cp = new ChartPoint()
                    {
                        Category = line.Values[i].Category,
                        Value = line.Values[i].Value,
                        PosX = x,
                        PosY = y
                    };

                    var dataPoint = new Ellipse
                    {
                        Width = POINTRADIUS * 2,
                        Height = POINTRADIUS * 2,
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
                    Canvas.SetLeft(dp, p.PosX - POINTRADIUS);
                    Canvas.SetTop(dp, p.PosY - POINTRADIUS);
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