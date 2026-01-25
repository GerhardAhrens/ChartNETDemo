namespace ChartNETDemo
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    public class GanttTask
    {
        public string Title { get; set; } = "";
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Brush Fill { get; set; } = Brushes.SteelBlue;
    }

    /// <summary>
    /// Interaktionslogik für GanttChartControl.xaml
    /// </summary>
    public partial class GanttChartControl : UserControl
    {
        private const double LeftMargin = 120;
        private const double RightMargin = 20;
        private const double TopMargin = 20;
        private const double BottomMargin = 60;
        private const double RowHeight = 30;

        public GanttChartControl()
        {
            InitializeComponent();
            SizeChanged += (_, _) => Redraw();
        }

        #region DependencyProperty

        public IEnumerable<GanttTask> Tasks
        {
            get => (IEnumerable<GanttTask>)GetValue(TasksProperty);
            set => SetValue(TasksProperty, value);
        }

        public static readonly DependencyProperty TasksProperty =
            DependencyProperty.Register(
                nameof(Tasks),
                typeof(IEnumerable<GanttTask>),
                typeof(GanttChartControl),
                new PropertyMetadata(null, (_, __) => ((GanttChartControl)_).Redraw()));

        public IEnumerable<GanttDependency> Dependencies
        {
            get => (IEnumerable<GanttDependency>)GetValue(DependenciesProperty);
            set => SetValue(DependenciesProperty, value);
        }

        public static readonly DependencyProperty DependenciesProperty =
            DependencyProperty.Register(
                nameof(Dependencies),
                typeof(IEnumerable<GanttDependency>),
                typeof(GanttChartControl),
                new PropertyMetadata(null, (_, __) => ((GanttChartControl)_).Redraw()));

        #endregion

        #region Today Line

        public bool ShowTodayLine
        {
            get => (bool)GetValue(ShowTodayLineProperty);
            set => SetValue(ShowTodayLineProperty, value);
        }

        public static readonly DependencyProperty ShowTodayLineProperty =
            DependencyProperty.Register(
                nameof(ShowTodayLine),
                typeof(bool),
                typeof(GanttChartControl),
                new PropertyMetadata(true, (_, __) => ((GanttChartControl)_).Redraw()));

        public Brush TodayLineBrush
        {
            get => (Brush)GetValue(TodayLineBrushProperty);
            set => SetValue(TodayLineBrushProperty, value);
        }

        public static readonly DependencyProperty TodayLineBrushProperty =
            DependencyProperty.Register(
                nameof(TodayLineBrush),
                typeof(Brush),
                typeof(GanttChartControl),
                new PropertyMetadata(Brushes.Red, (_, __) => ((GanttChartControl)_).Redraw()));

        public double TodayLineThickness
        {
            get => (double)GetValue(TodayLineThicknessProperty);
            set => SetValue(TodayLineThicknessProperty, value);
        }

        public static readonly DependencyProperty TodayLineThicknessProperty =
            DependencyProperty.Register(
                nameof(TodayLineThickness),
                typeof(double),
                typeof(GanttChartControl),
                new PropertyMetadata(2.0, (_, __) => ((GanttChartControl)_).Redraw()));

        #endregion

        public class GanttDependency
        {
            public GanttTask From { get; set; } = null!;
            public GanttTask To { get; set; } = null!;
        }


        #region Rendering

        private void Redraw()
        {
            PART_Canvas.Children.Clear();

            if (Tasks == null || Tasks.Any() == false)
            {
                return;
            }

            this.DrawAxes();
            this.DrawYAxisLabels();
            this.DrawTasks();
            this.DrawDependencies();
            this.DrawTodayLine();
            this.DrawXAxisLabels();
        }

        /// <summary>
        /// Achsen zeichnen (x,y)
        /// </summary>
        private void DrawAxes()
        {
            double height = ActualHeight - BottomMargin;

            // Y-Achse
            PART_Canvas.Children.Add(new Line
            {
                X1 = LeftMargin,
                X2 = LeftMargin,
                Y1 = TopMargin,
                Y2 = height,
                Stroke = Brushes.Black
            });

            // X-Achse
            PART_Canvas.Children.Add(new Line
            {
                X1 = LeftMargin,
                X2 = ActualWidth - RightMargin,
                Y1 = height,
                Y2 = height,
                Stroke = Brushes.Black
            });
        }

        /// <summary>
        /// Y-Achse Beschriftungen zeichnen
        /// </summary>
        private void DrawYAxisLabels()
        {
            var taskList = Tasks.ToList();

            for (int i = 0; i < taskList.Count; i++)
            {
                var label = new TextBlock
                {
                    Text = taskList[i].Title,
                    FontSize = 12
                };

                Canvas.SetLeft(label, 10);
                Canvas.SetTop(label, TopMargin + i * RowHeight + 5);
                PART_Canvas.Children.Add(label);
            }
        }

        /// <summary>
        /// Einzelne Tasks zeichnen
        /// </summary>
        private void DrawTasks()
        {
            var taskList = Tasks.ToList();

            DateTime minDate = taskList.Min(t => t.Start);
            DateTime maxDate = taskList.Max(t => t.End);

            double totalDays = (maxDate - minDate).TotalDays;
            double plotWidth = ActualWidth - LeftMargin - RightMargin;

            for (int i = 0; i < taskList.Count; i++)
            {
                var task = taskList[i];

                double x =
                    LeftMargin +
                    (task.Start - minDate).TotalDays / totalDays * plotWidth;

                double width =
                    (task.End - task.Start).TotalDays / totalDays * plotWidth;

                double y = TopMargin + i * RowHeight + 5;

                var rect = new Rectangle
                {
                    Width = Math.Max(1, width),
                    Height = RowHeight - 10,
                    Fill = task.Fill,
                    RadiusX = 3,
                    RadiusY = 3
                };

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                PART_Canvas.Children.Add(rect);
            }
        }

        /// <summary>
        /// X-Achsenbeschriftungen zeichnen
        /// </summary>
        private void DrawXAxisLabels()
        {
            var taskList = Tasks.ToList();

            DateTime minDate = taskList.Min(t => t.Start);
            DateTime maxDate = taskList.Max(t => t.End);

            int tickCount = 5;
            double plotWidth = ActualWidth - LeftMargin - RightMargin;
            double y = ActualHeight - BottomMargin;

            for (int i = 0; i <= tickCount; i++)
            {
                DateTime date =
                    minDate.AddDays((maxDate - minDate).TotalDays * i / tickCount);

                double x = LeftMargin + plotWidth * i / tickCount;

                // Tick
                PART_Canvas.Children.Add(new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = y,
                    Y2 = y + 5,
                    Stroke = Brushes.Black
                });

                // Label
                var label = new TextBlock
                {
                    Text = date.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture),
                    FontSize = 11
                };

                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(label, x - label.DesiredSize.Width / 2);
                Canvas.SetTop(label, y + 8);

                PART_Canvas.Children.Add(label);
            }
        }

        private void DrawTodayLine()
        {
            if (!ShowTodayLine || Tasks == null || !Tasks.Any())
                return;

            var taskList = Tasks.ToList();

            DateTime minDate = taskList.Min(t => t.Start).Date;
            DateTime maxDate = taskList.Max(t => t.End).Date;
            DateTime today = DateTime.Today;

            // Nur anzeigen, wenn Heute im Bereich liegt
            if (today < minDate || today > maxDate)
                return;

            double plotWidth = ActualWidth - LeftMargin - RightMargin;
            double plotHeight = ActualHeight - BottomMargin;

            double totalDays = (maxDate - minDate).TotalDays;
            if (totalDays <= 0)
                return;

            double x =
                LeftMargin +
                (today - minDate).TotalDays / totalDays * plotWidth;

            var line = new Line
            {
                X1 = x,
                X2 = x,
                Y1 = TopMargin,
                Y2 = plotHeight,
                Stroke = TodayLineBrush,
                StrokeThickness = TodayLineThickness,
                StrokeDashArray = new DoubleCollection { 4, 4 } // gestrichelt
            };

            PART_Canvas.Children.Add(line);
        }

        private Rect GetTaskRect(GanttTask task, int index, DateTime minDate, DateTime maxDate)
        {
            double plotWidth = ActualWidth - LeftMargin - RightMargin;
            double totalDays = (maxDate - minDate).TotalDays;

            double x =
                LeftMargin +
                (task.Start - minDate).TotalDays / totalDays * plotWidth;

            double width =
                (task.End - task.Start).TotalDays / totalDays * plotWidth;

            double y = TopMargin + index * RowHeight + 5;

            return new Rect(x, y, Math.Max(1, width), RowHeight - 10);
        }

        private void DrawDependencies()
        {
            if (Dependencies == null || !Dependencies.Any())
                return;

            var taskList = Tasks.ToList();
            DateTime minDate = taskList.Min(t => t.Start);
            DateTime maxDate = taskList.Max(t => t.End);

            foreach (var dep in Dependencies)
            {
                int fromIndex = taskList.IndexOf(dep.From);
                int toIndex = taskList.IndexOf(dep.To);

                if (fromIndex < 0 || toIndex < 0)
                    continue;

                Rect fromRect = GetTaskRect(dep.From, fromIndex, minDate, maxDate);
                Rect toRect = GetTaskRect(dep.To, toIndex, minDate, maxDate);

                // Punkte
                Point start = new(fromRect.Right, fromRect.Top + fromRect.Height / 2);
                Point mid1 = new(start.X + 10, start.Y);
                Point mid2 = new(mid1.X, toRect.Top + toRect.Height / 2);
                Point end = new(toRect.Left, toRect.Top + toRect.Height / 2);

                // Linie (3 Segmente)
                DrawLine(start, mid1);
                DrawLine(mid1, mid2);
                DrawLine(mid2, end);

                // Pfeilspitze
                DrawArrowHead(end);
            }
        }

        private void DrawLine(Point p1, Point p2)
        {
            PART_Canvas.Children.Add(new Line
            {
                X1 = p1.X,
                Y1 = p1.Y,
                X2 = p2.X,
                Y2 = p2.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            });
        }

        private void DrawArrowHead(Point end)
        {
            const double size = 6;

            var arrow = new Polygon
            {
                Fill = Brushes.Black,
                Points = new PointCollection
        {
            end,
            new Point(end.X - size, end.Y - size / 2),
            new Point(end.X - size, end.Y + size / 2)
        }
            };

            PART_Canvas.Children.Add(arrow);
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

            using FileStream fs = new FileStream(filePath, FileMode.Create);
            encoder.Save(fs);
        }
        #endregion
    }
}
