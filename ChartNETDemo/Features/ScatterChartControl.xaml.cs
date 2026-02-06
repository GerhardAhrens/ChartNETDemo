namespace ChartNETDemo
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public class ScatterPoint : INotifyPropertyChanged
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Category { get; set; }
        public Brush Color { get; set; }

        private double _plotX;
        public double PlotX
        {
            get => this._plotX;
            set
            {
                if (this._plotX != value)
                {
                    this._plotX = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private double _plotY;
        public double PlotY
        {
            get => this._plotY;
            set
            {
                if (this._plotY != value)
                {
                    this._plotY = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class GridLine
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
    }

    public class LegendItem
    {
        public string Name { get; set; }
        public Brush Color { get; set; }
    }

    public class AxisTick
    {
        public double Position { get; set; }   // Pixelposition im Canvas
        public string Label { get; set; }      // Textwert
    }

    /// <summary>
    /// Interaktionslogik für ScatterChartControl.xaml
    /// </summary>
    public partial class ScatterChartControl : UserControl
    {
        private const double PADDING = 40;
        private const int GRIDLINECOUNT = 10;

        public ScatterChartControl()
        {
            this.InitializeComponent();
            this.Loaded += (_, __) => this.Recalculate();
            this.PlotArea.SizeChanged += (_, __) => this.Recalculate();
        }

        #region ItemsSource DP

        public ObservableCollection<ScatterPoint> ItemsSource
        {
            get => (ObservableCollection<ScatterPoint>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(ObservableCollection<ScatterPoint>),
                typeof(ScatterChartControl),
                new PropertyMetadata(null, OnItemsChanged));

        #endregion

        public bool IsLogarithmicX
        {
            get => (bool)GetValue(IsLogarithmicXProperty);
            set => SetValue(IsLogarithmicXProperty, value);
        }

        public static readonly DependencyProperty IsLogarithmicXProperty =
            DependencyProperty.Register(nameof(IsLogarithmicX),
                typeof(bool),
                typeof(ScatterChartControl),
                new PropertyMetadata(false, OnItemsChanged));


        public bool IsLogarithmicY
        {
            get => (bool)GetValue(IsLogarithmicYProperty);
            set => SetValue(IsLogarithmicYProperty, value);
        }

        public static readonly DependencyProperty IsLogarithmicYProperty =
            DependencyProperty.Register(nameof(IsLogarithmicY),
                typeof(bool),
                typeof(ScatterChartControl),
                new PropertyMetadata(false, OnItemsChanged));

        public ObservableCollection<LegendItem> Legend { get; } = new();
        public ObservableCollection<AxisTick> XTicks { get; } = new();
        public ObservableCollection<AxisTick> YTicks { get; } = new();
        public ObservableCollection<GridLine> GridLines { get; } = new();

        #region Create Chart
        private static double Log(double v) => Math.Log10(v);

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ScatterChartControl)d).Recalculate();
        }

        private void BuildLegend()
        {
            this.Legend.Clear();

            if (this.ItemsSource == null)
            {
                return;
            }

            foreach (var g in this.ItemsSource.GroupBy(p => p.Category))
            {
                this.Legend.Add(new LegendItem
                {
                    Name = g.Key,
                    Color = g.First().Color
                });
            }
        }

        private void BuildGrid(double width, double height)
        {
            this.GridLines.Clear();

            double plotWidth = width - PADDING * 2;
            double plotHeight = height - PADDING * 2;

            for (int i = 0; i <= GRIDLINECOUNT; i++)
            {
                double x = PADDING + i * plotWidth / GRIDLINECOUNT;
                double y = PADDING + i * plotHeight / GRIDLINECOUNT;

                this.GridLines.Add(new GridLine { X1 = x, Y1 = PADDING, X2 = x, Y2 = height - PADDING });
                this.GridLines.Add(new GridLine { X1 = PADDING, Y1 = y, X2 = width - PADDING, Y2 = y });
            }
        }

        private void Recalculate()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (this.ItemsSource == null || this.ItemsSource.Any() == false)
                {
                    return;
                }

                double width = 0;
                double height = 0;

                if (this.PlotArea.ActualWidth > 0 || this.PlotArea.ActualHeight > 0)
                {
                    width = this.PlotArea.ActualWidth;
                    height = this.PlotArea.ActualHeight;
                }
                else
                {
                    if (this.PlotArea.Width > 0 || this.PlotArea.Height > 0)
                    {
                        width = this.PlotArea.Width;
                        height = this.PlotArea.Height;
                    }
                    else
                    { 
                        return;
                    }
                }

                this.BuildGrid(width, height);
                this.BuildLegend();

                // -----------------------------
                // Datenbereich bestimmen
                // -----------------------------

                double minX = this.ItemsSource.Min(p => p.X);
                double maxX = this.ItemsSource.Max(p => p.X);
                double minY = this.ItemsSource.Min(p => p.Y);
                double maxY = this.ItemsSource.Max(p => p.Y);

                // Sicherheit (für Log & Linear)
                if (maxX - minX <= 0)
                {
                    maxX = minX + 1;
                }

                if (maxY - minY <= 0)
                {
                    maxY = minY + 1;
                }

                // Log braucht > 0
                if (this.IsLogarithmicX == true)
                {
                    minX = Math.Max(minX, 1e-6);
                }

                if (this.IsLogarithmicY == true)
                {
                    minY = Math.Max(minY, 1e-6);
                }

                double plotWidth = width - PADDING * 2;
                double plotHeight = height - PADDING * 2;

                /* Skalen vorbereiten */
                double xMinScale = this.IsLogarithmicX ? Math.Log10(minX) : minX;
                double xMaxScale = this.IsLogarithmicX ? Math.Log10(maxX) : maxX;

                double yMinScale = this.IsLogarithmicY ? Math.Log10(minY) : minY;
                double yMaxScale = this.IsLogarithmicY ? Math.Log10(maxY) : maxY;

                if (xMaxScale - xMinScale <= 0)
                {
                    xMaxScale = xMinScale + 1;
                }
                if (yMaxScale - yMinScale <= 0)
                {
                    yMaxScale = yMinScale + 1;
                }

                /* Punkte skalieren */
                foreach (var p in this.ItemsSource)
                {
                    double xValue = this.IsLogarithmicX ? Math.Log10(Math.Max(p.X, 1e-6)) : p.X;
                    double yValue = this.IsLogarithmicY ? Math.Log10(Math.Max(p.Y, 1e-6)) : p.Y;

                    double xNorm = (xValue - xMinScale) / (xMaxScale - xMinScale);
                    double yNorm = (yValue - yMinScale) / (yMaxScale - yMinScale);

                    p.PlotX = PADDING + xNorm * plotWidth;
                    p.PlotY = PADDING + (1 - yNorm) * plotHeight;
                }

                /* Achsenticks aktualisieren */
                this.BuildAxisTicks(minX, maxX, minY, maxY, width, height);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void BuildAxisTicks(double minX, double maxX, double minY, double maxY, double width, double height)
        {
            this.XTicks.Clear();
            this.YTicks.Clear();

            double plotWidth = width - PADDING * 2;
            double plotHeight = height - PADDING * 2;

            this.BuildLogOrLinearX(minX, maxX, plotWidth);
            this.BuildLogOrLinearY(minY, maxY, plotHeight);
        }

        private void BuildLinearXTicks(double min, double max, double width)
        {
            int ticks = 6;

            for (int i = 0; i < ticks; i++)
            {
                double t = (double)i / (ticks - 1);
                double value = min + t * (max - min);

                this.XTicks.Add(new AxisTick
                {
                    Position = PADDING + t * width,
                    Label = value.ToString("0.##", CultureInfo.CurrentCulture)
                });
            }
        }

        private void BuildLinearYTicks(double min, double max, double height)
        {
            int ticks = 6;

            for (int i = 0; i < ticks; i++)
            {
                double t = (double)i / (ticks - 1);
                double value = min + t * (max - min);

                this.YTicks.Add(new AxisTick
                {
                    Position = PADDING + (1 - t) * height,
                    Label = value.ToString("0.##", CultureInfo.CurrentCulture)
                });
            }
        }

        private void BuildLogOrLinearX(double min, double max, double plotWidth)
        {
            if (this.IsLogarithmicX == false)
            {
                this.BuildLinearXTicks(min, max, plotWidth);
                return;
            }

            double logMin = Math.Log10(min);
            double logMax = Math.Log10(max);

            int decadeStart = (int)Math.Floor(logMin);
            int decadeEnd = (int)Math.Ceiling(logMax);

            for (int decade = decadeStart; decade <= decadeEnd; decade++)
            {
                for (int i = 1; i <= 9; i++)
                {
                    double value = i * Math.Pow(10, decade);

                    if (value < min || value > max)
                    {
                        continue;
                    }

                    double norm = (Math.Log10(value) - logMin) / (logMax - logMin);

                    this.XTicks.Add(new AxisTick
                    {
                        Position = PADDING + norm * plotWidth,
                        Label = value.ToString("0.##", CultureInfo.CurrentCulture)
                    });
                }
            }
        }

        private void BuildLogOrLinearY(double min, double max, double plotHeight)
        {
            if (this.IsLogarithmicY == false)
            {
                this.BuildLinearYTicks(min, max, plotHeight);
                return;
            }

            double logMin = Math.Log10(min);
            double logMax = Math.Log10(max);

            int decadeStart = (int)Math.Floor(logMin);
            int decadeEnd = (int)Math.Ceiling(logMax);

            for (int decade = decadeStart; decade <= decadeEnd; decade++)
            {
                for (int i = 1; i <= 9; i++)
                {
                    double value = i * Math.Pow(10, decade);

                    if (value < min || value > max)
                    {
                        continue;
                    }

                    double norm = (Math.Log10(value) - logMin) / (logMax - logMin);

                    YTicks.Add(new AxisTick
                    {
                        Position = PADDING + (1 - norm) * plotHeight,
                        Label = value.ToString("0.##", CultureInfo.CurrentCulture)
                    });
                }
            }
        }

        #endregion Create Chart

        #region Export to PNG

        public void ExportToPng(string filePath, double dpi = 96)
        {
            if (ActualWidth == 0 || ActualHeight == 0)
            {
                return;
            }

            // Layout aktualisieren erzwingen
            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, ActualWidth, ActualHeight));
            UpdateLayout();

            var renderBitmap = new RenderTargetBitmap((int)(ActualWidth * dpi / 96.0)
                , (int)(ActualHeight * dpi / 96.0)
                , dpi, dpi, PixelFormats.Pbgra32);

            renderBitmap.Render(this);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using var stream = File.Create(filePath);
            encoder.Save(stream);
        }

        #endregion
    }
}
