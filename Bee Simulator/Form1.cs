using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
        private HiveForm hiveForm = new HiveForm();
        private FieldForm fieldForm = new FieldForm();
        private Renderer renderer;

        public Form1()
        {
            InitializeComponent();

            MoveChildForms();
            hiveForm.Show(this);
            fieldForm.Show(this);
            ResetSimulater();

            timer1.Interval = 50;
            timer1.Tick += RunFrame;
            timer1.Enabled = false;
            UpdateStats(new TimeSpan());
        }

        private void ResetSimulater()
        {
            framesRun = 0;
            world = new World(new BeeMessage(SendMessage));
            renderer = new Renderer(world, hiveForm, fieldForm);
        }

        private void MoveChildForms()
        {
            hiveForm.Location = new Point(Location.X + Width + 10, Location.Y);
            fieldForm.Location = new Point(Location.X, Location.Y + Math.Max(Height, hiveForm.Height) + 10);
        }

        private void SendMessage(int ID, string message)
        {
            toolStripStatus.Text = "Bee #" + ID + ": " + message;

            var beeGroups = from bee in world.Bees
                            group bee by bee.CurrentState into beeGroup
                            orderby beeGroup.Key
                            select beeGroup;

            listBox1.Items.Clear();
            foreach (var group in beeGroups)
            {
                string s;

                if (group.Count() == 1)
                    s = "";
                else
                    s = "s";

                listBox1.Items.Add(group.Key.ToString() + ": " + group.Count() + " bee" + s);

                if (group.Key == BeeState.Idle && group.Count() == world.Bees.Count && framesRun > 0)
                {
                    listBox1.Items.Add("Simulation Ended: All bees are idle.");
                    toolStripBtnStartSimulation.Text = "Simulation Ended";
                    toolStripStatus.Text = "Simulation Ended";
                    timer1.Stop();
                }
            }
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
            renderer.Render();
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
                //toolStripStatus.Text = "Simulation Paused";
            }
            else
            {
                toolStripBtnStartSimulation.Text = "Pause Simulation";
                timer1.Start();
                //toolStripStatus.Text = "Simulation Running";
            }
        }

        private void toolStripBtnReset_Click(object sender, EventArgs e)
        {
            renderer.Reset();
            ResetSimulater();

            if (!timer1.Enabled)
                toolStripBtnStartSimulation.Text = "Start Simulation";
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            World currentWorld = world;
            int currentFramesRun = framesRun;
            bool simRunning = timer1.Enabled;

            if (simRunning)
                timer1.Stop();

            openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                try
                {
                    using (Stream input = File.OpenRead(openFileDialog1.FileName))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        world = (World)bf.Deserialize(input);
                        framesRun = (int)bf.Deserialize(input);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to read the simulator file.\r\n" + ex.Message, "Bee Simulator Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    world = currentWorld;
                    framesRun = currentFramesRun;
                }

            world.Hive.MessageSender = new BeeMessage(SendMessage);
            foreach (Bee bee in world.Bees)
                bee.MessageSender = new BeeMessage(SendMessage);

            if (simRunning)
                timer1.Start();

            renderer.Reset();
            renderer = new Renderer(world, hiveForm, fieldForm);
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            bool simRunning = timer1.Enabled;

            if (simRunning)
                timer1.Stop();

            saveFileDialog1.InitialDirectory = openFileDialog1.InitialDirectory;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                try
                {
                    using (Stream output = File.Create(saveFileDialog1.FileName))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(output, world);
                        bf.Serialize(output, framesRun);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to save the simulator file.\r\n" + ex.Message, "Bee Simulator Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            if (simRunning)
                timer1.Start();
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            MoveChildForms();
        }
    }
}