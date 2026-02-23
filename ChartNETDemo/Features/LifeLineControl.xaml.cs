namespace ChartNETDemo
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;

    public class GridVisualHost : FrameworkElement
    {
        private readonly VisualCollection _children;
        private DrawingVisual _gridVisual;

        public GridVisualHost()
        {
            _children = new VisualCollection(this);
            _gridVisual = new DrawingVisual();
            _children.Add(_gridVisual);
        }

        public void Draw(Action<DrawingContext> drawAction)
        {
            using var dc = _gridVisual.RenderOpen();
            drawAction(dc);
        }

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
            => _children[index];
    }

    public class LifeChartData : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public Brush Stroke { get; set; }

        public ObservableCollection<double> Values { get; } = new();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Interaktionslogik für LifeLineControl.xaml
    /// </summary>
    public partial class LifeLineControl : UserControl
    {
        private const double LeftMargin = 50;
        private const double BottomMargin = 30;

        private GridVisualHost _gridHost;
        private readonly Dictionary<LifeChartData, Polyline> _lineMap = new();
        private INotifyCollectionChanged _currentCollection;
        private readonly List<TextBlock> _axisLabels = new();

        public LifeLineControl()
        {
            InitializeComponent();

            _gridHost = new GridVisualHost();

            // Grid ganz nach hinten
            Canvas.SetZIndex(_gridHost, -100);

            PART_Canvas.Children.Add(_gridHost);

            Loaded += (_, _) => RedrawAll();
            SizeChanged += (_, _) => RedrawAll();
        }
        #region DependencyProperties

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(LifeLineControl),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public double MinValue
        {
            get => (double)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(
                nameof(MinValue),
                typeof(double),
                typeof(LifeLineControl),
                new PropertyMetadata(-150.0, OnScaleChanged));

        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(
                nameof(MaxValue),
                typeof(double),
                typeof(LifeLineControl),
                new PropertyMetadata(150.0, OnScaleChanged));

        public double XStep
        {
            get => (double)GetValue(XStepProperty);
            set => SetValue(XStepProperty, value);
        }

        public static readonly DependencyProperty XStepProperty =
            DependencyProperty.Register(
                nameof(XStep),
                typeof(double),
                typeof(LifeLineControl),
                new PropertyMetadata(20.0));

        public Brush ChartBackground
        {
            get => (Brush)GetValue(ChartBackgroundProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public static readonly DependencyProperty ChartBackgroundProperty =
            DependencyProperty.Register(
                nameof(ChartBackground),
                typeof(Brush),
                typeof(LifeLineControl),
                new PropertyMetadata(null, OnBackgroundChanged));

        public double MinorGridSpacing
        {
            get => (double)GetValue(MinorGridSpacingProperty);
            set => SetValue(MinorGridSpacingProperty, value);
        }

        public static readonly DependencyProperty MinorGridSpacingProperty =
            DependencyProperty.Register(
                nameof(MinorGridSpacing),
                typeof(double),
                typeof(LifeLineControl),
                new PropertyMetadata(10.0, OnGridChanged));


        public double MajorGridSpacing
        {
            get => (double)GetValue(MajorGridSpacingProperty);
            set => SetValue(MajorGridSpacingProperty, value);
        }

        public static readonly DependencyProperty MajorGridSpacingProperty =
            DependencyProperty.Register(
                nameof(MajorGridSpacing),
                typeof(double),
                typeof(LifeLineControl),
                new PropertyMetadata(50.0, OnGridChanged));


        public Brush MinorGridBrush
        {
            get => (Brush)GetValue(MinorGridBrushProperty);
            set => SetValue(MinorGridBrushProperty, value);
        }

        public static readonly DependencyProperty MinorGridBrushProperty =
            DependencyProperty.Register(
                nameof(MinorGridBrush),
                typeof(Brush),
                typeof(LifeLineControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(40, 255, 0, 0)), OnGridChanged));


        public Brush MajorGridBrush
        {
            get => (Brush)GetValue(MajorGridBrushProperty);
            set => SetValue(MajorGridBrushProperty, value);
        }

        public static readonly DependencyProperty MajorGridBrushProperty =
            DependencyProperty.Register(
                nameof(MajorGridBrush),
                typeof(Brush),
                typeof(LifeLineControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(90, 255, 0, 0)), OnGridChanged));


        #endregion

        #region PropertyChanged Callbacks

        private static void OnGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LifeLineControl)d).RedrawAll();
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LifeLineControl)d;
            control.AttachCollection(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
        }

        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LifeLineControl)d).RedrawAll();
        }

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LifeLineControl)d).Background = e.NewValue as Brush;
        }

        #endregion

        #region AttachCollection

        private void AttachCollection(IEnumerable oldCollection, IEnumerable newCollection)
        {
            // Alte Collection abmelden
            if (_currentCollection != null)
                _currentCollection.CollectionChanged -= OnCollectionChanged;

            // Nur Linien entfernen
            foreach (var line in _lineMap.Values)
            {
                PART_Canvas.Children.Remove(line);
            }

            _lineMap.Clear();

            if (newCollection == null)
                return;

            if (newCollection is INotifyCollectionChanged notifyCollection)
            {
                _currentCollection = notifyCollection;
                _currentCollection.CollectionChanged += OnCollectionChanged;
            }

            foreach (LifeChartData line in newCollection)
            {
                AddLine(line);
            }

            RedrawAll();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.NewItems != null)
                {
                    foreach (LifeChartData line in e.NewItems)
                        AddLine(line);
                }

                if (e.OldItems != null)
                {
                    foreach (LifeChartData line in e.OldItems)
                        RemoveLine(line);
                }
            });
        }

        #endregion

        #region Line Handling

        private void AddLine(LifeChartData line)
        {
            var polyline = new Polyline
            {
                Stroke = line.Stroke,
                StrokeThickness = 2
            };

            _lineMap[line] = polyline;
            PART_Canvas.Children.Add(polyline);

            line.Values.CollectionChanged += (s, e) =>
            {
                Dispatcher.Invoke(() => DrawLine(line));
            };

            DrawLine(line);
        }

        private void RemoveLine(LifeChartData line)
        {
            if (_lineMap.TryGetValue(line, out var polyline))
            {
                PART_Canvas.Children.Remove(polyline);
                _lineMap.Remove(line);
            }
        }

        #endregion

        #region Drawing

        private void RedrawAll()
        {
            if (!IsLoaded)
                return;

            DrawGrid();
            DrawAxes();
            DrawAxisLabels();

            foreach (var kvp in _lineMap)
            {
                if (!PART_Canvas.Children.Contains(kvp.Value))
                    PART_Canvas.Children.Add(kvp.Value);
            }
        }

        private void DrawGrid()
        {
            double width = PART_Canvas.ActualWidth;
            double height = PART_Canvas.ActualHeight - BottomMargin;

            if (width <= 0 || height <= 0)
                return;

            _gridHost.Width = width;
            _gridHost.Height = height;

            _gridHost.Draw(dc =>
            {
                var minorPen = new Pen(MinorGridBrush, 1);
                var majorPen = new Pen(MajorGridBrush, 1.5);

                minorPen.Freeze();
                majorPen.Freeze();

                // Minor Vertical
                for (double x = LeftMargin; x <= width; x += MinorGridSpacing)
                {
                    dc.DrawLine(minorPen,
                        new Point(x, 0),
                        new Point(x, height));
                }

                // Minor Horizontal
                for (double y = 0; y <= height; y += MinorGridSpacing)
                {
                    dc.DrawLine(minorPen,
                        new Point(LeftMargin, y),
                        new Point(width, y));
                }

                // Major Vertical
                for (double x = LeftMargin; x <= width; x += MajorGridSpacing)
                {
                    dc.DrawLine(majorPen,
                        new Point(x, 0),
                        new Point(x, height));
                }

                // Major Horizontal
                for (double y = 0; y <= height; y += MajorGridSpacing)
                {
                    dc.DrawLine(majorPen,
                        new Point(LeftMargin, y),
                        new Point(width, y));
                }
            });
        }

        private void DrawLine(LifeChartData line)
        {
            if (!_lineMap.TryGetValue(line, out var polyline))
                return;

            double width = PART_Canvas.ActualWidth - LeftMargin;
            double height = PART_Canvas.ActualHeight - BottomMargin;

            if (width <= 0 || height <= 0)
                return;

            // 1️⃣ Alle bestehenden Punkte nach links verschieben
            for (int i = 0; i < polyline.Points.Count; i++)
            {
                var p = polyline.Points[i];
                polyline.Points[i] = new Point(p.X - XStep, p.Y);
            }

            // 2️⃣ Neue Y-Position berechnen (letzter Wert)
            double value = line.Values[^1];

            double normalized = (value - MinValue) / (MaxValue - MinValue);

            double y = height - (normalized * height);

            // 3️⃣ Neuen Punkt rechts hinzufügen
            polyline.Points.Add(new Point(
                LeftMargin + width,
                y));

            // 4️⃣ Punkte löschen, die links rausgelaufen sind
            while (polyline.Points.Count > 0 &&
                   polyline.Points[0].X < LeftMargin)
            {
                polyline.Points.RemoveAt(0);
            }
        }

        private void ClearAxisLabels()
        {
            foreach (var tb in _axisLabels)
                PART_Canvas.Children.Remove(tb);

            _axisLabels.Clear();
        }

        private void DrawAxisLabels()
        {
            ClearAxisLabels();

            double width = PART_Canvas.ActualWidth;
            double height = PART_Canvas.ActualHeight - BottomMargin;

            if (width <= 0 || height <= 0)
                return;

            // 🔹 Y-Achsen-Beschriftung
            double valueRange = MaxValue - MinValue;
            int steps = 5; // Anzahl der Beschriftungen

            for (int i = 0; i <= steps; i++)
            {
                double value = MinValue + (valueRange / steps) * i;
                double normalized = (value - MinValue) / valueRange;
                double y = height - (normalized * height);

                var tb = CreateLabel(value.ToString("0", CultureInfo.CurrentCulture), 5, y - 10);
                _axisLabels.Add(tb);
                PART_Canvas.Children.Add(tb);
            }

            // 🔹 X-Achsen-Beschriftung (Zeitachse)
            int secondsVisible = 5; // z.B. 5 Sekunden sichtbar
            double pixelsPerSecond = 100; // abhängig von XStep & Timer

            for (int i = 0; i <= secondsVisible; i++)
            {
                double x = LeftMargin + width - (i * pixelsPerSecond);

                if (x < LeftMargin)
                    continue;

                var tb = CreateLabel($"{i}s", x - 10, height + 5);
                _axisLabels.Add(tb);
                PART_Canvas.Children.Add(tb);
            }
        }

        private static TextBlock CreateLabel(string text, double x, double y)
        {
            var tb = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = 11
            };

            Canvas.SetLeft(tb, x);
            Canvas.SetTop(tb, y);

            return tb;
        }

        private void DrawAxes()
        {
            double width = PART_Canvas.ActualWidth;
            double height = PART_Canvas.ActualHeight;

            if (width <= 0 || height <= 0)
                return;

            var xAxis = new Line
            {
                X1 = LeftMargin,
                Y1 = height - BottomMargin,
                X2 = width,
                Y2 = height - BottomMargin,
                Stroke = Brushes.White
            };

            var yAxis = new Line
            {
                X1 = LeftMargin,
                Y1 = 0,
                X2 = LeftMargin,
                Y2 = height - BottomMargin,
                Stroke = Brushes.White
            };

            PART_Canvas.Children.Add(xAxis);
            PART_Canvas.Children.Add(yAxis);
        }

        #endregion    
    }
}
