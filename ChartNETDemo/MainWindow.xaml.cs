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

        private string _WindowTitel;

        public string WindowTitel
        {
            get { return _WindowTitel; }
            set
            {
                if (this._WindowTitel != value)
                {
                    this._WindowTitel = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<ChartLine> _chartLines;

        public ObservableCollection<ChartLine> ChartLines
        {
            get { return _chartLines; }
            set
            {
                if (this._chartLines != value)
                {
                    this._chartLines = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(this.BtnCloseApplication, "Click", this.OnCloseApplication);

            ChartLines = new ObservableCollection<ChartLine>
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

                App.ApplicationExit();
            }
            else
            {
                e.Cancel = true;
            }
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