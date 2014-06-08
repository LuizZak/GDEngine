using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Globalization;

using Microsoft.Xna.Framework;

namespace GDEngine3.Utils
{
    // I really don't remember where I got this from... probably a quick google search would do it, but I'm too damn lazy.
    // So, if you're the author of this file, please contact me!
    /// <summary>
    /// RectangleF implementation that provides functionality to represent a floating point coordinates rectangle
    /// </summary>
    public struct RectangleF : IEquatable<RectangleF>
    {
        /// <summary>
        /// Unite two rectangles into a single RectangleF that contains both rectangles.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static RectangleF Union(RectangleF value1, RectangleF value2)
        {
            RectangleF rectangle = new RectangleF();

            float num6 = value1.X + value1.Width;
            float num5 = value2.X + value2.Width;
            float num4 = value1.Y + value1.Height;
            float num3 = value2.Y + value2.Height;
            float num2 = (value1.X < value2.X) ? value1.X : value2.X;
            float num = (value1.Y < value2.Y) ? value1.Y : value2.Y;
            float num8 = (num6 > num5) ? num6 : num5;
            float num7 = (num4 > num3) ? num4 : num3;

            rectangle.X = num2;
            rectangle.Y = num;
            rectangle.Width = num8 - num2;
            rectangle.Height = num7 - num;

            return rectangle;
        }

