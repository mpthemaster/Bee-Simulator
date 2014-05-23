using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bee_Simulator
{
    internal class Renderer
    {
        private World world;
        private HiveForm hiveForm;
        private FieldForm fieldForm;

        private Dictionary<Flower, PictureBox> flowersLookup = new Dictionary<Flower, PictureBox>();
        private List<Flower> deadFlowers = new List<Flower>();
        private Dictionary<Bee, BeeControl> beesLookup = new Dictionary<Bee, BeeControl>();
        private List<Bee> retiredBees = new List<Bee>();

        public Renderer(World world, HiveForm hiveForm, FieldForm fieldForm)
        {
            this.world = world;
            this.hiveForm = hiveForm;
            this.fieldForm = fieldForm;
        }

        public void Render()
        {
            DrawBees();
            DrawFlowers();
            RemoveRetiredBeesAndDeadFlowers();
        }

        public void Reset()
        {
            foreach (PictureBox flower in flowersLookup.Values)
            {
                fieldForm.Controls.Remove(flower);
                flower.Dispose();
            }

            foreach (BeeControl bee in beesLookup.Values)
            {
                hiveForm.Controls.Remove(bee);
                fieldForm.Controls.Remove(bee);
                bee.Dispose();
            }

            flowersLookup.Clear();
            beesLookup.Clear();
        }

        private void RemoveRetiredBeesAndDeadFlowers()
        {
            foreach (Bee bee in retiredBees)
                beesLookup.Remove(bee);
            retiredBees.Clear();

            foreach (Flower flower in deadFlowers)
                flowersLookup.Remove(flower);
            deadFlowers.Clear();
        }

        private void DrawFlowers()
        {
            //Add new flowers' pictureboxes.
            foreach (Flower flower in world.Flowers)
            {
                if (!flowersLookup.ContainsKey(flower))
                {
                    PictureBox flowerControl = new PictureBox()
                    {
                        Width = 45,
                        Height = 55,
                        Image = Properties.Resources.Flower,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Location = flower.Location
                    };
                    flowersLookup.Add(flower, flowerControl);
                    fieldForm.Controls.Add(flowerControl);
                }
            }

            //Remove dead flowers' pictureboxes.
            foreach (Flower flower in flowersLookup.Keys)
            {
                if (!world.Flowers.Contains(flower))
                {
                    PictureBox flowerControlToRemove = flowersLookup[flower];
                    fieldForm.Controls.Remove(flowerControlToRemove);
                    flowerControlToRemove.Dispose();
                    deadFlowers.Add(flower);
                }
            }
        }

        private void DrawBees()
        {
            BeeControl beeControl;

            //Add new bees' pictures or move existing bees' picture locations, possibly between forms.
            foreach (Bee bee in world.Bees)
            {
                beeControl = GetBeeControl(bee);
                if (bee.InsideHive)
                {
                    if (fieldForm.Controls.Contains(beeControl))
                        MoveBeeFromFieldToHive(beeControl);
                }
                else if (hiveForm.Controls.Contains(beeControl))
                    MoveBeeFromHiveToField(beeControl);
                beeControl.Location = bee.Location;
            }

            //Remove retired bees' pictures.
            foreach (Bee bee in beesLookup.Keys)
            {
                if (!world.Bees.Contains(bee))
                {
                    beeControl = beesLookup[bee];
                    if (fieldForm.Controls.Contains(beeControl))
                        fieldForm.Controls.Remove(beeControl);
                    if (hiveForm.Controls.Contains(beeControl))
                        hiveForm.Controls.Remove(beeControl);
                    beeControl.Dispose();
                    retiredBees.Add(bee);
                }
            }
        }

        private void MoveBeeFromHiveToField(BeeControl beeControl)
        {
            hiveForm.Controls.Remove(beeControl);
            beeControl.Size = new Size(20, 20);
            fieldForm.Controls.Add(beeControl);
            beeControl.BringToFront();
        }

        private void MoveBeeFromFieldToHive(BeeControl beeControl)
        {
            fieldForm.Controls.Remove(beeControl);
            beeControl.Size = new Size(40, 40);
            hiveForm.Controls.Add(beeControl);
            beeControl.BringToFront();
        }

        private BeeControl GetBeeControl(Bee bee)
        {
            BeeControl beeControl;

            if (!beesLookup.ContainsKey(bee))
            {
                beeControl = new BeeControl() { Width = 40, Height = 40 };
                beesLookup.Add(bee, beeControl);
                hiveForm.Controls.Add(beeControl);
                beeControl.BringToFront();
            }
            else
                beeControl = beesLookup[bee];

            return beeControl;
        }
    }
}