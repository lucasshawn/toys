using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewQuestion
{
    class GridGeneratorOptions
    {
        private int minWidth;
        private int maxWidth;
        private int minHeight;
        private int maxHeight;
        private int blockCount = 5;
        
        public int BlockCount { get => blockCount; set => blockCount = value; }
        public int MinWidth { get => minWidth; set => minWidth = value; }
        public int MaxWidth { get => maxWidth; set => maxWidth = value; }
        public int MinHeight { get => minHeight; set => minHeight = value; }
        public int MaxHeight { get => maxHeight; set => maxHeight = value; }
    }
}
