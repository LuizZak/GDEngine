using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

using GDEngine3.Display;

namespace GDEngine3.Utils
{
    /// <summary>
    /// A content storage for AnimationDescriptors
    /// </summary>
    public static class GDAnimStorage
    {
        /// <summary>
        /// The dictionary of AnimDescriptors stored on this GDAnimStorage class
        /// </summary>
        public static Dictionary<string, GDAnimStorageItem> Content;

        /// <summary>
        /// A graphics device used to create textures during runtime
        /// </summary>
        public static GraphicsDevice device;

        /// <summary>
        /// Static constructor fot the GDAnimStorage class
        /// </summary>
        static GDAnimStorage()
        {
            Content = new Dictionary<string, GDAnimStorageItem>();
        }

        /// <summary>
        /// Adds an AnimationDescriptor directly into the dictionary, and assign a key to it
        /// </summary>
        /// <param name="desc">The AnimationDescriptor object</param>
        /// <param name="keyName">The key used to refere to the item</param>
        public static AnimationDescriptor Add(AnimationDescriptor desc, string keyName)
        {
            // Check if the key already exists
            if (Content.ContainsKey(keyName))
                throw new ArgumentException("The given key name " + keyName + " already exists in the dictionary", "keyName");

            desc.Name = keyName;
            Content[keyName] = new GDAnimStorageItem(desc, keyName);

            return desc;
        }

        /// <summary>
        /// Removes the AnimationDescriptor binded to the given keyName from the storage.
        /// It does not free the AnimationDescriptor, but instead only wipes out any reference of it from the storage.
        /// </summary>
        /// <param name="keyName">The keyName that is binded to an AnimationDescriptor</param>
        /// <returns>The removed AnimationDescriptor</returns>
        public static AnimationDescriptor Remove(string keyName)
        {
            // Get a temp copy of the descriptor
            AnimationDescriptor desc = Get(keyName);

            // Remove it from the contents
            Content.Remove(keyName);

            // Return the copy of the descriptor
            return desc;
        }

        /// <summary>
        /// Gets an AnimationDescriptor assigned with the given keyname
        /// </summary>
        /// <param name="keyName">The keyname to look for</param>
        /// <returns>The AnimationDescriptor assigned with that keyname</returns>
        public static AnimationDescriptor Get(string keyName)
        {
            AnimationDescriptor desc = Content[keyName].AnimDesc;

            desc.Name = keyName;

            return desc;
        }

        /// <summary>
        /// Gets the GDAnimStorageItem assigned with the given keyname
        /// </summary>
        /// <param name="keyName">The keyname to look for</param>
        /// <returns>The assigned with that keyname</returns>
        public static GDAnimStorageItem GetContent(string keyName)
        {
            return Content[keyName];
        }

        /// <summary>
        /// Returns a list of all the items currently stored in the GDAnimStorage class
        /// </summary>
        /// <returns>A list of all the items currently stored in the GDAnimStorage class</returns>
        public static GDAnimStorageItem[] GetAllItems()
        {
            GDAnimStorageItem[] items = new GDAnimStorageItem[Content.Count];
            int i = 0;

            // Traverse the Dictionary that holds all the animation descriptors
            foreach (KeyValuePair<string, GDAnimStorageItem> k in Content)
            {
                items[i++] = k.Value;
            }

            return items;
        }

        /// <summary>
        /// Frees up all used resources
        /// </summary>
        /// <param name="force">Whether to free the textures from memory as well</param>
        public static void Free(bool force)
        {
            // Traverse the Dictionary that holds all the animation descriptors
            foreach (KeyValuePair<string, GDAnimStorageItem> k in Content)
            {
                // Free the descriptor
                k.Value.Free(force);
            }

            // Clear the content dictionary
            Content.Clear();
        }
    }

    /// <summary>
    /// Describes a Texture item used in the Content bunker
    /// </summary>
    public class GDAnimStorageItem
    {
        /// <summary>
        /// The AnimationDescriptor binded to this GDAnimStorageItem
        /// </summary>
        public AnimationDescriptor AnimDesc;

        /// <summary>
        /// The name identifier for this GDAnimStorageItem
        /// </summary>
        public string Name;

        /// <summary>
        /// Creates a new instance of the GDAnimStorageItem class
        /// </summary>
        /// <param name="desc">The AnimationDescriptor to assign to this GDAnimStorageItem</param>
        /// <param name="name">The name to assign to this GDAnimStorageItem</param>
        public GDAnimStorageItem(AnimationDescriptor desc, string name)
        {
            AnimDesc = desc;
            Name = name;
        }

        /// <summary>
        /// Frees up all resources used by this GDAnimStorageItem
        /// </summary>
        /// <param name="force">Whether to free up all AnimationDescriptor strips and frames</param>
        public void Free(bool force)
        {
            // If the freeing is forced, remove all textures from memory
            if (force)
            {
                // If we have an undisposed strip, dispose it
                if(AnimDesc.Strip != null && !AnimDesc.Strip.IsDisposed)
                    AnimDesc.Strip.Dispose();

                // Iterate through all the animation frames
                if(AnimDesc.Frames != null)
                    foreach (Texture2D text in AnimDesc.Frames)
                    {
                        // If the frame texture is undisposed, dispose it
                        if (!text.IsDisposed)
                            text.Dispose();
                    }

                // Clear out all variables from the descriptor as well
                AnimDesc.Strip = null;
                AnimDesc.Frames = null;
            }

            AnimDesc = new AnimationDescriptor();
        }
    }
}