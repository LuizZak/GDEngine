using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GDEngine3.Utils;

namespace GDEngine3.Display
{
    /// <summary>
    /// Represents a Camera, which allows the Engine to scroll the view around
    /// </summary>
    public class GDCamera
    {
        /// <summary>
        /// Gets or sets the camera's X position
        /// </summary>
        public float x
        {
            get { return X; }
            set
            {
                if (X != value)
                {
                    X = value;
                    Constraint();
                }
            }
        }
        /// <summary>
        /// Gets or sets the camera's Y position
        /// </summary>
        public float y
        {
            get { return Y; }
            set
            {
                if (Y != value)
                {
                    Y = value;
                    Constraint();
                }
            }
        }

        /// <summary>
        /// Gets or sets the camera's width
        /// </summary>
        public float width
        {
            get { return Width; }
            set
            {
                if (Width != value)
                {
                    Width = value;
                    Constraint();
                }
            }
        }
        /// <summary>
        /// Gers or sets the camera's height
        /// </summary>
        public float height
        {
            get { return Height; }
            set
            {
                if (Height != value)
                {
                    Height = value;
                    Constraint();
                }
            }
        }

        /// <summary>
        /// The rotation of the camera, in radians
        /// </summary>
        public float rotation = 0f;

        /// <summary>
        /// Gets or sets the camera's X scale
        /// </summary>
        public float scaleX
        {
            get { return Width / origWidth; }
            set { Width = origWidth * value; }
        }
        /// <summary>
        /// Gets or sets the camera's Y scale
        /// </summary>
        public float scaleY
        {
            get { return Height / origHeight; }
            set { Height = origHeight * value; }
        }

        
        /// <summary>
        /// The camera's starting width
        /// </summary>
        public float origWidth;
        /// <summary>
        /// The camera's starting height
        /// </summary>
        public float origHeight;

        /// <summary>
        /// The boundaries of this camera
        /// </summary>
        protected internal RectangleF Boundaries = RectangleF.Empty;

        /// <summary>
        /// The camera's X position
        /// </summary>
        protected internal float X;
        /// <summary>
        /// The camera's Y position
        /// </summary>
        protected internal float Y;
        /// <summary>
        /// The camera's width
        /// </summary>
        protected internal float Width;
        /// <summary>
        /// The camera's height
        /// </summary>
        protected internal float Height;

        /// <summary>
        /// Gets or sets the boundaries for this camera
        /// </summary>
        public RectangleF boundaries
        {
            get { return Boundaries; }
            set
            {
                if (Boundaries != value)
                {
                    Boundaries = value;
                    Constraint();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the GDCamera class
        /// </summary>
        public GDCamera()
        {

        }

        /// <summary>
        /// Initializes a new instance of the GDCamera class
        /// </summary>
        /// <param name="X">The X position of the camera</param>
        /// <param name="Y">The Y position of the camera</param>
        public GDCamera(float X, float Y)
        {
            this.X = X;
            this.Y = Y;

            Width = origWidth = 640;
            Height = origHeight = 480;

            scaleX = 1;
            scaleY = 1;
        }

        /// <summary>
        /// Initializes a new instance of the GDCamera class
        /// </summary>
        /// <param name="X">The X position of the camera</param>
        /// <param name="Y">The Y position of the camera</param>
        /// <param name="Width">The camera's Width</param>
        /// <param name="Height">The camera's Height</param>
        public GDCamera(float X, float Y, float Width, float Height)
        {
            this.X = X;
            this.Y = Y;

            Width = origWidth = Width;
            Height = origHeight = Height;

            scaleX = 1;
            scaleY = 1;
        }

        /// <summary>
        /// Constraints the camera inside the boundaries
        /// </summary>
        public void Constraint()
        {
            if (Boundaries == null || Boundaries.IsEmpty)
                return;

            if (X + width > Boundaries.X + Boundaries.Width)
                X = Boundaries.X + Boundaries.Width - width;
            if (Y + height > Boundaries.Y + Boundaries.Height)
                Y = Boundaries.Y + Boundaries.Height - height;

            if (X < Boundaries.X)
                X = Boundaries.X;

            if (Y < Boundaries.Y)
                Y = Boundaries.Y;
        }

        /// <summary>
        /// Reset the camera parameters
        /// </summary>
        public void Reset()
        {
            X = 0;
            Y = 0;

            Width = origWidth = 640;
            Height = origHeight = 480;

            scaleX = 1;
            scaleY = 1;
        }
    }
}
