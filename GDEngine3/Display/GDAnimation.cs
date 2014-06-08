using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Draw = System.Drawing;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

using GDEngine3;
using GDEngine3.Utils;

namespace GDEngine3.Display
{
    /// <summary>
    /// Class that represents an animation, which is a striped texture
    /// that has a boundary data used to draw the frames
    /// </summary>
    public class GDAnimation : GDEntity
    {
        //// EVENTS:

        /// <summary>
        /// Happens when the animation finishes playing
        /// </summary>
        /// <param name="target">The animation that has ended</param>
        public delegate void AnimationOverEventArgs(GDAnimation target);

        /// <summary>
        /// Happens when the animation finishes playing
        /// </summary>
        public event AnimationOverEventArgs AnimationOver;

        /// <summary>
        /// Happens when the animation advances one frame
        /// </summary>
        /// <param name="target">The animation that has ended</param>
        /// <param name="newFrame">The new frame that was updated</param>
        public delegate void FrameAdvanceEventArgs(GDAnimation target, int newFrame);

        /// <summary>
        /// Happens when the animation advances one frame
        /// </summary>
        public event FrameAdvanceEventArgs FrameAdvanced;

        //// FUNCTIONS:

        /// <summary>
        /// Initializes a new animation object
        /// </summary>
        /// <param name="device">The GraphicsDevice instance used to initialize the textures</param>
        public GDAnimation(GraphicsDevice device)
            : base()
        {
            Device = device;
        }

        /// <summary>
        /// Initializes a new animation object
        /// </summary>
        /// <param name="device">The GraphicsDevice instance used to initialize the textures</param>
        /// <param name="animation">The AnimationDescriptor that will be loaded on this GDAnimation</param>
        public GDAnimation(GraphicsDevice device, AnimationDescriptor animation)
            : base()
        {
            Device = device;

            LoadDescriptor(animation);
        }

        /// <summary>
        /// Initializes a new animation object, and loads the frames from an image
        /// </summary>
        /// <param name="device">The GraphicsDevice instance used to initialize the textures</param>
        /// <param name="ImagePath">The image strip to extract the animation from</param>
        /// <param name="frameWidth">The frame width to extract. Each frame will be extracted as (frameWidth % ImageWidth)</param>
        /// <param name="frameHeight">The frame height to extract. Each frame will be extracted as (frameHeight % ImageHeight)</param>
        public GDAnimation(GraphicsDevice device, string ImagePath, int frameWidth, int frameHeight)
            : base()
        {
            Device = device;
            
            LoadAnimation(ImagePath, frameWidth, frameHeight);
        }

        /// <summary>
        /// Initializes a new animation object, loads the frames from an image, setting a max frame count
        /// </summary>
        /// <param name="device">The GraphicsDevice instance used to initialize the textures</param>
        /// <param name="ImagePath">The image strip to extract the animation from</param>
        /// <param name="frameWidth">The frame width to extract. Each frame will be extracted as (frameWidth % ImageWidth)</param>
        /// <param name="frameHeight">The frame height to extract. Each frame will be extracted as (frameHeight % ImageHeight)</param>
        /// <param name="frameCount">The frame count to extract</param>
        public GDAnimation(GraphicsDevice device, string ImagePath, int frameWidth, int frameHeight, int frameCount)
            : base()
        {
            Device = device;

            LoadAnimation(ImagePath, frameWidth, frameHeight, frameCount);
        }

        /// <summary>
        /// Initializes a new animation object, loads the frames from an image starting from a given frame to a max frame count
        /// </summary>
        /// <param name="device">The GraphicsDevice instance used to initialize the textures</param>
        /// <param name="ImagePath">The image strip to extract the animation from</param>
        /// <param name="frameWidth">The frame width to extract. Each frame will be extracted as (frameWidth % ImageWidth)</param>
        /// <param name="frameHeight">The frame height to extract. Each frame will be extracted as (frameHeight % ImageHeight)</param>
        /// <param name="frameCount">The frame count to extract</param>
        /// <param name="firstFrame">The first frame start to extracting from</param>
        public GDAnimation(GraphicsDevice device, string ImagePath, int frameWidth, int frameHeight, int frameCount, int firstFrame)
            : base()
        {
            Device = device;

            LoadAnimation(ImagePath, frameWidth, frameHeight, frameCount, firstFrame);
        }

        /// <summary>
        /// Initializes a new animation object, loads the frames from an image starting from a given frame to a max frame count
        /// </summary>
        /// <param name="device">The GraphicsDevice instance used to initialize the textures</param>
        /// <param name="ImagePath">The image strip to extract the animation from</param>
        /// <param name="frameWidth">The frame width to extract. Each frame will be extracted as (frameWidth % ImageWidth)</param>
        /// <param name="frameHeight">The frame height to extract. Each frame will be extracted as (frameHeight % ImageHeight)</param>
        /// <param name="frameCount">The frame count to extract</param>
        /// <param name="firstFrame">The first frame start to extracting from</param>
        /// <param name="storeInFrames">Whether to store them in separate frames instead of in a single strip. Might solve issues with 
        /// sprites beeing too large</param>
        public GDAnimation(GraphicsDevice device, string ImagePath, int frameWidth, int frameHeight, int frameCount, int firstFrame, bool storeInFrames)
            : base()
        {
            Device = device;

            LoadAnimation(ImagePath, frameWidth, frameHeight, frameCount, firstFrame, storeInFrames);
        }

        /// <summary>
        /// Loads an animation from the given files into a single image strip
        /// </summary>
        /// <param name="files">A list of images that the animation will be extracted from</param>
        public void LoadAnimation(string[] files)
        {
            LoadAnimation(files, false);
        }

