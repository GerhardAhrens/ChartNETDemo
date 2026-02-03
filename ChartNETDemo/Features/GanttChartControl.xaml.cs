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

    public sealed class GanttTask
    {
        public string Title { get; set; } = "";
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Brush Fill { get; set; } = Brushes.SteelBlue;
    }

    public sealed class GanttDependency
    {
        public GanttTask From { get; set; } = null!;
        public GanttTask To { get; set; } = null!;
    }

    /// <summary>
    /// Interaktionslogik für GanttChartControl.xaml
    /// </summary>
    public partial class GanttChartControl : UserControl
    {
        private const double LEFTMARGIN = 120;
        private const double RIGHTMARGIN = 20;
        private const double TOPMARGIN = 20;
        private const double BOTTOMMARGIN = 60;
        private const double ROWHEIGHT = 30;

        public GanttChartControl()
        {
            this.InitializeComponent();
            this.SizeChanged += (_, _) => this.Redraw();
        }

        #region DependencyProperty

        public IEnumerable<GanttTask> ItemSource
        {
            get => (IEnumerable<GanttTask>)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }

        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
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

        #region Rendering

        private void Redraw()
        {
            PART_Canvas.Children.Clear();

            if (this.ItemSource == null || this.ItemSource.Any() == false)
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
            double height = ActualHeight - BOTTOMMARGIN;

            // Y-Achse
            this.PART_Canvas.Children.Add(new Line
            {
                X1 = LEFTMARGIN,
                X2 = LEFTMARGIN,
                Y1 = TOPMARGIN,
                Y2 = height,
                Stroke = Brushes.Black
            });

            // X-Achse
            this.PART_Canvas.Children.Add(new Line
            {
                X1 = LEFTMARGIN,
                X2 = ActualWidth - RIGHTMARGIN,
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
            var taskList = this.ItemSource.ToList();

            for (int i = 0; i < taskList.Count; i++)
            {
                var label = new TextBlock
                {
                    Text = taskList[i].Title,
                    FontSize = 12
                };

                Canvas.SetLeft(label, 10);
                Canvas.SetTop(label, TOPMARGIN + i * ROWHEIGHT + 5);
                this.PART_Canvas.Children.Add(label);
            }
        }

        /// <summary>
        /// Einzelne Tasks zeichnen
        /// </summary>
        private void DrawTasks()
        {
            var taskList = ItemSource.ToList();

            DateTime minDate = taskList.Min(t => t.Start);
            DateTime maxDate = taskList.Max(t => t.End);

            double totalDays = (maxDate - minDate).TotalDays;
            double plotWidth = ActualWidth - LEFTMARGIN - RIGHTMARGIN;

            for (int i = 0; i < taskList.Count; i++)
            {
                var task = taskList[i];

                double x = LEFTMARGIN + (task.Start - minDate).TotalDays / totalDays * plotWidth;

                double width = (task.End - task.Start).TotalDays / totalDays * plotWidth;

                double y = TOPMARGIN + i * ROWHEIGHT + 5;

                var rect = new Rectangle
                {
                    Width = Math.Max(1, width),
                    Height = ROWHEIGHT - 10,
                    Fill = task.Fill,
                    RadiusX = 3,
                    RadiusY = 3,

                    /* Tooltip hinzufügen */
                    ToolTip = new ToolTip
                    {
                        Content = $"Titel: {task.Title}\nStart: {task.Start.ToShortDateString()} \nEnde: {task.End.ToShortDateString()}"
                    }
                };

                ToolTipService.SetInitialShowDelay(rect, 100);
                ToolTipService.SetShowDuration(rect, 2500);

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                this.PART_Canvas.Children.Add(rect);
            }
        }

        /// <summary>
        /// X-Achsenbeschriftungen zeichnen
        /// </summary>
        private void DrawXAxisLabels()
        {
            var taskList = ItemSource.ToList();

            DateTime minDate = taskList.Min(t => t.Start);
            DateTime maxDate = taskList.Max(t => t.End);

            int tickCount = 5;
            double plotWidth = ActualWidth - LEFTMARGIN - RIGHTMARGIN;
            double y = ActualHeight - BOTTOMMARGIN;

            for (int i = 0; i <= tickCount; i++)
            {
                DateTime date = minDate.AddDays((maxDate - minDate).TotalDays * i / tickCount);

                double x = LEFTMARGIN + plotWidth * i / tickCount;

                // Tick
                this.PART_Canvas.Children.Add(new Line
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
                    Text = date.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture), FontSize = 11
                };

                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(label, x - label.DesiredSize.Width / 2);
                Canvas.SetTop(label, y + 8);

                this.PART_Canvas.Children.Add(label);
            }
        }

        private void DrawTodayLine()
        {
            if (this.ShowTodayLine == false || this.ItemSource == null || this.ItemSource.Any() == false)
            {
                return;
            }

            var taskList = ItemSource.ToList();

            DateTime minDate = taskList.Min(t => t.Start).Date;
            DateTime maxDate = taskList.Max(t => t.End).Date;
            DateTime today = DateTime.Today;

            // Nur anzeigen, wenn Heute im Bereich liegt
            if (today < minDate || today > maxDate)
            {
                return;
            }

            double plotWidth = ActualWidth - LEFTMARGIN - RIGHTMARGIN;
            double plotHeight = ActualHeight - BOTTOMMARGIN;

            double totalDays = (maxDate - minDate).TotalDays;
            if (totalDays <= 0)
            {
                return;
            }

            double x = LEFTMARGIN + (today - minDate).TotalDays / totalDays * plotWidth;

            var line = new Line
            {
                X1 = x,
                X2 = x,
                Y1 = TOPMARGIN,
                Y2 = plotHeight,
                Stroke = TodayLineBrush,
                StrokeThickness = TodayLineThickness,
                StrokeDashArray = new DoubleCollection { 4, 4 } // gestrichelt
            };

            this.PART_Canvas.Children.Add(line);
        }

        private Rect GetTaskRect(GanttTask task, int index, DateTime minDate, DateTime maxDate)
        {
            double plotWidth = ActualWidth - LEFTMARGIN - RIGHTMARGIN;
            double totalDays = (maxDate - minDate).TotalDays;

            double x = LEFTMARGIN + (task.Start - minDate).TotalDays / totalDays * plotWidth;

            double width = (task.End - task.Start).TotalDays / totalDays * plotWidth;

            double y = TOPMARGIN + index * ROWHEIGHT + 5;

            return new Rect(x, y, Math.Max(1, width), ROWHEIGHT - 10);
        }

        private void DrawDependencies()
        {
            if (this.Dependencies == null || this.Dependencies.Any() == false)
            {
                return;
            }

            var taskList = ItemSource.ToList();
            DateTime minDate = taskList.Min(t => t.Start);
            DateTime maxDate = taskList.Max(t => t.End);

            foreach (var dep in Dependencies)
            {
                int fromIndex = taskList.IndexOf(dep.From);
                int toIndex = taskList.IndexOf(dep.To);

                if (fromIndex < 0 || toIndex < 0)
                {
                    continue;
                }

                Rect fromRect = GetTaskRect(dep.From, fromIndex, minDate, maxDate);
                Rect toRect = GetTaskRect(dep.To, toIndex, minDate, maxDate);

                // Punkte
                Point start = new(fromRect.Right, fromRect.Top + fromRect.Height / 2);
                Point mid1 = new(start.X + 10, start.Y);
                Point mid2 = new(mid1.X, toRect.Top + toRect.Height / 2);
                Point end = new(toRect.Left, toRect.Top + toRect.Height / 2);

                // Linie (3 Segmente)
                this.DrawLine(start, mid1);
                this.DrawLine(mid1, mid2);
                this.DrawLine(mid2, end);

                // Pfeilspitze
                this.DrawArrowHead(end);
            }
        }

        private void DrawLine(Point p1, Point p2)
        {
            this.PART_Canvas.Children.Add(new Line
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

            this.PART_Canvas.Children.Add(arrow);
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
