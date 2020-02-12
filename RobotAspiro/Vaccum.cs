using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotAspiro
{
    class Vaccum
    {
        private int[,] beliefs;
        private List<Cell> desires = new List<Cell>();
        private List<char> intentions = new List<char>();

        private int[,] scores;
        private int maxActions = 10;
        private int nbActions;
        private int bestNbActions;
        private double probToChange = 1;
        private int battery;
        private int timer;

        private Cell myCell;

        private Form1 env;

        private Stopwatch stopWatch = new Stopwatch();

        public Vaccum(Form1 env)
        {
            this.env = env;
            scores = new int[maxActions, 3];

            stopWatch.Start();

            nbActions = maxActions;
            bestNbActions = maxActions;
        }

        public void checkPerformance()
        {
            scores[nbActions - 1, 0] += 1;
            scores[nbActions - 1, 1] += this.env.getVaccumScore();
            scores[nbActions - 1, 2] = scores[nbActions - 1, 1] / scores[nbActions - 1, 0];

            Console.WriteLine("### Scores ###");

            for (int i = 0; i < scores.GetLength(0); i++)
            {
                for (int j = 0; j < scores.GetLength(1); j++)
                {
                    Console.Write('|'+scores[i, j].ToString());
                }
                Console.WriteLine();
            }

            Console.WriteLine();

            for (int i = 0; i < scores.GetLength(0); i++)
            {
                if(scores[i, 0] > 0)
                {
                    if(scores[i,2] > scores[bestNbActions-1, 2])
                    {
                        bestNbActions = i + 1;
                    }
                }
            }

            Random rand = new Random();

            double willChange = (double) rand.Next(100) / 100;

            if(willChange > 1 - probToChange)
            {
               if(nbActions == maxActions) { nbActions--; }
               else if(nbActions == 1) { nbActions++; }
               else
               {
                    int moreOrLess = rand.Next(2);

                    switch(moreOrLess)
                    {
                        case 0:
                            nbActions--;
                            break;
                        case 1:
                            nbActions++;
                            break;

                    }
               }

               probToChange -= 0.01;
            }
            else
            {
                nbActions = bestNbActions;
            }

            Console.WriteLine("nbActionsChoose:" + nbActions);
        }

        public void run()
        {
            while(true)
            {
                useSensors();

                /*for (int i = 0; i < beliefs.GetLength(0); i++)
                {
                    for (int j = 0; j < beliefs.GetLength(1); j++)
                    {
                        Console.Write(beliefs[i, j]);
                    }
                    Console.WriteLine();
                }*/

                //Console.WriteLine();

                updateMyState(myCell);

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

        public void updateMyState(Cell actualCell, int dist=0)
        {
            Cell currentCell = new Cell();
            Cell bestCell = null;
            
            int nbDirt = 0;

            int[,] map = (int[,]) beliefs.Clone();

            while(dist < maxActions)
            {   
                for (int i = 0; i < map.GetLength(0); i++)
                {
                    for (int j = 0; j < map.GetLength(1); j++)
                    {
                        if (map[i, j] > 0)
                        {
                            currentCell.x = j;
                            currentCell.y = i;

                            if (bestCell == null)
                            {
                                bestCell = new Cell(currentCell.x, currentCell.y);
                            }
                            else if (actualCell.getDistance(currentCell) < actualCell.getDistance(bestCell))
                            {
                                bestCell.x = currentCell.x;
                                bestCell.y = currentCell.y;
                                dist += actualCell.getDistance(bestCell);
                            }

                            nbDirt++;
                        }
                    }
                }
                if (nbDirt == 0)
                {
                    desires.Add(new Cell(2, 2));
                    break;
                }
                else
                {
                    desires.Add(new Cell(bestCell.x, bestCell.y));
                    map[bestCell.y, bestCell.x] = 0;
                }

                actualCell = new Cell(bestCell.x,bestCell.y);
                bestCell = null;

                nbDirt = 0;
            } 

            //Console.WriteLine("Dist : " + myCell.getDistance(bestCell));


            if(stopWatch.ElapsedMilliseconds > 60000/env.getSpeed())
            {
                checkPerformance();
                stopWatch.Reset();
                stopWatch.Start();
            }
        }

        public void chooseAnAction()
        {
            // Breadth - first search

            Cell startCell = myCell;
            Cell endCell;

            List<Cell> path = new List<Cell>();

            foreach (Cell desire in desires)
            {
                endCell = new Cell(desire.x, desire.y);

                path.Clear();

                //path = BFS(startCell, endCell);
                path = Greedy(startCell, endCell);

                setIntentions(path);

                startCell = new Cell(endCell.x, endCell.y);
            }
        }

        public List<Cell> BFS(Cell start, Cell end)
        {
            Dictionary<Cell, Cell> tree = new Dictionary<Cell, Cell> { { start, null } };
            Queue<Cell> frontier = new Queue<Cell>();

            frontier.Enqueue(start);

            Cell currentCell;

            while (frontier.Count > 0)
            {
                currentCell = frontier.Dequeue();

                if (currentCell.x == end.x && currentCell.y == end.y) break;

                foreach (Cell cell in currentCell.getNeighbor(env.getNumOfCells()))
                {
                    if (tree.ContainsKey(cell)) { continue; };

                    frontier.Enqueue(cell);
                    tree.Add(cell, currentCell);
                }
            }

            return start.getCellPath(end, tree);
        }

        public List<Cell> Greedy(Cell start, Cell end)
        {
            
            //Console.WriteLine("end cell :\n x:" + endCell.x + "y :" + endCell.y);

            Cell currentCell;

            Dictionary<Cell, Cell> tree = new Dictionary<Cell, Cell> { { start, null } };
            //List<(Cell, int)> frontier = new List<(Cell, int)> {(startCell, 1000)};
            Queue<(Cell, int)> frontier = new Queue<(Cell, int)>();

            frontier.Enqueue((start, 1000));
            //frontier.Add((startCell, 0));

            while (frontier.Count > 0)
            {
                //currentCell = frontier[0].Item1;
                //frontier.RemoveAt(0);
                currentCell = frontier.Dequeue().Item1;

                if (currentCell.x == end.x && currentCell.y == end.y) break;

                foreach (Cell cell in currentCell.getNeighbor(env.getNumOfCells()))
                {
                    if (tree.ContainsKey(cell)) { continue; }

                    //frontier.Enqueue(cell);
                    frontier.Enqueue((cell, end.getDistance(cell)));
                    tree.Add(cell, currentCell);
                }

                frontier = sortFrontier(frontier);
            }

            return start.getCellPath(end, tree);
        }

        public Queue<(Cell, int)> sortFrontier(Queue<(Cell, int)> frontier)
        {
            int i, j;

            int frontierSize = frontier.Count();

            List<(Cell, int)> listFrontier = new List<(Cell, int)>();
            Queue<(Cell, int)> newFrontier = new Queue<(Cell, int)>();

            for (i = 0; i < frontierSize; i++)
            {
                listFrontier.Add(frontier.Dequeue());
            }

            (Cell, int) temp;

            for (i = 1; i < listFrontier.Count; i++)
            {
                j = i;
                while (j > 0 && listFrontier[j - 1].Item2 > listFrontier[j].Item2)
                {
                    temp = listFrontier[j];
                    listFrontier[j] = listFrontier[j - 1];
                    listFrontier[j - 1] = temp;
                    j--;
                }
            }

            for (i = 0; i < frontierSize; i++)
            {
                newFrontier.Enqueue(listFrontier[i]);
            }

            return newFrontier;
        }

        public List<char> addActions(List<char> actions, char action)
        {
            if(actions.Count < nbActions)
            {
                actions.Add(action);
            }

            return actions;
        }
        
        public void setIntentions(List<Cell> path)
        {
            List<char> actions = new List<char>();

            for(int i=0; i<path.Count; i++)
            {
                Cell cell = new Cell(path[i].x, path[i].y);
                
                int val = beliefs[cell.y,cell.x];

                switch(val)
                {
                    case 1:
                        actions = addActions(actions, 'C');
                        break;
                    case 2:
                        actions = addActions(actions, 'P');
                        break;
                    case 3:
                        actions = addActions(actions, 'P');
                        actions = addActions(actions, 'C');
                        break;
                }

                if(i < path.Count -1)
                {
                    Cell nextCell = new Cell(path[i + 1].x, path[i + 1].y);
                    
                    if(cell.x - nextCell.x > 0) { actions = addActions(actions, 'L'); }
                    else if (cell.x - nextCell.x < 0) { actions = addActions(actions, 'R'); }
                    else if (cell.y - nextCell.y > 0) { actions = addActions(actions, 'U'); }
                    else if (cell.y - nextCell.y < 0) { actions = addActions(actions, 'D'); }

                }
            }

            this.intentions.AddRange(actions);
        }

        public void doIt()
        {
            env.setVaccumMove(this.intentions);

            this.intentions.Clear();
            this.desires.Clear();
        }
    }
}
