using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for Board.xaml
    /// </summary>
    public partial class Board : UserControl
    {
        List<Point> visited = new List<Point>();
        PathFinderOptions options = null;

        public Board()
        {
            InitializeComponent();
            this.SizeChanged += Board_SizeChanged;
        }

        private void Board_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitBoard();
        }

        internal void Initialize(PathFinderOptions options)
        {
            this.options = options;
            InitBoard();
        }

        public int[,] Grid
        {
            get => options.Grid;
        }

        public void AddVisited(Point pt)
        {
            visited.Add(pt);
            AddVisitedToBoard(pt);
        }

        private void InitBoard()
        {
            if (options == null) return;
            var cWidth = canvas.ActualWidth - 0;
            var cHeight = canvas.ActualHeight - 0;
            Rect r = new Rect(new Size(cWidth / Grid.GetLength(0), cHeight / Grid.GetLength(1)));
            canvas.Children.Clear();
            visited.Clear();

            // Add vertical lines
            for (double x = 0; x < cWidth+1; x += r.Width)
                AddLineToBoard(x, x, 0, cHeight);

            // Add horizontal lines
            for (double y = 0; y < cHeight+1; y += r.Height)
                AddLineToBoard(0, cWidth, y, y);

            // Add any visited rects
            foreach (var p in this.visited) AddVisitedToBoard(p);

            // Walk the board and add blocks
            AddBlocksToBoard(Grid);
        }

        private void AddBlocksToBoard(int[,] grid)
        {
            for (var x = 0; x < grid.GetLength(0); x++)
                for (var y = 0; y < grid.GetLength(1); y++)
                    if (grid[x, y] == 1) AddBlockToBoard(new Point(x, y));
        }

        private void AddLineToBoard(double x1, double x2, double y1, double y2)
        {
            Line line = new Line();
            line.SnapsToDevicePixels = true;
            line.Stroke = Brushes.Black;
            line.StrokeThickness = 2;
            line.X1 = x1;
            line.X2 = x2;
            line.Y1 = y1;
            line.Y2 = y2;
            canvas.Children.Add(line);
        }

        private void AddBlockToBoard(Point pt)
        {
            AddRectToBoard(pt, Brushes.Black, Brushes.Black);
        }

        private void AddVisitedToBoard(Point p)
        {
            AddRectToBoard(p, Brushes.Black, Brushes.Yellow);
        }

        private void AddRectToBoard(Point pt, Brush stroke, Brush fill)
        {
            var rWidth = canvas.ActualWidth / Grid.GetLength(0);
            var rHeight = canvas.ActualHeight / Grid.GetLength(1);

            Rectangle r = new Rectangle();
            r.SnapsToDevicePixels = true;
            r.Stroke = stroke;
            r.Fill = fill;
            r.Width = rWidth;
            r.Height = rHeight;
            canvas.Children.Add(r);
            Canvas.SetLeft(r, pt.X * rWidth);
            Canvas.SetTop(r, pt.Y * rHeight);
        }
    }
}
