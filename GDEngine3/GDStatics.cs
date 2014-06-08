using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Store static variables that are usable accross all classes that can access this class
/// </summary>
public static class GDStatics
{
    /// <summary>
    /// The standard GraphicsDevice currently being used by the engine
    /// </summary>
    public static GraphicsDevice Device;

    /// <summary>
    /// Sums all the given points to a point
    /// </summary>
    /// <param name="p">The point to have the value modified</param>
    /// <param name="points">A list of points to have their values added</param>
    /// <returns>The first given PointF value</returns>
    public static PointF Add(this PointF p, params PointF[] points)
    {
        foreach (PointF p2 in points)
        {
            p.X += p2.X;
            p.Y += p2.Y;
        }

        return p;
    }

    /// <summary>
    /// Sums all the given points to a RectangleF
    /// </summary>
    /// <param name="p">The RectangleF to have the value modified</param>
    /// <param name="points">A list of points to have their values added</param>
    /// <returns>The first given RectangleF value</returns>
    public static RectangleF Add(this RectangleF p, params PointF[] points)
    {
        RectangleF rec = new RectangleF(p.X, p.Y, p.Width, p.Height);

        foreach (PointF p2 in points)
        {
            rec.X += p2.X;
            rec.Y += p2.Y;
        }

        return rec;
    }

    /// <summary>
    /// Returns the portion of the two given rectangles that intersect
    /// </summary>
    /// <param name="rect1">A rectangle</param>
    /// <param name="rect2">Another rectangle</param>
    /// <returns>A rectangle that represents the portion of the two given rectangles that that is intersecting</returns>
    public static RectangleF Intersection(this RectangleF rect1, RectangleF rect2)
    {
        RectangleF rectangle = new RectangleF();
        float maxWidth1 = rect1.X + rect1.Width;
        float maxWidth2 = rect2.X + rect2.Width;
        float maxHeight1 = rect1.Y + rect1.Height;
        float maxHeight2 = rect2.Y + rect2.Height;
        float maxX = (rect1.X > rect2.X) ? rect1.X : rect2.X;
        float maxY = (rect1.Y > rect2.Y) ? rect1.Y : rect2.Y;
        float minMaxWidth = (maxWidth1 < maxWidth2) ? maxWidth1 : maxWidth2;
        float minMaxHeight = (maxHeight1 < maxHeight2) ? maxHeight1 : maxHeight2;

        if ((minMaxWidth > maxX) && (minMaxHeight > maxY))
        {
            rectangle.X = maxX;
            rectangle.Y = maxY;
            rectangle.Width = minMaxWidth - maxX;
            rectangle.Height = minMaxHeight - maxY;

            return rectangle;
        }

        rectangle.X = 0;
        rectangle.Y = 0;
        rectangle.Width = 0;
        rectangle.Height = 0;

        return rectangle;
    }
}