using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InterviewQuestion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PathFinderOptions pathfinderOptions = new PathFinderOptions()
        {
            StepDelayInMs = 50
        };
        GridGeneratorOptions generatorOptions = new GridGeneratorOptions()
        {
            BlockCount = 5,
            MinWidth = 25,
            MaxWidth = 25,
            MinHeight = 25,
            MaxHeight = 25
        };

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.genoptionsPanel.DataContext = generatorOptions;
            this.pathOptionsPanel.DataContext = pathfinderOptions;
        }

        private void Log(string msg)
        {
            this.tb.AppendText(msg);
        }

        private IPathFinder PathFactory()
        {
            if (path1.IsChecked.Value) return new Path1();
            if (path2.IsChecked.Value) return new Path2();

            return new Path1();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Create a new board and init with blocks
            GridGenerator gen = new GridGenerator(this.generatorOptions);
            var grid = gen.Generate();
            this.pathfinderOptions.Grid = grid;

            this.board.Initialize(pathfinderOptions);
            IPathFinder path = PathFactory();
            path.Initialize(pathfinderOptions);
            path.Finished += (b) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.go.Content = "Go";
                    this.go.Click += Button_Click;
                }));
            };
            path.Log += (msg) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.tb.AppendText(msg);
                    Debug.WriteLine(msg);
                }));
            };
            path.NodeVisited += (p) => System.Threading.Thread.Sleep(pathfinderOptions.StepDelayInMs);

            this.go.Content = "Cancel";
            this.go.Click -= Button_Click;
            this.go.Click += (s, e2) => path.Cancel();

            Task.Run(() =>
            {
                // Create a grid and traverse    
                path.NodeVisited += delegate (Point p)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        Log($"{p}\r\n");
                        board.AddVisited(p);
                    }));
                };
                path.FindPath(new Point(0, 0), new Point(grid.GetLength(0) - 1, grid.GetLength(1) - 1));
            });
        }

        private void Path_NodeVisited(Point pt)
        {
            throw new NotImplementedException();
        }
    }
}
