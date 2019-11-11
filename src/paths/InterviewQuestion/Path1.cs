using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InterviewQuestion
{
    class Path1 : IPathFinder
    {
        public event OnNodeVisited NodeVisited;
        public event OnFinished Finished;
        PathFinderOptions options = null;
        bool isCancelled = false;

        public void Initialize(PathFinderOptions options)
        {
            NodeVisited = delegate { };
            Finished = delegate { };
            this.options = options;
        }

        double calcIntFromPoint(double gridWidth, Point p)
        {
            return (p.Y * gridWidth) + p.X;
        }

        public bool FindPath(Point start, Point end)
        {
            // Basic checks before starting
            //if (start == end)
            //{
            //    this.Finished(true);
            //    return true;
            //}

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
                visited.Add(calcIntFromPoint(gridWidth, p));
                NodeVisited(p);
                //if (p.X == end.X && p.Y == end.Y)
                //{
                //    this.Finished(true);
                //    return true;
                //}

                visited.Add(calcIntFromPoint(options.Grid.GetLength(0), p));
                foreach (Point newPoint in GetNeighbors(p, visited)) pointsToProcess.Push(newPoint);
                System.Threading.Thread.Sleep(options.StepDelayInMs);
            }

            this.Finished(isCancelled);
            return false;
        }

        private IEnumerable<Point> GetNeighbors(Point p, HashSet<double> visited)
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
            List<Point> ret = new List<Point>();
            ret.AddRange(points.Where(pt =>
            {
                return (
                    CheckBounds(pt) &&
                    !visited.Contains(calcIntFromPoint(gridWidth, pt)) &&
                    options.Grid[(int)pt.X,(int)pt.Y] == 0                     
                );
            }));
            return ret;
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
