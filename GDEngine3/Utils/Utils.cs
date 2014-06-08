using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace GDEngine3.Utils
{
    /// <summary>
    /// Some util stuff
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Turns a color from the UInt format to XNA's Color class
        /// </summary>
        /// <param name="color">The UInt representing a color</param>
        /// <param name="preMult">Whether the value is premultiplied or not</param>
        /// <returns>The color represented by XNA's Color class</returns>
        public static Color ColorFromUint(uint color, bool preMult = false)
        {
            if(preMult)
                return new Color((int)(color >> 16) & 0xFF, (int)(color >> 8) & 0xFF, (int)(color & 0xFF), (int)(color >> 24) & 0xFF);
            else
                return Color.FromNonPremultiplied((int)(color >> 16) & 0xFF, (int)(color >> 8) & 0xFF, (int)(color & 0xFF), (int)(color >> 24) & 0xFF);
        }

        /// <summary>
        /// Traces all the given objects into Visual Studio's Output window
        /// </summary>
        /// <param name="obj">An array of objects to output</param>
        public static void trace(params object[] obj)
        {
            for(int i = 0; i < obj.Length; i++)
            {
                object ob = obj[i];

                if (ob == null)
                    System.Diagnostics.Debug.Write("null");
                else
                    System.Diagnostics.Debug.Write(ob.ToString());

                if (i != obj.Length - 1)
                    System.Diagnostics.Debug.Write("  -  ");
            }

            System.Diagnostics.Debug.WriteLine("");
        }
    }
}