        /// <summary>
        /// Loads an animation from the given files
        /// </summary>
        /// <param name="files">A list of images that the animation will be extracted from</param>
        /// <param name="storeInFrames">Whether to store them in separate frames instead of in a single strip. Might solve issues with 
        /// sprites beeing too large</param>
        public void LoadAnimation(string[] files, bool storeInFrames)
        {
            // Loads the animated .GIF
            System.Drawing.Image img;// = System.Drawing.Image.FromFile(path);

            int frameCount = files.Length - 10, startingFrame = 0, lastFrame = files.Length - 10;//, Width, Height;

            AnimationDescriptor tempDesc = new AnimationDescriptor();

            // Calculate the frame count
            // frameCount = img.GetFrameCount(fd);

            // Check if frames are out of range
            if (lastFrame > frameCount || (lastFrame < startingFrame && lastFrame > 0))
                throw new Exception("Target frames where out of range.");

            // If last frame is not set, set it:
            if (lastFrame == -1)
                lastFrame = frameCount;

            // Temporary Graphics object
            System.Drawing.Graphics g;

            // Calculate strip boundary:
            int x_cells = (int)Math.Ceiling(Math.Sqrt(frameCount));
            int y_cells = (int)Math.Floor(Math.Sqrt(frameCount));

            img = System.Drawing.Image.FromFile(files[0]);

            Width = tempDesc.Width = img.Width;
            Height = tempDesc.Height = img.Height;

            OriginalWidth = Width;
            OriginalHeight = Height;

            int StripWidth = x_cells * (img.Width);
            int StripHeight = y_cells * (img.Height);

            // Temporary BITS array:
            System.Drawing.Bitmap stripBit = new System.Drawing.Bitmap(StripWidth, StripHeight);

            g = System.Drawing.Graphics.FromImage(stripBit);

            tempDesc.PlaybackRate = 1;

            // Create the frame bound collection
            tempDesc.FrameBounds = new FrameBoundCollection();
            tempDesc.FrameBounds.FrameBounds = new int[frameCount][];
            tempDesc.FrameBounds.OffsetX = new int[frameCount];
            tempDesc.FrameBounds.OffsetY = new int[frameCount];
            tempDesc.FrameCount = frameCount;

            // Loop thru' the selected frames and transfer them
            if (!storeInFrames)
            {
                // Load all images into a single texture atlas
                for (int i = startingFrame; i < lastFrame; i++)
                {
                    // Select the next frame
                    img = System.Drawing.Image.FromFile(files[i - startingFrame]);

                    // Creates the new frame and use the Graphics object to transfer the frame

                    int x = (i - startingFrame) % x_cells * img.Width;
                    int y = (i - startingFrame) / x_cells * img.Height;

                    tempDesc.FrameBounds[i - startingFrame] = new int[] { x, y, tempDesc.Width, tempDesc.Height };
                    tempDesc.FrameBounds.OffsetX[i - startingFrame] = 0;
                    tempDesc.FrameBounds.OffsetY[i - startingFrame] = 0;

                    g.DrawImage(img, new System.Drawing.Rectangle(x, y, img.Width, img.Height), new System.Drawing.Rectangle(0, 0, img.Width, img.Height), System.Drawing.GraphicsUnit.Pixel);

                    img.Dispose();
                }

                // Copy the image into a temporary memory stream, and back at the texture strip
                MemoryStream s = new MemoryStream();
                stripBit.Save(s, System.Drawing.Imaging.ImageFormat.Png);

                tempDesc.Strip = Texture2D.FromStream(Device, s);

                // Dispose of the temporary memory stream
                s.Dispose();
            }
            else
            {
                // Create the frame texture array
                tempDesc.Frames = new Texture2D[frameCount];
                tempDesc.UseFrames = true;

                // Loop thru' the selected frames and transfer them
                for (int i = startingFrame; i < lastFrame; i++)
                {
                    FileStream s = new FileStream(files[i - startingFrame], FileMode.Open, FileAccess.Read);

                    // Load the current frame
                    tempDesc.Frames[i - startingFrame] = Texture2D.FromStream(Device, s);

                    s.Dispose();
                }
            }

            // Update playback information
            tempDesc.Play = tempDesc.FrameCount > 1 ? true : false;
            tempDesc.FPS = -1;
            tempDesc.Reverse = false;

            // Set the descriptor
            Descriptor = tempDesc;

            // Dispose used objects
            g.Dispose();
            stripBit.Dispose();

            // Set the animation loaded flag
            loaded = true;

            HasArea = true;

            // Set the entity as dirty
            AfterMove();
            AfterScale();
        }

        /// <summary>
        /// Loads an animation
        /// </summary>
        /// <param name="path">The image strip to extract the animation from</param>
        /// <param name="frameWidth">The frame width to extract. Each frame will be extracted as (frameWidth % ImageWidth)</param>
        /// <param name="frameHeight">The frame height to extract. Each frame will be extracted as (frameHeight % ImageHeight)</param>
        public void LoadAnimation(string path, int frameWidth, int frameHeight)
        {
            Image = System.Drawing.Image.FromFile(path);

            LoadAnimation(path, frameHeight, frameHeight, -1, 0);
        }

        /// <summary>
        /// Loads an animation
        /// </summary>
        /// <param name="path">The image strip to extract the animation from</param>
        /// <param name="frameWidth">The frame width to extract. Each frame will be extracted as (frameWidth % ImageWidth)</param>
        /// <param name="frameHeight">The frame height to extract. Each frame will be extracted as (frameHeight % ImageHeight)</param>
        /// <param name="frameCount">The frame count to extract</param>
        public void LoadAnimation(string path, int frameWidth, int frameHeight, int frameCount)
        {
            Image = System.Drawing.Image.FromFile(path);

            LoadAnimation(path, frameHeight, frameHeight, frameCount, 0);
        }

        /// <summary>
        /// Loads an animation
        /// </summary>
        /// <param name="path">The image strip to extract the animation from</param>
        /// <param name="frameWidth">The frame width to extract. Each frame will be extracted as (frameWidth % ImageWidth)</param>
        /// <param name="frameHeight">The frame height to extract. Each frame will be extracted as (frameHeight / ImageHeight)</param>
        /// <param name="frameCount">The frame count to extract</param>
        /// <param name="firstFrame">The first frame start to extracting from</param>
        public void LoadAnimation(string path, int frameWidth, int frameHeight, int frameCount, int firstFrame)
        {
            Image = System.Drawing.Image.FromFile(path);

            // Calculate cells dimensions
            int x_cells = Image.Width / frameWidth;
            int y_cells = Image.Height / frameHeight;

            AnimationDescriptor tempDesc = new AnimationDescriptor();

            // No frame count set? Calculate by the image size
            if (frameCount == -1)
                frameCount = (x_cells * y_cells) - firstFrame;

            // Frame count larger than frames on image? Trim the variable
            if (firstFrame + frameCount > x_cells * y_cells)
                frameCount = (x_cells * y_cells) - firstFrame;

            FileStream s = new FileStream(path, FileMode.Open, FileAccess.Read);

            // Create the strip
            tempDesc.Strip = Texture2D.FromStream(Device, s);

            tempDesc.PlaybackRate = 1;

            s.Close();
            s.Dispose();

            // Create the frame boundary information
            tempDesc.FrameBounds = new FrameBoundCollection();
            tempDesc.FrameBounds.FrameBounds = new int[frameCount][];
            tempDesc.FrameBounds.OffsetX = new int[frameCount];
            tempDesc.FrameBounds.OffsetY = new int[frameCount];

            for (int cell = firstFrame; cell < firstFrame + frameCount; cell++)
            {
                // Calculate the offset onto the input image:
                int x = cell % x_cells * frameWidth;
			    int y = cell / x_cells * frameHeight;

                // Set the frame bounding information
                tempDesc.FrameBounds[cell - firstFrame] = new int[] { x, y, frameWidth, frameHeight };
                tempDesc.FrameBounds.OffsetX[cell - firstFrame] = 0;
                tempDesc.FrameBounds.OffsetY[cell - firstFrame] = 0;
            }

            // Set dimensions
            Width = tempDesc.Width = frameWidth;
            Height = tempDesc.Height = frameHeight;

            OriginalWidth = Width;
            OriginalHeight = Height;

            // Setup the animation data
            tempDesc.CurrentFrame = 0;
            tempDesc.FrameCount = frameCount;

            tempDesc.Play = tempDesc.FrameCount > 1 ? true : false;
            tempDesc.FPS = -1;
            tempDesc.Reverse = false;

            Descriptor = tempDesc;

            loaded = true;

            HasArea = true;

            AfterMove();
            AfterScale();
        }

