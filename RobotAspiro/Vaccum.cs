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
        private int[,] desires;
        private int[] intentions;

        private int score;
        private int nbActions;

        private Queue envQueue;

        public Vaccum(Queue queue)
        {
            this.envQueue = queue;
        }

        public void run()
        {
            while(true)
            {
                useSensors();
                updateMyState();
                chooseAnAction();
                doIt();

                System.Threading.Thread.Sleep(1000);
            }
        }

        public void useSensors()
        {
            envQueue.Enqueue("getMap");

            while ( envQueue.Contains("getMap") == true || envQueue.Count == 0) 
            {
                continue; 
            }

            this.beliefs = (int[,]) envQueue.Dequeue();
        }

        public void updateMyState()
        {
            this.desires = this.beliefs;
            
            for (int i = 0; i < beliefs.GetLength(0); i++)
            {
                for (int j = 0; j < beliefs.GetLength(1); j++)
                {
                    if(this.desires[i,j] != 0)
                    {
                        this.desires[i,j] = 0;
                    }
                }
            }
        }

        public void chooseAnAction()
        {
            // Breadth - first search
            

        }

        public void doIt()
        {

        }
    }
}
