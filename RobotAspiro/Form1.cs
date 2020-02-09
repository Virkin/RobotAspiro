using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobotAspiro
{
    public partial class Form1 : Form
    {
        private List<Cell> dirts = new List<Cell>();
        private Cell vaccumPos;

        private int numOfCells = 5;
        private int cellSize = 100;

        private int[,] map;

        private Image dirtImage = Image.FromFile("../../D.png");
        private Image vaccumImage = Image.FromFile("../../robotAspiro.png");

        private Vaccum vaccum;
        private Queue queue;

        private Queue vaccumMove;
        private int cptMove = 0;
        private int maxMove = 60;

        private int intervalMs;

        public Form1()
        {
            InitializeComponent();

            this.queue = new Queue();
            this.vaccumMove = new Queue();

            vaccumPos = new Cell(1, 1);

            this.vaccum = new Vaccum(queue);

            Thread t = new Thread(new ThreadStart(this.vaccum.run));
            t.Start();

            map = new int[numOfCells, numOfCells];

            intervalMs = (int)(Math.Round(1000.0/maxMove));

            gameTimer.Interval = intervalMs;
            gameTimer.Tick += UpdateScreen;
            gameTimer.Start();

            vaccumMove.Enqueue('L');
            vaccumMove.Enqueue('U');
            vaccumMove.Enqueue('R');
            vaccumMove.Enqueue('D');

            StartGame();
        }

        private void StartGame()
        {
            GenerateDirt();
        }

        private int[,] getMap()
        {
            return this.map;
        }

        private void GenerateDirt()
        {
            Cell newDirt = new Cell();

            Random rand = new Random();

            Boolean findCell = false;

            while (!findCell)
            {
                newDirt.x = rand.Next(0, numOfCells);
                newDirt.y = rand.Next(0, numOfCells);

                map[newDirt.y, newDirt.x] = 1;

                findCell = true;

                foreach (Cell dirt in dirts)
                {
                    
                    if(newDirt.x == dirt.x && newDirt.y == dirt.y)
                    {
                        findCell = false;
                        break;
                    }
                }
            }
            

            dirts.Add(newDirt);

            //Console.WriteLine("x:" + newDirt.x + " | y: "+newDirt.y);
        }

        private void UpdateScreen(object sender, EventArgs e)
        {
            Random rand = new Random();
            int prob = rand.Next(0, 100);

            if (prob >= 100 - (intervalMs / 1000))
            {
                GenerateDirt();
            }

            if(vaccumMove.Count == 0)
            {
                vaccumMove.Enqueue('L');
                vaccumMove.Enqueue('U');
                vaccumMove.Enqueue('R');
                vaccumMove.Enqueue('D');
            }
           
            if (this.queue.Count > 0)
            {
                if ((string)this.queue.Dequeue() == "getMap")
                {
                    this.queue.Enqueue(getMap());
                }
            }

            board.Invalidate();
        }

        private void board_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen p = new Pen(Color.Black);

            for (int y = 0; y < numOfCells; y++)
            {
                g.DrawLine(p, 0, y * cellSize, numOfCells * cellSize, y * cellSize);
            }

            for (int x = 0; x < numOfCells; x++)
            {
                g.DrawLine(p, x * cellSize, 0, x * cellSize, numOfCells * cellSize);
            }

            RectangleF srcRect = new RectangleF(0, 0, 100, 100);
            GraphicsUnit units = GraphicsUnit.Pixel;

            foreach (Cell dirt in dirts)
            {
                // Create rectangle for source image.

                // Draw image to screen.
                g.DrawImage(dirtImage, dirt.x * cellSize, dirt.y* cellSize, srcRect, units);
            }

            if (cptMove <= maxMove)
            {
                switch (vaccumMove.Peek())
                {
                    case 'L':
                        g.DrawImage(vaccumImage, (vaccumPos.x * cellSize) - (cptMove * (cellSize / maxMove)), vaccumPos.y * cellSize, srcRect, units);
                        break;

                    case 'R':
                        g.DrawImage(vaccumImage, (vaccumPos.x * cellSize) + (cptMove * (cellSize / maxMove)), vaccumPos.y * cellSize, srcRect, units);
                        break;
                    case 'U':
                        g.DrawImage(vaccumImage, vaccumPos.x * cellSize, (vaccumPos.y * cellSize) - (cptMove * (cellSize / maxMove)), srcRect, units);
                        break;
                    case 'D':
                        g.DrawImage(vaccumImage, vaccumPos.x * cellSize, (vaccumPos.y * cellSize) + (cptMove * (cellSize / maxMove)), srcRect, units);
                        break;

                }
                cptMove++;
                Console.WriteLine("move");
            }
            else
            {
                cptMove = 0;
                char move = (char)vaccumMove.Dequeue();

                switch (move)
                {
                    case 'L':
                        vaccumPos.x -= 1;
                        break;

                    case 'R':
                        vaccumPos.x += 1;
                        break;
                    case 'U':
                        vaccumPos.y -= 1;
                        break;
                    case 'D':
                        vaccumPos.y += 1;
                        break;
                }
            }
        }
    }
}