        /// <summary>
        /// Loads an animation
        /// </summary>
        /// <param name="path">The image strip to extract the animation from</param>
        /// <param name="frameWidth">The frame width to extract. Each frame will be extracted as (frameWidth % ImageWidth)</param>
        /// <param name="frameHeight">The frame height to extract. Each frame will be extracted as (frameHeight / ImageHeight)</param>
        /// <param name="frameCount">The frame count to extract</param>
        /// <param name="firstFrame">The first frame start to extracting from</param>
        /// <param name="storeInFrames">Whether to store the individual frames on an array</param>
        public void LoadAnimation(string path, int frameWidth, int frameHeight, int frameCount, int firstFrame, bool storeInFrames)
        {
            Image = System.Drawing.Image.FromFile(path);

            // Calculate cells dimensions
            int x_cells = Image.Width / frameWidth;
            int y_cells = Image.Height / frameHeight;

            AnimationDescriptor tempDesc = new AnimationDescriptor();

            // No frame count set? Calculate by the image size
            if (frameCount == -1)
                frameCount = (x_cells * y_cells) - firstFrame;

            // Frame count larger than frames on image? Trim the variable
            if (firstFrame + frameCount > x_cells * y_cells)
                frameCount = (x_cells * y_cells) - firstFrame;

            if (!storeInFrames)
            {
                FileStream s = new FileStream(path, FileMode.Open, FileAccess.Read);

                // Create the strip
                tempDesc.Strip = Texture2D.FromStream(Device, s);

                tempDesc.PlaybackRate = 1;

                s.Close();
                s.Dispose();

                // Create the frame boundary information
                tempDesc.FrameBounds = new FrameBoundCollection();
                tempDesc.FrameBounds.FrameBounds = new int[frameCount][];
                tempDesc.FrameBounds.OffsetX = new int[frameCount];
                tempDesc.FrameBounds.OffsetY = new int[frameCount];

                int f = 0;
            
                for (int cell = firstFrame; cell < firstFrame + frameCount; cell++)
                {
                    // Calculate the offset onto the input image:
                    int x = cell % x_cells * frameWidth;
                    int y = cell / x_cells * frameHeight;

                    // Set the frame bounding information
                    tempDesc.FrameBounds[cell - firstFrame] = new int[] { x, y, frameWidth, frameHeight };
                    tempDesc.FrameBounds.OffsetX[cell - firstFrame] = 0;
                    tempDesc.FrameBounds.OffsetY[cell - firstFrame] = 0;
                    f++;
                }
            }
            else
            {
                tempDesc.UseFrames = true;

                System.Drawing.Graphics g;

                MemoryStream s;

                tempDesc.Frames = new Texture2D[frameCount];

                // Iterate through all the frames
                for (int i = firstFrame; i < firstFrame + frameCount; i++)
                {
                    // Creates the new frame and use the Graphics object
                    // to transfer the frame

                    int x = i % x_cells * frameWidth;
                    int y = i / x_cells * frameHeight;

                    // Create and redraw the temporary frame into a temporary memory stream
                    System.Drawing.Bitmap Frame = new System.Drawing.Bitmap(frameWidth, frameHeight);

                    g = System.Drawing.Graphics.FromImage(Frame);
                    g.DrawImage(Image, new System.Drawing.Rectangle(0, 0, Image.Width, Image.Height), new System.Drawing.Rectangle(x, y, Image.Width, Image.Height), System.Drawing.GraphicsUnit.Pixel);
                    g.Flush();

                    s = new MemoryStream();

                    // Save the frame
                    Frame.Save(s, System.Drawing.Imaging.ImageFormat.Png);
                    Frame.Dispose();

                    // Reajust the position
                    s.Position = 0;

                    // Create the frame
                    tempDesc.Frames[i - firstFrame] = Texture2D.FromStream(Device, s);

                    // Dispose of the temporary stream
                    s.Dispose();
                }
            }

            // Set dimensions
            Width = tempDesc.Width = frameWidth;
            Height = tempDesc.Height = frameHeight;

            OriginalWidth = Width;
            OriginalHeight = Height;

            // Setup the animation data
            tempDesc.CurrentFrame = 0;
            tempDesc.FrameCount = frameCount;

            tempDesc.Play = tempDesc.FrameCount > 1 ? true : false;
            tempDesc.FPS = -1;
            tempDesc.CurrentFrame = 0;
            tempDesc.Reverse = false;

            Descriptor = tempDesc;

            loaded = true;

            HasArea = true;

            AfterMove();
            AfterScale();
        }

        /// <summary>
        /// Loads an animation
        /// </summary>
        /// <param name="texture">The texture to copy the animation from</param>
        /// <param name="frameWidth">The frame width to extract. Each frame will be extracted as (frameWidth % ImageWidth)</param>
        /// <param name="frameHeight">The frame height to extract. Each frame will be extracted as (frameHeight / ImageHeight)</param>
        /// <param name="frameCount">The frame count to extract</param>
        /// <param name="firstFrame">The first frame start to extracting from</param>
        public void LoadAnimation(Texture2D texture, int frameWidth, int frameHeight, int frameCount = -1, int firstFrame = 0)
        {
            LoadDescriptor(CreateDescriptor(texture, frameWidth, frameHeight, frameCount, firstFrame));
        }

        /// <summary>
        /// Creates a descriptor to be used in a GDAnimation based on the given parameters
        /// </summary>
        /// <param name="texture">The texture to copy the animation from</param>
        /// <param name="frameWidth">The frame width to extract. Each frame will be extracted as (frameWidth % ImageWidth)</param>
        /// <param name="frameHeight">The frame height to extract. Each frame will be extracted as (frameHeight / ImageHeight)</param>
        /// <param name="frameCount">The frame count to extract</param>
        /// <param name="firstFrame">The first frame start to extracting from</param>
        /// <param name="fps">The FPS to playback the animation at</param>
        /// <param name="frameskip">Whether to make use of frame skipping to smooth the animation over jagged framerate</param>
        /// <param name="endAction">The action the GDAnimation takes when the last frame is reached</param>
        /// <param name="optionalDescs">Set of optional parameters to be created</param>
        public static AnimationDescriptor CreateDescriptor(Texture2D texture, int frameWidth, int frameHeight, int frameCount = -1, int firstFrame = 0, int fps = -1, bool frameskip = true, AnimationEndAcion endAction = AnimationEndAcion.DoNothing, int optionalDescs = 0)
        {
            // Trim out the dimensions:
            frameWidth = (frameWidth > texture.Width ? texture.Width : frameWidth);
            frameHeight = (frameHeight > texture.Height ? texture.Height : frameHeight);

            // Calculate cells dimensions
            int x_cells = texture.Width / frameWidth;
            int y_cells = texture.Height / frameHeight;

            AnimationDescriptor tempDesc = new AnimationDescriptor();

            tempDesc.PlaybackRate = 1;

            // No frame count set? Calculate by the image size
            if (frameCount == -1)
                frameCount = (x_cells * y_cells) - firstFrame;

            // Frame count larger than frames on image? Trim the variable
            if (firstFrame + frameCount > x_cells * y_cells)
                frameCount = (x_cells * y_cells) - firstFrame;

            // Create the strip
            tempDesc.Strip = texture;

            // Create the frame boundary information
            tempDesc.FrameBounds = new FrameBoundCollection();
            tempDesc.FrameBounds.FrameBounds = new int[frameCount][];
            tempDesc.FrameBounds.OffsetX = new int[frameCount];
            tempDesc.FrameBounds.OffsetY = new int[frameCount];
            
            // Iterate through the frame count, calculating the image frame offset
            for (int cell = firstFrame; cell < firstFrame + frameCount; cell++)
            {
                // Calculate the offset onto the input image:
                int x = cell % x_cells * frameWidth;
                int y = cell / x_cells * frameHeight;

                // Set the frame bounding information

                // Check for the flip frame orders flag
                if ((optionalDescs & (int)DescOptions.FlipFrames) == 1)
                {
                    tempDesc.FrameBounds[frameCount - cell - 1] = new int[] { x, y, frameWidth, frameHeight };
                    tempDesc.FrameBounds.OffsetX[frameCount - cell - 1] = 0;
                    tempDesc.FrameBounds.OffsetY[frameCount - cell - 1] = 0;
                }
                else
                {
                    tempDesc.FrameBounds[cell - firstFrame] = new int[] { x, y, frameWidth, frameHeight };
                    tempDesc.FrameBounds.OffsetX[cell - firstFrame] = 0;
                    tempDesc.FrameBounds.OffsetY[cell - firstFrame] = 0;
                }
            }

            // Setup the size
            tempDesc.Width = frameWidth;
            tempDesc.Height = frameHeight;

            // Setup the animation data
            tempDesc.CurrentFrame = 0;
            tempDesc.FrameCount = frameCount;

            tempDesc.Play = tempDesc.FrameCount > 1 ? true : false;
            tempDesc.FPS = fps;
            tempDesc.FrameSkipEnabled = frameskip;
            tempDesc.CurrentFrame = 0;
            tempDesc.Reverse = false;
            tempDesc.EndAcion = endAction;

            return tempDesc;
        }

