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
        // Croyance du robot
        private int[,] beliefs;

        // Désires du robot
        private List<Cell> desires = new List<Cell>();
        
        // Intentions du robot
        private List<char> intentions = new List<char>();

        // Score
        private int[,] scores;
        
        // Nombre maximal d'action possible que le robot peut faire
        private int maxActions = 10;

        // Nombre d'action actuel choisi par le robot
        private int nbActions;

        // La meilleur action actuelle
        private int bestNbActions;

        // Probabilité de changé de nombre d'action réalisable même si potentiel inférieur à la meilleur
        private double probToChange = 1;

        // La cellule du robot
        private Cell myCell;

        // L'environnement
        private Form1 env;

        // Chronomètre pour un appretissage régulier
        private Stopwatch stopWatch = new Stopwatch();

        // Constructeur
        public Vaccum(Form1 env)
        {
            // Enregistrement de l'environneent
            this.env = env;

            /* Initialisation du tableau de score pour l'apprentissage
               Il y 3 colonnes pour chaque nombre d'action : 
               - Nombre d'essaies, 
               - Sommes des essaies 
               - Moyenne */
            scores = new int[maxActions, 3];

            // Démarrage du chronomètre
            stopWatch.Start();

            // On définit le nombre initial d'action comme le maximum possible
            nbActions = maxActions;
            bestNbActions = maxActions;
        }

        // Méthode pour l'appentissage du robot
        public void checkPerformance()
        {
            // On incrémente de 1 le nombre d'essaie
            scores[nbActions - 1, 0] += 1;
            // On ajoute le nouveau du dernier essaie à la somme existante
            scores[nbActions - 1, 1] += this.env.getVaccumScore();
            // Calcule de la moyenne des essaies
            scores[nbActions - 1, 2] = scores[nbActions - 1, 1] / scores[nbActions - 1, 0];

            // Affichage du tableau dans le terminal pour visualiser l'évolution de l'apprentissage

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

            // On regarde quel est le meilleur nombre d'action selon les moyennes actuelles

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

            /* On réalise un recuit-simulé pour choisir quel nombre d'action choisir
               Au début du lancement de programme on acceptera très souvent de choisir un nombre d'action qui n'est pas la meilleure
               pour vérifier si elle est vraiment mauvaise.
               L'environnement genèrant de manière aléatoire la poussière et le bijoux, le score obtenu varie beaucoup
               Mais en effectuant la moyenne des scores, on remarque , au fur et à mesure,
               que plus le nombre d'action est petit plus le score est bon.
               Finalement, quand le programme est lancé depuis longtemps, on va prendre très souvent la meilleure solution.*/

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
                // Boucle de l'agent
                
                // Utilisation des senseurs pour observer l'environnemnt (complètement observable dans notre cas)
                useSensors();

                // Mis à jour de l'état mental du robot
                updateMyState(myCell);

                // Recherche des actions à effectuer pour satisfaire les désires du robot
                chooseAnAction();

                // Envoi des intentions vers l'environnement grâce aux effecteurs
                doIt();

            }
        }

        // Utilisaton des senseurs
        public void useSensors()
        {
            this.beliefs = env.getMap();

            this.myCell = env.GetVaccumPos();

        }

        // Mis à jour de l'état mentale
        public void updateMyState(Cell actualCell, int dist=0)
        {
            Cell currentCell = new Cell();
            Cell bestCell = null;
            
            int nbDirt = 0;

            int[,] map = (int[,]) beliefs.Clone();

            /* On regarde le score obtenu toute les minutes (temps ajustable en change le paramètre speed de l'environnemnt) 
               et on redéfini le nombre d'action possible */ 
            if (stopWatch.ElapsedMilliseconds > 60000 / env.getSpeed())
            {
                checkPerformance();
                stopWatch.Reset();
                stopWatch.Start();
            }

            /* Tant que l'on peut faire encore des actions,on cherche une nouvelle cellule
               On cherchera toujous la plus proche selon la dernière trouvé*/

            while (dist < nbActions)
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
                    // On revient à l'état initial (au millieu) si aucune case n'est sale
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
        }


        // Choix des actions à effectuer pour réaliser les désires
        public void chooseAnAction()
        {
            Cell startCell = myCell;
            Cell endCell;

            List<Cell> path = new List<Cell>();

            foreach (Cell desire in desires)
            {
                endCell = new Cell(desire.x, desire.y);

                path.Clear();

                // On peut choisir entre Breadth-first search ou un greedy search pour l'exploration
                // Ici, le BFS est optimal car le coût par étape vaut 1

                path = BFS(startCell, endCell);
                //path = Greedy(startCell, endCell);

                setIntentions(path);

                startCell = new Cell(endCell.x, endCell.y);
            }
        }

        // Algorithme du Breadth-first search
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

        // Algorithme du greedy search
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

        // Ajoute une action dans la liste seulement si sa taille est inférieur au nombre d'actions autorisés
        public List<char> addActions(List<char> actions, char action)
        {
            if(actions.Count < nbActions)
            {
                actions.Add(action);
            }

            return actions;
        }
        
        // Mis à jours des intentions
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
                        actions = addActions(actions, 'T');
                        break;
                    case 3:
                        actions = addActions(actions, 'T');
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

        // Envoi des actions vers l'environnement
        public void doIt()
        {
            env.setVaccumMove(this.intentions);

            this.intentions.Clear();
            this.desires.Clear();
        }
    }
}
