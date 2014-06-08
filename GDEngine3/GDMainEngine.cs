using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

using SColor = System.Drawing.Color;
using MColor = Microsoft.Xna.Framework.Graphics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

using GDEngine3.Display;
using GDEngine3.Screen;
using GDEngine3.Utils;

namespace GDEngine3
{
    /// <summary>
    /// Main class for the GDEngine.
    /// </summary>
    public class GDMainEngine : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the GDMainEngine, and prepares the engine to be used in a game
        /// </summary>
        /// <param name="game">The Game instance that is using this engine</param>
        /// <param name="device">The GraphicsDevice object that will be used to initialize graphics during the game</param>
        public GDMainEngine(Game game, GraphicsDevice device)
        {
            // Assign some starting variables
            Game = game;

            Device = device;

            Width  = device.PresentationParameters.BackBufferWidth;
            Height = device.PresentationParameters.BackBufferHeight;

            RenderTarget = new RenderTarget2D(Device, Width, Height, true, device.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

            Device.SetRenderTarget(null);

            Root = new GDEntity(0, 0, Width, Height, this);
            Root.isRoot = true;

            Removed = new List<GDEntity>();

            Camera = new GDCamera(0, 0, Width, Height);

            // Generate the 1x1 pixel texture
            if (GDTextureFactory.PixelTexture == null)
            {
                GDTextureFactory.PixelTexture = new Texture2D(device, 1, 1);
                GDTextureFactory.PixelTexture.SetData<Color>(new Color[] { Color.White });
            }

            // Set the content providers device
            GDContent.device = device;

            // Set the statics GraphicsDevice value
            GDStatics.Device = device;
        }

        /// <summary>
        /// Switches from the current screen to the new provided one
        /// </summary>
        /// <param name="newScreen">The new screen to switch to. Remember to only pass a recently instantiated GDScreen (don't call GDScreen.Init() !)</param>
        public void SwitchScreen(GDScreen newScreen)
        {
            if (CurrentScreen != null)
            {
                CurrentScreen.End();
            }

            CurrentScreen = newScreen;
            CurrentScreen.MainEngine = this;
            CurrentScreen.Init();

            if (IsLoaded)
                CurrentScreen.LoadContent(Game.Content);
        }

        /// <summary>
        /// Adds an entity to the root entity's children list
        /// </summary>
        /// <param name="newChild">The new children to add</param>
        /// <returns>The entity created</returns>
        public GDEntity addChild(GDEntity newChild)
        {
            if (CurrentScreen != null)
            {
                CurrentScreen.Root.addChild(newChild);
            }
            else
                Root.addChild(newChild, -1);

            return newChild;
        }

        /// <summary>
        /// Loads all content
        /// </summary>
        public void LoadContent()
        {
            if (CurrentScreen != null && !CurrentScreen.Loaded)
                CurrentScreen.LoadContent(Game.Content);

            IsLoaded = true;
        }

        /// <summary>
        /// Unloads all content
        /// </summary>
        public void UnloadContent()
        {
            if (CurrentScreen != null && CurrentScreen.Loaded)
                CurrentScreen.UnloadContent();

            IsLoaded = false;
        }

        /// <summary>
        /// Disposes the engine, releasing all used resources (including Strips, Images and Content providers)
        /// </summary>
        public void Dispose()
        {
            Root.Free(true);

            // Dispose the content providers
            GDAnimStorage.Free(true);
            GDSoundStorage.Free(true);
            GDContent.Free(true);
        }

        /// <summary>
        /// Updates all the entities attached to this engine recursively
        /// </summary>
        /// <param name="Time">The GameData object that contains the Delta Time property</param>
        public void Update(GameTime Time)
        {
            Current = Time.ElapsedGameTime;
            CurrentLong = Current.Milliseconds;

            if (CurrentScreen != null)
            {
                CurrentScreen.Update(Time);
            }
            else
                Root.Update(Time);
        }

        /// <summary>
        /// Prepare the engine for rendering
        /// </summary>
        public void BeginDraw()
        {
            if(CurrentScreen != null)
                CurrentScreen.BeginDraw();
        }

        /// <summary>
        /// Redraws the engine
        /// </summary>
        /// <param name="graphics">The SpriteBatch used to render the engine to the screen</param>
        /// <param name="Buffered">Whether to use the buffered graphics, withouth redrawing the engine (non functioning for now)</param>
        /// <param name="Time">The GameTime object that contains the DeltaTime information</param>
        public void Redraw(SpriteBatch graphics, bool Buffered, GameTime Time)
        {
            // Draws the root and all visible children
            if (CurrentScreen != null)
            {
                // Redraw the root
                CurrentScreen.Redraw(graphics, Buffered, Time);
            }
            else
            {
                // Make the camera ajustments
                if (UseCamera)
                {
                    Root.x = -Camera.X / Camera.scaleX;
                    Root.y = -Camera.Y / Camera.scaleY;

                    Root.scaleX = 1 / Camera.scaleX;
                    Root.scaleY = 1 / Camera.scaleY;
                }

                // Refresh the Root if the display list has been made dirty
                if (Root.IsDirty())
                {
                    Root.absoluteTransform = Matrix.Identity;
                    Root.ApplyMatrixRecursive();
                }

                // Draw the entities
                graphics.Begin(SpriteSortMode.Deferred, DesiredBlendState, DesiredSamplerState, DepthStencilState.Default, RasterizerState.CullNone);
                Root.Draw(graphics, new RectangleF(0, 0, Width, Height), Point.Zero);
                graphics.End();
            }
        }

        /// <summary>
        /// Call this function after the draw function, and after the spriteBatch.end call, to free
        /// the memory from the used resources that must be cleaned
        /// </summary>
        public void PostDraw()
        {
            CurrentScreen.PostDraw();

            // Check for killed entities:
            for (int i = 0; i < Removed.Count; i++)
            {
                if(Removed[i].parent != null)
                    Removed[i].parent.removeChild(Removed[i]);

                Removed[i].Free(false);
            }

            Removed.Clear();
        }


        //// SETUP:

        /// <summary>
        /// The game this engine belongs to
        /// </summary>
        public Game Game;

        /// <summary>
        /// The device rendering this engine
        /// </summary>
        public GraphicsDevice Device;

        /// <summary>
        /// The current level on screen
        /// </summary>
        public GDScreen CurrentScreen;

        /// <summary>
        /// Whether there's content currently loaded
        /// </summary>
        public bool IsLoaded;

        /// <summary>
        /// The root entity that all other entities are child of
        /// </summary>
        public GDEntity Root;

        /// <summary>
        /// List of removed entities waiting to be cleaned
        /// </summary>
        public List<GDEntity> Removed;

        /// <summary>
        /// Current timespan
        /// </summary>
        public TimeSpan Current;

        /// <summary>
        /// Current timespan in long form (milliseconds)
        /// </summary>
        public long CurrentLong;


        //// GRAPHICS:

        /// <summary>
        /// Width of this engine
        /// </summary>
        public int Width;

        /// <summary>
        /// Height of this engine
        /// </summary>
        public int Height;

        /// <summary>
        /// Camera used for rendering
        /// </summary>
        public GDCamera Camera;

        /// <summary>
        /// Whether to use the camera transformations
        /// </summary>
        public bool UseCamera = true;

        /// <summary>
        /// The SamplerState to use when rendering the graphics.  
        /// It defaults to SamplerState.AnistropicClamp. 
        /// If you want to render pixel art without blurriness, change the state to SamplerState.PointClamp
        /// </summary>
        public SamplerState DesiredSamplerState = SamplerState.AnisotropicClamp;

        /// <summary>
        /// The BlendState to use when rendering the graphics. 
        /// It defaults to BlendState.AlphaBlend. 
        /// If you're loading the textures outside the content processor, use BlendState.NonPremultiplied
        /// </summary>
        public BlendState DesiredBlendState = BlendState.AlphaBlend;

        /// <summary>
        /// Render target associated with this instance
        /// </summary>
        public RenderTarget2D RenderTarget;

        /// <summary>
        /// Graphics used for drawing
        /// </summary>
        public SpriteBatch Graphics;

        /// <summary>
        /// Image buffer
        /// </summary>
        public Texture2D Buffer;

        /// <summary>
        /// The background color used for drawing
        /// </summary>
        public Color BackgroundColor = Color.Black;
    }
}