using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotAspiro
{
    class Cell
    {
        public int x { get; set; }
        public int y { get; set; }

        public Cell(int x=0, int y=0)
        {
            this.x = x;
            this.y = y;
        }
    }
}
