using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee_Simulator
{
    internal enum BeeState
    {
        Idle,
        FlyingToFlower,
        GatheringNectar,
        ReturningToHive,
        MakingHoney,
        Retired
    }

    internal class Bee
    {
        private const double HoneyConsumed = .5;
        private const int MoveRate = 3;
        private const double MinimumFlowerNectar = 1.5;
        private const int CareerSpan = 1000;

        /// <summary>
        /// The age of the bee.
        /// </summary>
        public int Age { get; private set; }

        /// <summary>
        /// Whether the bee is inside the hive.
        /// </summary>
        public bool InsideHive { get; private set; }

        /// <summary>
        /// The amount of nectar the bee has collected from flowers.
        /// </summary>
        public double NectorCollected { get; private set; }

        private Point location;

        /// <summary>
        /// The current location of the bee.
        /// </summary>
        public Point Location { get { return location; } }

        public BeeState CurrentState { get; private set; }

        private int ID;
        private Flower destinationFlower;

        /// <summary>
        /// Creates a bee inside the hive.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="location"></param>
        public Bee(int id, Point location)
        {
            ID = id;
            Age = 0;
            this.location = location;
            InsideHive = true;
            destinationFlower = null;
            NectorCollected = 0;
            CurrentState = BeeState.Idle;
        }

        /// <summary>
        /// Updates the bee for the current frame.
        /// </summary>
        /// <param name="random">A random number generator.</param>
        public void Go(Random random)
        {
            Age++;

            switch (CurrentState)
            {
                case BeeState.Idle:
                    if (Age > CareerSpan)
                        CurrentState = BeeState.Retired;
                    else
                    {
                        //Insert idling activity.
                    }
                    break;

                case BeeState.FlyingToFlower:
                    break;

                case BeeState.GatheringNectar:
                    double nectar = destinationFlower.HarvestNectar();
                    if (nectar > 0)
                        NectorCollected += nectar;
                    else
                        CurrentState = BeeState.ReturningToHive;
                    break;

                case BeeState.ReturningToHive:
                    if (!InsideHive)
                    {
                        //Move towards the hive.
                    }
                    else
                    {
                        //Do something in the hive.
                    }
                    break;

                case BeeState.MakingHoney:
                    if (NectorCollected < .5)
                    {
                        NectorCollected = 0;
                        CurrentState = BeeState.Idle;
                    }
                    else
                    {
                        //Turn nectar into honey.
                    }
                    break;

                case BeeState.Retired: //Do nothing.
                    break;
            }
        }

        /// <summary>
        /// Moves the bee towards a destination.
        /// </summary>
        /// <param name="destination">The new location for the bee to move to.</param>
        /// <returns>Returns whether the bee has reached the destination.</returns>
        private bool MoveTowardsLocation(Point destination)
        {
            if (Math.Abs(destination.X - location.X) <= MoveRate && Math.Abs(destination.Y - location.Y) <= MoveRate)
                return true;

            if (destination.X > location.X)
                location.X += MoveRate;
            else if (destination.X < location.X)
                location.X -= MoveRate;

            if (destination.Y > location.Y)
                location.Y += MoveRate;
            else if (destination.Y < location.Y)
                location.Y -= MoveRate;

            return false;
        }
    }
}