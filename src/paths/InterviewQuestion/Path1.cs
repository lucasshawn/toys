using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InterviewQuestion
{

    class ReverseComparer : IComparer<double>
    {
        public int Compare(double x, double y)
        {
            return x.CompareTo(y);
        }
    }

    class Path1 : IPathFinder
    {
        public event OnNodeVisited NodeVisited;
        public event OnFinished Finished;
        public event OnLog Log;
        PathFinderOptions options = null;
        bool isCancelled = false;

        public void Initialize(PathFinderOptions options)
        {
            NodeVisited = delegate { };
            Finished = delegate { };
            Log = delegate { };
            this.options = options;
        }

        double calcIntFromPoint(double gridWidth, Point p)
        {
            return (p.Y * gridWidth) + p.X;
        }

        public bool FindPath(Point start, Point end)
        {
            // Basic checks before starting
            if (start == end)
            {
                this.Finished(true);
                return true;
            }

            if (!CheckBounds(start) || !CheckBounds(end))
            {
                this.Finished(false);
                return false;
            }

            // Init
            int gridWidth = options.Grid.GetLength(0);
            HashSet<double> visited = new HashSet<double>();
            Stack<Point> pointsToProcess = new Stack<Point>();

            // Let's get this party started
            pointsToProcess.Push(start);
            while (pointsToProcess.Count() > 0 && !isCancelled)
            {
                Point p = pointsToProcess.Pop();

                // Immediately add to visited
                visited.Add(calcIntFromPoint(gridWidth, p));

                // Let subscribers know we hit a visited node
                NodeVisited(p);

                // Are we at the end?
                if (p.X == end.X && p.Y == end.Y)
                {
                    this.Finished(true);
                    return true;
                }

                // Grab neighbors (in order of closeness)
                var newPoints = GetNeighbors(p, end, visited);
                if (newPoints.Count() > 0)
                {
                    foreach (Point newPoint in newPoints)
                        pointsToProcess.Push(newPoint);
                }                    
            }

            this.Finished(isCancelled);
            return false;
        }
        private IEnumerable<Point> GetNeighbors(Point p, Point target, HashSet<double> visited)
        {
            // Up to 4 points can be found for neighbors
            Point[] points = new Point[]
            {
                new Point(p.X - 1, p.Y),    // Left
                new Point(p.X + 1, p.Y),    // Right
                new Point(p.X, p.Y - 1),    // Down
                new Point(p.X, p.Y + 1),    // Up
            };

            // Ensure the added points are legit
            int gridWidth = options.Grid.GetLength(0);
            SortedList<double, Point> proximityMap = new SortedList<double, Point>(new ReverseComparer());
            foreach(var pt in points)
            {
                if (CheckBounds(pt) &&
                    !visited.Contains(calcIntFromPoint(gridWidth, pt)) &&
                    options.Grid[(int)pt.X, (int)pt.Y] == 0)
                {
                    var xd = Math.Pow(target.X - pt.X, 2);
                    var yd = Math.Pow(target.Y - pt.Y, 2);
                    var distance = Math.Sqrt(xd + yd);
                    while (proximityMap.ContainsKey(distance)) distance += .001;
                    Log($"Point({pt}) End({target}) Distance({distance})");
                    proximityMap.Add(distance, pt);
                }
            }

            // Now pull them out in order of weighted size
            return proximityMap.Values;
        }

        private bool CheckBounds(Point p)
        {
            // Ensure point is with the boundaries
            if (p.X < 0) return false;
            if (p.Y < 0) return false;
            if (p.X + 1 > options.Grid.GetLength(0)) return false;
            if (p.Y + 1 > options.Grid.GetLength(1)) return false;
            return true;
        }

        public void Cancel()
        {
            isCancelled = true;
        }
    }
}
