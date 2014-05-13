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
        public BeeMessage MessageSender;

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
        private Hive hive;
        private World world;

        /// <summary>
        /// Creates a bee inside the hive.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="location"></param>
        public Bee(int id, Point location, Hive hive, World world)
        {
            this.hive = hive;
            this.world = world;
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
            BeeState oldState = CurrentState;

            switch (CurrentState)
            {
                case BeeState.Idle:
                    if (Age > CareerSpan)
                        CurrentState = BeeState.Retired;
                    else if (world.Flowers.Count > 0 && hive.ConsumeHoney(HoneyConsumed))
                    {
                        Flower flower = world.Flowers[random.Next(world.Flowers.Count)];

                        if (flower.Nectar >= MinimumFlowerNectar && flower.Alive)
                        {
                            destinationFlower = flower;
                            CurrentState = BeeState.FlyingToFlower;
                        }
                    }
                    break;

                case BeeState.FlyingToFlower:
                    if (!world.Flowers.Contains(destinationFlower))
                        CurrentState = BeeState.ReturningToHive;
                    else if (InsideHive)
                    {
                        if (MoveTowardsLocation(hive.GetLocation("Exit")))
                        {
                            InsideHive = false;
                            location = hive.GetLocation("Entrance");
                        }
                    }
                    else
                        if (MoveTowardsLocation(destinationFlower.Location))
                            CurrentState = BeeState.GatheringNectar;
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
                        if (MoveTowardsLocation(hive.GetLocation("Entrance")))
                        {
                            InsideHive = true;
                            location = hive.GetLocation("Exit");
                        }
                    }
                    else
                        if (MoveTowardsLocation(hive.GetLocation("HoneyFactory")))
                            CurrentState = BeeState.MakingHoney;
                    break;

                case BeeState.MakingHoney:
                    if (NectorCollected < .5)
                    {
                        NectorCollected = 0;
                        CurrentState = BeeState.Idle;
                    }
                    else
                        if (hive.AddHoney(.5))
                            NectorCollected -= .5;
                        else
                            NectorCollected = 0;
                    break;

                case BeeState.Retired: //Do nothing.
                    break;
            }

            if (oldState != CurrentState && MessageSender != null)
                MessageSender(ID, CurrentState.ToString());
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