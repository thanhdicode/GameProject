using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameMenu.TetrisBlock
{
    public class GameGrid
    {
        private readonly int[,] grid;
        public int Row { get; }
        public int Col { get; }
        public int this[int r, int c]
        {
            get => grid[r, c];
            set => grid[r, c] = value;
        }
        public GameGrid(int row, int col)
        {
            Row = row;
            Col = col;
            grid = new int[row, col];
        }
        public bool IsInside(int r, int c)
        {
            return r >= 0 && r < Row && c >= 0 && c < Col;
        }
        public bool IsEmpty(int r, int c)
        {
            return IsInside(r, c) && grid[r, c] == 0;
        }
        public bool IsRowFull(int r)
        {
            for (int c = 0; c < Col; c++)
            {
                if (grid[r, c] == 0)
                {
                    return false;
                }
            }

            return true;
        }
        public bool IsRowEmpty(int r)
        {
            for (int c = 0; c < Col; c++)
            {
                if (grid[r, c] != 0)
                {
                    return false;
                }
            }

            return true;
        }
        private void ClearRow(int r)
        {
            for (int c = 0; c < Col; c++)
            {
                grid[r, c] = 0;
            }
        }
        private void MoveRowDown(int r, int numRows)
        {
            for (int c = 0; c < Col; c++)
            {
                grid[r + numRows, c] = grid[r, c];
                grid[r, c] = 0;
            }
        }
        public int ClearFullRow()
        {
            int cleared = 0;

            for (int r = Row - 1; r >= 0; r--)
            {
                if (IsRowFull(r))
                {
                    ClearRow(r);
                    cleared++;
                }
                else if (cleared > 0)
                {
                    MoveRowDown(r, cleared);
                }
            }

            return cleared;
        }
    }
}
