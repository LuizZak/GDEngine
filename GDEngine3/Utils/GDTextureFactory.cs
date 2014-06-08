using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

using GDEngine3;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace GDEngine3.Display
{
    /// <summary>
    /// Texture factory, used to create textures on-the-fly using GDI+ and DirectX
    /// </summary>
    public class GDTextureFactory
    {
        /// <summary>
        /// The created texture
        /// </summary>
        public Texture2D Texture;

        /// <summary>
        /// The graphics used to draw into the texture
        /// </summary>
        public Graphics Graphics;

        /// <summary>
        /// The bitmap used to flush the graphics into the texture
        /// </summary>
        public Bitmap Bitmap;

        /// <summary>
        /// The device used to create the texture
        /// </summary>
        public GraphicsDevice Device;

        /// <summary>
        /// Creates a new texture factory object
        /// </summary>
        /// <param name="device">A valid GraphicsDevice object</param>
        /// <param name="textureWidth">The width of the texture to create</param>
        /// <param name="textureHeight">The height of the texture to create</param>
        public GDTextureFactory(GraphicsDevice device, int textureWidth, int textureHeight)
        {
            Device = device;

            Bitmap = new Bitmap(textureWidth, textureHeight);

            Graphics = Graphics.FromImage(Bitmap);
        }

        /// <summary>
        /// Flush the graphics object into the texture
        /// </summary>
        /// <returns>The created texture</returns>
        public Texture2D Flush()
        {
            //string tempFile = Path.GetTempFileName();
            MemoryStream tempFile = new MemoryStream();

            Graphics.Flush(FlushIntention.Flush);

            Bitmap.Save(tempFile, System.Drawing.Imaging.ImageFormat.Png);

            tempFile.Position = 0;

            Texture = Texture2D.FromStream(Device, tempFile);

            tempFile.Dispose();

            return GetTextureCopy();
        }

        /// <summary>
        /// Gets a copy of the current texture in the Texture Factory
        /// </summary>
        /// <returns>A copy of the current texture in the Texture Factory</returns>
        public Texture2D GetTextureCopy()
        {
            uint[] Data = new uint[Texture.Width * Texture.Height];

            Texture.GetData<uint>(Data);

            Texture2D d = new Texture2D(Device, Texture.Width, Texture.Height);

            d.SetData<uint>(Data);

            return d;
        }

        /// <summary>
        /// Disposes this Texture Factory and free all used resources
        /// </summary>
        public void Dispose()
        {
            Graphics.Dispose();
            Texture.Dispose();
            Bitmap.Dispose();
        }

        /// <summary>
        /// Returns the static PixelTexture Texture2D reference
        /// </summary>
        /// <returns>The static PixelTexture Texture2D reference</returns>
        public static Texture2D GetPixelTexture()
        {
            return PixelTexture;
        }

        /// <summary>
        /// The static reference to the black Pixel Texture2D
        /// </summary>
        public static Texture2D PixelTexture;
    }
}
