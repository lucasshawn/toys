using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InterviewQuestion
{
    class Path2 : IPathFinder
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

        double hashPoint(double gridWidth, Point p)
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
            SortedList<double, Point> priQueue = new SortedList<double, Point>(new ReverseComparer());

            // Let's get this party started
            Enqueue(priQueue, start, end);
            while (priQueue.Keys.Count() > 0 && !isCancelled)
            {
                var p = Dequeue(priQueue);

                // Immediately add to visited
                visited.Add(hashPoint(gridWidth, p));

                // Let subscribers know we hit a visited node
                NodeVisited(p);

                // Are we at the end?
                if (p.X == end.X && p.Y == end.Y)
                {
                    this.Finished(true);
                    return true;
                }

                foreach (Point newPoint in GetNeighbors(p, end, visited))
                    Enqueue(priQueue, newPoint, end);
            }

            this.Finished(isCancelled);
            return false;
        }

        private Point Dequeue(SortedList<double, Point> priQueue)
        {
            Point p = priQueue[priQueue.Keys[0]];
            priQueue.Remove(priQueue.Keys[0]);
            return p;
        }

        private void Enqueue(SortedList<double, Point> priQueue, Point current, Point target)
        {
            var distance = GetDistance(current, target);
            while (priQueue.ContainsKey(distance)) distance += .001;
            priQueue.Add(distance, current);
        }

        private double GetDistance(Point p1, Point p2)
        {
            var xd = Math.Pow(p2.X - p1.X, 2);
            var yd = Math.Pow(p2.Y - p1.Y, 2);
            return Math.Sqrt(xd + yd);
        }
        private IEnumerable<Point> GetNeighbors(Point p, Point target, HashSet<double> visited)
        {
            // Up to 4 points can be found for neighbors
            List<Point> points = new List<Point>
            {
                new Point(p.X - 1, p.Y),    // Left
                new Point(p.X + 1, p.Y),    // Right
                new Point(p.X, p.Y - 1),    // Down
                new Point(p.X, p.Y + 1),    // Up
            };

            // Remove bad neighbors
            List<Point> retList = new List<Point>();
            int gridWidth = options.Grid.GetLength(0);
            retList.AddRange(points.Where(current =>
            {
                return (CheckBounds(current) &&
                    !visited.Contains(hashPoint(gridWidth, current)) &&
                    options.Grid[(int)current.X, (int)current.Y] == 0);
            }));
            Debug.WriteLine($"Neighbors: {string.Join("|",retList)}");

            return points;
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
