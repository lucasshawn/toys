using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewQuestion
{
    internal class PathFinderOptions
    {
        private int stepDelayInMs = 0;
        private int[,] grid = null;

        public int StepDelayInMs { get => stepDelayInMs; set => stepDelayInMs = value; }
        public int[,] Grid { get => grid; set => grid = value; }
    }
}
