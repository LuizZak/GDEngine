using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace GDEngine3.Utils
{
    /// <summary>
    /// Provides a place to store sound effects
    /// </summary>
    public static class GDSoundStorage
    {
        /// <summary>
        /// The dictionary that holds the SoundEffects
        /// </summary>
        public static Dictionary<string, GDSoundStorageItem> Content;

        /// <summary>
        /// Statically initalizes the GDSoundStorage class
        /// </summary>
        static GDSoundStorage()
        {
            Content = new Dictionary<string, GDSoundStorageItem>();
        }

        /// <summary>
        /// Adds an SoundEffect directly into the dictionary, and assign a key to it
        /// </summary>
        /// <param name="sound">The SoundEffect object</param>
        /// <param name="keyName">The key used to refere to the item</param>
        public static SoundEffect Add(SoundEffect sound, string keyName)
        {
            // Check if the key already exists
            if (Content.ContainsKey(keyName))
                throw new ArgumentException("The given key name " + keyName + " already exists in the dictionary", "keyName");

            Content[keyName] = new GDSoundStorageItem(sound, keyName);

            return sound;
        }

        /// <summary>
        /// Removes the SoundEffect binded to the given keyName from the storage.
        /// It does not free the SoundEffect, but instead only wipes out any reference of it from the storage.
        /// </summary>
        /// <param name="keyName">The keyName that is binded to an SoundEffect</param>
        /// <returns>The removec SoundEffect</returns>
        public static SoundEffect Remove(string keyName)
        {
            // Get a temp copy of the descriptor
            SoundEffect sound = Get(keyName);

            // Remove it from the contents
            Content.Remove(keyName);

            // Return the copy of the descriptor
            return sound;
        }

        /// <summary>
        /// Gets an SoundEffect assigned with the given keyname
        /// </summary>
        /// <param name="keyName">The keyname to look for</param>
        /// <returns>The SoundEffect assigned with that keyname</returns>
        public static SoundEffect Get(string keyName)
        {
            return Content[keyName].SoundFx;
        }

        /// <summary>
        /// Gets the GDSoundStorageItem assigned with the given keyname
        /// </summary>
        /// <param name="keyName">The keyname to look for</param>
        /// <returns>The assigned with that keyname</returns>
        public static GDSoundStorageItem GetContent(string keyName)
        {
            return Content[keyName];
        }

        /// <summary>
        /// Returns a list of all the items currently stored in the GDSoundStorage class
        /// </summary>
        /// <returns>A list of all the items currently stored in the GDSoundStorage class</returns>
        public static GDSoundStorageItem[] GetAllItems()
        {
            GDSoundStorageItem[] items = new GDSoundStorageItem[Content.Count];
            int i = 0;

            // Traverse the Dictionary that holds all the animation descriptors
            foreach (KeyValuePair<string, GDSoundStorageItem> k in Content)
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
            foreach (KeyValuePair<string, GDSoundStorageItem> k in Content)
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
    public class GDSoundStorageItem
    {
        /// <summary>
        /// The SoundEffect binded to this GDSoundStorageItem
        /// </summary>
        public SoundEffect SoundFx;

        /// <summary>
        /// The identifier for this GDSoundStorageItem
        /// </summary>
        public string Name;

        /// <summary>
        /// Creates a new instance of the GDSoundStorageItem class
        /// </summary>
        /// <param name="sound">The SoundEffect to assign to this GDSoundStorageItem</param>
        /// <param name="name">The name to assign to this GDSoundStorageItem</param>
        public GDSoundStorageItem(SoundEffect sound, string name)
        {
            SoundFx = sound;
            Name = name;
        }

        /// <summary>
        /// Frees up all resources used by this GDSoundStorageItem
        /// </summary>
        /// <param name="force">Whether to free up all SoundEffects strips and frames</param>
        public void Free(bool force)
        {
            // If the freeing is forced, remove all SoundEffects from memory
            if (force)
            {
                SoundFx.Dispose();
                SoundFx = null;
            }
        }
    }
}