        /// <summary>
        /// Creates a new empty AnimationDescriptor with the given texture and frame count
        /// </summary>
        /// <param name="texture">The texture to use on the descriptor</param>
        /// <param name="frameCount">The number of frames in the AnimationDescriptor</param>
        /// <returns>An empty descriptor with the given Texture2D and frame count</returns>
        public static AnimationDescriptor CreateEmptyDescriptor(Texture2D texture, int frameCount)
        {
            AnimationDescriptor desc = new AnimationDescriptor();

            desc.FrameCount = frameCount;
            desc.Strip = texture;
            desc.PlaybackRate = 1;
            desc.Play = (frameCount > 1);
            desc.EndAcion = AnimationEndAcion.DoNothing;
            desc.SpriteEffect = SpriteEffects.None;
            desc.UseFrames = false;

            // Create the frame boundary information
            desc.FrameBounds = new FrameBoundCollection();
            desc.FrameBounds.FrameBounds = new int[frameCount][];
            desc.FrameBounds.OffsetX = new int[frameCount];
            desc.FrameBounds.OffsetY = new int[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                desc.FrameBounds[i] = new int[] { 0, 0, 0, 0 };
            }

            return desc;
        }

        /// <summary>
        /// Loads a descriptor into this GDAnimation
        /// </summary>
        /// <param name="descriptor">The animation descriptor to be loaded</param>
        public void LoadDescriptor(AnimationDescriptor descriptor)
        {
            // Set dimensions
            Width = descriptor.Width;
            Height = descriptor.Height;

            OriginalWidth = Width;
            OriginalHeight = Height;

            Descriptor = descriptor;

            loaded = true;

            HasArea = true;

            AfterMove();
            AfterScale();
        }

        #region GIF loading

        /// <summary>
        /// Load an animation from a GIF file and returns a GDAnimation object
        /// </summary>
        /// <param name="device">A GraphicsDevice object to use when creating the GDAnimation object</param>
        /// <param name="path">The path to a valid GIF file</param>
        /// <param name="lastFrame">The last frame starting from startingFrame to be loaded. -1 to load all</param>
        /// <param name="startingFrame">The starting frame from which begin to copy</param>
        /// <param name="storeInFrames">Whether to store them in separate frames instead of in a single strip. Might solve issues with 
        /// sprites beeing too large</param>
        public static GDAnimation FromGIF(GraphicsDevice device, string path, int startingFrame, int lastFrame, bool storeInFrames)
        {
            // Return the resulting animation using the functions listed on this class
            int width, height;
            FrameBoundCollection bounds;
            int frames;

            Texture2D[] image = ImagesFromGIF(device, path, startingFrame, lastFrame, out width, out height, out frames, out bounds, storeInFrames);

            return FromTexture(device, image[0], width, height, frames, bounds);
        }

        /// <summary>
        /// Creates a new GDAnimation from a texture file
        /// </summary>
        /// <param name="device">A GraphicsDevice object to use when creating the GDAnimation object</param>
        /// <param name="TexStrip">A Texture2D strip to load the frames from</param>
        /// <param name="width">The frame width</param>
        /// <param name="height">The frame height</param>
        /// <param name="frames">The number of frames on this animation</param>
        /// <param name="frameBounds">A valid FrameBoundCollection object to bind to the GDAnimation's descriptor</param>
        /// <returns>A new GDAnimation with the given parameters</returns>
        public static GDAnimation FromTexture(GraphicsDevice device, Texture2D TexStrip, int width, int height, int frames, FrameBoundCollection frameBounds)
        {
            // Create the new animation
            GDAnimation anim = new GDAnimation(device);

            anim.AfterMove();
            anim.AfterScale();

            anim.LoadDescriptor(CreateDescriptor(TexStrip, width, height, frames));
            anim.Descriptor.FrameBounds = frameBounds;

            // Return the resulting animation
            return anim;
        }

        /// <summary>
        /// Creates a new GDAnimation from a texture file
        /// </summary>
        /// <param name="device">A GraphicsDevice object to use when creating the GDAnimation object</param>
        /// <param name="Frames">A collection of Texture2D to use as frames</param>
        /// <param name="width">The frame width</param>
        /// <param name="height">The frame height</param>
        /// <param name="frames">The number of frames on this animation</param>
        /// <param name="frameBounds">A valid FrameBoundCollection object to bind to the GDAnimation's descriptor</param>
        /// <returns>A new GDAnimation with the given parameters</returns>
        public static GDAnimation FromTexture(GraphicsDevice device, Texture2D[] Frames, int width, int height, int frames, FrameBoundCollection frameBounds)
        {
            // Create the new animation
            GDAnimation anim = new GDAnimation(device);

            // Create the descriptor
            AnimationDescriptor tempDesc = new AnimationDescriptor();

            // Setup the newly created object
            anim.loaded = true;

            anim.Width = tempDesc.Width = width;
            anim.Height = tempDesc.Height = height;

            anim.OriginalWidth = width;
            anim.OriginalHeight = height;

            tempDesc.FrameBounds = frameBounds;
            if (Frames.Length == 1)
            {
                tempDesc.Strip = Frames[0];
                tempDesc.UseFrames = false;
            }
            else
            {
                tempDesc.Frames = Frames;
                tempDesc.UseFrames = true;
            }
            tempDesc.FrameCount = frames;
            tempDesc.CurrentFrame = 0;

            tempDesc.PlaybackRate = 1;

            tempDesc.Play = tempDesc.FrameCount > 1 ? true : false;
            tempDesc.FPS = -1;

            anim.Descriptor = tempDesc;

            anim.HasArea = true;

            anim.AfterMove();
            anim.AfterScale();

            // Return the resulting animation
            return anim;
        }

