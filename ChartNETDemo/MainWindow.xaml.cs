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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Media;

namespace ChartNETDemo
{
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

            this.WindowTitel = "Minimal WPF Template";
            this.DataContext = this;
        }

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

        public List<GanttTask> Tasks
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
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnGanttChart, "Click", this.OnSelectedChart);

            this.LineChartDemoData();
            this.PieChartDemoData();
            this.BarChartDemoData();
            this.GanttChartDemoData();
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
                else if (btn.Tag.ToString() == "GanttChart")
                {
                    this.ChartTabControl.SelectedIndex = 3;
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
                //this.LineChart.ExportToPng(@"c:\temp\demo.png");
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

        private void GanttChartDemoData()
        {
            this.Tasks = new List<GanttTask>
            {
                new GanttTask
                {
                    Title = "Analyse",
                    Start = new DateTime(2024, 1, 1),
                    End   = new DateTime(2024, 1, 15),
                    Fill  = Brushes.SteelBlue
                },
                new GanttTask
                {
                    Title = "Implementierung",
                    Start = new DateTime(2024, 1, 10),
                    End   = new DateTime(2024, 2, 10),
                    Fill  = Brushes.Orange
                },
                new GanttTask
                {
                    Title = "Test",
                    Start = new DateTime(2024, 2, 1),
                    End   = new DateTime(2024, 2, 20),
                    Fill  = Brushes.Green
                }
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