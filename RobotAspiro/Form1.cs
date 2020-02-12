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
        private int cptDirt;

        private List<Cell> jewells = new List<Cell>();
        private int cptJewell;

        private Cell vaccumPos;
        private int vaccumScore = 0;

        private int numOfCells = 5;
        private int cellSize = 100;

        private int[,] map;

        private Image dirtImage = Image.FromFile("../../dirt.png");
        private Image jewellImage = Image.FromFile("../../jewell.png");
        private Image dirtJewellImage = Image.FromFile("../../DJ.png");
        private Image vaccumImage = Image.FromFile("../../robotAspiro.png");

        private Vaccum vaccum;

        private Queue vaccumMove;
        
        private int cptMove = 0;
        private int maxMove = 60;
        private double speed = 2;

        private int intervalMs;

        public Form1()
        {
            InitializeComponent();

            this.vaccumMove = new Queue();

            vaccumPos = new Cell(1, 1);

            this.vaccum = new Vaccum(this);

            Thread t = new Thread(new ThreadStart(this.vaccum.run));
            t.Start();

            map = new int[numOfCells, numOfCells];

            intervalMs = (int)(Math.Round(1000.0/maxMove));

            gameTimer.Interval = intervalMs;
            gameTimer.Tick += UpdateScreen;
            gameTimer.Start();

            /*vaccumMove.Enqueue('L');
            vaccumMove.Enqueue('U');
            vaccumMove.Enqueue('R');
            vaccumMove.Enqueue('D');*/

        }

        public Cell GetVaccumPos()
        {
            return vaccumPos;
        }

        public int getVaccumScore()
        {
            int score = vaccumScore;
            
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if(map[i,j] == 0)
                    {
                        score += 1;
                    }
                }
            }

            vaccumScore = 0;
            return score;
        }

        public int getNumOfCells()
        {
            return numOfCells;
        }

        public int[,] getMap()
        {
            
            return (int[,]) map.Clone();
        }

        public double getSpeed()
        {
            return speed;
        }

        public void setVaccumMove(List<char> vaccumActions)
        { 
            foreach(char action in vaccumActions)
            {
                vaccumMove.Enqueue(action);
            }

            while(vaccumMove.Count > 0) { continue; }
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

            if (map[newDirt.y, newDirt.x] == 2)
            {
                map[newDirt.y, newDirt.x] = 3;
            }
            else
            {
                map[newDirt.y, newDirt.x] = 1;
            }

            dirts.Add(newDirt);

            //Console.WriteLine("x:" + newDirt.x + " | y: "+newDirt.y);
        }

        private void GenerateJewel()
        {
            Cell newJewell = new Cell();

            Random rand = new Random();

            Boolean findCell = false;

            while (!findCell)
            {
                newJewell.x = rand.Next(0, numOfCells);
                newJewell.y = rand.Next(0, numOfCells);

                findCell = true;

                foreach (Cell jewell in jewells)
                {

                    if (newJewell.x == jewell.x && newJewell.y == jewell.y)
                    {
                        findCell = false;
                        break;
                    }
                }
            }

            if (map[newJewell.y, newJewell.x] == 1)
            {
                map[newJewell.y, newJewell.x] = 3;
            }
            else
            {
                map[newJewell.y, newJewell.x] = 2;
            }

            jewells.Add(newJewell);
        }

        private void UpdateScreen(object sender, EventArgs e)
        {

            if (cptDirt > 80)
            {
                GenerateDirt();
                cptDirt = 0;
            }
           
            cptDirt++; 

            if(cptJewell > 100)
            {
                GenerateJewel();
                cptJewell = 0;
            }

            cptJewell++;

            /*if(vaccumMove.Count == 0)
            {
                vaccumMove.Enqueue('L');
                vaccumMove.Enqueue('U');
                vaccumMove.Enqueue('R');
                vaccumMove.Enqueue('D');
            }*/

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

            /*foreach (Cell dirt in dirts)
            {
                // Create rectangle for source image.

                // Draw image to screen.
                g.DrawImage(dirtImage, dirt.x * cellSize, dirt.y* cellSize, srcRect, units);
            }

            foreach (Cell dirt in dirts)
            {
                // Create rectangle for source image.

                // Draw image to screen.
                g.DrawImage(dirtImage, dirt.x * cellSize, dirt.y * cellSize, srcRect, units);
            }*/

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if(map[i,j] > 0)
                    {
                        Image image = dirtImage;
                        switch (map[i, j])
                        {
                            case 1:
                                image = dirtImage;
                                g.DrawImage(image, j * cellSize, i * cellSize, srcRect, units);
                                break;
                            case 2:
                                image = jewellImage;
                                g.DrawImage(image, j * cellSize, i * cellSize, srcRect, units);
                                break;
                            case 3:
                                image = dirtImage;
                                g.DrawImage(image, j * cellSize, i * cellSize, srcRect, units);
                                image = jewellImage;
                                g.DrawImage(image, j * cellSize, i * cellSize, srcRect, units);
                                break;
                        }
                        
                    }  
                }
            }

            if (vaccumMove.Count > 0)
            {
                if (cptMove <= maxMove/speed)
                {
                    switch (vaccumMove.Peek())
                    {
                        case 'L':
                            g.DrawImage(vaccumImage, cellSize * (vaccumPos.x - ((float)speed * cptMove / maxMove)), vaccumPos.y * cellSize, srcRect, units);
                            break;

                        case 'R':
                            g.DrawImage(vaccumImage, cellSize * (vaccumPos.x + ((float)speed * cptMove / maxMove)), vaccumPos.y * cellSize, srcRect, units);
                            break;

                        case 'U':
                            g.DrawImage(vaccumImage, vaccumPos.x * cellSize, cellSize * (vaccumPos.y - ((float)speed * cptMove / maxMove)), srcRect, units);
                            break;

                        case 'D':
                            g.DrawImage(vaccumImage, vaccumPos.x * cellSize, cellSize * (vaccumPos.y + ((float)speed * cptMove / maxMove)), srcRect, units);
                            break;
                        case 'C':
                            g.DrawImage(vaccumImage, vaccumPos.x * cellSize, vaccumPos.y * cellSize, srcRect, units);

                            if (map[vaccumPos.y, vaccumPos.x] == 1)
                            {
                                vaccumScore += 3;
                            }
                            else if (map[vaccumPos.y, vaccumPos.x] > 1)
                            {
                                vaccumScore -= 20;
                            }

                            map[vaccumPos.y, vaccumPos.x] = 0;

                            foreach (Cell dirt in dirts)
                            {

                                if (vaccumPos.x == dirt.x && vaccumPos.y == dirt.y)
                                {
                                    dirts.Remove(dirt);
                                    break;
                                }
                            }

                            cptMove = (int) (maxMove/speed);
                            break;
                        case 'P':
                            g.DrawImage(vaccumImage, vaccumPos.x * cellSize, vaccumPos.y * cellSize, srcRect, units);
                            
                            if(map[vaccumPos.y, vaccumPos.x] == 3)
                            {
                                map[vaccumPos.y, vaccumPos.x] = 1;
                            }
                            else if(map[vaccumPos.y, vaccumPos.x] == 2)
                            {
                                map[vaccumPos.y, vaccumPos.x] = 0;
                            }

                            foreach (Cell jewell in jewells)
                            {

                                if (vaccumPos.x == jewell.x && vaccumPos.y == jewell.y)
                                {
                                    jewells.Remove(jewell);
                                    vaccumScore += 3;
                                    break;
                                }
                            }

                            cptMove = (int) (maxMove/speed);
                            break;
                    }

                    /*if (cptMove == 0 && map[vaccumPos.y, vaccumPos.x] > 0)
                    {
                        vaccumScore -= 5;
                    }*/

                    cptMove++;
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

                    vaccumScore -= 1;

                    g.DrawImage(vaccumImage, vaccumPos.x * cellSize, vaccumPos.y * cellSize, srcRect, units);

                }
            }
            
        }
    }
}
