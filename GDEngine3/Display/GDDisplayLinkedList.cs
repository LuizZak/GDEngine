using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace GDEngine3.Display
{
    /// <summary>
    /// Represents a linked list of display elements (GDEntity)
    /// </summary>
    public class GDDisplayLinkedList : IEnumerable
    {
        /// <summary>
        /// Gets the first node of the list
        /// </summary>
        public GDDisplayLinkedListLink List;

        /// <summary>
        /// Last link on the list
        /// </summary>
        public GDDisplayLinkedListLink Last;

        /// <summary>
        /// The number of elements in this List
        /// </summary>
        protected int size;

        /// <summary>
        /// Create a new instance o fthe GDDisplayLinkedList class
        /// </summary>
        public GDDisplayLinkedList()
        {
            size = 0;
        }

        /// <summary>
        /// Add the given Link into the list
        /// </summary>
        /// <param name="Link">The Link to add to the list</param>
        /// <param name="index">The index at which to insert this entity. Leave -1 to push to the end of the list</param>
        /// <exception cref="ArgumentException">The Link already has a parent list</exception>
        public void Add(GDDisplayLinkedListLink Link, int index = -1)
        {
            if (Link.ParentList != null)
                throw new Exception("Link was already in a list");

            if (index > size)
                throw new ArgumentException("Index is out of length of the list", "index");

            Link.ParentList = this;

            // Case: List is empty, set the link as the whole list
            if (List == null)
            {
                List = Link;
                Last = Link;
            }
            else
            {
                GDDisplayLinkedListLink link = null;

                if (index > 0 && index < size)
                    link = List.GetNextLink(index);

                if (link != null)
                {
                    if (link != Last)
                        link.Next.Previous = Link;
                    else
                        Last = Link;

                    Link.Next = link.Next;
                    Link.Previous = link;
                    link.Next = Link;
                }
                else if (index == 0)
                {
                    List.Previous = Link;
                    Link.Next = List;

                    List = Link;
                }
                else
                {
                    link = Last;

                    Last = Link;

                    Link.Next = link.Next;
                    Link.Previous = link;
                    link.Next = Link;
                }
            }

            size++;
        }

        /// <summary>
        /// Add the given GDEntity into the list
        /// </summary>
        /// <param name="Entity">The GDEntity to add to the list</param>
        /// <param name="index">The index at which to insert this entity. Leave -1 to push to the end of the list</param>
        /// <exception cref="ArgumentException">The GDEntity.Link already has a parent list</exception>
        public void Add(GDEntity Entity, int index = -1)
        {
            Add(Entity.Link, index);
        }

        /// <summary>
        /// Removes the given Link from the list of links
        /// </summary>
        /// <param name="Link">The Link to remove</param>
        public GDDisplayLinkedListLink Remove(GDDisplayLinkedListLink Link)
        {
            // Work only if this link belongs to this list
            if (Link.ParentList != this)
                return Link;

            // Condition: This is the only link left in the list
            // Manually clear list and set the size to 0
            if (size == 1)
            {
                List = null;
                Last = null;

                size = 0;

                return Link;
            }

            // Condition: This link is between the first and last items
            // Remove the link and re-link the previous and next links
            if (Link.Previous != null && Link.Next != null)
            {
                Link.Previous.Next = Link.Next;
                Link.Next.Previous = Link.Previous;
            }
            // Condition: This link is the first or last item
            // Work accordingly
            else
            {
                // Condition: This link is the first of the list
                // Remove reference to the next link, and set it as the first link
                if (Link.Previous == null)
                {
                    Link.Next.Previous = null;

                    List = Link.Next;
                }
                // Condition: This link is the last of the list
                // Remove reference to the previous link, and set it as the last link
                else if (Link.Next == null)
                {
                    Link.Previous.Next = null;
                    
                    Last = Link.Previous;
                }
            }

            // Clear the linkage
            Link.Next = null;
            Link.Previous = null;

            // This link is no longer owned by this list
            Link.ParentList = null;

            // Decrease the list size
            size--;

            return Link;
        }

        /// <summary>
        /// Removes the given GDEntity from the list
        /// </summary>
        /// <param name="Entity">The GDEntity to remove</param>
        public GDDisplayLinkedListLink Remove(GDEntity Entity)
        {
            return Remove(Entity.Link);
        }

        /// <summary>
        /// Clears this list of all items binded
        /// </summary>
        public void Clear()
        {
            if(size == 0)
                return;

            if(size == 1)
            {
                List.ParentList = null;

                List = null;
                Last = null;

                size = 0;

                return;
            }

            for (GDDisplayLinkedListLink link = List; link != null; link = link.Next)
            {
                if (link.Previous != null)
                {
                    link.Previous.Next = null;
                    link.Previous = null;
                }

                link.ParentList = null;
            }

            List = null;
            Last = null;

            size = 0;
        }

        /// <summary>
        /// Returns the index at which the given GDEntity resides
        /// </summary>
        /// <param name="Entity">The GDEntity to search for</param>
        /// <returns>At which the given entity resides</returns>
        public int IndexOf(GDEntity Entity)
        {
            return (List == null ? -1 : List.IndexOf(Entity, 0));
        }

        /// <summary>
        /// Returns the index at which the given GDDisplayLinkedListLink resides
        /// </summary>
        /// <param name="Link">The GDDisplayListLink to search for</param>
        /// <returns>At which the given entity resides</returns>
        public int IndexOf(GDDisplayLinkedListLink Link)
        {
            return List.IndexOf(Link, 0);
        }

        /// <summary>
        /// Gets the link at the given index
        /// </summary>
        /// <param name="index">The index to gather the node</param>
        /// <returns>The node</returns>
        public GDDisplayLinkedListLink GetLinkAt(int index)
        {
            return List.GetNextLink(index);
        }
        
        /// <summary>
        /// Returns true if the Link is present on the current list, false otherwise
        /// </summary>
        /// <param name="Link">The Link to search in the list</param>
        /// <returns>True if the Link is present, false otherwise</returns>
        public bool Contains(GDDisplayLinkedListLink Link)
        {
            return IndexOf(Link) > -1;
        }

        /// <summary>
        /// Copies this list into the given array
        /// </summary>
        /// <param name="Links">The array to copy the elements to</param>
        public void CopyTo(GDDisplayLinkedListLink[] Links)
        {
            if (List == null)
                return;

            GDDisplayLinkedListLink current = List;

            int i = 0;

            while (current != null)
            {
                Links[i] = current;

                current = current.Next;
                i++;
            }
        }

        /// <summary>
        /// Copies this list into the given array
        /// </summary>
        /// <param name="Links">The array to copy the elements to</param>
        public void CopyTo(GDEntity[] Links)
        {
            if (List == null)
                return;

            GDDisplayLinkedListLink current = List;

            int i = 0;

            while (current != null)
            {
                Links[i] = current.Entity;

                current = current.Next;
                i++;
            }
        }

        /// <summary>
        /// Gets the enumerator class
        /// </summary>
        /// <returns>The enumerator</returns>
        public IEnumerator GetEnumerator()
        {
            return new GDDisplayLinkedListEnum(List, true);
        }

        /// <summary>
        /// Gets a Link on the given index
        /// </summary>
        /// <param name="index">The Link index to search for</param>
        /// <returns>The Link, or null if out of the bounds</returns>
        public GDDisplayLinkedListLink this[int index]
        {
            get { return (List == null ? null : List.GetNextLink(index)); }
        }

        /// <summary>
        /// Gets a Link on the given index
        /// </summary>
        /// <param name="index">The entity to use as search index</param>
        /// <returns>The Link, or null if out of the bounds</returns>
        public GDDisplayLinkedListLink this[GDEntity index]
        {
            get { return (List == null ? null : List.GetNextLink(index)); }
        }

        /// <summary>
        /// Gets the number of Links in this list
        /// </summary>
        public int Count
        {
            get { return size; }
        }
    }

    /// <summary>
    /// Represents a node of the GDDisplayLinkedList class
    /// </summary>
    public class GDDisplayLinkedListLink
    {
        /// <summary>
        /// Entity that this Link represents
        /// </summary>
        public GDEntity Entity;

        /// <summary>
        /// The parent list this Link belongs to
        /// </summary>
        public GDDisplayLinkedList ParentList;

        /// <summary>
        /// Gets the next Link of the list
        /// </summary>
        public GDDisplayLinkedListLink Next;

        /// <summary>
        /// Gets the previous Link of the list
        /// </summary>
        public GDDisplayLinkedListLink Previous;

        /// <summary>
        /// Creates a new instance of the GDDisplayLinkedListLink class
        /// </summary>
        /// <param name="entity">An GDEntity that will be associated to this Link</param>
        public GDDisplayLinkedListLink(GDEntity entity)
        {
            Entity = entity;
        }

        /// <summary>
        /// Gets the link at index 'index' on the list
        /// </summary>
        /// <param name="index">The index to search for</param>
        /// <returns>The Link, or null if out of bouds</returns>
        public GDDisplayLinkedListLink GetNextLink(int index)
        {
            if (index == 0)
                return this;

            GDDisplayLinkedListLink link = this;

            while (link != null && index >= 0)
            {
                if (index == 0)
                {
                    return link;
                }

                link = link.Next;

                index--;
            }

            return null;
            //throw new IndexOutOfRangeException("Child index is outside of bounds");
        }

        /// <summary>
        /// Gets the Link with the given entity binded
        /// </summary>
        /// <param name="entity">The entity to search for binding</param>
        /// <returns>The Link, or null if not found</returns>
        public GDDisplayLinkedListLink GetNextLink(GDEntity entity)
        {
            if (entity == Entity)
                return this;

            GDDisplayLinkedListLink link = this;

            while (link != null)
            {
                if (entity == link.Entity)
                {
                    return link;
                }

                link = link.Next;
            }

            return null;
            throw new IndexOutOfRangeException("Child index is outside of bounds");
        }

        /// <summary>
        /// Gets the Link in this list
        /// </summary>
        /// <param name="Link">The Link to search for</param>
        /// <returns>The Link, or null if not found</returns>
        public GDDisplayLinkedListLink GetNextLink(GDDisplayLinkedListLink Link)
        {
            if (this == Link)
                return this;

            GDDisplayLinkedListLink link = this;

            while (link != null)
            {
                if (Link == link)
                {
                    return link;
                }

                link = link.Next;
            }

            return null;
            throw new IndexOutOfRangeException("Child index is outside of bounds");
        }

        /// <summary>
        /// Gets the index of the given Link into the List
        /// </summary>
        /// <param name="Link">The Link to search for</param>
        /// <param name="currentIndex">Current index, used to return the index</param>
        /// <returns>The index of the Link in the list</returns>
        public int IndexOf(GDDisplayLinkedListLink Link, int currentIndex)
        {
            if (this == Link)
            {
                return currentIndex;
            }
            else if (Next != null)
            {
                if (Next == Link) // Let's skip a function call right here...
                    return currentIndex + 1;
                else
                    return Next.IndexOf(Link, currentIndex + 1);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets the index of the given Link into the List
        /// </summary>
        /// <param name="Entity">The entity to search for binding</param>
        /// <param name="currentIndex">Current index, used to return the index</param>
        /// <returns>The index of the Link in the list</returns>
        public int IndexOf(GDEntity Entity, int currentIndex)
        {
            if (this.Entity == Entity)
                return currentIndex;
            else if (Next != null)
            {
                if (Next.Entity == Entity) // Let's skip a function call right here...
                    return currentIndex + 1;
                else
                    return Next.IndexOf(Entity, currentIndex + 1);
            }
            else
            {
                return -1;
            }
        }
    }

    /// <summary>
    /// Enumerator used to traverse a GDDisplayLinkedList class
    /// </summary>
    public class GDDisplayLinkedListEnum : IEnumerator
    {
        GDDisplayLinkedListLink current;

        /// <summary>
        /// Whether to return an entity when traversing the list
        /// </summary>
        public bool getEntity;

        /// <summary>
        /// The current object captured on this enumerator
        /// </summary>
        public object Current
        {
            get
            {
                if (!getEntity)
                    return current;
                else
                    return current.Entity;
            }
        }

        /// <summary>
        /// Intializes a new instance of the GDDisplayLinkedListEnum class
        /// </summary>
        /// <param name="FirstLink">The first link to start traversing from</param>
        /// <param name="getEntities">Whether to return entities instad of the linked GDDisplayLinkedListLink</param>
        public GDDisplayLinkedListEnum(GDDisplayLinkedListLink FirstLink, bool getEntities)
        {
            current = FirstLink;

            getEntity = getEntities;
        }

        /// <summary>
        /// Moves this enumerator to the next link
        /// </summary>
        /// <returns>Whether the move procedure was succesful</returns>
        public bool MoveNext()
        {
            if (current != null && count == 0)
            {
                count++;
                return true;
            }

            if (current == null || current.Next == null || (getEntity && current.Entity == null))
                return false;

            current = current.Next;

            return true;
        }

        /// <summary>
        /// Resets this enumerator
        /// </summary>
        public void Reset()
        {
            if (current != null)
                while (current.Previous != null)
                {
                    current = current.Previous;
                }
        }

        int count = 0;
    }
}