        /// <summary>
        /// Gets an image list from a GIF file
        /// </summary>
        /// <param name="device">The devide that will be used to instantialize the textures</param>
        /// <param name="path">The path to a valid GIF file</param>
        /// <param name="lastFrame">The last frame starting from startingFrame to be loaded. -1 to load all</param>
        /// <param name="startingFrame">The starting frame from which begin to copy</param>
        /// <param name="Width">The final Width size</param>
        /// <param name="Height">The final Heigth size</param>
        /// <param name="frameCount">The ammount of frames gathered from the GID file</param>
        /// <param name="FrameBounds">The frame bounds object resulted from the strip creation</param>
        /// <returns>The image list extracted from the GIF file</returns>
        /// <param name="storeInFrames">Whether to store them in separate frames instead of in a single strip. Might solve issues with 
        /// sprites beeing too large</param>
        public static Texture2D[] ImagesFromGIF(GraphicsDevice device, string path, int startingFrame, int lastFrame, out int Width, out int Height, out int frameCount, out FrameBoundCollection FrameBounds, bool storeInFrames)
        {
            // Loads the animated .GIF
            System.Drawing.Image img = System.Drawing.Image.FromFile(path);
            
            System.Drawing.Imaging.FrameDimension fd = new System.Drawing.Imaging.FrameDimension(img.FrameDimensionsList[0]);

            // Calculate the frame count
            frameCount = img.GetFrameCount(fd);
            
            // Check if frames are out of range
            if (lastFrame > frameCount || (lastFrame < startingFrame && lastFrame > 0))
                throw new Exception("Target frames where out of range.");

            // If last frame is not set, set it:
            if (lastFrame == -1)
                lastFrame = frameCount;

            // Create the new array of frames
            Texture2D[] Frames = new Texture2D[frameCount];
            Texture2D strip = null;

            // Temporary Graphics object
            System.Drawing.Graphics g;
            
            // Calculate strip boundary:
            int x_cells = (int)Math.Ceiling(Math.Sqrt(frameCount));
            int y_cells = (int)Math.Floor(Math.Sqrt(frameCount));

            int StripWidth  = x_cells * (Width = img.Width);
            int StripHeight = y_cells * (Height = img.Height);

            // Temporary BITS array:
            System.Drawing.Bitmap stripBit = new System.Drawing.Bitmap(StripWidth, StripHeight);

            string temp;

            g = System.Drawing.Graphics.FromImage(stripBit);

            FrameBounds = new FrameBoundCollection();
            FrameBounds.FrameBounds = new int[frameCount][];
            FrameBounds.OffsetX = new int[frameCount];
            FrameBounds.OffsetY = new int[frameCount];

            if (!storeInFrames)
            {
                // Loop thru' the selected frames and transfer them
                for (int i = startingFrame; i < lastFrame; i++)
                {
                    // Select the next frame
                    img.SelectActiveFrame(fd, i);

                    // Creates the new frame and use the Graphics object
                    // to transfer the frame

                    int x = (i - startingFrame) % x_cells * img.Width;
                    int y = (i - startingFrame) / x_cells * img.Height;

                    FrameBounds[(i - startingFrame)] = new int[] { x, y, Width, Height };
                    FrameBounds.OffsetX[(i - startingFrame)] = 0;
                    FrameBounds.OffsetY[(i - startingFrame)] = 0;

                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(img, new System.Drawing.Rectangle(x, y, img.Width, img.Height), new System.Drawing.Rectangle(0, 0, img.Width, img.Height), System.Drawing.GraphicsUnit.Pixel);
                }

                MemoryStream tmp = new MemoryStream();

                stripBit.Save(tmp, System.Drawing.Imaging.ImageFormat.Png);

                strip = Texture2D.FromStream(device, tmp);

                tmp.Dispose();

                Frames = new Texture2D[] { strip };
            }
            else
            {
                System.Drawing.Bitmap TempMap = null;

                // Loop thru' the selected frames and transfer them
                for (int i = startingFrame; i < lastFrame; i++)
                {
                    // Select the next frame
                    img.SelectActiveFrame(fd, i);

                    int cell = (i - startingFrame);

                    // Calculate the offset onto the input image:
                    int x = cell % x_cells * img.Width;
                    int y = cell / x_cells * img.Height;

                    // Create the new frame:
                    TempMap = new System.Drawing.Bitmap(img.Width, img.Height);
                    TempMap.GetHbitmap();

                    // Create a graphics object and use it to compute
                    // the new frame
                    g = System.Drawing.Graphics.FromImage(TempMap);

                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.AssumeLinear;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                    g.DrawImage(img, new Draw.Rectangle(0, 0, img.Width, img.Height), new Draw.Rectangle(0, 0, img.Width, img.Height), System.Drawing.GraphicsUnit.Pixel);
                    g.Flush(System.Drawing.Drawing2D.FlushIntention.Flush);
                    g.Dispose();

                    temp = Path.GetTempFileName();

                    MemoryStream tmp = new MemoryStream();

                    img.Save(tmp, System.Drawing.Imaging.ImageFormat.Png);

                    Frames[cell] = Texture2D.FromStream(device, tmp);

                    tmp.Dispose();

                    // File.Delete(temp);
                }
            }

            // Dispose used objects
            g.Dispose();
            img.Dispose();
            stripBit.Dispose();

            // Return the resulting animation
            return Frames;
        }

        #endregion

        /// <summary>
        /// Creates and returns a clone of this GDAnimation, optionally setting
        /// a new texture copy as well
        /// </summary>
        /// <param name="cloneTex">Whether to clone the texture as well, instead of using the same texture reference</param>
        /// <returns>The copy of this GDAnimation</returns>
        public GDAnimation Clone(bool cloneTex)
        {
            GDAnimation anim = new GDAnimation(Device);

            // Copy dimension and position information:
            anim.X = this.X;
            anim.Y = this.Y;
            anim.Position = this.Position;

            anim.loaded = this.loaded;

            anim.UserData = this.UserData;
            anim.Tint = this.Tint;

            anim.XOrigin = this.XOrigin;
            anim.YOrigin = this.YOrigin;
            anim.Origin = this.Origin;

            anim.Width = this.Width;
            anim.Height = this.Height;
            anim.OriginalWidth = this.OriginalWidth;
            anim.OriginalHeight = this.OriginalHeight;
            anim.Scale = this.Scale;
            anim.Unscaled = this.Unscaled;

            anim.HasArea = this.HasArea;

            anim.Rotation = this.Rotation;
            anim.Radians = this.Radians;

            // Copy the animation descriptor:
            anim.Descriptor = Descriptor;

            // Check whether to clone or just copy the texture over:
            if (cloneTex)
            {
                // Clone texture:
                Color[] data = new Color[Descriptor.Strip.Width * Descriptor.Strip.Height];

                Descriptor.Strip.GetData<Color>(data);

                anim.Descriptor.Strip = new Texture2D(this.Device, Descriptor.Strip.Width, Descriptor.Strip.Height);
                anim.Descriptor.Strip.SetData<Color>(data);

                data = null;
            }

            // Copy misc information:
            anim.Device = this.Device;

            return anim;
        }

