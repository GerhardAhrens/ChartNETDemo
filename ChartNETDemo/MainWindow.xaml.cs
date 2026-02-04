//-----------------------------------------------------------------------
// <copyright file="MainWindow.cs" company="Lifeprojects.de">
//     Class: MainWindow
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>23.01.2026 08:50:58</date>
//
// <summary>
// MainWindow mit Minimalfunktionen
// </summary>
//-----------------------------------------------------------------------

namespace ChartNETDemo
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            this.InitializeComponent();
            WeakEventManager<Window, RoutedEventArgs>.AddHandler(this, "Loaded", this.OnLoaded);
            WeakEventManager<Window, CancelEventArgs>.AddHandler(this, "Closing", this.OnWindowClosing);

            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            this.DemoDataPath = Path.Combine(new DirectoryInfo(currentDirectory).Parent.Parent.Parent.FullName, "DemoData");
            if (Directory.Exists(this.DemoDataPath) == false)
            {
                Directory.CreateDirectory(this.DemoDataPath);
            }

            this.WindowTitel = "Chart WPF Demo";
            this.DataContext = this;
        }

        public string DemoDataPath { get; private set; }

        public string WindowTitel
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<ChartLine> ChartLines
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged();
            }
        }

        public List<PieSegment> PieChartSegments
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged();
            }
        }

        public List<BarSeries> BarChartSeries
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged();
            }
        }

        public List<HorizontalBarSeries> BarChartHorizontalSeries
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged();
            }
        }

        public List<ColumnChartSeries> ColumnChartSeries
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged();
            }
        }

        public List<GanttTask> Tasks
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged();
            }
        }

        public List<TreemapGroup> Groups
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged();
            }
        }

        public List<HeatmapCell> HeatmapCellSource
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<ScatterPoint> ScatterChartSource
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnCloseApplication, "Click", this.OnCloseApplication);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnLineChart, "Click", this.OnSelectedChart);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnPieChart, "Click", this.OnSelectedChart);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnBarChart, "Click", this.OnSelectedChart);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnBarChartH, "Click", this.OnSelectedChart);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnColumnChart, "Click", this.OnSelectedChart);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnGanttChart, "Click", this.OnSelectedChart);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnTreeMapChart, "Click", this.OnSelectedChart);
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnHeadmapChart, "Click", this.OnSelectedChart);

            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnSaveToPng, "Click", this.OnChartSave);

            this.LineChartDemoData();
            this.PieChartDemoData();
            this.BarChartDemoData();
            this.BarChartHorizontalDemoData();
            this.ColumnChartDemoData();
            this.GanttChartDemoData();
            this.TreeMapChartDemoData();
            this.HeadmapChartDemoData();
            this.ScatterChartDemoData();
        }

        private void OnSelectedChart(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                if (btn.Tag.ToString() == "LineChart")
                {
                    this.ChartTabControl.SelectedIndex = 0;
                }
                else if (btn.Tag.ToString() == "PieChart")
                {
                    this.ChartTabControl.SelectedIndex = 1;
                }
                else if (btn.Tag.ToString() == "BarChart")
                {
                    this.ChartTabControl.SelectedIndex = 2;
                }
                else if (btn.Tag.ToString() == "BarChartH")
                {
                    this.ChartTabControl.SelectedIndex = 3;
                }
                else if (btn.Tag.ToString() == "ColumnChart")
                {
                    this.ChartTabControl.SelectedIndex = 4;
                }
                else if (btn.Tag.ToString() == "GanttChart")
                {
                    this.ChartTabControl.SelectedIndex = 5;
                }
                else if (btn.Tag.ToString() == "TreeMapChart")
                {
                    this.ChartTabControl.SelectedIndex = 6;
                }
                else if (btn.Tag.ToString() == "HeadmapChart")
                {
                    this.ChartTabControl.SelectedIndex = 7;
                }
            }
        }

        private void OnChartSave(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                if (this.ChartTabControl.SelectedIndex == 0)
                {
                    string demoDataImage = Path.Combine(this.DemoDataPath, "LineChartDemo.png");
                    this.LineChart.ExportToPng(demoDataImage);
                }
                else if (this.ChartTabControl.SelectedIndex == 1)
                {
                    string demoDataImage = Path.Combine(this.DemoDataPath, "PieChartDemo.png");
                    this.MyPieChart.ExportToPng(demoDataImage);
                }
                else if (this.ChartTabControl.SelectedIndex == 2)
                {
                    string demoDataImage = Path.Combine(this.DemoDataPath, "BarChartDemo.png");
                    this.MyBarChart.ExportToPng(demoDataImage);
                }
                else if (this.ChartTabControl.SelectedIndex == 3)
                {
                    string demoDataImage = Path.Combine(this.DemoDataPath, "BarChartHorizontalDemo.png");
                    this.MyBarChartHorizontal.ExportToPng(demoDataImage);
                }
                else if (this.ChartTabControl.SelectedIndex == 4)
                {
                    string demoDataImage = Path.Combine(this.DemoDataPath, "ColumnChartDemo.png");
                    this.MyColumnChart.ExportToPng(demoDataImage);
                }
                else if (this.ChartTabControl.SelectedIndex == 5)
                {
                    string demoDataImage = Path.Combine(this.DemoDataPath, "GanttChartDemo.png");
                    this.MyGanttChart.ExportToPng(demoDataImage);
                }
                else if (this.ChartTabControl.SelectedIndex == 6)
                {
                    string demoDataImage = Path.Combine(this.DemoDataPath, "TreeMapChartDemo.png");
                    this.MyTreeMapChart.ExportToPng(demoDataImage);
                }
                else if (this.ChartTabControl.SelectedIndex == 7)
                {
                    string demoDataImage = Path.Combine(this.DemoDataPath, "HeadmapChartDemo.png");
                    this.MyHeadmapChart.ExportToPng(demoDataImage);
                }
            }
        }

        private void OnCloseApplication(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = false;

            MessageBoxResult msgYN = MessageBox.Show("Wollen Sie die Anwendung beenden?", "Beenden", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (msgYN == MessageBoxResult.Yes)
            {
                //this.MyPieChart.ExportToPng(@"c:\temp\demo.png");

                App.ApplicationExit();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void LineChartDemoData()
        {
            this.ChartLines = new ObservableCollection<ChartLine>
                    {
                        new ChartLine
                        {
                            Title = "Umsatz",
                            Stroke = Brushes.Red,
                            Values =
                            {
                                new ChartPoint { Category = "2020", Value = 10 },
                                new ChartPoint { Category = "2021", Value = 30 },
                                new ChartPoint { Category = "2022", Value = 25 },
                                new ChartPoint { Category = "2023", Value = 60 },
                                new ChartPoint { Category = "2024", Value = 45 }
                            }
                        },
                                        new ChartLine
                        {
                            Title = "Kosten",
                            Stroke = Brushes.Blue,
                            Values =
                            {
                                new ChartPoint { Category = "2020", Value = 20 },
                                new ChartPoint { Category = "2021", Value = 25 },
                                new ChartPoint { Category = "2022", Value = 35 },
                                new ChartPoint { Category = "2023", Value = 40 },
                                new ChartPoint { Category = "2024", Value = 55 }
                            }
                        }
                    };
        }

        private void PieChartDemoData()
        {
            this.PieChartSegments = new List<PieSegment>
            {
                new PieSegment { Label="A", Value=30, Fill=Brushes.Red },
                new PieSegment { Label="B", Value=50, Fill=Brushes.Green },
                new PieSegment { Label="C", Value=20, Fill=Brushes.Blue },
                new PieSegment { Label="D", Value=25, Fill=Brushes.Orange }
            };
        }

        private void BarChartDemoData()
        {
            this.BarChartSeries = new List<BarSeries>
            {
                new BarSeries
                {
                    Title = "Serie A",
                    Fill = Brushes.SteelBlue,
                    Values =
                    {
                        new BarChartPoint { X="2020", Y=10 },
                        new BarChartPoint { X="2021", Y=20 },
                        new BarChartPoint { X="2022", Y=30 },
                        new BarChartPoint { X="2023", Y=25 },
                        new BarChartPoint { X="2024", Y=30 }
                    }
                },
                new BarSeries
                {
                    Title = "Serie B",
                    Fill = Brushes.Orange,
                    Values =
                    {
                        new BarChartPoint { X="2020", Y=5 },
                        new BarChartPoint { X="2021", Y=15 },
                        new BarChartPoint { X="2022", Y=10 },
                        new BarChartPoint { X="2023", Y=8 },
                        new BarChartPoint { X="2024", Y=12 }
                    }
                }
            };
        }

        private void BarChartHorizontalDemoData()
        {
            this.BarChartHorizontalSeries = new List<HorizontalBarSeries>
            {
                new HorizontalBarSeries
                {
                    Title = "Serie A",
                    Fill = Brushes.SteelBlue,
                    Values =
                    {
                        new HorizontalBarChartPoint { Y="2020", X=10 },
                        new HorizontalBarChartPoint { Y="2021", X=20 },
                        new HorizontalBarChartPoint { Y="2022", X=30 },
                        new HorizontalBarChartPoint { Y="2023", X=25 },
                        new HorizontalBarChartPoint { Y="2024", X=30 }
                    }
                },
                new HorizontalBarSeries
                {
                    Title = "Serie B",
                    Fill = Brushes.Orange,
                    Values =
                    {
                        new HorizontalBarChartPoint { Y="2020", X=5 },
                        new HorizontalBarChartPoint { Y="2021", X=15 },
                        new HorizontalBarChartPoint { Y="2022", X=10 },
                        new HorizontalBarChartPoint { Y="2023", X=8 },
                        new HorizontalBarChartPoint { Y="2024", X=12 }
                    }
                }
            };
        }

        private void ColumnChartDemoData()
        {
            ColumnChartSeries = new List<ColumnChartSeries>
            {
                new ColumnChartSeries
                {
                    Title = "Serie A",
                    Fill = Brushes.SteelBlue,
                    Values =
                    {
                        new() { X = "2020", Y = 10 },
                        new() { X = "2021", Y = 30 },
                        new() { X = "2022", Y = 25 }
                    }
                },
                new ColumnChartSeries
                {
                    Title = "Serie B",
                    Fill = Brushes.Orange,
                    Values =
                    {
                        new() { X = "2020", Y = 15 },
                        new() { X = "2021", Y = 20 },
                        new() { X = "2022", Y = 35 }
                    }
                },
                new ColumnChartSeries
                {
                    Title = "Serie C",
                    Fill = Brushes.Green,
                    Values =
                    {
                        new() { X = "2020", Y = 12 },
                        new() { X = "2021", Y = 18 },
                        new() { X = "2022", Y = 39 }
                    }
                }
            };
        }

        private void GanttChartDemoData()
        {
            var taskA = new GanttTask
            {
                Title = "Analyse",
                Start = new DateTime(DateTime.Now.Year, 1, 1),
                End = new DateTime(DateTime.Now.Year, 1, 15),
                Fill = Brushes.SteelBlue
            };

            var taskB = new GanttTask
            {
                Title = "Implementierung",
                Start = new DateTime(DateTime.Now.Year, 1, 10),
                End = new DateTime(DateTime.Now.Year, 2, 10),
                Fill = Brushes.Orange
            };

            var taskC = new GanttTask
            {
                Title = "Test",
                Start = new DateTime(DateTime.Now.Year, 2, 1),
                End = new DateTime(DateTime.Now.Year, 2, 20),
                Fill = Brushes.Green
            };

            var taskD = new GanttTask
            {
                Title = "Dokumentation",
                Start = new DateTime(DateTime.Now.Year, 2, 1),
                End = new DateTime(DateTime.Now.Year, 3, 10),
                Fill = Brushes.Yellow
            };

            var taskE = new GanttTask
            {
                Title = "Im Betrieb",
                Start = new DateTime(DateTime.Now.Year, 3, 1),
                End = new DateTime(DateTime.Now.Year, 4, 1),
                Fill = Brushes.Red
            };

            this.Tasks = new List<GanttTask>();
            this.Tasks.Add(taskA);
            this.Tasks.Add(taskB);
            this.Tasks.Add(taskC);
            this.Tasks.Add(taskD);
            this.Tasks.Add(taskE);

            MyGanttChart.Dependencies = new[]
            {
                new GanttDependency { From = taskA, To = taskB },
                new GanttDependency { From = taskB, To = taskC },
                new GanttDependency { From = taskC, To = taskE },
            };
        }

        private void TreeMapChartDemoData()
        {
            Groups = new List<TreemapGroup>
            {
                new TreemapGroup
                {
                    Title = "Europa",
                    Fill = Brushes.SteelBlue,
                    Items =
                    {
                        new TreemapItem { Label = "Deutschland", Value = 40 },
                        new TreemapItem { Label = "Frankreich", Value = 30 },
                        new TreemapItem { Label = "Italien", Value = 20 }
                    }
                },
                new TreemapGroup
                {
                    Title = "Asien",
                    Fill = Brushes.Orange,
                    Items =
                    {
                        new TreemapItem { Label = "China", Value = 60 },
                        new TreemapItem { Label = "Japan", Value = 25 }
                    }
                },
                new TreemapGroup
                {
                    Title = "Nord Amerika",
                    Fill = Brushes.Green,
                    Items =
                    {
                        new TreemapItem { Label = "USA", Value = 75 },
                        new TreemapItem { Label = "Kanada", Value = 35 },
                        new TreemapItem { Label = "Alaska", Value = 35 }
                    }
                }
            };
        }

        private void HeadmapChartDemoData()
        {
            this.HeatmapCellSource = new List<HeatmapCell>
            {
                new HeatmapCell { X = "Mo", Y = "A", Value = 100 },
                new HeatmapCell { X = "Di", Y = "A", Value = 70 },
                new HeatmapCell { X = "Mi", Y = "A", Value = 5 },
                new HeatmapCell { X = "Do", Y = "A", Value = 6 },

                new HeatmapCell { X = "Mo", Y = "B", Value = 5 },
                new HeatmapCell { X = "Di", Y = "B", Value = 12 },
                new HeatmapCell { X = "Mi", Y = "B", Value = 17 },
                new HeatmapCell { X = "Do", Y = "B", Value = 7 }
            };
        }

        private void ScatterChartDemoData()
        {
            this.ScatterChartSource = new ObservableCollection<ScatterPoint>
            {
                new() { X=1, Y=2, Category="Gruppe A", Color=Brushes.Red },
                new() { X=2, Y=5, Category="Gruppe B", Color=Brushes.Blue },
                new() { X=3, Y=3, Category="Gruppe A", Color=Brushes.Red },
                new() { X=4, Y=6, Category="Gruppe C", Color=Brushes.Green } 
            };
        }

        #region INotifyPropertyChanged implementierung
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler == null)
            {
                return;
            }

            var e = new PropertyChangedEventArgs(propertyName);
            handler(this, e);
        }
        #endregion INotifyPropertyChanged implementierung
    }
}