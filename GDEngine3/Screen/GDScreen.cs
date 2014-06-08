using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using GDEngine3.Event;
using GDEngine3.Utils;

namespace GDEngine3.Screen
{
    /// <summary>
    /// A GDScreen is a class used as a level container for all the entities currently on screen.
    /// </summary>
    public class GDScreen
    {
        /// <summary>
        /// Called when the screen is first initialized
        /// </summary>
        public virtual void Init()
        {
            Root = new GDEntity();
            Root.isRoot = true;
            Root.Engine = MainEngine;

            eventHandler = new GDDefaultEventHandler();
        }

        /// <summary>
        /// Called when the screen is deinitialized
        /// </summary>
        public virtual void End()
        {
            eventHandler.Clear();
            UnloadContent();
            MainEngine = null;
        }

        /// <summary>
        /// Loads all the content of the screen
        /// </summary>
        public virtual void LoadContent(ContentManager Content)
        {
            Loaded = true;
        }

        /// <summary>
        /// Unloads all the content of the screen
        /// </summary>
        public virtual void UnloadContent()
        {
            // Skip method when there is no content loaded
            if (!Loaded)
                return;

            // Unload the root entity and set the Loaded flag to false
            Root.Free(false);
            Loaded = false;
        }

        /// <summary>
        /// Updates all the entities in the screen
        /// </summary>
        /// <param name="Time">The GameTime object</param>
        public virtual void Update(GameTime Time)
        {
            // Update the transformation matrix
            if (Root.IsDirty())
            {
                Root.absoluteTransform = Matrix.Identity;
                Root.ApplyMatrixRecursive();
            }

            Root.Update(Time);
        }

        /// <summary>
        /// Prepares the level for rendering
        /// </summary>
        public virtual void BeginDraw()
        {

        }

        /// <summary>
        /// Redraws the level
        /// </summary>
        /// <param name="graphics">The SpriteBatch to which the level will be rendered on</param>
        /// <param name="Buffered">If the graphics should be buffered</param>
        /// <param name="time">The elapsed time</param>
        public virtual void Redraw(SpriteBatch graphics, bool Buffered, GameTime time)
        {
            // Draws the root and all visible children
            if (UseCamera)
            {
                Root.x = -MainEngine.Camera.X / MainEngine.Camera.scaleX;
                Root.y = -MainEngine.Camera.Y / MainEngine.Camera.scaleY;

                Root.scaleX = 1 / MainEngine.Camera.scaleX;
                Root.scaleY = 1 / MainEngine.Camera.scaleY;
            }

            if (Root.IsDirty())
            {
                Root.absoluteTransform = Matrix.Identity;
                Root.ApplyMatrixRecursive();
            }

            graphics.Begin(DesiredSortMode, DesiredBlendState, DesiredSamplerState, DesiredDepthStencilState, DesiredRasterizerState);
            
            Root.Draw(graphics, new RectangleF(0, 0, MainEngine.Width, MainEngine.Height), Point.Zero);

            graphics.End();
        }

        /// <summary>
        /// Call this function after the draw function, and after the spriteBatch.end call, to free
        /// the memory from the used resources that must be cleaned
        /// </summary>
        public virtual void PostDraw()
        {
            // Check for killed entities:
            for (int i = 0; i < Removed.Count; i++)
            {
                if (Removed[i].parent != null)
                    Removed[i].parent.removeChild(Removed[i]);

                Removed[i].Free(false);
            }

            Removed.Clear();
        }

        /// <summary>
        /// Registers the given IEventReceiver object to receive events of the given type
        /// </summary>
        /// <param name="receiver">The event receiver to register</param>
        /// <param name="eventType">The event type to notify the receiver of</param>
        public void RegisterEventReceiver(IEventReceiver receiver, string eventType)
        {
            eventHandler.RegisterEventReceiver(receiver, eventType);
        }

        /// <summary>
        /// Unregisters the given IEventReceiver object from receiving events of the given type
        /// </summary>
        /// <param name="receiver">The event receiver to unregister</param>
        /// <param name="eventType">The event type to unregister</param>
        public void UnregisterEventReceiver(IEventReceiver receiver, string eventType)
        {
            eventHandler.UnregisterEventReceiver(receiver, eventType);
        }

        /// <summary>
        /// Unregisters the given IEventReceiver object from receiving all events it is currently registered on
        /// </summary>
        /// <param name="receiver">The event receiver</param>
        public void UnregisterEventReceiverFromAllEvents(IEventReceiver receiver)
        {
            eventHandler.UnregisterEventReceiverFromAllEvents(receiver);
        }

        /// <summary>
        /// Broadcasts the given GDEvent to all currently registered receivers
        /// </summary>
        /// <param name="gameEvent">The event to broadcast</param>
        public void BroadcastEvent(GDEvent gameEvent)
        {
            eventHandler.BroadcastEvent(gameEvent);
        }        

        /// <summary>
        /// The internal event handler for the game screen
        /// </summary>
        private IEventHandler eventHandler;

        /// <summary>
        /// The root entity which all entities are children of
        /// </summary>
        public GDEntity Root;

        /// <summary>
        /// The GDMainEngine reference. It points to the engine owning this GDScreen
        /// </summary>
        public GDMainEngine MainEngine;

        /// <summary>
        /// List of removed entities waiting to be cleaned
        /// </summary>
        private List<GDEntity> Removed;

        /// <summary>
        /// The SpriteSortMode to use when rendering the graphics.
        /// It defaults to SpriteSortMode.Immediate.
        /// If you want to use shaders on the graphics of some entities but not others, change to SpriteSortMode.Immediate
        /// </summary>
        public SpriteSortMode DesiredSortMode = SpriteSortMode.Immediate;

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
        /// The DepthStencilState to use when rendering the graphics.
        /// It defaults to DepthStencilState.Default.
        /// </summary>
        public DepthStencilState DesiredDepthStencilState = DepthStencilState.Default;

        /// <summary>
        /// The RasterizerState to use when rendering the graphics.
        /// It defaults to RasterizerState.CullNone.
        /// If you want to clip sprites with the GraphicsDevice.ScissorsRectangle, provide a RasterizerState with ScissorsTestEnabled set to true
        /// </summary>
        public RasterizerState DesiredRasterizerState = new RasterizerState() { CullMode = CullMode.None, ScissorTestEnable = true };

        /// <summary>
        /// Whether to use the camera transformations
        /// </summary>
        public bool UseCamera = true;

        /// <summary>
        /// Whether the content level is loaded
        /// </summary>
        public bool Loaded = false;
    }
}