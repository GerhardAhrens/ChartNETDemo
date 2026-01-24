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
            InitializeComponent();
            SizeChanged += (_, _) => Redraw();
        }

        #region DependencyProperty

        public IEnumerable<PieSegment> Segments
        {
            get => (IEnumerable<PieSegment>)GetValue(SegmentsProperty);
            set => SetValue(SegmentsProperty, value);
        }

        public static readonly DependencyProperty SegmentsProperty =
            DependencyProperty.Register(
                nameof(Segments),
                typeof(IEnumerable<PieSegment>),
                typeof(PieChartControl),
                new PropertyMetadata(null, (_, __) => ((PieChartControl)_).Redraw()));

        #endregion

        #region Rendering

        private void Redraw()
        {
            PART_Canvas.Children.Clear();

            if (Segments == null || !Segments.Any() || ActualWidth <= 0 || ActualHeight <= 0)
                return;

            double centerX = ActualWidth / 2 - 80; // Platz für Legende
            double centerY = ActualHeight / 2;
            double radius = Math.Min(centerX, centerY) - 10;

            double total = Segments.Sum(s => s.Value);
            if (total == 0) return;

            double startAngle = 0;

            foreach (var segment in Segments)
            {
                double sweepAngle = segment.Value / total * 360;

                // Segment
                var path = CreatePieSlice(centerX, centerY, radius, startAngle, sweepAngle, segment.Fill);
                PART_Canvas.Children.Add(path);

                // Label
                AddSegmentLabel(centerX, centerY, radius, startAngle, sweepAngle, segment, total);

                startAngle += sweepAngle;
            }

            DrawLegend(centerX * 2 + 20, 20, 100);
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

            PART_Canvas.Children.Add(text);
        }

        private void DrawLegend(double startX, double startY, double width)
        {
            const double boxSize = 16;
            const double spacing = 4;
            const double lineHeight = boxSize + spacing;

            int i = 0;
            foreach (var segment in Segments)
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
                PART_Canvas.Children.Add(rect);

                // Label
                var text = new TextBlock
                {
                    Text = $"{segment.Label} ({segment.Value})",
                    FontSize = 12
                };
                text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(text, startX + boxSize + spacing);
                Canvas.SetTop(text, startY + i * lineHeight + (boxSize - text.DesiredSize.Height) / 2);
                PART_Canvas.Children.Add(text);

                i++;
            }
        }

        #endregion

        #region Export
        /// <summary>
        /// Exportiert das Chart als PNG
        /// </summary>
        /// <param name="filePath"></param>
        public void ExportToPng(string filePath)
        {
            var size = new Size(ActualWidth, ActualHeight);

            Measure(size);
            Arrange(new Rect(size));
            UpdateLayout();

            var rtb = new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96, 96,
                PixelFormats.Pbgra32);

            rtb.Render(this);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using var fs = System.IO.File.Create(filePath);
            encoder.Save(fs);
        }
        #endregion Export
    }
}
