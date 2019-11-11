using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InterviewQuestion
{
    public delegate void OnNodeVisited(Point pt);
    public delegate void OnFinished(bool finishedSuccessfully);
    interface IPathFinder
    {
        event OnNodeVisited NodeVisited;
        event OnFinished Finished;
        void Initialize(PathFinderOptions options);
        bool FindPath(Point start, Point end);
        void Cancel();
    }
}
