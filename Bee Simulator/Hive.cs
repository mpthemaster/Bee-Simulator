using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bee_Simulator
{
    internal class Hive
    {
        public double Honey { get; private set; }

        private Dictionary<string, Point> locations;
        private int beeCount = 0;

        private const int InitialBees = 6;
        private const double InitialHoney = 3.2;
        private const double MaxHoney = 15;
        private const double NectarHoneyRatio = .25;
        private const int MaxBees = 8;
        private const double MinHoneyForCreatingBees = 4;

        public Hive()
        {
            Honey = InitialHoney;
            InitializeLocations();
            Random ran = new Random();
            for (int i = 0; i < InitialBees; i++)
                AddBee(ran);
        }

        public Point GetLocation(string location)
        {
            Point locationP = locations[location];

            if (locationP != null)
                return locationP;
            else
                throw new ArgumentException("Unknown location: " + location);
        }

        private void InitializeLocations()
        {
            locations = new Dictionary<string, Point>();
            locations.Add("Entrance", new Point(600, 100));
            locations.Add("Nursery", new Point(95, 147));
            locations.Add("HoneyFactory", new Point(157, 98));
            locations.Add("Exit", new Point(194, 213));
        }

        public bool AddHoney(double nectar)
        {
            return true;
        }

        public bool ConsumeHoney(double amount)
        {
            return true;
        }

        private void AddBee(Random random)
        {
        }

        public void Go(Random random)
        {
        }
    }
}