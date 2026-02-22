namespace ChartNETDemo
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;

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

        private readonly Dictionary<LifeChartData, Polyline> _lineMap = new();
        private INotifyCollectionChanged _currentCollection;

        public LifeLineControl()
        {
            InitializeComponent();
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
                new PropertyMetadata(-100.0, OnScaleChanged));

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
                new PropertyMetadata(100.0, OnScaleChanged));

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
                new PropertyMetadata(Brushes.Black));
        #endregion

        #region PropertyChanged Callbacks

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LifeLineControl)d;
            control.AttachCollection(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
        }

        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LifeLineControl)d).RedrawAll();
        }

        #endregion

        #region AttachCollection

        private void AttachCollection(IEnumerable oldCollection, IEnumerable newCollection)
        {
            // Alte Collection abmelden
            if (_currentCollection != null)
                _currentCollection.CollectionChanged -= OnCollectionChanged;

            _lineMap.Clear();
            PART_Canvas.Children.Clear();

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

            DrawAxes();
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

            PART_Canvas.Children.Clear();
            DrawAxes();

            foreach (var kvp in _lineMap)
            {
                PART_Canvas.Children.Add(kvp.Value);
                DrawLine(kvp.Key);
            }
        }

        private const double XStep = 10;

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

            double normalized =
                (value - MinValue) /
                (MaxValue - MinValue);

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
