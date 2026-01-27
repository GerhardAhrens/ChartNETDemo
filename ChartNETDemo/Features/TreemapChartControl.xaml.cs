namespace ChartNETDemo
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    public class TreemapItem
    {
        public string Label { get; set; } = "";
        public double Value { get; set; }
    }

    public class TreemapGroup
    {
        public string Title { get; set; } = "";
        public Brush Fill { get; set; } = Brushes.SteelBlue;
        public IList<TreemapItem> Items { get; set; } = new List<TreemapItem>();
    }

    /// <summary>
    /// Interaktionslogik für TreemapChartControl.xaml
    /// </summary>
    public partial class TreemapChartControl : UserControl
    {
        private const double PADDING = 4;

        public TreemapChartControl()
        {
            InitializeComponent();
            SizeChanged += (_, _) => Redraw();
        }

        #region DependencyProperty

        public IEnumerable<TreemapGroup> ItemSource
        {
            get => (IEnumerable<TreemapGroup>)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }

        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
                typeof(IEnumerable<TreemapGroup>),
                typeof(TreemapChartControl),
                new PropertyMetadata(null, (_, __) => ((TreemapChartControl)_).Redraw()));

        public bool ShowLabels
        {
            get => (bool)GetValue(ShowLabelsProperty);
            set => SetValue(ShowLabelsProperty, value);
        }

        public static readonly DependencyProperty ShowLabelsProperty =
            DependencyProperty.Register(
                nameof(ShowLabels),
                typeof(bool),
                typeof(TreemapChartControl),
                new PropertyMetadata(true, (_, __) => ((TreemapChartControl)_).Redraw()));

        #endregion

        #region Legend Properties

        public bool ShowLegend
        {
            get => (bool)GetValue(ShowLegendProperty);
            set => SetValue(ShowLegendProperty, value);
        }

        public static readonly DependencyProperty ShowLegendProperty =
            DependencyProperty.Register(
                nameof(ShowLegend),
                typeof(bool),
                typeof(TreemapChartControl),
                new PropertyMetadata(true, (_, __) => ((TreemapChartControl)_).Redraw()));

        public LegendPosition LegendPosition
        {
            get => (LegendPosition)GetValue(LegendPositionProperty);
            set => SetValue(LegendPositionProperty, value);
        }

        public static readonly DependencyProperty LegendPositionProperty =
            DependencyProperty.Register(
                nameof(LegendPosition),
                typeof(LegendPosition),
                typeof(TreemapChartControl),
                new PropertyMetadata(LegendPosition.Top, (_, __) =>
                {
                    var c = (TreemapChartControl)_;
                    c.UpdateLegendLayout();
                }));

        #endregion

        #region Rendering

        private void Redraw()
        {
            this.PART_Canvas.Children.Clear();
            this.PART_Legend.Children.Clear();

            if (this.ItemSource == null || this.ItemSource.Any() == false)
                return;

            this.DrawTreemap(new Rect(0, 0, ActualWidth, ActualHeight));

            if (ShowLegend == true)
            {
                this.DrawLegend();
                this.UpdateLegendLayout();
            }
        }

        private void DrawTreemap(Rect area)
        {
            var groups = ItemSource.ToList();
            double totalValue = groups.Sum(g => g.Items.Sum(i => i.Value));
            if (totalValue <= 0)
                return;

            bool horizontal = area.Width >= area.Height;
            double offset = 0;

            foreach (var group in groups)
            {
                double groupValue = group.Items.Sum(i => i.Value);
                if (groupValue <= 0)
                    continue;

                double ratio = groupValue / totalValue;

                Rect groupRect = horizontal
                    ? new Rect(area.X + offset, area.Y, area.Width * ratio, area.Height)
                    : new Rect(area.X, area.Y + offset, area.Width, area.Height * ratio);

                this.DrawGroup(group, groupRect);

                offset += horizontal
                    ? area.Width * ratio
                    : area.Height * ratio;
            }
        }

        private void DrawGroup(TreemapGroup group, Rect area)
        {
            var items = group.Items.Where(i => i.Value > 0).ToList();
            double total = items.Sum(i => i.Value);
            if (total <= 0)
                return;

            bool horizontal = area.Width >= area.Height;
            double offset = 0;

            foreach (var item in items)
            {
                double ratio = item.Value / total;

                Rect rect = horizontal
                    ? new Rect(area.X + offset, area.Y, area.Width * ratio, area.Height)
                    : new Rect(area.X, area.Y + offset, area.Width, area.Height * ratio);

                this.DrawItem(rect, group.Fill, item.Label);

                offset += horizontal
                    ? area.Width * ratio
                    : area.Height * ratio;
            }
        }

        private void DrawItem(Rect rect, Brush fill, string label)
        {
            rect = new Rect(
                rect.X + PADDING,
                rect.Y + PADDING,
                Math.Max(0, rect.Width - 2 * PADDING),
                Math.Max(0, rect.Height - 2 * PADDING));

            if (rect.Width <= 2 || rect.Height <= 2)
                return;

            var border = new Rectangle
            {
                Width = rect.Width,
                Height = rect.Height,
                Fill = fill,
                Stroke = Brushes.White,
                StrokeThickness = 1
            };

            Canvas.SetLeft(border, rect.X);
            Canvas.SetTop(border, rect.Y);
            PART_Canvas.Children.Add(border);

            if (ShowLabels && rect.Width > 40 && rect.Height > 20)
            {
                var text = new TextBlock
                {
                    Text = label,
                    Foreground = Brushes.White,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap
                };

                Canvas.SetLeft(text, rect.X + 4);
                Canvas.SetTop(text, rect.Y + 4);
                this.PART_Canvas.Children.Add(text);
            }
        }

        private void DrawLegend()
        {
            this.PART_Legend.Children.Clear();

            foreach (var group in ItemSource)
            {
                var row = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(4)
                };

                row.Children.Add(new Rectangle
                {
                    Width = 14,
                    Height = 14,
                    Fill = group.Fill,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5,
                    Margin = new Thickness(5, 0, 6, 0)
                });

                row.Children.Add(new TextBlock
                {
                    Text = group.Title,
                    VerticalAlignment = VerticalAlignment.Center
                });

                this.PART_Legend.Children.Add(row);
            }
        }

        private void UpdateLegendLayout()
        {
            if (this.PART_RootGrid == null || this.PART_Canvas == null || this.PART_Legend == null)
                return;

            this.PART_RootGrid.RowDefinitions.Clear();
            this.PART_RootGrid.ColumnDefinitions.Clear();

            Grid.SetRow(this.PART_Canvas, 0);
            Grid.SetColumn(this.PART_Canvas, 0);
            Grid.SetRow(this.PART_Legend, 0);
            Grid.SetColumn(this.PART_Legend, 0);

            switch (this.LegendPosition)
            {
                case LegendPosition.Left:
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    Grid.SetColumn(this.PART_Legend, 0);
                    Grid.SetColumn(this.PART_Canvas, 1);
                    this.PART_Legend.Orientation = Orientation.Vertical;
                    break;

                case LegendPosition.Right:
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    this.PART_RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    Grid.SetColumn(this.PART_Canvas, 0);
                    Grid.SetColumn(this.PART_Legend, 1);
                    this.PART_Legend.Orientation = Orientation.Vertical;
                    break;

                case LegendPosition.Top:
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition());

                    Grid.SetRow(this.PART_Legend, 0);
                    Grid.SetRow(this.PART_Canvas, 1);
                    this.PART_Legend.Orientation = Orientation.Horizontal;
                    break;

                case LegendPosition.Bottom:
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition());
                    this.PART_RootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    Grid.SetRow(this.PART_Canvas, 0);
                    Grid.SetRow(this.PART_Legend, 1);
                    this.PART_Legend.Orientation = Orientation.Horizontal;
                    break;
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