        /// <summary>
        /// Updates the entity, advancing the animation frames.
        /// </summary>
        /// <param name="Time">The elapsed game time</param>
        public override void Update(GameTime Time)
        {
            // Test first if the animation is supposed to play
            if (Descriptor.Play)
            {                
                float fps = 1000.0f / (Descriptor.FPS * Descriptor.PlaybackRate);
                Descriptor.LastFrame += (float)Engine.Current.TotalMilliseconds;

                // Used to check for frame advancing
                int lastFrame = Descriptor.CurrentFrameInt;
                AnimationDescriptor lastDesc = Descriptor;

                // Whether the animation looped in this frame
                bool looped = false;

                // Play as fast as possible
                if (Descriptor.FPS == -1)
                {
                    if (!Descriptor.Reverse)
                    {
                        // Check for animation end
                        if ((Descriptor.CurrentFrame + 1) >= Descriptor.FrameCount)
                        {
                            // Call the event that alerts the animation has ended
                            if (AnimationOver != null)
                                AnimationOver.Invoke(this);

                            // Check if this entity is set to die once the animation ends
                            if (Descriptor.EndAcion == AnimationEndAcion.Die)
                            {
                                // Exclude this animation and free all resources
                                Parent.removeChild(this);
                                Free(false);
                                dead = true;

                                goto skipanim;
                            }
                            // Check if this entity is set to stop animation once the animation ends
                            else if (Descriptor.EndAcion == AnimationEndAcion.Stop)
                            {
                                // Pause this animation and set the current frame as the last
                                Descriptor.Play = false;
                                Descriptor.CurrentFrame = Descriptor.FrameCount;

                                goto skipanim;
                            }
                        }

                        // Advance it one frame at a time
                        Descriptor.CurrentFrame = (Descriptor.CurrentFrame + 1) % Descriptor.FrameCount;
                    }
                    else
                    {
                        // Check for animation end
                        if ((Descriptor.CurrentFrame - 1) <= 0)
                        {
                            // Call the event that alerts the animation has ended
                            if (AnimationOver != null)
                                AnimationOver.Invoke(this);

                            // Check if this entity is set to die once the animation ends
                            if (Descriptor.EndAcion == AnimationEndAcion.Die)
                            {
                                // Exclude this animation and free all resources
                                Parent.removeChild(this);
                                Free(false);
                                dead = true;

                                goto skipanim;
                            }
                            // Check if this entity is set to stop animation once the animation ends
                            else if (Descriptor.EndAcion == AnimationEndAcion.Stop)
                            {
                                // Pause this animation and set the current frame as the last
                                Descriptor.Play = false;
                                Descriptor.CurrentFrame = Descriptor.FrameCount;

                                goto skipanim;
                            }
                        }

                        // Advance it one frame at a time
                        Descriptor.CurrentFrame += (Descriptor.CurrentFrame == 0 ? Descriptor.FrameCount : -1);
                    }
                }
                // Play with a preset framerate
                else if ((Descriptor.LastFrame) > fps)
                {
                    if (!Descriptor.Reverse)
                    {
                        // Check if animation should still play
                        if (Math.Floor(Descriptor.CurrentFrame + (Descriptor.FrameSkipEnabled ? (Descriptor.LastFrame / fps) : 1)) >= Descriptor.FrameCount)
                        {
                            looped = true;

                            // Call the event that alerts the animation has ended
                            if (AnimationOver != null)
                                AnimationOver.Invoke(this);

                            // Check if this entity is set to die once the animation ends
                            if (Descriptor.EndAcion == AnimationEndAcion.Die)
                            {
                                // Exclude this animation and free all resources
                                Parent.removeChild(this);
                                Free(false);
                                dead = true;

                                goto skipanim;
                            }
                            // Check if this entity is set to stop animation once the animation ends
                            else if (Descriptor.EndAcion == AnimationEndAcion.Stop)
                            {
                                // Pause this animation and set the current frame as the last
                                Descriptor.Play = false;
                                Descriptor.CurrentFrame = Descriptor.FrameCount;

                                goto skipanim;
                            }
                        }

                        // Frameskip check
                        if (Descriptor.FrameSkipEnabled)
                            Descriptor.CurrentFrame = (int)Math.Floor((Descriptor.CurrentFrame + (Descriptor.LastFrame / fps)));
                        //Descriptor.CurrentFrame = (Descriptor.CurrentFrame + (Descriptor.LastFrame / fps)) % Descriptor.FrameCount;
                        else
                            Descriptor.CurrentFrame = (Descriptor.CurrentFrame + 1);

                        // Reset FPS counter
                        if (!looped)
                        {
                            Descriptor.LastFrame -= fps;
                        }
                        else
                        {
                            Descriptor.LastFrame = 0;
                        }

                        Descriptor.CurrentFrame = Descriptor.CurrentFrame % Descriptor.FrameCount;
                    }
                    else
                    {
                        // Check if animation should still play
                        //if (Math.Floor(Descriptor.CurrentFrame - (Descriptor.FrameSkipEnabled ? (Descriptor.LastFrame / fps) : 1)) < 0)
                        if (Descriptor.CurrentFrameInt - 1 < 0)
                        {
                            looped = true;

                            // Call the event that alerts the animation has ended
                            if (AnimationOver != null)
                                AnimationOver.Invoke(this);

                            // Check if this entity is set to die once the animation ends
                            if (Descriptor.EndAcion == AnimationEndAcion.Die)
                            {
                                // Exclude this animation and free all resources
                                Parent.removeChild(this);
                                Free(false);
                                dead = true;

                                goto skipanim;
                            }
                            // Check if this entity is set to stop animation once the animation ends
                            else if (Descriptor.EndAcion == AnimationEndAcion.Stop)
                            {
                                // Pause this animation and set the current frame as the last
                                Descriptor.Play = false;
                                Descriptor.CurrentFrame = Descriptor.FrameCount;

                                goto skipanim;
                            }
                        }

                        // Frameskip check
                        if (Descriptor.FrameSkipEnabled)
                            Descriptor.CurrentFrame = (int)Math.Ceiling(Descriptor.CurrentFrame - (Descriptor.LastFrame / fps));
                              //Descriptor.CurrentFrame = (Descriptor.CurrentFrame + (Descriptor.LastFrame / fps)) % Descriptor.FrameCount;
                        else
                            Descriptor.CurrentFrame = (Descriptor.CurrentFrame - 1);

                        // Reset FPS counter
                        if (!looped)
                        {
                            Descriptor.LastFrame -= fps;
                        }
                        else
                        {
                            Descriptor.LastFrame = 0;
                            Descriptor.CurrentFrame = Descriptor.FrameCount - (1 - Descriptor.LastFrame / fps);
                        }

                        if (Descriptor.CurrentFrame < 0)
                            Descriptor.CurrentFrame = 0;
                    }
                }

                // Check frame advancing
                if (lastFrame != Descriptor.CurrentFrameInt && lastDesc == Descriptor)
                {
                    if (FrameAdvanced != null)
                    {
                        // Call the event that alerts the animation has advanced at least one frame
                        if (!Descriptor.Reverse)
                        {
                            // If the animation has not looped yet
                            if (Descriptor.CurrentFrameInt != 0)
                            {
                                for (int cf = lastFrame; cf < Descriptor.CurrentFrameInt; cf++)
                                {
                                    FrameAdvanced.Invoke(this, cf);
                                }
                            }
                            // If the animation did loop, launch the FrameAdvanced on the last frame played
                            else
                                FrameAdvanced.Invoke(this, lastFrame);
                        }
                        else
                        {
                            // If the animation has not looped yet
                            if (Descriptor.CurrentFrameInt != Descriptor.FrameCount - 1)
                            {
                                for (int cf = lastFrame; cf > Descriptor.CurrentFrameInt; cf--)
                                {
                                    FrameAdvanced.Invoke(this, cf);
                                }
                            }
                            // If the animation did loop, launch the FrameAdvanced on the last frame played
                            else
                                FrameAdvanced.Invoke(this, lastFrame);
                        }
                    }
                }
            }
            // Handle paused animation cases
            else
            {
                // Special case: The animation is one frame long, and its stop action is set to 'Die'
                if (Descriptor.FrameCount == 1 && Descriptor.EndAcion == AnimationEndAcion.Die)
                {
                    // Exclude this animation and free all resources
                    Parent.removeChild(this, true);
                    dead = true;
                }
            }
        skipanim:

            base.Update(Time);
        }

