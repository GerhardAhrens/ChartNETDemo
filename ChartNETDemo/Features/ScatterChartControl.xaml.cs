namespace ChartNETDemo
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
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
            get => _plotX;
            set
            {
                if (_plotX != value)
                {
                    _plotX = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _plotY;
        public double PlotY
        {
            get => _plotY;
            set
            {
                if (_plotY != value)
                {
                    _plotY = value;
                    OnPropertyChanged();
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

    /// <summary>
    /// Interaktionslogik für ScatterChartControl.xaml
    /// </summary>
    public partial class ScatterChartControl : UserControl
    {
        private const double PADDING = 40;
        private const int GRIDLINECOUNT = 10;

        public ScatterChartControl()
        {
            InitializeComponent();
            this.Loaded += (_, __) => Recalculate();
            this.PlotArea.SizeChanged += (_, __) => Recalculate();
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

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ScatterChartControl)d).Recalculate();
        }

        #endregion

        #region Legend

        public ObservableCollection<LegendItem> Legend { get; } = new();

        private void BuildLegend()
        {
            Legend.Clear();

            if (ItemsSource == null) return;

            foreach (var g in ItemsSource.GroupBy(p => p.Category))
            {
                Legend.Add(new LegendItem
                {
                    Name = g.Key,
                    Color = g.First().Color
                });
            }
        }

        #endregion

        #region Gridlines

        public ObservableCollection<GridLine> GridLines { get; } = new();

        private void BuildGrid(double width, double height)
        {
            GridLines.Clear();

            double plotWidth = width - PADDING * 2;
            double plotHeight = height - PADDING * 2;

            for (int i = 0; i <= GRIDLINECOUNT; i++)
            {
                double x = PADDING + i * plotWidth / GRIDLINECOUNT;
                double y = PADDING + i * plotHeight / GRIDLINECOUNT;

                GridLines.Add(new GridLine { X1 = x, Y1 = PADDING, X2 = x, Y2 = height - PADDING });
                GridLines.Add(new GridLine { X1 = PADDING, Y1 = y, X2 = width - PADDING, Y2 = y });
            }
        }

        #endregion

        #region Scaling

        private void Recalculate()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (ItemsSource == null || !ItemsSource.Any())
                    return;

                double width = 0;
                double height = 0;

                if (PlotArea.ActualWidth > 0 || PlotArea.ActualHeight > 0)
                {
                    width = PlotArea.ActualWidth;
                    height = PlotArea.ActualHeight;
                }
                else
                {
                    if (PlotArea.Width > 0 || PlotArea.Height > 0)
                    {
                        width = PlotArea.Width;
                        height = PlotArea.Height;
                    }
                    else
                    { 
                        return;
                    }
                }


                BuildGrid(width, height);
                BuildLegend();

                double minX = ItemsSource.Min(p => p.X);
                double maxX = ItemsSource.Max(p => p.X);
                double minY = ItemsSource.Min(p => p.Y);
                double maxY = ItemsSource.Max(p => p.Y);

                if (Math.Abs(maxX - minX) < 0.00001) maxX = minX + 1;
                if (Math.Abs(maxY - minY) < 0.00001) maxY = minY + 1;

                double plotWidth = width - PADDING * 2;
                double plotHeight = height - PADDING * 2;

                foreach (var p in ItemsSource)
                {
                    bool po = ReferenceEquals(p, ItemsSource.First());
                    double xNorm = (p.X - minX) / (maxX - minX);
                    double yNorm = (p.Y - minY) / (maxY - minY);

                    p.PlotX = PADDING + xNorm * plotWidth;
                    p.PlotY = PADDING + (1 - yNorm) * plotHeight;
                }

            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        #endregion

        #region PNG Export

        public void SaveToPng(string filePath)
        {
            if (PlotArea.ActualWidth == 0 || PlotArea.ActualHeight == 0)
                return;

            var rtb = new RenderTargetBitmap(
                (int)PlotArea.ActualWidth,
                (int)PlotArea.ActualHeight,
                96, 96,
                PixelFormats.Pbgra32);

            rtb.Render(PlotArea);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using var fs = File.OpenWrite(filePath);
            encoder.Save(fs);
        }

        #endregion
    }
}
