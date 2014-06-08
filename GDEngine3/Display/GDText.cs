using System;
using System.Collections.Generic;
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
    /// Represents a Text displaying entity that can render strings onto the screen
    /// </summary>
    public class GDText : GDEntity
    {
        /// <summary>
        /// The text to display on this GDText
        /// </summary>
        private string text = "";

        /// <summary>
        /// The font used to display the text
        /// </summary>
        private SpriteFont spriteFont;

        /// <summary>
        /// The alignment of this GDText's text
        /// </summary>
        private Align align;

        /// <summary>
        /// The text this GDText should display on screen
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = value;
                    Width = (OriginalWidth = spriteFont.MeasureString(text).X) * scaleX;
                    Height = (OriginalHeight = spriteFont.MeasureString(text).Y) * scaleY;
                    AlignText();
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// The SpriteBatch, describing the font used to display the text on screen
        /// </summary>
        public SpriteFont SpriteFont
        {
            get { return spriteFont; }
            set
            {
                if (spriteFont != value)
                {
                    spriteFont = value;
                    Width = (OriginalWidth = spriteFont.MeasureString(text).X) * scaleX;
                    Height = (OriginalHeight = spriteFont.MeasureString(text).Y) * scaleY;
                    AlignText();
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// The SpriteBatch, describing the font used to display the text on screen
        /// </summary>
        public Align TextAlign
        {
            get { return align; }
            set
            {
                if (align != value)
                {
                    align = value;
                    Width = (OriginalWidth = spriteFont.MeasureString(text).X) * scaleX;
                    Height = (OriginalHeight = spriteFont.MeasureString(text).Y) * scaleY;
                    AlignText();
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Whether to round the position of the text before rendering it onto the screen.
        /// </summary>
        public bool RoundPoints;

        /// <summary>
        /// Creates a new instance of the GDText entity
        /// </summary>
        /// <param name="font">The font used to render the text</param>
        /// <param name="text">A string to draw on the screen</param>
        public GDText(SpriteFont font, String text = "")
        {
            this.text = text;
            spriteFont = font;

            Width = OriginalWidth = spriteFont.MeasureString(text).X;
            Height = OriginalHeight = spriteFont.MeasureString(text).Y;

            HasArea = true;
        }

        /// <summary>
        /// Draws this text on screen
        /// </summary>
        /// <param name="g">The SpriteBatch that will be used to render the text</param>
        /// <param name="redrawRect">The redraw rectangle object that tells the invalidated area to redraw</param>
        /// <param name="offset"></param>
        public override void PostDraw(SpriteBatch g, RectangleF redrawRect, Point offset)
        {
            base.PostDraw(g, redrawRect, offset);

            // Draw the sprite
            Vector2 position, imageOffset;

            position = absolutePosition;
            imageOffset = absoluteOffset;

            // Get the position
            if (RoundPoints)
            {
                position.X = (float)Math.Round(position.X);
                position.Y = (float)Math.Round(position.Y);

                imageOffset.X = (float)Math.Round(absoluteOffset.X);
                imageOffset.Y = (float)Math.Round(absoluteOffset.Y);
            }

            // Draw the string into the sprite batch
            g.DrawString(spriteFont, text, position, Color.FromNonPremultiplied(absoluteTint.ToVector4()), MathHelper.ToRadians(absoluteRotation), imageOffset, absoluteScale, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Nullifies the SpriteFont to prepare this GDText for garbage collection. 
        /// It also frees all children entities in chain order
        /// </summary>
        /// <param name="ForceFree">
        /// Whether to forcefully free textures by disposing them. 
        /// Set to false to only nullify their references
        /// </param>
        public override void Free(bool ForceFree)
        {
            base.Free(ForceFree);

            text = "";
            spriteFont = null;
        }

        /// <summary>
        /// Updates the text alignment
        /// </summary>
        public void AlignText()
        {
            if (align == Align.Left)
            {
                XOrigin = 0;
                YOrigin = 0;
            }
            else if(align == Align.Center)
            {
                YOrigin = 0;
                XOrigin = OriginalWidth / 2;
            }
            else if (align == Align.Right)
            {
                YOrigin = 0;
                XOrigin = OriginalWidth;
            }

            AfterMove();
        }

        /// <summary>
        /// Calculates the Area and LocalArea rectangles of this GDText, taking into account the text's size
        /// </summary>
        public override void CalculateArea()
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

        /// <summary>
        /// The alignment of the text
        /// </summary>
        public enum Align
        {
            /// <summary>
            /// No text alignment is done
            /// </summary>
            None,
            /// <summary>
            /// Aligns the text on the left-most point
            /// </summary>
            Left,
            /// <summary>
            /// Aligns the text on the center point
            /// </summary>
            Center,
            /// <summary>
            /// Aligns the text on the right-most point
            /// </summary>
            Right
        }
    }
}