        /// <summary>
        /// Simple rec used to draw the sprite. I have put it outside here
        /// to save some 'new' calling.
        /// </summary>
        Rectangle rec = new Rectangle(0, 0, 0, 0);
        bool dead = false;

        /// <summary>
        /// Renders this GDAnimation entity with the desired configurations
        /// </summary>
        /// <param name="g">The SpriteBatch object to render this entity into</param>
        /// <param name="redrawRect">The clipping redraw rectangle</param>
        /// <param name="offset">The offset to draw this entity to</param>
        public override void PostDraw(SpriteBatch g, RectangleF redrawRect, Point offset)
        {
            // Check for unloaded animation states that could possibly break the rest of the code
            if (dead || !loaded) return;

            // Get a int copy of the current frame that's going to be used to get the int-based Frame indexer
            int frame = Descriptor.CurrentFrameInt > Descriptor.FrameCount - 1 ? Descriptor.FrameCount - 1 : Descriptor.CurrentFrameInt;

            // Draw the sprite
            Vector2 position, imageOffset;

            position = absoluteFramePosition;
            imageOffset = absoluteOffset;

            // If the RoundPoints flag is set to true, we round the positions off before rendering
            if (RoundPoints || Descriptor.RoundPoints)
            {
                float factor = 2;

                if(position.X != (int)position.X)
                    position.X = (int)((float)Math.Round(position.X / (engine.Camera.scaleX * factor)) * (engine.Camera.scaleX * factor));
                if (position.Y != (int)position.Y)
                    position.Y = (int)((float)Math.Round(position.Y / (engine.Camera.scaleY * factor)) * (engine.Camera.scaleY * factor));

                if (imageOffset.X != (int)imageOffset.X)
                    imageOffset.X = (int)((float)Math.Round(imageOffset.X / (engine.Camera.scaleX * factor)) * (engine.Camera.scaleX * factor));
                if (imageOffset.Y != (int)imageOffset.Y)
                    imageOffset.Y = (int)((float)Math.Round(imageOffset.Y / (engine.Camera.scaleY * factor)) * (engine.Camera.scaleY * factor));
            }

            // Test whether to use the absoluteTint as it is or premultiply the tint now
            bool usePremultiplied = (engine.CurrentScreen == null ? engine.DesiredBlendState == BlendState.AlphaBlend : engine.CurrentScreen.DesiredBlendState == BlendState.AlphaBlend);

            // Check whether the use's using a Strip of frames or indexed Texture2Ds
            if (!Descriptor.UseFrames)
            {
                // Get the frame bounds:
                int frameX = Descriptor.FrameBounds[frame][0];
                int frameY = Descriptor.FrameBounds[frame][1];
                int frameWidth = Descriptor.FrameBounds[frame][2];
                int frameHeight = Descriptor.FrameBounds[frame][3];

                // Calculate the drawing rectangle
                rec.X = frameX;
                rec.Y = frameY;
                rec.Width = frameWidth;
                rec.Height = frameHeight;

                // Draw the frame! Finnaly.
                g.Draw(Descriptor.Strip, position, rec, (usePremultiplied ? Color.FromNonPremultiplied(absoluteTint.ToVector4()) : absoluteTint), MathHelper.ToRadians(absoluteRotation), imageOffset, absoluteScale, Descriptor.SpriteEffect, 0);
            }
            else
            {
                // Draw the frame off from the indexed Texture2Ds.
                g.Draw(Descriptor.Frames[frame], position, null, (usePremultiplied ? Color.FromNonPremultiplied(absoluteTint.ToVector4()) : absoluteTint), MathHelper.ToRadians(absoluteRotation), imageOffset, absoluteScale, Descriptor.SpriteEffect, 0);
            }
            
            // Draw base:
            base.PostDraw(g, redrawRect, offset);
        }

        /// <summary>
        /// Frees this entity from the memory
        /// </summary>
        public override void Free(bool ForceFree)
        {
            base.Free(ForceFree);

            if (Image != null)
            {
                try
                {
                    Image.Dispose();
                }
                catch (Exception) { }

                Image = null;
            }

            if (ForceFree)
            {
                try
                {
                    if (Descriptor.Strip != null && !Descriptor.Strip.IsDisposed)
                    {
                        Descriptor.Strip.Dispose();
                    }

                    if (Descriptor.Frames != null)
                    {
                        foreach (Texture2D tex in Descriptor.Frames)
                        {
                            if (tex != null && !tex.IsDisposed)
                                tex.Dispose();
                        }
                    }
                }
                catch (Exception) { }
            }

            Descriptor.FrameBounds.FrameBounds = null;
            Descriptor.Strip = null;
            Descriptor.Frames = null;

            loaded = false;
        }

        /// <summary>
        /// Calculates the screen area of this GDAnimation using the animation's width and height as size parameters
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

        /// <summary>
        /// Apply the transformation matrix recursively througout all the children entities, and update the absolute frame positions
        /// </summary>
        public override void ApplyMatrixRecursive()
        {
            base.ApplyMatrixRecursive();

            if (loaded && !Descriptor.UseFrames)
            {
                absoluteFramePosition = absolutePosition + GDMath.Rotate(new Vector2(Descriptor.FrameBounds.OffsetX[Descriptor.CurrentFrameInt], Descriptor.FrameBounds.OffsetY[Descriptor.CurrentFrameInt]) * absoluteScale, absoluteRotation);
            }
            else
            {
                absoluteFramePosition = absolutePosition;
            }
        }

        /// <summary>
        /// Whether to round the position of the image before drawing it into the screen.
        /// </summary>
        public bool RoundPoints = true;

        /// <summary>
        /// Whether there is a descriptor loaded into the animation
        /// </summary>
        protected bool loaded = false;

