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
        
        // Liste de poussière
        private List<Cell> dirts = new List<Cell>();
        // Compteur de tempo de pop de poussière
        private int cptDirt;

        // Liste de bijoux
        private List<Cell> jewells = new List<Cell>();
        // Liste de compteur de bijoux
        private int cptJewell;

        // Position du robot
        private Cell vaccumPos;
        // Score du robot
        private int vaccumScore = 0;

        // Nombre de cellule pour un coté du plateau
        private int numOfCells = 5;
        // Taille des cellules (en px)
        private int cellSize = 100;

        // Tableau de la carte de l'environnemet pour savoir rapidement l'état actuel
        private int[,] map;

        // Chargement des sprites
        private Image dirtImage = Image.FromFile("../../dirt.png");
        private Image jewellImage = Image.FromFile("../../jewell.png");
        private Image vaccumImage = Image.FromFile("../../robotAspiro.png");

        // Le robot
        private Vaccum vaccum;

        // Queue avec les actions envoyé par le roboy
        private Queue vaccumMove;
        
        // Compteur de mouvement pour l'animation du robot
        private int cptMove = 0;
        private int maxMove = 60;

        // Vitesse du robot
        private double speed = 2;

        // Intervale de temps entre chaque refresh de l'interface
        private int intervalMs;

        // Constructeur de l'environnement
        public Form1()
        {
            // Initialisation des elements de l'interface
            InitializeComponent();

            // Initialisation de la queue de mouvement
            this.vaccumMove = new Queue();

            // Position initial du robot
            vaccumPos = new Cell(1, 1);

            // Instantiation du robot
            this.vaccum = new Vaccum(this);

            // Lancement du thread pour le robot
            Thread t = new Thread(new ThreadStart(this.vaccum.run));
            t.Start();

            // Initialisation de la map
            map = new int[numOfCells, numOfCells];

            // Initialisation des paramètre de rafraichissement de l'interface
            intervalMs = (int)(Math.Round(1000.0/maxMove));

            gameTimer.Interval = intervalMs;
            gameTimer.Tick += UpdateScreen;
            gameTimer.Start();

        }

        // Retourne la position du robot
        public Cell GetVaccumPos()
        {
            return vaccumPos;
        }

        // Retourne le score du robot
        public int getVaccumScore()
        {
            int score = vaccumScore;

            // On ajoute +1 au score pour chaque case propre à chaque appel du getter.
            
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

        // Retourne la taille de l'environnement
        public int getNumOfCells()
        {
            return numOfCells;
        }

        // Retourne la map
        public int[,] getMap()
        {
            
            return (int[,]) map.Clone();
        }


        // Retourne la vitesse
        public double getSpeed()
        {
            return speed;
        }

        // Insert les actions dans la queue de mouvement du robot
        public void setVaccumMove(List<char> vaccumActions)
        { 
            foreach(char action in vaccumActions)
            {
                vaccumMove.Enqueue(action);
            }

            while(vaccumMove.Count > 0) { continue; }
        }

        // Génère une nouvelle poussière
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

        // Génère un nouveau bijoux
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

        // Méthode appelé au début de chaque rafraichissement
        private void UpdateScreen(object sender, EventArgs e)
        {

            /* On génère de nouvelles poussière ou bijoux régulièremnent
               Ici, on génère légèrement plus de poussière mais les valeurs dans les ifs peuvent être modifié selon les envies */

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

           
            // Force le rafraichissement du board
            board.Invalidate();
        }

        // "Repeint" le plateau avec les éléments mis à jours (nouvelles poussières, mouvement du robot, etc...)
        private void board_Paint(object sender, PaintEventArgs e)
        {
            // Tracage des lignes du plateau
            
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

            // Affichage des poussière et des bijoux

            RectangleF srcRect = new RectangleF(0, 0, 100, 100);
            GraphicsUnit units = GraphicsUnit.Pixel;

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if(map[i,j] > 0)
                    {
                        Image image = dirtImage;
                        switch (map[i, j])
                        {
                            // 1 = case poussière
                            case 1: 
                                image = dirtImage;
                                g.DrawImage(image, j * cellSize, i * cellSize, srcRect, units);
                                break;
                            
                            // 2 = case bijoux
                            case 2:
                                image = jewellImage;
                                g.DrawImage(image, j * cellSize, i * cellSize, srcRect, units);
                                break;
                            
                            // 3 = case poussière/bijoux
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

            // Animation du robot

            if (vaccumMove.Count > 0)
            {
                if (cptMove <= maxMove/speed)
                {
                    switch (vaccumMove.Peek())
                    {
                        // L = Left
                        case 'L':
                            g.DrawImage(vaccumImage, cellSize * (vaccumPos.x - ((float)speed * cptMove / maxMove)), vaccumPos.y * cellSize, srcRect, units);
                            break;

                        // R = Right
                        case 'R':
                            g.DrawImage(vaccumImage, cellSize * (vaccumPos.x + ((float)speed * cptMove / maxMove)), vaccumPos.y * cellSize, srcRect, units);
                            break;

                        // U = Up
                        case 'U':
                            g.DrawImage(vaccumImage, vaccumPos.x * cellSize, cellSize * (vaccumPos.y - ((float)speed * cptMove / maxMove)), srcRect, units);
                            break;

                        // D = Down
                        case 'D':
                            g.DrawImage(vaccumImage, vaccumPos.x * cellSize, cellSize * (vaccumPos.y + ((float)speed * cptMove / maxMove)), srcRect, units);
                            break;

                        // C = Clean
                        case 'C':
                            g.DrawImage(vaccumImage, vaccumPos.x * cellSize, vaccumPos.y * cellSize, srcRect, units);

                            if (map[vaccumPos.y, vaccumPos.x] == 1)
                            {
                                // On ajoute 3 points au score pour chaque poussière aspirer
                                vaccumScore += 3;
                            }
                            else if (map[vaccumPos.y, vaccumPos.x] > 1)
                            {
                                // Mais on en enlève 20 pour chaque bijoux ramasser par erreur
                                vaccumScore -= 20;
                            }

                            // Mis à jours du tableau du plateau et listes en conséquences

                            map[vaccumPos.y, vaccumPos.x] = 0;

                            foreach (Cell dirt in dirts)
                            {

                                if (vaccumPos.x == dirt.x && vaccumPos.y == dirt.y)
                                {
                                    dirts.Remove(dirt);
                                    break;
                                }
                            }

                            foreach (Cell jewell in jewells)
                            {

                                if (vaccumPos.x == jewell.x && vaccumPos.y == jewell.y)
                                {
                                    jewells.Remove(jewell);
                                    break;
                                }
                            }

                            cptMove = (int) (maxMove/speed);
                            break;

                        // T = Take
                        case 'T':
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
                                    
                                    // On ajoute 3 points au score pour chaque bijoux pris.
                                    
                                    vaccumScore += 3;
                                    break;
                                }
                            }

                            cptMove = (int) (maxMove/speed);
                            break;
                    }

                    cptMove++;
                }
                else
                {
                    cptMove = 0;
                    char move = (char)vaccumMove.Dequeue();

                    // On met à jour la position du robot selon l'action effectué

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

                    // On enlève 1 points pour chaque action effectuer
                    vaccumScore -= 1;

                    g.DrawImage(vaccumImage, vaccumPos.x * cellSize, vaccumPos.y * cellSize, srcRect, units);

                }
            }
            
        }
    }
}
