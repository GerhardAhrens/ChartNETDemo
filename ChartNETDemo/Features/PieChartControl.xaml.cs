namespace ChartNETDemo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    public class PieSegment
    {
        public string Label { get; set; }
        public double Value { get; set; }
        public Brush Fill { get; set; } = Brushes.Blue;
    }

    /// <summary>
    /// Interaktionslogik für PieChartControl.xaml
    /// </summary>
    public partial class PieChartControl : UserControl
    {
        public PieChartControl()
        {
            this.InitializeComponent();
            this.SizeChanged += (_, _) => this.Redraw();
        }

        #region DependencyProperty

        public IEnumerable<PieSegment> ItemSource
        {
            get => (IEnumerable<PieSegment>)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }

        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
                typeof(IEnumerable<PieSegment>),
                typeof(PieChartControl),
                new PropertyMetadata(null, (_, __) => ((PieChartControl)_).Redraw()));

        #endregion

        #region Rendering

        private void Redraw()
        {
            PART_Canvas.Children.Clear();

            if (this.ItemSource == null || this.ItemSource.Any() == false || this.ActualWidth <= 0 || this.ActualHeight <= 0)
                return;

            double centerX = this.ActualWidth / 2 - 80; // Platz für Legende
            double centerY = this.ActualHeight / 2;
            double radius = Math.Min(centerX, centerY) - 10;

            double total = this.ItemSource.Sum(s => s.Value);
            if (total == 0) return;

            double startAngle = 0;

            foreach (var segment in this.ItemSource)
            {
                double sweepAngle = segment.Value / total * 360;

                // Segment
                var path = CreatePieSlice(centerX, centerY, radius, startAngle, sweepAngle, segment.Fill);
                this.PART_Canvas.Children.Add(path);

                // Label
                this.AddSegmentLabel(centerX, centerY, radius, startAngle, sweepAngle, segment, total);

                startAngle += sweepAngle;
            }

            this.DrawLegend(centerX * 2 + 20, 20, 100);
        }

        private static Path CreatePieSlice(double cx, double cy, double r, double startAngle, double sweepAngle, Brush fill)
        {
            bool isLargeArc = sweepAngle > 180;

            double startRadians = (Math.PI / 180) * startAngle;
            double endRadians = (Math.PI / 180) * (startAngle + sweepAngle);

            Point startPoint = new Point(
                cx + r * Math.Cos(startRadians),
                cy + r * Math.Sin(startRadians));

            Point endPoint = new Point(
                cx + r * Math.Cos(endRadians),
                cy + r * Math.Sin(endRadians));

            var segment = new PathFigure
            {
                StartPoint = new Point(cx, cy),
                IsClosed = true
            };

            segment.Segments.Add(new LineSegment(startPoint, true));
            segment.Segments.Add(new ArcSegment(
                endPoint,
                new Size(r, r),
                0,
                isLargeArc,
                SweepDirection.Clockwise,
                true));

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(segment);

            return new Path
            {
                Data = pathGeometry,
                Fill = fill,
                Stroke = Brushes.White,
                StrokeThickness = 1
            };
        }

        private void AddSegmentLabel(double cx, double cy, double r, double startAngle, double sweepAngle, PieSegment segment, double total)
        {
            double midAngle = startAngle + sweepAngle / 2;
            double radians = midAngle * Math.PI / 180;

            double labelRadius = r * 0.6;
            double x = cx + labelRadius * Math.Cos(radians);
            double y = cy + labelRadius * Math.Sin(radians);

            var text = new TextBlock
            {
                Text = $"{segment.Label} ({segment.Value / total * 100:0.#}%)",
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeights.Bold
            };

            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            Canvas.SetLeft(text, x - text.DesiredSize.Width / 2);
            Canvas.SetTop(text, y - text.DesiredSize.Height / 2);

            this.PART_Canvas.Children.Add(text);
        }

        private void DrawLegend(double startX, double startY, double width)
        {
            const double boxSize = 16;
            const double spacing = 4;
            const double lineHeight = boxSize + spacing;

            int i = 0;
            foreach (var segment in this.ItemSource)
            {
                // Farbkasten
                var rect = new Rectangle
                {
                    Width = boxSize,
                    Height = boxSize,
                    Fill = segment.Fill,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                Canvas.SetLeft(rect, startX);
                Canvas.SetTop(rect, startY + i * lineHeight);
                this.PART_Canvas.Children.Add(rect);

                // Label
                var text = new TextBlock
                {
                    Text = $"{segment.Label} ({segment.Value})",
                    FontSize = 12
                };
                text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(text, startX + boxSize + spacing);
                Canvas.SetTop(text, startY + i * lineHeight + (boxSize - text.DesiredSize.Height) / 2);
                this.PART_Canvas.Children.Add(text);

                i++;
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
