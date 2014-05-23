using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bee_Simulator
{
    public class Renderer
    {
        private World world;
        private HiveForm hiveForm;
        private FieldForm fieldForm;

        private Bitmap hiveInside, hiveOutside, flower;
        private Bitmap[] beeAnimationLarge = new Bitmap[4], beeAnimationSmall = new Bitmap[4];

        public Renderer(World world, HiveForm hiveForm, FieldForm fieldForm)
        {
            this.world = world;
            this.hiveForm = hiveForm;
            this.fieldForm = fieldForm;
            hiveForm.Renderer = this;
            fieldForm.Renderer = this;
            InitializeImages();
        }

        public static Bitmap ResizeImage(Image imageToResize, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resizedImage))
                g.DrawImage(imageToResize, 0, 0, width, height);
            return resizedImage;
        }

        private void InitializeImages()
        {
            hiveInside = ResizeImage(Properties.Resources.Hive__inside_, hiveForm.ClientSize.Width, hiveForm.ClientSize.Height);
            hiveOutside = ResizeImage(Properties.Resources.Hive__outside_, 50, 50);
            flower = ResizeImage(Properties.Resources.Flower, 40, 40);

            beeAnimationSmall[0] = ResizeImage(Properties.Resources.Bee_animation_1, 20, 20);
            beeAnimationSmall[1] = ResizeImage(Properties.Resources.Bee_animation_2, 20, 20);
            beeAnimationSmall[2] = ResizeImage(Properties.Resources.Bee_animation_3, 20, 20);
            beeAnimationSmall[3] = ResizeImage(Properties.Resources.Bee_animation_4, 20, 20);

            beeAnimationLarge[0] = ResizeImage(Properties.Resources.Bee_animation_1, 40, 40);
            beeAnimationLarge[1] = ResizeImage(Properties.Resources.Bee_animation_2, 40, 40);
            beeAnimationLarge[2] = ResizeImage(Properties.Resources.Bee_animation_3, 40, 40);
            beeAnimationLarge[3] = ResizeImage(Properties.Resources.Bee_animation_4, 40, 40);
        }

        private int cell = 0, frame = 0;

        public void AnimateBees()
        {
            frame++;
            if (frame >= 6)
                frame = 0;
            switch (frame)
            {
                case 0: cell = 0; break;
                case 1: cell = 1; break;
                case 2: cell = 2; break;
                case 3: cell = 3; break;
                case 4: cell = 2; break;
                case 5: cell = 1; break;
                default: cell = 0; break;
            }
            hiveForm.Invalidate();
            fieldForm.Invalidate();
        }

        public void PaintHive(Graphics graphics)
        {
            graphics.FillRectangle(Brushes.SkyBlue, 0, 0, hiveForm.Width, hiveForm.Height);
            graphics.DrawImageUnscaled(hiveInside, 0, 0);

            foreach (Bee bee in world.Bees)
                if (bee.InsideHive)
                    graphics.DrawImageUnscaled(beeAnimationLarge[cell], bee.Location);
        }

        public void PaintField(Graphics graphics)
        {
            graphics.FillRectangle(Brushes.SkyBlue, 0, 0, fieldForm.Width, fieldForm.Height / 2);
            graphics.FillRectangle(Brushes.ForestGreen, 0, fieldForm.Height / 2, fieldForm.Width, fieldForm.Height / 2);
            graphics.FillEllipse(Brushes.Gold, 10, 10, 50, 50);

            using (Pen p = new Pen(Color.SaddleBrown, 20))
                graphics.DrawLine(p, fieldForm.ClientRectangle.Width, 0, fieldForm.Width - 100, 50);
            graphics.DrawImageUnscaled(hiveOutside, fieldForm.Width - 100 - hiveOutside.Width / 2, 50);

            foreach (Flower flower in world.Flowers)
                graphics.DrawImageUnscaled(this.flower, flower.Location);

            foreach (Bee bee in world.Bees)
                if (!bee.InsideHive)
                    graphics.DrawImageUnscaled(beeAnimationSmall[cell], bee.Location);
        }
    }
}