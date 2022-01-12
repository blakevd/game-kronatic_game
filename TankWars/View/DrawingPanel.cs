using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TankWars
{
    public class DrawingPanel : Panel
    {
        private int clientId;
        private World theWorld;

        public DrawingPanel(World w, int cp)
        {
            DoubleBuffered = true;
            theWorld = w;
            clientId = cp;
        }


        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);


        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            e.Graphics.TranslateTransform((int)worldX, (int)worldY);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            Tank p = o as Tank;

            int width = 60; // size of tank 60 x 60
            int width2 = 50;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle rect1 = new Rectangle(-(width / 2), -(width / 2), width, width);
            Rectangle rect2 = new Rectangle(-(width2 / 2), -(width2 / 2), width2, width2);
            e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\BlueTank.png"), rect1);
            e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\BlueTurret.png"), rect2);

        }

        private void WallDrawer(object o, PaintEventArgs e)
        {
            Wall w = o as Wall;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Create a TextureBrush object.
            TextureBrush tBrush = new TextureBrush(new Bitmap("..\\..\\..\\Resources\\Images\\WallSprite.png"), WrapMode.Tile);

            Rectangle rect = new Rectangle(-25, -25, 50, 50);

            e.Graphics.FillRectangle(tBrush, (float)w.v1.GetX(), (float)w.v1.GetY(), 50, 50);
            e.Graphics.FillRectangle(tBrush, (float)w.v2.GetX(), (float)w.v2.GetY(), 50, 50);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            Powerup p = o as Powerup;

          
        }


        // This method is invoked when the DrawingPanel needs to be re-drawn
        protected override void OnPaint(PaintEventArgs e)
        {
            if (theWorld is null)
            {
                return;
            }

            // Center the view on the middle of the world,
            // since the image and world use different coordinate systems
            int viewSize = Size.Width; // view is square, so we can just use width
            theWorld.Tanks.TryGetValue(clientId, out Tank client);
            if (!(client is null))
            {
               // e.Graphics.TranslateTransform((float)-client.getLoc().GetX() + (viewSize / 2), (float)-client.getLoc().GetY() + viewSize / 2);
            }
            else
                e.Graphics.TranslateTransform(-(viewSize / 2), -(viewSize / 2));
            lock (theWorld)
            {
                Rectangle backgroundSize = new Rectangle(-(theWorld.size / 2), -(theWorld.size / 2), theWorld.size, theWorld.size);
                e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\Background.png"), backgroundSize);

                // Draw the Tanks
                foreach (Tank play in theWorld.Tanks.Values)
                {
                   // DrawObjectWithTransform(e, play, play.getLoc().GetX(), play.getLoc().GetY(), play.getOri().ToAngle(), TankDrawer);

                }

                // Draw the powerups
                foreach (Powerup pow in theWorld.Powerups.Values)
                {
                    //  DrawObjectWithTransform(e, pow, pow.GetLocation().GetX(), pow.GetLocation().GetY(), 0, PowerupDrawer);
                }

                // Draw the walls
                foreach (Wall wall in theWorld.Walls.Values)
                {
                    DrawObjectWithTransform(e, wall, 0, 0, 0, WallDrawer);
                }
            }
            // Do anything that Panel (from which we inherit) needs to do
            base.OnPaint(e);
        }
        public void addWorldAndTank(World world, int p)
        {
            theWorld = world;
            clientId = p;
        }
    }
}