        /// <summary>Initializes a floating point rectangle</summary>
        /// <param name="x">The x-coordinate of the rectangle's lower right corner</param>
        /// <param name="y">The y-coordinate of the rectangle's lower right corner</param>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="height">Height of the rectangle</param>
        public RectangleF(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Returns a hard copy of this RectangleF struct
        /// </summary>
        /// <returns>An exact copy of this RectangleF struct</returns>
        public RectangleF Clone()
        {
            return new RectangleF(X, Y, Width, Height);
        }

        /// <summary>Changes the position of the RectangleF</summary>
        /// <param name="amount">The values to adjust the position of the rectangle by</param>
        public void Offset(Vector2 amount)
        {
            Offset(amount.X, amount.Y);
        }

        /// <summary>Changes the position of the RectangleF</summary>
        /// <param name="offsetX">Change in the x-position</param>
        /// <param name="offsetY">Change in the y-position</param>
        public void Offset(float offsetX, float offsetY)
        {
            this.X += offsetX;
            this.Y += offsetY;
        }

        /// <summary>Changes the position of the RectangleF and return a copy of this translated RectangleF</summary>
        /// <param name="offsetX">Change in the x-position</param>
        /// <param name="offsetY">Change in the y-position</param>
        public RectangleF LocalOffset(float offsetX, float offsetY)
        {
            RectangleF copy = this;

            copy.X += offsetX;
            copy.Y += offsetY;

            return copy;
        }

        /// <summary>
        /// Resizes this rectangle to a new given size
        /// </summary>
        /// <param name="newWidth">The new Width</param>
        /// <param name="newHeight">The new Height</param>
        public void Resize(float newWidth, float newHeight)
        {
            Width = newWidth;
            Height = newHeight;
        }

        /// <summary>
        /// Returns a copy of this rectangle resized to a new given size
        /// </summary>
        /// <param name="newWidth">The new Width</param>
        /// <param name="newHeight">The new Height</param>
        public RectangleF LocalResize(float newWidth, float newHeight)
        {
            return new RectangleF(X, Y, newWidth, newHeight);
        }

        /// <summary>
        /// Pushes the edges of the Rectangle out by the horizontal and
        /// vertical values specified
        /// </summary>
        /// <param name="horizontalAmount">Value to push the sides out by</param>
        /// <param name="verticalAmount">Value to push the top and bottom out by</param>
        public void Inflate(float horizontalAmount, float verticalAmount)
        {
            this.X -= horizontalAmount;
            this.Y -= verticalAmount;
            this.Width += horizontalAmount * 2;
            this.Height += verticalAmount * 2;
        }

        /// <summary>
        /// Creates and returns a set of points that represents this rectangle's corners
        /// </summary>
        /// <returns>The set of points that represent the rectangle's corners</returns>
        public Vector2[] GetPoints()
        {
            Vector2[] points = new Vector2[4];

            points[0] = new Vector2(X, Y);
            points[1] = new Vector2(X + Width, Y);
            points[2] = new Vector2(X + Width, Y + Height);
            points[3] = new Vector2(X, Y + Height);

            return points;
        }

        /// <summary>Determines whether the rectangle contains a specified Point</summary>
        /// <param name="point">The point to evaluate</param>
        /// <returns>
        ///   True if the specified point is contained within this rectangle; false otherwise
        /// </returns>
        public bool Contains(Vector2 point)
        {
            return Contains(point.X, point.Y);
        }

        /// <summary>Determines whether the rectangle contains a specified Point</summary>
        /// <param name="point">The point to evaluate</param>
        /// <param name="result">
        ///   True if the specified point is contained within this rectangle; false otherwise
        /// </param>
        public void Contains(ref Vector2 point, out bool result)
        {
            result = Contains(point.X, point.Y);
        }

        /// <summary>
        ///   Determines whether this Rectangle contains a specified point represented by
        ///   its x- and y-coordinates
        /// </summary>
        /// <param name="x">The x-coordinate of the specified point</param>
        /// <param name="y">The y-coordinate of the specified point</param>
        /// <returns>
        ///   True if the specified point is contained within this rectangle; false otherwise
        /// </returns>
        public bool Contains(float x, float y)
        {
            return
              (x >= this.X) &&
              (y >= this.Y) &&
              (x < this.X + this.Width) &&
              (y < this.Y + this.Height);
        }

        /// <summary>
        ///   Determines whether the rectangle contains another rectangle in its entirety
        /// </summary>
        /// <param name="other">The rectangle to evaluate</param>
        /// <returns>
        ///   True if the rectangle entirely contains the specified rectangle; false otherwise
        /// </returns>
        public bool Contains(RectangleF other)
        {
            bool result;
            Contains(ref other, out result);
            return result;
        }

        /// <summary>
        ///   Determines whether this rectangle entirely contains a specified rectangle
        /// </summary>
        /// <param name="other">The rectangle to evaluate</param>
        /// <param name="result">
        ///   On exit, is true if this rectangle entirely contains the specified rectangle,
        ///   or false if not
        /// </param>
        public void Contains(ref RectangleF other, out bool result)
        {
            result =
              (other.X >= this.X) &&
              (other.Y >= this.Y) &&
              ((other.X + other.Width) <= (this.X + this.Width)) &&
              ((other.Y + other.Height) <= (this.Y + this.Height));
        }

        /// <summary>
        ///   Determines whether a specified rectangle intersects with this rectangle
        /// </summary>
        /// <param name="rectangle">The rectangle to evaluate</param>
        /// <returns>
        ///   True if the specified rectangle intersects with this one; false otherwise
        /// </returns>
        public bool Intersects(RectangleF rectangle)
        {
            return
              (rectangle.X < (this.X + this.Width)) &&
              (rectangle.Y < (this.Y + this.Height)) &&
              ((rectangle.X + rectangle.Width) > this.X) &&
              ((rectangle.Y + rectangle.Height) > this.Y);
        }

        /// <summary>
        ///   Determines whether a specified rectangle intersects with this rectangle
        /// </summary>
        /// <param name="rectangle">The rectangle to evaluate</param>
        /// <param name="result">
        ///   True if the specified rectangle intersects with this one; false otherwise
        /// </param>
        public void Intersects(ref RectangleF rectangle, out bool result)
        {
            result =
              (rectangle.X < (this.X + this.Width)) &&
              (rectangle.Y < (this.Y + this.Height)) &&
              ((rectangle.X + rectangle.Width) > this.X) &&
              ((rectangle.Y + rectangle.Height) > this.Y);
        }

        /// <summary>
        /// Resets this rectangle, setting the size and position to 0
        /// </summary>
        public void Reset()
        {
            X = Y = Width = Height = 0;
        }

        /// <summary>
        ///   Determines whether the specified rectangle is equal to this rectangle
        /// </summary>
        /// <param name="other">The rectangle to compare with this rectangle</param>
        /// <returns>
        ///   True if the specified rectangle is equal to the this rectangle; false otherwise
        /// </returns>
        public bool Equals(RectangleF other)
        {
            return
              (this.X == other.X) &&
              (this.Y == other.Y) &&
              (this.Width == other.Width) &&
              (this.Height == other.Height);
        }

        /// <summary>
        ///   Returns a value that indicates whether the current instance is equal to a
        ///   specified object
        /// </summary>
        /// <param name="other">Object to make the comparison with</param>
        /// <returns>
        ///   True if the current instance is equal to the specified object; false otherwise
        /// </returns>
        public override bool Equals(object other)
        {
            if (!(other is RectangleF))
            {
                return false;
            }

            return Equals((RectangleF)other);
        }

        /// <summary>Retrieves a string representation of the current object</summary>
        /// <returns>String that represents the object</returns>
        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;

            return string.Format(
              currentCulture, "{{X:{0} Y:{1} Width:{2} Height:{3}}}",
          this.X.ToString(currentCulture),
              this.Y.ToString(currentCulture),
              this.Width.ToString(currentCulture),
              this.Height.ToString(currentCulture)
            );
        }

