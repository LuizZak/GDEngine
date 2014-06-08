using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

using GDEngine3;
using GDEngine3.Utils;

namespace GDEngine3.Display
{
    /// <summary>
    /// Class that represents an image
    /// </summary>
    public class GDImage : GDEntity
    {
        /// <summary>
        /// Image use to draw
        /// </summary>
        public Texture2D Texture;

        /// <summary>
        /// Sprite effect to apply to the final image
        /// </summary>
        public SpriteEffects SpriteEffect = 0;

        /// <summary>
        /// Whether to round the position of the image before drawing it into the screen.
        /// </summary>
        public bool RoundPoints = true;

        /// <summary>
        /// Whether there is a texture loaded into this GDImage
        /// </summary>
        protected bool loaded = false;

        /// <summary>
        /// Gets a value telling whether there is a texture loaded into this GDImage
        /// </summary>
        public bool Loaded
        {
            get { return loaded; }
        }

        /// <summary>
        /// Creates and returns a clone of this GDImage, optionally setting
        /// a new texture copy as well
        /// </summary>
        /// <param name="cloneTex">Whether to clone the texture as well, instead of using the same texture data</param>
        /// <returns>The copy of this GDImage</returns>
        public virtual GDImage Clone(bool cloneTex)
        {
            GDImage clone = new GDImage(Device);

            // Check whether to clone or just copy the texture over:
            if (cloneTex)
            {
                // Clone texture:
                Color[] data = new Color[Texture.Width * Texture.Height];

                Texture.GetData<Color>(data);

                clone.Texture = new Texture2D(this.Device, this.Texture.Width, this.Texture.Height);
                clone.Texture.SetData<Color>(data);
            }
            else
            {
                // Copy strip:
                clone.Texture = this.Texture;
            }

            // Copy dimension and position information:
            clone.X = this.X;
            clone.Y = this.Y;
            clone.Position = this.Position;

            clone.XOrigin = this.XOrigin;
            clone.YOrigin = this.YOrigin;
            clone.Origin = this.Origin;

            clone.Width = this.Width;
            clone.Height = this.Height;
            clone.OriginalWidth = this.OriginalWidth;
            clone.OriginalHeight = this.OriginalHeight;
            clone.Scale = this.Scale;
            clone.Unscaled = this.Unscaled;

            clone.Rotation = this.Rotation;
            clone.Radians = this.Radians;

            // Copy misc information:
            clone.Device = this.Device;

            // Return the created image
            return clone;
        }

        /// <summary>
        /// Creates a new instance of the GDImage class, and use a Device to initialize the Texture2D
        /// binded to this GDImage
        /// </summary>
        /// <param name="device">The GraphicsDevice that will be used to render the image on the screen</param>
        public GDImage(GraphicsDevice device)
        {
            // Set the device:
            Device = device;
        }

        /// <summary>
        /// Creates a new instance of the GDImage class, set a Device, and load an image from the given path
        /// </summary>
        /// <param name="device">The GraphicsDevice that will be used to render the image on the screen</param>
        /// <param name="ImagePath">The path of a valid image file (.png, .jpg, .bmp, .tga, .gif, etc...) to load the texture from</param>
        public GDImage(GraphicsDevice device, string ImagePath)
        {
            // Set the device:
            Device = device;

            // Load the image:
            LoadImage(ImagePath);
        }

        /// <summary>
        /// Creates a new instance of the GDImage class, set a Device, and set the Texture2D that will be rendered to the screen
        /// </summary>
        /// <param name="device">The GraphicsDevice that will be used to render the image on the screen</param>
        /// <param name="texture">The Texture2D that will be rendered to the screen on each PostDraw call</param>
        public GDImage(GraphicsDevice device, Texture2D texture)
        {
            // Set the device:
            Device = device;

            LoadTexture(texture);
        }

        /// <summary>
        /// Creates a GDImage instance using a texture as reference
        /// </summary>
        /// <param name="device">A valid GraphicsDevice object used to create the GDImage</param>
        /// <param name="texture">The texture to create the image from</param>
        /// <returns>The created GDImage</returns>
        public static GDImage FromTexture(GraphicsDevice device, Texture2D texture)
        {
            // Return the image
            return new GDImage(device, texture);
        }

