using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee_Simulator
{
    [Serializable]
    internal class Flower
    {
        private const int LifeSpanMin = 15000;
        private const int LifeSpanMax = 30000;
        private const double InitialNectar = 1.5;
        private const double MaxNectar = 5;
        private const double NectarAddedPerTurn = .01;
        private const double NectarGatheredPerTurn = .3;

        private int lifespan;

        /// <summary>
        /// The location of the flower.
        /// </summary>
        public Point Location { get; private set; }

        /// <summary>
        /// The age of the flower.
        /// </summary>
        public int Age { get; private set; }

        /// <summary>
        /// Whether the flower is alive.
        /// </summary>
        public bool Alive { get; private set; }

        /// <summary>
        /// The current amount of nectar the flower currently has stored.
        /// </summary>
        public double Nectar { get; private set; }

        /// <summary>
        /// The amount of nectar that has been harvested from the flower.
        /// </summary>
        public double NectarHarvested { get; set; }

        /// <summary>
        /// Creates a flower.
        /// </summary>
        /// <param name="location">The specified location of the flower.</param>
        /// <param name="random">A random number generator.</param>
        public Flower(Point location, Random random)
        {
            Location = location;
            Age = 0;
            Alive = true;
            Nectar = InitialNectar;
            NectarHarvested = 0;
            lifespan = random.Next(LifeSpanMin, LifeSpanMax + 1);
        }

        /// <summary>
        /// Harvests nectar from the flower.
        /// </summary>
        /// <returns>The amount of nectar that has been harvested.</returns>
        public double HarvestNectar()
        {
            if (NectarGatheredPerTurn > Nectar)
                return 0;
            else
            {
                Nectar -= NectarGatheredPerTurn;
                NectarHarvested += NectarGatheredPerTurn;
                return NectarGatheredPerTurn;
            }
        }

        /// <summary>
        /// Updates the flower.
        /// </summary>
        public void Go()
        {
            if (++Age > lifespan)
                Alive = false;
            else
            {
                Nectar += NectarAddedPerTurn;
                if (Nectar > MaxNectar)
                    Nectar = MaxNectar;
            }
        }
    }
}