        /// <summary>
        /// Gets a value telling whether there is a descriptor loaded into the animation
        /// </summary>
        public bool Loaded
        {
            get { return loaded; }
        }

        /// <summary>
        /// Descriptor used to describe this animation
        /// </summary>
        public AnimationDescriptor Descriptor;

        /// <summary>
        /// The absolute position to draw the frame on the screen
        /// </summary>
        public Vector2 absoluteFramePosition;

        // TEMPORARY INTERNAL VARIABLES

        /// <summary>
        /// Animation strip. This will be disposed
        /// right after the animation is loaded
        /// </summary>
        protected System.Drawing.Image Image;
    }

    /// <summary>
    /// Describes an animation
    /// </summary>
    public struct AnimationDescriptor
    {
        /// <summary>
        /// A unique name used for serializing
        /// </summary>
        public string Name;

        /// <summary>
        /// The animation strip
        /// </summary>
        public Texture2D Strip;

        /// <summary>
        /// The animation frames
        /// </summary>
        public Texture2D[] Frames;

        /// <summary>
        /// The animation bounds
        /// </summary>
        public FrameBoundCollection FrameBounds;

        /// <summary>
        /// Whether to use separate frames instead of a single sprite strip
        /// </summary>
        public bool UseFrames;

        /// <summary>
        /// If the animation should play
        /// </summary>
        public bool Play;

        /// <summary>
        /// The animation speed in frames per second (-1 means as fast as possible)
        /// </summary>
        public int FPS;

        /// <summary>
        /// The rate at which the playback is set. Defaults to 1
        /// </summary>
        public float PlaybackRate;

        /// <summary>
        /// Whether the animation should play backwards
        /// </summary>
        public bool Reverse;

        /// <summary>
        /// The animation's frame count
        /// </summary>
        public int FrameCount;

        /// <summary>
        /// The animation's current frame
        /// </summary>
        public float CurrentFrame;

        /// <summary>
        /// Gets or sets the animation's current frame as an integer number
        /// </summary>
        public int CurrentFrameInt
        {
            set { CurrentFrame = Math.Max(Math.Min(value, FrameCount - 1), 0); }
            get { return (int)Math.Floor(CurrentFrame); }
        }

        /// <summary>
        /// What the animation should do when it ends
        /// </summary>
        public AnimationEndAcion EndAcion;

        /// <summary>
        /// Whether to use frame skipping, which will keep
        /// the animation FPS constant, even if the engine
        /// FPS is slower. This does NOT smooth the animation,
        /// but rather skips the frames that were supposed
        /// to be rendered by the Engine. Default is false.
        /// </summary>
        public bool FrameSkipEnabled;

        /// <summary>
        /// Keeps track of the last frame, in game time
        /// </summary>
        public float LastFrame;

        /// <summary>
        /// Sprite effect to apply to the final image
        /// </summary>
        public SpriteEffects SpriteEffect;

        /// <summary>
        /// The width of the animation
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the animation
        /// </summary>
        public int Height;

        /// <summary>
        /// Whether to round the position of the image before drawing it into the screen.
        /// </summary>
        public bool RoundPoints;

        /// <summary>
        /// Changes the current frame of this descriptor and playbacks the animation
        /// </summary>
        /// <param name="frame">The frame to start the animation from</param>
        public void GotoAndPlay(int frame)
        {
            CurrentFrameInt = frame;
            Play = true;
        }

        /// <summary>
        /// Changes the current frame of this descriptor and stops the animation
        /// </summary>
        /// <param name="frame">The frame to set the animation at</param>
        public void GotoAndStop(int frame)
        {
            CurrentFrameInt = frame;
            Play = false;
        }

        /// <summary>
        /// Performs a comparision between two AnimationDescriptor objects
        /// </summary>
        /// <param name="desc1">The first descriptor to compare</param>
        /// <param name="desc2">The second descriptor to compare</param>
        /// <returns>Whether the descriptors are similar</returns>
        public static bool operator ==(AnimationDescriptor desc1, AnimationDescriptor desc2)
        {
            return (desc1.Strip == desc2.Strip && desc1.Name == desc2.Name);
        }

        /// <summary>
        /// Performs a comparision between two AnimationDescriptor objects
        /// </summary>
        /// <param name="desc1">The first descriptor to compare</param>
        /// <param name="desc2">The second descriptor to compare</param>
        /// <returns>Whether the descriptors are not similar</returns>
        public static bool operator !=(AnimationDescriptor desc1, AnimationDescriptor desc2)
        {
            return (desc1.Strip != desc2.Strip || desc1.Name != desc2.Name);
        }

        /// <summary>
        /// Returns whether this AnimationDescriptor equals to the object provided
        /// </summary>
        /// <param name="obj">An object to compare</param>
        /// <returns>Whether this AnimationDescriptor equals to the object provided</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is AnimationDescriptor))
            {
                return false;
            }

            return ((AnimationDescriptor)obj).Strip == Strip && ((AnimationDescriptor)obj).Name == Name;
        }

        /// <summary>
        /// Gets a hash code for this AnimationDescriptor object
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a string that describes this AnimationDescriptor
        /// </summary>
        /// <returns>A string that describes this AnimationDescriptor</returns>
        public override string ToString()
        {
            return "{ Name: " + Name + ", Width: " + Width + " Height: " + Height + " FPS: " + FPS + " }";
        }
    }

    /// <summary>
    /// Represents a frame bound collecion, which helps
    /// defining a frame of an animation located into an
    /// image strip
    /// </summary>
    public struct FrameBoundCollection
    {
        /// <summary>
        /// Represents a frame bound. A frame bound is composed
        /// of four ints on the second dimension of the array,
        /// defining the x and y location and width and height
        /// sizes of the strip that composes the frame on the
        /// first dimension.
        /// </summary>
        public int[][] FrameBounds;

        /// <summary>
        /// The X offset of the frame
        /// </summary>
        public int[] OffsetX;

        /// <summary>
        /// The Y offset of the frame
        /// </summary>
        public int[] OffsetY;

        /// <summary>
        /// Gets the frame bound information based on a given frame
        /// </summary>
        /// <param name="frameIndex">The frame to get information</param>
        /// <returns>The information from the given frame</returns>
        public int[] this[int frameIndex]
        {
            get { return FrameBounds[frameIndex]; }
            set { FrameBounds[frameIndex] = value; }
        }
    }

    // Enums:

    /// <summary>
    /// List of optional parameters to provide for the CreateDescriptor method
    /// </summary>
    public enum DescOptions
    {
        /// <summary>
        /// Flip the frame orders so the animation runs backwards from original frame orders
        /// </summary>
        FlipFrames = 1
    }

    /// <summary>
    /// States what an animation should do when it reaches the last frame of the animation
    /// </summary>
    public enum AnimationEndAcion
    {
        /// <summary>
        /// The animation should stay stopped.
        /// No action is taken
        /// </summary>
        DoNothing = 0,
        /// <summary>
        /// The animation should die (is excluded) from the parent list and
        /// the entity is permanently deleted
        /// </summary>
        Die = 1,
        /// <summary>
        /// Stops the animation
        /// </summary>
        Stop = 2
    }
}