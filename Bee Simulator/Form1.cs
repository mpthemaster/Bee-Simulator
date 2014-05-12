using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bee_Simulator
{
    public partial class Form1 : Form
    {
        private World world;
        private int framesRun;

        public Form1()
        {
            InitializeComponent();
            world = new World();
        }

        private void UpdateStats(TimeSpan frameDuration)
        {
            lblBees.Text = world.Bees.Count.ToString();
            lblFlowers.Text = world.Flowers.Count.ToString();
            lblHoneyInHive.Text = String.Format("{0:f3}", world.Hive.Honey);

            double nectar = 0;
            foreach (Flower flower in world.Flowers)
                nectar += flower.Nectar;
            lblNectarInFlowers.Text = String.Format("{0:f3}", nectar);

            lblFramesRun.Text = framesRun.ToString();

            double milliseconds = frameDuration.TotalMilliseconds;
            if (milliseconds != 0)
                lblFrameRate.Text = string.Format("{0:f0} ({1:f1} ms)", 1000 / milliseconds, milliseconds);
            else
                lblFrameRate.Text = "N/A";
        }
    }
}