using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotAspiro
{
    class Vaccum
    {
        private int[,] beliefs;
        private Cell desires = new Cell();
        private List<char> intentions = new List<char>();

        private int score;
        private int nbActions;

        private Cell myCell;

        private Form1 env;

        public Vaccum(Form1 env)
        {
            this.env = env;
        }

        public void run()
        {
            while(true)
            {
                useSensors();

                for (int i = 0; i < beliefs.GetLength(0); i++)
                {
                    for (int j = 0; j < beliefs.GetLength(1); j++)
                    {
                        Console.Write(beliefs[i, j]);
                    }
                    Console.WriteLine();
                }

                Console.WriteLine();

                updateMyState();

                //Console.WriteLine("x : " + desires.x + "y : " + desires.y);

                chooseAnAction();
                doIt();

                //System.Threading.Thread.Sleep(1000);
            }
        }

        public void useSensors()
        {
            this.beliefs = env.getMap();

            this.myCell = env.GetVaccumPos();

        }

        public void updateMyState()
        {
            Cell currentCell = new Cell();
            Cell bestCell = new Cell(99,99);

            int nbDirt = 0;

            for (int i = 0; i < beliefs.GetLength(0); i++)
            {
                for (int j = 0; j < beliefs.GetLength(1); j++)
                {
                    if (beliefs[i,j] > 0)
                    {
                        currentCell.x = j;
                        currentCell.y = i;

                        if(bestCell == null)
                        {
                            bestCell = new Cell(currentCell.x, currentCell.y);
                        }
                        else if(myCell.getDistance(currentCell) < myCell.getDistance(bestCell))
                        {
                            bestCell.x = currentCell.x;
                            bestCell.y = currentCell.y;
                        }

                        nbDirt++;
                    }
                }
            }

            if(nbDirt == 0)
            {
                bestCell.x = 2;
                bestCell.y = 2;
            }

            //Console.WriteLine("Dist : " + myCell.getDistance(bestCell));

            this.desires.x = bestCell.x;
            this.desires.y = bestCell.y;


        }

        public void chooseAnAction()
        {
            // Breadth - first search

            Cell startCell = myCell;
            Cell endCell = desires;

            //Console.WriteLine("end cell :\n x:" + endCell.x + "y :" + endCell.y);

            Cell currentCell;

            Dictionary<Cell, Cell> tree = new Dictionary<Cell, Cell> { { myCell, null } };
            Queue<Cell> frontier = new Queue<Cell>();

            frontier.Enqueue(startCell);

            while (frontier.Count > 0)
            {
                currentCell = frontier.Dequeue();

                if (currentCell.x == endCell.x && currentCell.y == endCell.y) break;

                foreach (Cell cell in currentCell.getNeighbor(env.getNumOfCells()))
                {
                    if (tree.ContainsKey(cell)) { continue; };

                    frontier.Enqueue(cell);
                    tree.Add(cell, currentCell);
                } 
            }

            List<Cell> path = myCell.getCellPath(endCell, tree);
            setIntentions(path);
        }

        public void setIntentions(List<Cell> path)
        {
            this.intentions.Clear();
            
            List<char> actions = new List<char>();

            for(int i=0; i<path.Count; i++)
            {
                Cell cell = new Cell(path[i].x, path[i].y);
                
                int val = beliefs[cell.y,cell.x];

                switch(val)
                {
                    case 1:
                        actions.Add('C');
                        break;
                    case 2:
                        actions.Add('P');
                        break;
                    case 3:
                        actions.Add('P');
                        actions.Add('C');
                        break;
                }

                if(i < path.Count -1)
                {
                    Cell nextCell = new Cell(path[i + 1].x, path[i + 1].y);
                    
                    if(cell.x - nextCell.x > 0) { actions.Add('L'); }
                    else if (cell.x - nextCell.x < 0) { actions.Add('R'); }
                    else if (cell.y - nextCell.y > 0) { actions.Add('U'); }
                    else if (cell.y - nextCell.y < 0) { actions.Add('D'); }

                }
            }

            this.intentions = actions;
        }

        public void doIt()
        {
            env.setVaccumMove(this.intentions);
        }
    }
}
