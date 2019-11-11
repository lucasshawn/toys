using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewQuestion
{
    class GridGenerator
    {
        Random rand = new Random();
        GridGeneratorOptions options = null;
        public GridGenerator(GridGeneratorOptions options)
        {
            this.options = options;
        }

        public int[,] Generate()
        {
            int[,] grid = GenerateBlankGrid();
            FillBlocks(grid, options.BlockCount);
            return grid;
        }
        
        private int[,] GenerateBlankGrid()
        {
            var width = rand.Next(options.MinWidth, options.MaxWidth);
            var height = rand.Next(options.MinHeight, options.MaxHeight);
            int[,] grid = new int[width, height];
            return grid;
        }

        private void FillBlocks(int[,] grid, int blockCount)
        {
            var width = grid.GetLength(0);
            var height = grid.GetLength(1);
            while (blockCount > 0)
            {
                var index = rand.Next(width * height);
                grid[index % width, index / width] = 1;
                blockCount--;
            }
        }
    }   
}
