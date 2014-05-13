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
        private Random random = new Random();
        private int framesRun = 0;
        private DateTime start = DateTime.Now;
        private DateTime end;

        public Form1()
        {
            InitializeComponent();
            world = new World();

            timer1.Interval = 50;
            timer1.Tick += RunFrame;
            timer1.Enabled = false;
            UpdateStats(new TimeSpan());
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

        public void RunFrame(object sender, EventArgs e)
        {
            framesRun++;
            world.Go(random);
            end = DateTime.Now;
            TimeSpan frameDuration = end - start;
            start = end;
            UpdateStats(frameDuration);
        }

        private void toolStripBtnStartSimulation_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                toolStripBtnStartSimulation.Text = "Resume Simulation";
                timer1.Stop();
                toolStripStatus.Text = "Simulation Paused";
            }
            else
            {
                toolStripBtnStartSimulation.Text = "Pause Simulation";
                timer1.Start();
                toolStripStatus.Text = "Simulation Running";
            }
        }

        private void toolStripBtnReset_Click(object sender, EventArgs e)
        {
            framesRun = 0;
            world = new World();

            if (!timer1.Enabled)
                toolStripBtnStartSimulation.Text = "Start Simulation";
        }
    }
}