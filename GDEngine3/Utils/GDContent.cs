using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace GDEngine3.Utils
{
    /// <summary>
    /// A content bunker used to store commonly used textures
    /// </summary>
    public static class GDContent
    {
        /// <summary>
        /// The dictionary of content stored on this GDContent class
        /// </summary>
        public static Dictionary<string, GDContentItem> Content;

        /// <summary>
        /// A graphics device used to create textures during runtime
        /// </summary>
        public static GraphicsDevice device;

        /// <summary>
        /// Static constructor fot the GDContent class
        /// </summary>
        static GDContent()
        {
            Content = new Dictionary<string, GDContentItem>();
        }

        /// <summary>
        /// Adds an item in the dictionary from a file, and assign a key to it
        /// </summary>
        /// <param name="fileName">The file name of the texture file</param>
        /// <param name="keyName">The key used to refere to the item. Leave blank for filename</param>
        public static void AddFromFile(string fileName, string keyName = "")
        {
            if (keyName == "")
                keyName = fileName;

            // Check if the key already exists
            if(Content.ContainsKey(keyName))
                throw new ArgumentException("The given key name " + keyName + " already exists in the dictionary", "keyName");

            FileStream f = File.OpenRead(fileName);

            Content[keyName] = new GDContentItem(Texture2D.FromStream(device, f), keyName);

            f.Close();
        }

        /// <summary>
        /// Adds a texture directly into the dictionary, and assign a key to it
        /// </summary>
        /// <param name="texture">The Texture2D object</param>
        /// <param name="keyName">The key used to refere to the item</param>
        public static void AddFromTexture(Texture2D texture, string keyName)
        {
            // Check if the key already exists
            if (Content.ContainsKey(keyName))
                throw new ArgumentException("The given key name " + keyName + " already exists in the dictionary", "keyName");

            Content[keyName] = new GDContentItem(texture, keyName);
        }

        /// <summary>
        /// Adds an item in the dictionary from a Stream, and assign a key to it
        /// </summary>
        /// <param name="stream">The stream of the texture file. Stream is not disposed after use</param>
        /// <param name="keyName">The key used to refere to the item</param>
        public static void AddFromStream(Stream stream, string keyName)
        {
            // Check if the key already exists
            if (Content.ContainsKey(keyName))
                throw new ArgumentException("The given key name " + keyName + " already exists in the dictionary", "keyName");

            Content[keyName] = new GDContentItem(Texture2D.FromStream(device, stream), keyName);
        }

        /// <summary>
        /// Returns a Texture2D binded to the given key name
        /// </summary>
        /// <param name="keyName">A keyname that is binded to one of the textures on the GDContent</param>
        /// <returns>A Texture2D that is binded to the given key name</returns>
        public static Texture2D Get(string keyName)
        {
            return Content[keyName].Texture;
        }

        /// <summary>
        /// Returns the GDContentItem binded to the given key name
        /// </summary>
        /// <param name="keyName">A keyname that is binded to one of the GDContentItems on the GDContent</param>
        /// <returns>A GDContentItem that is binded to the given key name</returns>
        public static GDContentItem GetContent(string keyName)
        {
            return Content[keyName];
        }

        /// <summary>
        /// Returns a list of all the items currently stored in the GDContent class
        /// </summary>
        /// <returns>A list of all the items currently stored in the GDContent class</returns>
        public static GDContentItem[] GetAllItems()
        {
            GDContentItem[] items = new GDContentItem[Content.Count];
            int i = 0;

            // Traverse the Dictionary that holds all the animation descriptors
            foreach (KeyValuePair<string, GDContentItem> k in Content)
            {
                items[i++] = k.Value;
            }

            return items;
        }

        /// <summary>
        /// Frees up all used resources
        /// </summary>
        public static void Free(bool force)
        {
            foreach (KeyValuePair<string, GDContentItem> k in Content)
            {
                k.Value.Free(force);
            }

            Content.Clear();
        }
    }

    /// <summary>
    /// Describes a Texture item used in the Content bunker
    /// </summary>
    public class GDContentItem
    {
        /// <summary>
        /// The texture binded to this GDContentItem
        /// </summary>
        public Texture2D Texture;

        /// <summary>
        /// The name identifier for this GDContentItem
        /// </summary>
        public string Name;

        /// <summary>
        /// Initializes a new instance of the GDContentItem class
        /// </summary>
        /// <param name="texture">A valid Texture2D to bind to this GDContentItem</param>
        /// <param name="name">The name to assign to this GDContentItem</param>
        public GDContentItem(Texture2D texture, string name)
        {
            Texture = texture;
            Name = name;
        }

        /// <summary>
        /// Frees this GDContentItem object
        /// </summary>
        /// <param name="force">Whether to dispose of the binded Texture2D as well</param>
        public void Free(bool force)
        {
            if(force)
                Texture.Dispose();

            Texture = null;
        }
    }
}