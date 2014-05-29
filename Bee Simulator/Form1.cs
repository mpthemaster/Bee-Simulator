using System;
using System.Drawing;
using System.Drawing.Printing;
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
            end = DateTime.Now;
            TimeSpan frameDuration = end - start;
            start = end;
            UpdateStats(frameDuration);
            hiveForm.Invalidate();
            fieldForm.Invalidate();
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

        private void timer2_Tick(object sender, EventArgs e)
        {
            renderer.AnimateBees();
        }

        private void printToolStripButton_Click(object sender, EventArgs e)
        {
            bool simRunning = timer1.Enabled;

            if (simRunning)
                timer1.Stop();

            PrintDocument document = new PrintDocument();
            document.PrintPage += document_PrintPage;
            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = document;
            preview.ShowDialog(this);

            if (simRunning)
                timer1.Start();
        }

        private void document_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Size stringSize;

            //Thee Bee Simulator oval and text.
            using (Font arial24bold = new Font("Arial", 24, FontStyle.Bold))
            {
                stringSize = Size.Ceiling(g.MeasureString("Bee Simulator", arial24bold));

                g.FillEllipse(Brushes.Gray, new Rectangle(e.MarginBounds.X + 2, e.MarginBounds.Y + 2, stringSize.Width + 30, stringSize.Height + 30));
                g.FillEllipse(Brushes.Black, new Rectangle(e.MarginBounds.X, e.MarginBounds.Y, stringSize.Width + 30, stringSize.Height + 30));
                g.DrawString("Bee Simulator", arial24bold, Brushes.Gray, e.MarginBounds.X + 17, e.MarginBounds.Y + 17);
                g.DrawString("Bee Simulator", arial24bold, Brushes.White, e.MarginBounds.X + 15, e.MarginBounds.Y + 15);
            }

            int tableX = e.MarginBounds.X + (int)stringSize.Width + 50;
            int tableWidth = e.MarginBounds.X + e.MarginBounds.Width - tableX - 20;
            int firstColumnX = tableX + 2;
            int secondColumnX = tableX + tableWidth / 2 + 5;
            int tableY = e.MarginBounds.Y;

            //The table's borders.
            g.DrawLine(Pens.Black, firstColumnX, tableY, firstColumnX, tableY + 144);
            g.DrawLine(Pens.Black, secondColumnX, tableY, secondColumnX, tableY + 144);
            g.DrawLine(Pens.Black, tableX + tableWidth + 2, tableY, tableX + tableWidth + 2, tableY + 144);
            g.DrawLine(Pens.Black, firstColumnX, tableY, firstColumnX + tableWidth, tableY);

            //The table's data within.
            tableY = PrintTableRow(g, tableX, tableWidth, firstColumnX, secondColumnX, tableY, "Bees", world.Bees.Count.ToString());
            tableY = PrintTableRow(g, tableX, tableWidth, firstColumnX, secondColumnX, tableY, "Flowers", world.Flowers.Count.ToString());
            tableY = PrintTableRow(g, tableX, tableWidth, firstColumnX, secondColumnX, tableY, "Honey in Hive", String.Format("{0:f3}", world.Hive.Honey));
            tableY = PrintTableRow(g, tableX, tableWidth, firstColumnX, secondColumnX, tableY, "Nectar in Flowers", lblNectarInFlowers.Text);
            tableY = PrintTableRow(g, tableX, tableWidth, firstColumnX, secondColumnX, tableY, "Frames Run", lblFramesRun.Text);
            PrintTableRow(g, tableX, tableWidth, firstColumnX, secondColumnX, tableY, "Frame Rate", lblFrameRate.Text);

            //The Hive and Field pictures.
            using (Pen blackPen = new Pen(Brushes.Black, 2))
            using (Bitmap hiveBitmap = new Bitmap(hiveForm.ClientSize.Width, hiveForm.ClientSize.Height))
            using (Bitmap fieldBitmap = new Bitmap(fieldForm.ClientSize.Width, fieldForm.ClientSize.Height))
            {
                using (Graphics hiveGraphics = Graphics.FromImage(hiveBitmap))
                    renderer.PaintHive(hiveGraphics);

                int hiveWidth = e.MarginBounds.Width / 2;
                float ratio = (float)hiveBitmap.Height / (float)hiveBitmap.Width;
                int hiveHeight = (int)(hiveWidth * ratio);
                int hiveX = e.MarginBounds.X + (e.MarginBounds.Width - hiveWidth) / 2;
                int hiveY = e.MarginBounds.Height / 3;

                g.DrawImage(hiveBitmap, hiveX, hiveY, hiveWidth, hiveHeight);
                g.DrawRectangle(blackPen, hiveX, hiveY, hiveWidth, hiveHeight);

                using (Graphics fieldGraphics = Graphics.FromImage(fieldBitmap))
                    renderer.PaintField(fieldGraphics);

                int fieldWidth = e.MarginBounds.Width;
                ratio = (float)fieldBitmap.Height / (float)fieldBitmap.Width;
                int fieldHeight = (int)(fieldWidth * ratio);
                int fieldX = e.MarginBounds.X;
                int fieldY = e.MarginBounds.Y + e.MarginBounds.Height - fieldHeight;

                g.DrawImage(fieldBitmap, fieldX, fieldY, fieldWidth, fieldHeight);
                g.DrawRectangle(blackPen, fieldX, fieldY, fieldWidth, fieldHeight);
            }
        }

        private int PrintTableRow(Graphics printGraphics, int tableX, int tableWidth, int firstColumnX, int secondColumnX, int tableY, string firstColumn, string secondColumn)
        {
            Font arial12 = new Font("Arial", 12);
            Size stringSize = Size.Ceiling(printGraphics.MeasureString(firstColumn, arial12));
            tableX += 2;

            printGraphics.DrawString(firstColumn, arial12, Brushes.Black, firstColumnX, tableY);
            printGraphics.DrawString(secondColumn, arial12, Brushes.Black, secondColumnX, tableY);

            tableY += (int)stringSize.Height + 2;
            printGraphics.DrawLine(Pens.Black, tableX, tableY, tableX + tableWidth, tableY);
            arial12.Dispose();
            return tableY;
        }
    }
}