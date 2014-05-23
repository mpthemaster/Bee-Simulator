using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Bee_Simulator
{
    [Serializable]
    public class Hive
    {
        public double Honey { get; private set; }

        [NonSerialized]
        public BeeMessage MessageSender;

        private Dictionary<string, Point> locations;
        private int beeCount = 0;
        private World world;

        private const int InitialBees = 6;
        private const double InitialHoney = 3.2;
        private const double MaxHoney = 15;
        private const double NectarHoneyRatio = .25;
        private const int MaxBees = 8;
        private const double MinHoneyForCreatingBees = 4;

        public Hive(World world, BeeMessage messageSender)
        {
            MessageSender = messageSender;
            this.world = world;
            Honey = InitialHoney;
            InitializeLocations();
            Random ran = new Random();
            for (int i = 0; i < InitialBees; i++)
                AddBee(ran);
        }

        public Point GetLocation(string location)
        {
            if (locations.Keys.Contains(location))
                return locations[location];
            else
                throw new ArgumentException("Unknown location: " + location);
        }

        private void InitializeLocations()
        {
            locations = new Dictionary<string, Point>();
            locations.Add("Entrance", new Point(690, 80));
            locations.Add("Nursery", new Point(86, 146));
            locations.Add("HoneyFactory", new Point(160, 58));
            locations.Add("Exit", new Point(190, 170));
        }

        public bool AddHoney(double nectar)
        {
            double honeyToAdd = nectar * NectarHoneyRatio;

            if (honeyToAdd + Honey > MaxHoney)
                return false;

            Honey += honeyToAdd;
            return true;
        }

        public bool ConsumeHoney(double amount)
        {
            if (amount > Honey)
                return false;

            Honey -= amount;
            return true;
        }

        private void AddBee(Random random)
        {
            beeCount++;

            int r1 = random.Next(100) - 50;
            int r2 = random.Next(100) - 50;

            Point startPoint = new Point(locations["Nursery"].X + r1, locations["Nursery"].Y + r2);
            Bee newBee = new Bee(beeCount, startPoint, this, world);
            newBee.MessageSender += this.MessageSender;
            world.Bees.Add(newBee);
        }

        public void Go(Random random)
        {
            if (Honey > MinHoneyForCreatingBees && world.Bees.Count < MaxBees && random.Next(10) == 1)
                AddBee(random);
        }
    }
}