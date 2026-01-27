namespace ChartNETDemo
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

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
        private const double LeftMargin = 80;
        private const double RightMargin = 20;
        private const double TopMargin = 20;
        private const double BottomMargin = 60;
        private const double LegendWidth = 20;
        private const double LegendMargin = 10;


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

        #region Rendering

        private void Redraw()
        {
            PART_Canvas.Children.Clear();

            if (ItemSource == null || !ItemSource.Any())
                return;

            DrawHeatmap();
            DrawAxes();
            DrawAxisLabels();

            if (ShowColorLegend)
                DrawColorLegend();
        }

        private void DrawHeatmap()
        {
            var list = ItemSource.ToList();

            var xLabels = list.Select(c => c.X).Distinct().ToList();
            var yLabels = list.Select(c => c.Y).Distinct().ToList();

            double plotWidth = ActualWidth - LeftMargin - RightMargin;
            double plotHeight = ActualHeight - TopMargin - BottomMargin;

            if (plotWidth <= 0 || plotHeight <= 0)
                return;

            double cellWidth = plotWidth / xLabels.Count;
            double cellHeight = plotHeight / yLabels.Count;

            double min = list.Min(c => c.Value);
            double max = list.Max(c => c.Value);
            if (Math.Abs(max - min) < 0.0001)
                max = min + 1;

            foreach (var cell in list)
            {
                int xIndex = xLabels.IndexOf(cell.X);
                int yIndex = yLabels.IndexOf(cell.Y);

                double t = (cell.Value - min) / (max - min);
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

                Canvas.SetLeft(rect, LeftMargin + xIndex * cellWidth);
                Canvas.SetTop(rect, TopMargin + (yLabels.Count - 1 - yIndex) * cellHeight);
                PART_Canvas.Children.Add(rect);
            }
        }

        private void DrawAxes()
        {
            double plotHeight = ActualHeight - BottomMargin;

            // Y-Achse
            PART_Canvas.Children.Add(new Line
            {
                X1 = LeftMargin,
                X2 = LeftMargin,
                Y1 = TopMargin,
                Y2 = plotHeight,
                Stroke = Brushes.Black
            });

            // X-Achse
            PART_Canvas.Children.Add(new Line
            {
                X1 = LeftMargin,
                X2 = ActualWidth - RightMargin,
                Y1 = plotHeight,
                Y2 = plotHeight,
                Stroke = Brushes.Black
            });
        }

        private void DrawAxisLabels()
        {
            var list = ItemSource.ToList();
            var xLabels = list.Select(c => c.X).Distinct().ToList();
            var yLabels = list.Select(c => c.Y).Distinct().ToList();

            double plotWidth = ActualWidth - LeftMargin - RightMargin;
            double plotHeight = ActualHeight - TopMargin - BottomMargin;

            double cellWidth = plotWidth / xLabels.Count;
            double cellHeight = plotHeight / yLabels.Count;

            // X Labels
            for (int i = 0; i < xLabels.Count; i++)
            {
                var tb = new TextBlock { Text = xLabels[i], FontSize = 12 };
                tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Canvas.SetLeft(tb,
                    LeftMargin + i * cellWidth + cellWidth / 2 - tb.DesiredSize.Width / 2);

                Canvas.SetTop(tb, plotHeight + 5);
                PART_Canvas.Children.Add(tb);
            }

            // Y Labels
            for (int i = 0; i < yLabels.Count; i++)
            {
                var tb = new TextBlock { Text = yLabels[i], FontSize = 12 };
                tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Canvas.SetLeft(tb,
                    LeftMargin - tb.DesiredSize.Width - 10);

                Canvas.SetTop(tb,
                    TopMargin + (yLabels.Count - 1 - i) * cellHeight + cellHeight / 2 - tb.DesiredSize.Height / 2);

                PART_Canvas.Children.Add(tb);
            }
        }

        private void DrawColorLegend()
        {
            var list = ItemSource.ToList();
            if (!list.Any())
                return;

            double min = list.Min(c => c.Value);
            double max = list.Max(c => c.Value);
            if (Math.Abs(max - min) < 0.0001)
                max = min + 1;

            double plotHeight = ActualHeight - TopMargin - BottomMargin;
            if (plotHeight <= 0)
                return;

            double x = ActualWidth - RightMargin + LegendMargin;
            double y = TopMargin;

            int steps = 100;
            double stepHeight = plotHeight / steps;

            for (int i = 0; i < steps; i++)
            {
                double t = 1.0 - i / (double)(steps - 1);
                Brush brush = InterpolateBrush(MinValueColor, MaxValueColor, t);

                var rect = new Rectangle
                {
                    Width = LegendWidth,
                    Height = stepHeight + 1,
                    Fill = brush,
                    StrokeThickness = 0
                };

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y + i * stepHeight);
                PART_Canvas.Children.Add(rect);
            }

            // Max label
            var maxLabel = new TextBlock
            {
                Text = max.ToString("0.##"),
                FontSize = 11
            };
            maxLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(maxLabel, x + LegendWidth + 5);
            Canvas.SetTop(maxLabel, y - maxLabel.DesiredSize.Height / 2);
            PART_Canvas.Children.Add(maxLabel);

            // Min label
            var minLabel = new TextBlock
            {
                Text = min.ToString("0.##"),
                FontSize = 11
            };
            minLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(minLabel, x + LegendWidth + 5);
            Canvas.SetTop(minLabel, y + plotHeight - minLabel.DesiredSize.Height / 2);
            PART_Canvas.Children.Add(minLabel);
        }

        #endregion

        #region Helpers

        private static Brush InterpolateBrush(Brush min, Brush max, double t)
        {
            if (min is not SolidColorBrush a || max is not SolidColorBrush b)
                return min;

            byte r = (byte)(a.Color.R + (b.Color.R - a.Color.R) * t);
            byte g = (byte)(a.Color.G + (b.Color.G - a.Color.G) * t);
            byte bl = (byte)(a.Color.B + (b.Color.B - a.Color.B) * t);

            return new SolidColorBrush(Color.FromRgb(r, g, bl));
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