        /// <summary>Gets the hash code for this object</summary>
        /// <returns>Hash code for this object</returns>
        public override int GetHashCode()
        {
            return
              this.X.GetHashCode() ^
              this.Y.GetHashCode() ^
              this.Width.GetHashCode() ^
              this.Height.GetHashCode();
        }

        /// <summary>Compares two rectangles for equality</summary>
        /// <param name="first">Source rectangle</param>
        /// <param name="second">Source rectangle</param>
        /// <returns>True if the rectangles are equal; false otherwise</returns>
        public static bool operator ==(RectangleF first, RectangleF second)
        {
            return
              (first.X == second.X) &&
              (first.Y == second.Y) &&
              (first.Width == second.Width) &&
              (first.Height == second.Height);
        }

        /// <summary>Compares two rectangles for inequality</summary>
        /// <param name="first">Source rectangle</param>
        /// <param name="second">Source rectangle</param>
        /// <returns>True if the rectangles are not equal; false otherwise</returns>
        public static bool operator !=(RectangleF first, RectangleF second)
        {
            return
              (first.X != second.X) ||
              (first.Y != second.Y) ||
              (first.Width != second.Width) ||
              (first.Height != second.Height);
        }

        /// <summary>
        /// Multiplies the position and size of a RectangleF by the given number
        /// </summary>
        /// <param name="rectangle">The rectangle to multiply</param>
        /// <param name="value">The value to multiply</param>
        /// <returns>The rectangle, with its position and size multiplied by the given value</returns>
        public static RectangleF operator *(RectangleF rectangle, float value)
        {
            return new RectangleF(rectangle.X * value, rectangle.Y * value, rectangle.Width * value, rectangle.Height * value);
        }

        /// <summary>
        /// Divides the position and size of a RectangleF by the given number
        /// </summary>
        /// <param name="rectangle">The rectangle to divide</param>
        /// <param name="value">The value to divide</param>
        /// <returns>The rectangle, with its position and size divided by the given value</returns>
        public static RectangleF operator /(RectangleF rectangle, float value)
        {
            return new RectangleF(rectangle.X / value, rectangle.Y / value, rectangle.Width / value, rectangle.Height / value);
        }

        /// <summary>
        /// Gets the x-coordinate of the left side of the rectangle
        /// </summary>
        public float Left
        {
            get { return this.X; }
        }
        /// <summary>
        /// Gets the x-coordinate of the right side of the rectangle
        /// </summary>
        public float Right
        {
            get { return (this.X + this.Width); }
        }
        /// <summary>
        /// Gets the y-coordinate of the top of the rectangle
        /// </summary>
        public float Top
        {
            get { return this.Y; }
        }
        /// <summary>
        /// Gets the y-coordinate of the bottom of the rectangle
        /// </summary>
        public float Bottom
        {
            get { return (this.Y + this.Height); }
        }
        /// <summary>
        /// Gets whether this RectangleF entity is empty (all values equal to 0)
        /// </summary>
        public bool IsEmpty
        {
            get { return (Y == 0 && X == 0 && Width == 0 && Height == 0); }
        }

        /// <summary>Returns a Rectangle with all of its values set to zero</summary>
        /// <returns>An empty Rectangle</returns>
        public static RectangleF Empty
        {
            get { return empty; }
        }

        /// <summary>Specifies the x-coordinate of the rectangle</summary>
        public float X;
        /// <summary>Specifies the y-coordinate of the rectangle</summary>
        public float Y;
        /// <summary>Specifies the width of the rectangle</summary>
        public float Width;
        /// <summary>Specifies the height of the rectangle</summary>
        public float Height;

        /// <summary>
        /// Gets or sets a Vector2 representing this RectangleF's position
        /// </summary>
        public Vector2 Position
        {
            get { return new Vector2(X, Y); }
            set { X = value.X; Y = value.Y; }
        }
        /// <summary>
        /// Gets or sets a Vector2 representing this RectangleF's size
        /// </summary>
        public Vector2 Size
        {
            get { return new Vector2(Width, Height); }
            set { Width = value.X; Height = value.Y; }
        }

        /// <summary>
        /// Represents an empty RectangleF object
        /// </summary>
        private static RectangleF empty = new RectangleF();
    }
}