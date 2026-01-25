namespace ChartNETDemo
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
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

        #endregion

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

        #endregion
    }
}
