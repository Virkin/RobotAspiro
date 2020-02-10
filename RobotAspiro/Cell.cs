using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotAspiro
{
    public class Cell
    {
        public int x { get; set; }
        public int y { get; set; }

        public Cell(int x = 0, int y = 0)
        {
            this.x = x;
            this.y = y;
        }

        public int getDistance(Cell next)
        {
            return Math.Abs(this.x - next.x) + Math.Abs(this.y - next.y);
        }

        public List<Cell> getNeighbor(int max)
        {
            List<Cell> neighbor = new List<Cell>();

            if (x > 0) { neighbor.Add(new Cell(x - 1, y)); }
            if (x < max - 1) { neighbor.Add(new Cell(x + 1, y)); }
            if (y > 0) { neighbor.Add(new Cell(x, y - 1)); }
            if (y < max - 1) { neighbor.Add(new Cell(x, y + 1)); }

            return neighbor;

        }

        public List<Cell> getCellPath(Cell end, Dictionary<Cell, Cell> tree)
        {
            List<Cell> path = new List<Cell>();

            Cell currentCell = new Cell();

            currentCell.x = end.x;
            currentCell.y = end.y;

            while (currentCell.x != this.x || currentCell.y != this.y)
            {
                Cell pathCell = new Cell(currentCell.x, currentCell.y);
                path.Add(pathCell);
                
                foreach (KeyValuePair<Cell, Cell> node in tree)
                {
                    Cell key = node.Key;
                    if (currentCell.x == key.x && currentCell.y == key.y)
                    {
                        Cell value = node.Value;

                        if (node.Value != null)
                        {
                            currentCell.x = value.x;
                            currentCell.y = value.y;
                            break;
                        }
                    }
                }

                //cell = tree[cell];
            }

            path.Add(this);

            path.Reverse();

            return path;
        }
    }
}