        /// <summary>
        /// Loads an image
        /// </summary>
        /// <param name="path">The image's path</param>
        public void LoadImage(string path)
        {
            FileStream s = new FileStream(path, FileMode.Open);

            // Create a new texture
            Texture = Texture2D.FromStream(Device, s);

            s.Dispose();

            // Set the size
            Width = Texture.Width;
            Height = Texture.Height;

            // Set the original size
            OriginalWidth = Texture.Width;
            OriginalHeight = Texture.Height;

            // Call some functions
            AfterMove();
            AfterScale();

            HasArea = true;

            loaded = true;
        }

        /// <summary>
        /// Assigns a texture to this GDImage
        /// </summary>
        /// <param name="tex">The texture to assign</param>
        public void LoadTexture(Texture2D tex)
        {
            Texture = tex;

            if (tex != null)
            {
                // Set the size
                Width = Texture.Width;
                Height = Texture.Height;

                // Set the original size
                OriginalWidth = Texture.Width;
                OriginalHeight = Texture.Height;
            }
            else
            {
                // Set the size
                Width = 0;
                Height = 0;

                // Set the original size
                OriginalWidth = 0;
                OriginalHeight = 0;
            }

            // Call some functions
            AfterMove();
            AfterScale();

            HasArea = true;

            loaded = (tex != null);
        }

        /// <summary>
        /// Renders the Texture2D binded to this GDImage with the desired configurations
        /// </summary>
        /// <param name="g">The SpriteBatch object to render this entity into</param>
        /// <param name="redrawRect">The clipping redraw rectangle</param>
        /// <param name="offset">The offset to draw this entity to</param>
        public override void PostDraw(SpriteBatch g, RectangleF redrawRect, Point offset)
        {
            // Exit this function if there's no image provided
            if (Texture == null)
                return;

            // Draw the sprite
            Vector2 position, imageOffset;

            position = absolutePosition;
            imageOffset = absoluteOffset;

            // If the RoundPoints flag is set to true, we round the positions off before rendering
            if (RoundPoints)
            {
                float factor = 2;

                position.X = (int)((float)Math.Round(position.X / (engine.Camera.scaleX * factor)) * (engine.Camera.scaleX * factor));
                position.Y = (int)((float)Math.Round(position.Y / (engine.Camera.scaleY * factor)) * (engine.Camera.scaleY * factor));

                imageOffset.X = (int)((float)Math.Round(imageOffset.X / (engine.Camera.scaleX * factor)) * (engine.Camera.scaleX * factor));
                imageOffset.Y = (int)((float)Math.Round(imageOffset.Y / (engine.Camera.scaleY * factor)) * (engine.Camera.scaleY * factor));
            }

            // Test whether to use the absoluteTint as it is or premultiply the tint now
            bool usePremultiplied = (engine.CurrentScreen == null ? engine.DesiredBlendState == BlendState.AlphaBlend : engine.CurrentScreen.DesiredBlendState == BlendState.AlphaBlend);

            // Draw the texture on the SpriteBatch using all the parameters that have just been calculated
            g.Draw(Texture, position, null, (usePremultiplied ? Color.FromNonPremultiplied(absoluteTint.ToVector4()) : absoluteTint), MathHelper.ToRadians(absoluteRotation), imageOffset, absoluteScale, SpriteEffect, 0);
            
            // Draw base:
            base.PostDraw(g, redrawRect, offset);
        }

        /// <summary>
        /// Frees this entity from the memory
        /// </summary>
        public override void Free(bool ForceFree)
        {
            base.Free(ForceFree);

            if (ForceFree && Texture != null && !Texture.IsDisposed)
            {
                Texture.Dispose();
            }

            Texture = null;
        }

        /// <summary>
        /// Calcualtes the area of this GDImage, taking the assigned Texture2D size into consideration
        /// </summary>
        public override void CalculateArea()
        {
            if (loaded)
            {
                Points = GDMath.GetRectangle(absolutePosition, new Vector2(OriginalWidth, OriginalHeight), absoluteOffset, absoluteScale, absoluteRotation * (float)(Math.PI / 180));
                LocalPoints = GDMath.GetRectangle(Vector2.Zero, new Vector2(OriginalWidth, OriginalHeight), origin, Vector2.One, 0);

                Area = GDMath.GetRectangleArea(Points);
                LocalArea = GDMath.GetRectangleArea(LocalPoints);

                HasArea = true;

                // Try to use the collision shape as an area instead
                if (HasCollisionShape)
                {
                    Area = RectangleF.Union(Area, CollisionRecMod);
                    LocalArea = RectangleF.Union(LocalArea, CollisionRec);
                }
            }
            else
            {
                base.CalculateArea();
            }
        }
    }
}