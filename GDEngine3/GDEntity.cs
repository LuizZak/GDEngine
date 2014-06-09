using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

using GDEngine3.Display;
using GDEngine3.Screen;
using GDEngine3.Utils;

using GDEngine3.Collision;

namespace GDEngine3
{
    /// <summary>
    /// Represents an entity. All visual objects
    /// are derived from this class
    /// </summary>
    public class GDEntity
    {
        //////////// Functions:

        /// <summary>
        /// Initializes a new instance of the GDEntity class
        /// </summary>
        public GDEntity()
        {
            X = Y = 0;
            OriginalWidth = OriginalHeight = Width = Height = 0;
            ScaleX = ScaleY = 1;

            Init();
        }

        /// <summary>
        /// Initializes a new instance of the GDEntity class
        /// </summary>
        /// <param name="name">A name for this entity</param>
        public GDEntity(String name) : this()
        {
            this.name = name;
        }

        /// <summary>
        /// Initializes a new instance of the GDEntity class
        /// </summary>
        /// <param name="x">The X position of the entity on screen</param>
        /// <param name="y">The Y position of the entity on screen</param>
        public GDEntity(float x, float y)
        {
            X = x;
            Y = y;
            OriginalWidth = OriginalHeight = Width = Height = 0;
            ScaleX = ScaleY = 1;

            Init();
        }

        /// <summary>
        /// Initializes a new instance of the GDEntity class
        /// </summary>
        /// <param name="x">The X position of the entity on screen</param>
        /// <param name="y">The Y position of the entity on screen</param>
        /// <param name="originalHeight">The starting Width size</param>
        /// <param name="originalWidth">The starting Height size</param>
        public GDEntity(float x, float y, float originalWidth, float originalHeight)
        {
            X = x;
            Y = y;
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;

            Width = Height = 0;
            ScaleX = ScaleY = 1;

            Init();
        }

        /// <summary>
        /// Initializes a new instance of the GDEntity class
        /// </summary>
        /// <param name="x">The X position of the entity on screen</param>
        /// <param name="y">The Y position of the entity on screen</param>
        /// <param name="originalHeight">The starting Width size</param>
        /// <param name="originalWidth">The starting Height size</param>
        /// <param name="engine">The engine reference</param>
        public GDEntity(float x, float y, float originalWidth, float originalHeight, GDMainEngine engine)
        {
            X = x;
            Y = y;
            
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;

            Width = Height = 0;
            ScaleX = ScaleY = 1;

            Engine = engine;

            Init();
        }

        // Private:

        /// <summary>
        /// Called once when the entity is created
        /// </summary>
        void Init()
        {
            Position = new Vector2(X, Y);
            Origin   = new Vector2(XOrigin, YOrigin);
            Scale    = new Vector2(ScaleX, ScaleY);
            Rotation = 0;

            Tint = new Color(255, 255, 255, 255);
            Alpha = 1;

            Points = new Vector2[4];
            LocalPoints = new Vector2[4];

            Children = new GDDisplayLinkedList();
            Link = new GDDisplayLinkedListLink(this);

            Dirty = true;

            Unscaled = true;

            HasCollisionShape = false;
        }

        /// <summary>
        /// Called when the entity is being destroyed
        /// </summary>
        void End()
        {
            while (Children.Count > 0)
            {
                GDEntity entity = Children[0].Entity;
                entity.End();
                removeChild(entity);
                entity.Link.Entity = null;
                entity.Link = null;
            }

            Children.Clear();
        }

        // Public:

        /// <summary>
        /// Adds an entity to this entity's children list
        /// </summary>
        /// <param name="newChild">The new children to add</param>
        /// <param name="index">The index at which to place this entity. Leave -1 to push to the top</param>
        /// <returns>The entity created</returns>
        public virtual GDEntity addChild(GDEntity newChild, int index = -1)
        {
            // Cannot add an entity into itself!
            if (newChild == this)
                throw new Exception("Cannot add an entity into itself");

            if (newChild == null)
                throw new ArgumentNullException("newChild");

            // If the child is child from this entity, push it into the bottom of the children list (top of the display list)
            if (newChild.Parent == this)
            {
                removeChild(newChild);
                addChild(newChild, -1);

                return newChild;
            }
            
            // Entity already has a parent!
            if (newChild.Parent != null)
                throw new Exception("Entity is already a child of another entity");

            if (getParent(this, newChild))
                throw new Exception("Cannot add an entity as a child of its own children (or child of the children's children, etc)");

            // Add the entity to the children's list
            Children.Add(newChild, index);

            // Forces the new child to apply the transformation matrix so it gets valid absolute transformations right away
            newChild.ApplyMatrixRecursive();

            // Call the newChild's addedToEntity function
            newChild.addedToEntity(this);

            // Assign the newChild's ChildIndex of the
            newChild.ChildIndex = NumChild;

            // Increase the number of children
            NumChild++;

            // Dirty the area
            RecurseParentDirty();
            newChild.RecurseDirty();

            return newChild;
        }

        /// <summary>
        /// Removes an entity from this entity's children list
        /// </summary>
        /// <param name="child">The child entity to remove</param>
        /// <param name="markForRemove">Whether to mark for removal. Default is false</param>
        public virtual void removeChild(GDEntity child, bool markForRemove = false)
        {
            // Not child of this entity!
            if (child.Parent != this)
                throw new Exception("This entity's not the given child's parent. Function must be called from child's parent");

            if (markForRemove)
            {
                Engine.Removed.Add(child);
            }
            else
            {
                Children.Remove(child);
                child.removedFromEntity(this);

                // Ajust the child's ChildIndex
                child.ChildIndex = -1;

                // Avoid re-sorting the entities if this entity is being disposed
                if (!Disposing)
                {
                    // Sort the children's ChildIndexes
                    sortChildren();
                }

                // Decrease the number of children
                NumChild--;
            }

            // Avoid recursive calls if this entity is being disposed
            if (!Disposing)
            {
                // Dirty the area
                SetDirty();
            }
        }

        /// <summary>
        /// Happens when this entity is added to another entity's display list
        /// </summary>
        /// <param name="parent">The new parent entity</param>
        public virtual void addedToEntity(GDEntity parent)
        {
            this.Parent = parent;

            if (this.Engine == null && parent.Engine != null)
            {
                this.Engine = parent.Engine;
                RecurseEngine(this.Engine);
            }

            if (this.Root == null && (parent.root != null || parent.isRoot))
            {
                this.Root = (parent.isRoot ? parent : parent.Root);
                RecurseRoot(this.Root);
            }
        }

        /// <summary>
        /// Happens when this entity is removed from another entity's display list
        /// </summary>
        /// <param name="parent">The old parent entity</param>
        public virtual void removedFromEntity(GDEntity parent)
        {
            this.Parent = null;

            RecurseEngine(null);
            RecurseRoot(null);
        }

        /// <summary>
        /// Counts and sorts the each child's ChildIndex property
        /// </summary>
        public virtual void sortChildren()
        {
            // Define a temp variable used when sorting the entities
            int ci = 0;

            // Loop through each child in the Children list
            foreach (GDEntity en in Children)
            {
                en.ChildIndex = ci++;
            }
        }

        /// <summary>
        /// Returns whether the given entity is parented in some degree from the given entity
        /// </summary>
        /// <param name="currentObject">A GDEntity to seek the parent of</param>
        /// <param name="parentToSeek">A GDEntity to test parenthood</param>
        /// <returns>Whether the given entity is parented in some degree from the given entity</returns>
        public bool getParent(GDEntity currentObject, GDEntity parentToSeek)
        {
            if (currentObject.Parent == parentToSeek)
                return true;

            if (currentObject.Parent == null)
                return false;
            else
                return getParent(currentObject.Parent, parentToSeek);
        }

        /// <summary>
        /// Updates the entity and all child entities recursively
        /// </summary>
        /// <param name="Time">The timespan data</param>
        public virtual void Update(GameTime Time)
        {
            // Update logic goes here
            if (Children.Count == 0)
                return;

            GDEntity[] entities = new GDEntity[NumChild];

            Children.CopyTo(entities);

            foreach (GDEntity child in entities)
            {
                if(child != null && child.Parent == this)
                    child.Update(Time);
            }
        }

        /// <summary>
        /// Redraws this entity
        /// </summary>
        /// <param name="g">The SpriteBath object that this entity will be draw on</param>
        /// <param name="redrawRect">The region of the engine that is being redrawn (unused)</param>
        /// /// <param name="offset">The offset coordinates passed by the User when redrawing</param>
        public virtual void Draw(SpriteBatch g, RectangleF redrawRect, Point offset)
        {
            // No need to draw if this instance has been freed
            if (Freed)
                return;

            // If the alpha is equals to 0, skip drawing because the entity will be invisible
            if (alpha == 0)
                return;

            // If this entity is dirty, refresh the area
            if (Dirty)
                RefreshArea();

            // Test whether the entity intersects with the redraw rectangle
            if (!Area.Intersects(redrawRect))
                return;

            // Set the clipping rectangle now
            Rectangle oldRect = GDStatics.Device.ScissorRectangle;
            Rectangle compostRect = new Rectangle(0, 0, engine.Width, engine.Height);

            if (MaskRectangle.Width != -1 && MaskRectangle.Height != -1)
            {
                compostRect = new Rectangle((int)(absolutePosition.X + MaskRectangle.X), (int)(absolutePosition.Y + MaskRectangle.Y), (int)(MaskRectangle.Width * absoluteScale.X), (int)(MaskRectangle.Height * absoluteScale.Y));
            }

            compostRect = Rectangle.Intersect(GDStatics.Device.ScissorRectangle, compostRect);

            bool resetState = false;
            if (GDStatics.Device.ScissorRectangle != compostRect)
            {
                resetState = true;
                GDStatics.Device.ScissorRectangle = compostRect;

                g.End();
                g.Begin(engine.CurrentScreen.DesiredSortMode, engine.CurrentScreen.DesiredBlendState, engine.CurrentScreen.DesiredSamplerState, engine.CurrentScreen.DesiredDepthStencilState, engine.CurrentScreen.DesiredRasterizerState);
            }

            // Post draw it
            PostDraw(g, redrawRect, offset);

            if (resetState)
            {
                g.End();
                g.Begin(engine.CurrentScreen.DesiredSortMode, engine.CurrentScreen.DesiredBlendState, engine.CurrentScreen.DesiredSamplerState, engine.CurrentScreen.DesiredDepthStencilState, engine.CurrentScreen.DesiredRasterizerState);
            }

            // If there are no children to draw, skip this next part
            if (Children.Count == 0)
            {
                GDStatics.Device.ScissorRectangle = oldRect;
                return;
            }

            // Copy all entities to a temporary array
            GDEntity[] entities = new GDEntity[NumChild];
            Children.CopyTo(entities);

            // Iterate the children list
            foreach (GDEntity child in entities)
            {
                // Render child entity if it is visible
                if (child.visible)
                    child.Draw(g, redrawRect, offset);
            }

            GDStatics.Device.ScissorRectangle = oldRect;
        }

        /// <summary>
        /// Post draws this entity
        /// </summary>
        /// <param name="g">The SpriteBath object that this entity will be draw on</param>
        /// <param name="redrawRect">The region of the engine that is being redrawn (unused)</param>
        /// <param name="offset">The offset coordinates passed by the User when redrawing</param>
        public virtual void PostDraw(SpriteBatch g, RectangleF redrawRect, Point offset) { }

        /// <summary>
        /// Seaches the display tree recursively for an entity with the given name.
        /// </summary>
        /// <param name="name">The name of the entity to search for</param>
        /// <param name="includeSelf">Whether to include this entity on the search space</param>
        /// <returns>The first entity with the name matching the name parameter. Returns null if no entities were found</returns>
        public virtual GDEntity EntityNamed(String name, bool includeSelf = false)
        {
            if (includeSelf && this.name == name)
            {
                return this;
            }

            foreach (GDEntity entity in Children)
            {
                return EntityNamed(name, true);
            }

            return null;
        }

        /// <summary>
        /// Returns a RectangleF that can be used as a collision box for this entity.
        /// If there's no Collision rectangle, the Area is returned instead.
        /// </summary>
        /// <returns>A valid RectangleF box that can be used as a collision box</returns>
        public virtual RectangleF GetValidCollisionBox()
        {
            return HasCollisionShape ? CollisionRecMod : Area;
        }

        /// <summary>
        /// Gets the point on screen that is the equivalent of the given point on this entity
        /// </summary>
        /// <param name="tx">The X coordinate of the point</param>
        /// <param name="ty">The Y coordinate of the point</param>
        /// <returns>The point, transformed to local coordinates</returns>
        public virtual Vector2 ToLocalPoint(float tx, float ty)
        {
            return (Vector2.Transform(new Vector2(tx, ty) * absoluteScale, absoluteTransform) + absoluteOffset);
        }

        /// <summary>
        /// Get an entity that's under the given point
        /// </summary>
        /// <param name="x">The X screen coordinate to check for</param>
        /// <param name="y">The Y screen coordinate to check for</param>
        /// <returns>An entity that's undet the point, or null if none</returns>
        public virtual GDEntity GetAtPoint(float x, float y)
        {
            return GetAtPoint(x, y, 0xFFFF);
        }

        /// <summary>
        /// Get an entity that's under the given point, in local entity coordinates
        /// </summary>
        /// <param name="tx">The X screen coordinate to check for</param>
        /// <param name="ty">The Y screen coordinate to check for</param>
        /// <returns>An entity that's undet the point, or null if none</returns>
        public virtual GDEntity GetAtLocalPoint(float tx, float ty)
        {
            //Vector2 vec2 = Vector2.Transform(new Vector2(tx, ty), absoluteTransform);
            Vector2 vec2 = ToLocalPoint(tx, ty);

            return GetAtPoint(vec2.X, vec2.Y, 0xFFFF);
        }

        /// <summary>
        /// Get an entity that's under the given point, in local entity coordinates
        /// </summary>
        /// <param name="tx">The X screen coordinate to check for</param>
        /// <param name="ty">The Y screen coordinate to check for</param>
        /// <param name="bitmask">The bitmask used for collision filtering</param>
        /// <returns>An entity that's undet the point, or null if none</returns>
        public virtual GDEntity GetAtLocalPoint(float tx, float ty, int bitmask)
        {
            Vector2 vec2 = ToLocalPoint(tx, ty);

            return GetAtPoint(vec2.X, vec2.Y, bitmask);
        }

        /// <summary>
        /// Get an entity that's under the given point
        /// </summary>
        /// <param name="x">The X screen coordinate to check for</param>
        /// <param name="y">The Y screen coordinate to check for</param>
        /// <param name="bitmask">The bitmask used for collision filtering</param>
        /// <returns>An entity that's undet the point, or null if none</returns>
        public virtual GDEntity GetAtPoint(float x, float y, int bitmask)
        {
            // Early bitwise out
            if ((collisionBitmask & bitmask) == 0)
                return null;

            // If this entity's not visible, skip right away
            if (!visible)
                return null;

            // If the area doesn't contains the point, return null:
            if (!Area.Contains(x, y))
                return null;

            // Return entity and temp entity:
            GDEntity e = null;
            GDEntity r = null;

            // Check children entities for possible point contact:
            if (NumChild > 0)
            {
                GDDisplayLinkedListLink curLink = Children.Last;

                while (curLink != null)
                {
                    r = curLink.Entity.GetAtPoint(x, y, bitmask);

                    if (r != null && e != r)
                    {
                        e = r;
                        break;
                    }

                    curLink = curLink.Previous;
                }
            }

            // If no children inside that point, check self:
            if (e == null && !isRoot)
            {
                Vector2 endPt;
                endPt.X = GetValidCollisionBox().Right + 1;
                endPt.Y = y;

                // line we are testing against goes from pt -> endPt.
                bool inside = false;
                Vector2[] edges = (hasCollisionShape ? collisionPoints : Points);
                Vector2 edgeSt = edges[0];
                Vector2 edgeEnd;

                for (int i = 0; i < edges.Length; i++)
                {
                    // the current edge is defined as the line from edgeSt -> edgeEnd.
                    edgeEnd = edges[(i + 1) % edges.Length];

                    // perform check now...
                    if (((edgeSt.Y <= y) && (edgeEnd.Y > y)) || ((edgeSt.Y > y) && (edgeEnd.Y <= y)))
                    {
                        // this line crosses the test line at some point... does it do so within our test range?
                        float slope = (edgeEnd.X - edgeSt.X) / (edgeEnd.Y - edgeSt.Y);
                        float hitX = edgeSt.X + ((y - edgeSt.Y) * slope);

                        if ((hitX >= x) && (hitX <= endPt.X))
                            inside = !inside;
                    }
                    edgeSt = edgeEnd;
                }

                if (inside)
                    e = this;
            }

            // Return entity found:
            return e;
        }

        /// <summary>
        /// Returns whether this entity collides with the given GDEntity.
        /// This function uses the Area and the Oriented Area to check for hittest.
        /// This also checks for children entities.
        /// </summary>
        /// <param name="Entity">The entity to test collision for</param>
        /// <param name="bitmask">The bitmask used for collision sorting</param>
        /// <param name="self">Whether to check for this entity as well</param>
        /// <returns>Whether the two entities collide in any way</returns>
        public bool HitTest(GDEntity Entity, int bitmask = 0xFFFF, bool self = true)
        {
            GDEntity e1 = null;
            GDEntity e2 = null;

            return HitTest(Entity, ref e1, ref e2, self, bitmask, this);
        }

        /// <summary>
        /// Returns whether this entity collides with the given GDEntity, as well
        /// as the two entities (child of this, and the child of the given Entity) which are colliding
        /// This function uses the Area and the Oriented Area to check for hittest.
        /// This also checks for children entities.
        /// </summary>
        /// <param name="Entity">The entity to test collision for</param>
        /// <param name="Entity1">The first entity (either this or a child of this entity) that's colliding</param>
        /// <param name="Entity2">The second entity (either the given entity or one of its children) that's colliding</param>
        /// <param name="bitmask">The bitmask used for collision filtering</param>
        /// <param name="self">Whether to test this entity as well</param>
        /// <param name="topEntity">The top entity that started the chain</param>
        /// <returns>Whether the two entities collide in any way</returns>
        public bool HitTest(GDEntity Entity, ref GDEntity Entity1, ref GDEntity Entity2, bool self, int bitmask = 0xFFFF, GDEntity topEntity = null)
        {
            // Check if we're not testing for a self collision if not allowed
            if (Entity == topEntity && !self)
                return false;

            // Check if the bitmasks match
            if ((Entity.collisionBitmask & bitmask) == 0)
                return false;

            // If any of the objects is dirty, recurse them up:
            if (this.Dirty)
                RefreshArea();

            // Refresh the other entity's area as well:
            if (Entity.Dirty)
                Entity.RefreshArea();

            // Check for bounding box collision:
            if (this.Area.Intersects(Entity.Area))
            {
                // They are perhaps colliding:
                Vector2[] Points1 = (HasCollisionShape ? CollisionShape : Points);
                Vector2[] Points2 = (Entity.HasCollisionShape ? Entity.CollisionShape : Entity.Points);

                RectangleF b1 = GetValidCollisionBox();
                RectangleF b2 = Entity.GetValidCollisionBox();

                // If the recs intersect, check polygon-wise:
                if (!Entity.isRoot && b1.Intersects(b2))
                {
                    if (GDMath.PolygonCollision(Points1, Points2).Intersect)
                    {
                        // Set the entities:
                        Entity1 = this;
                        Entity2 = Entity;

                        return true;
                    }
                }

                // Test for children clips of the other entity:
                foreach (GDEntity child in Entity.Children)
                {
                    if (HitTest(child, ref Entity1, ref Entity2, self, bitmask, topEntity))
                        return true;
                }

                // Test this children's clips as well:
                foreach (GDEntity child in Children)
                {
                    if(child.HitTest(Entity, ref Entity1, ref Entity2, self, bitmask, topEntity))
                        return true;
                }
            }

            // If the areas don't intersect, means there's no way the two entities are colliding:
            return false;
        }

        /// <summary>
        /// Queries a rectangle on this entity, returning all the entities inside the rectangle area
        /// </summary>
        /// <param name="rec">The rectangle to query for</param>
        /// <param name="localCoords">
        /// Whether to use local or screen coordinates when querying for the rectangle. If set to 
        /// true, the position and size of the rectangle will be scaled to this entity's absolute
        /// transform, and that new rectangle will be used to query the child entities
        /// </param>
        /// <param name="firstMatch">Whether to quit on the first match found</param>
        /// <param name="bitmask">The bit mask used to filter objects when searching for the entities</param>
        /// <param name="list">The list to add the entities to. Leave empty to create and return a new list</param>
        /// <param name="iter">The interaction counter</param>
        /// <returns>A list of all the entities that match the coordinates of the rectangle, and match the bitmask</returns>
        public List<GDEntity> QueryRec(RectangleF rec, bool localCoords = true, bool firstMatch = false, int bitmask = 0xFFFF, List<GDEntity> list = null, int iter = 0)
        {
            if (list == null)
                list = new List<GDEntity>();

            if ((collisionBitmask & bitmask) == 0)
                return list;

            Vector2[] points;

            if (localCoords && iter == 0)
            {
                points = GDMath.GetRectangle(absolutePosition + rec.Position, rec.Size, absoluteOffset, absoluteScale, absoluteRotation * (float)(Math.PI / 180));
            }
            else
            {
                points = rec.GetPoints();
            }

            // Check for bounding box collision:
            if (this.GetArea().Intersects(rec))
            {
                // They are perhaps colliding:

                Vector2[] Points1 = (HasCollisionShape && CollisionShape != null ? CollisionShape : Points);
                RectangleF b1 = GetValidCollisionBox();

                // If the recs intersect, check polygon-wise:
                if (!isRoot && b1.Intersects(rec))
                {
                    if (GDMath.PolygonCollision(Points1, points).Intersect)
                    {
                        // Push this entity to the list
                        list.Add(this);
                    }
                }

                // Check child entities as well
                foreach (GDEntity child in Children)
                {
                    child.QueryRec(rec, localCoords, firstMatch, bitmask, list, iter);
                }
            }

            iter ++;

            return list;
        }

        /// <summary>
        /// Queries a segment on this entity, returning all the entities that cross the given segment
        /// </summary>
        /// <param name="segStart">The start of the line segment</param>
        /// <param name="segEnd">The end of the line segment</param>
        /// <param name="localCoords">
        /// Whether to use local or screen coordinates when querying for the segment. If set to 
        /// true, the segment vectors be scaled to this entity's absolute transform, and that new 
        /// segment will be used to query the child entities
        /// </param>
        /// <param name="firstMatch">Whether to quit on the first match found</param>
        /// <param name="bitmask">The bit mask used to filter objects when searching for the entities</param>
        /// <param name="list">The list to add the entities to. Leave empty to create and return a new list</param>
        /// <param name="segmentArea">The area of the segment currently being queried. You'll normally not need to provide no value other than null to this parameter</param>
        /// <returns>A list of all the entities that cross the line segment, and match the bitmask</returns>
        public List<GDEntity> QuerySegment(Vector2 segStart, Vector2 segEnd, bool localCoords = false, bool firstMatch = false, int bitmask = 0xFFFF, List<GDEntity> list = null, RectangleF? segmentArea = null)
        {
            if (list == null)
                list = new List<GDEntity>();

            if ((collisionBitmask & bitmask) == 0)
                return list;

            // Generate an AABB bounding box out of the points
            RectangleF area;

            if (segmentArea.HasValue)
            {
                area = segmentArea.Value;
            }
            else
            {
                area = GDMath.GetRectangleArea(segStart, segEnd);
            }

            // Check for bounding box collision:
            if (this.GetArea().Intersects(area))
            {
                // Get the collision shape points
                Vector2[] Points1 = (HasCollisionShape && CollisionShape != null ? CollisionShape : Points);
                RectangleF validArea = GetValidCollisionBox();

                // Check whether one of the segment's ends is inside the rectangle
                if ((validArea.Contains(segStart) && GDMath.PointInPolygon(segStart, Points1)) ||
                    (validArea.Contains(segEnd) && GDMath.PointInPolygon(segEnd, Points1)))
                {
                    list.Add(this);
                }
                // If the points are outside the polygon, check the segment against all the collision box sides
                else
                {
                    // Iterate through all the lines, checking one by one:
                    for (int i = 0; i < Points1.Length; i++)
                    {
                        Vector2 seg2Start, seg2End;

                        seg2Start = Points1[i];
                        seg2End = Points1[(i + 1) % Points1.Length];

                        if (GDMath.LinesIntersect(segStart, segEnd, seg2Start, seg2End))
                        {
                            list.Add(this);
                            break;
                        }
                    }
                }

                // Iterate through the children entities as well
                foreach (GDEntity child in Children)
                {
                    child.QuerySegment(segStart, segEnd, localCoords, firstMatch, bitmask, list, area);
                }
            }

            return list;
        }

        /// <summary>
        /// Queries an entity into this entity, returning all the entities that are colliding with this entity or its children
        /// </summary>
        /// <param name="query">The entity to use in the query</param>
        /// <param name="firstMatch">Whether to quit on the first match found</param>
        /// <param name="bitmask">The bit mask used to filter objects when searching for the entities</param>
        /// <param name="list">The list to add the entities to. Leave empty to create and return a new list</param>
        /// <param name="self">Whether to test this entity as well</param>
        /// <param name="topEntity">An GDEntity used to stop searching when self searching is not enabled</param>
        /// <returns>A list of all the entities that collide with the given entity, and match the bitmask</returns>
        public List<GDEntity> QueryEntity(GDEntity query, bool firstMatch = false, int bitmask = 0xFFFF, List<GDEntity> list = null, bool self = false, GDEntity topEntity = null)
        {
            if (list == null)
                list = new List<GDEntity>();

            // Check for redundancy
            if (list.Contains(this))
                return list;

            if (topEntity == null)
                topEntity = query;

            // Check if we're not testing for a self collision if not allowed
            if (this == topEntity && !self)
                return list;

            // Check if the bitmasks match
            if ((this.collisionBitmask & bitmask) == 0)
                return list;

            // Check for bounding box collision:
            if (this.GetArea().Intersects(query.GetArea()))
            {
                // They are perhaps colliding:
                Vector2[] Points1 = (HasCollisionShape && CollisionShape != null ? CollisionShape : Points);
                Vector2[] Points2 = (query.HasCollisionShape && query.CollisionShape != null ? query.CollisionShape : query.Points);

                RectangleF b1 = GetValidCollisionBox();
                RectangleF b2 = query.GetValidCollisionBox();

                // If the recs intersect, check polygon-wise:
                if (!this.isRoot && b1.Intersects(b2))
                {
                    if (GDMath.PolygonCollision(Points1, Points2).Intersect)
                    {
                        list.Add(this);
                        return list;
                    }
                }

                // Test this children's clips as well:
                foreach (GDEntity child in Children)
                {
                    child.QueryEntity(query, firstMatch, bitmask, list, self, topEntity);
                }

                // Test for children clips of the other entity:
                foreach (GDEntity child in query.Children)
                {
                    if (child.collisionBitmask != 0)
                        this.QueryEntity(child, firstMatch, bitmask, list, self, topEntity);
                }
            }

            // If the areas don't intersect, means there's no way the two entities are colliding:
            return list;
        }

        /// <summary>
        /// Check whether this entity will collide with the given entity if it's placed at the given location
        /// </summary>
        /// <param name="Entity">The entity to test collision of</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="bitmask">The bitmask used for collision filtering</param>
        /// <param name="self">Whether to test this entity as well</param>
        /// <param name="topEntity">The top entity that started the chain</param>
        /// <returns>Whether the two entities collide in any way</returns>
        public virtual bool WillCollide(GDEntity Entity, float x, float y, bool self, int bitmask = 0xFFFF, GDEntity topEntity = null)
        {
            float tx = X;
            float ty = Y;

            X = x;
            Y = y;

            SetDirty();
            Root.ApplyMatrixRecursive();
            Root.RefreshArea();

            bool coll = HitTest(Entity, bitmask, self);

            X = tx;
            Y = ty;

            SetDirty();
            Root.ApplyMatrixRecursive();
            Root.RefreshArea();

            return coll;
        }

        /// <summary>
        /// Casts a ray at this GDEntity and recursively tests all entities for a hit. The returning RaycastResult will contain
        /// information about whether or not the ray hit something, the position where it hit something, and what it hit
        /// </summary>
        /// <param name="startPoint">The starting position of the ray</param>
        /// <param name="endPoint">The ending position of the ray</param>
        /// <param name="bitmask">A bitmask used to filter collisions</param>
        /// <param name="rayArea">The area of the ray currently being casted. You'll normally not need to provide no value other than null to this parameter</param>
        /// <returns>A RaycastResult compounding the result of the raycast operation</returns>
        public RaycastResult Raycast(Vector2 startPoint, Vector2 endPoint, int bitmask = 0xFFFF, RectangleF? rayArea = null)
        {
            if ((collisionBitmask & bitmask) == 0)
                return new RaycastResult() { Entity = null, Hit = false, Position = endPoint, Ratio = 0 };

            // Generate an AABB bounding box out of the points
            RectangleF area;

            if (rayArea.HasValue)
            {
                area = rayArea.Value;
            }
            else
            {
                area = GDMath.GetRectangleArea(startPoint, endPoint);
            }

            RaycastResult result = new RaycastResult();
            result.Hit = false;
            result.Position = endPoint;

            // Check for bounding box collision:
            if (this.GetArea().Intersects(area))
            {
                // Get the collision shape points
                Vector2[] Points1 = (HasCollisionShape && CollisionShape != null ? CollisionShape : Points);

                if(area.Intersects(GetValidCollisionBox()))
                {
                    // Iterate through all the lines, checking one by one:
                    for (int i = 0; i < Points1.Length; i++)
                    {
                        Vector2 seg2Start, seg2End;

                        seg2Start = Points1[i];
                        seg2End = Points1[(i + 1) % Points1.Length];

                        GDMath.LineCollisionResult lineResult = GDMath.LinesIntersection(startPoint, endPoint, seg2Start, seg2End);

                        if (lineResult.Intersect)
                        {
                            endPoint = lineResult.IntersectionPoint;

                            result.Entity = this;
                            result.Hit = true;
                            result.Position = endPoint;
                            result.Ratio = lineResult.Line1Ratio;

                            area = GDMath.GetRectangleArea(startPoint, endPoint);
                        }
                    }
                }

                // Iterate through the children entities as well
                foreach (GDEntity child in Children)
                {
                    RaycastResult childResult = child.Raycast(startPoint, endPoint, bitmask, area);

                    if (childResult.Hit)
                    {
                        result = childResult;
                        endPoint = result.Position;

                        area = GDMath.GetRectangleArea(startPoint, endPoint);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Clear this and every children object to allow garbage collecting to work
        /// </summary>
        public virtual void Clear()
        {
            Disposing = true;

            while (Children.Count > 1)
            {
                if (Children.List.Entity.Parent == this)
                {
                    Children.List.Entity.Clear();

                    removeChild(Children.List.Entity);
                }
            }
            
            Children.Clear();
        }

        /// <summary>
        /// Frees this entity and all child entities from memory
        /// </summary>
        /// <param name="ForceFree">Whether to force the free of all resources (Strips and Images included)</param>
        public virtual void Free(bool ForceFree)
        {
            Disposing = true;
            Freed = true;

            foreach(GDEntity e in Children)
            {
                e.Free(ForceFree);
            }

            End();
        }

        /// <summary>
        /// Changes the origin to the given 
        /// </summary>
        /// <param name="x">The X coordinate of the new origin</param>
        /// <param name="y">The Y coordinate of the new origin</param>
        public void SetOrigin(float x, float y)
        {
            XOrigin = x;
            YOrigin = y;

            Origin = new Vector2(x, y);

            AfterMove();
        }

        /// <summary>
        /// Changes the origin to the given 
        /// </summary>
        /// <param name="vec">The vector that contains the coordinates of the new origin</param>
        public void SetOrigin(Vector2 vec)
        {
            XOrigin = vec.X;
            YOrigin = vec.Y;

            Origin = vec;

            AfterMove();
        }
        
        // Privates:

        /// <summary>
        /// This function is called every time the entity is moved, or the origin is changed
        /// </summary>
        protected virtual void AfterMove()
        {
            Position.X = X;
            Position.Y = Y;

            Origin.X = XOrigin;
            Origin.Y = YOrigin;

            SetDirty();
        }

        /// <summary>
        /// This function is called every time the entity is scaled, or the size is changed
        /// </summary>
        protected virtual void AfterScale()
        {
            Unscaled = (ScaleX == 1 && ScaleY == 1);

            Scale.X = ScaleX;
            Scale.Y = ScaleY;

            SetDirty();
        }

        /// <summary>
        /// This function is called every time the entity is rotated
        /// </summary>
        protected virtual void AfterRotate()
        {
            Radians = MathHelper.ToRadians(Rotation);

            SetDirty();
        }

        /// <summary>
        /// Recurses over the children list, updating the parent engine
        /// </summary>
        protected void RecurseEngine(GDMainEngine newEngine)
        {
            Engine = newEngine;

            if(newEngine != null)
                Screen = newEngine.CurrentScreen;
            else
                Screen = null;

            foreach (GDEntity child in Children)
            {
                child.RecurseEngine(newEngine);
            }
        }

        /// <summary>
        /// Recurses over the children list, updating the root entity
        /// </summary>
        protected void RecurseRoot(GDEntity newRoot)
        {
            Root = newRoot;

            foreach (GDEntity child in Children)
            {
                child.RecurseRoot(newRoot);
            }
        }

        /// <summary>
        /// Recurses over the children, updating the redrawing area
        /// </summary>
        public virtual void RefreshArea()
        {
            if (!Dirty)
                return;

            // Refresh the collision rectangle
            RefreshCollision();

            // Calculate the area using the overloaded CalculateArea() method
            CalculateArea();

            // Refresh the children entities as well
            foreach (GDEntity ent in Children)
            {
                ent.RefreshArea();

                if (ent.HasArea)
                {
                    // Transform the child's local area
                    GDMath.TransformPoints(ent.LocalPoints, LocalPoints, ent.position, new Vector2(), ent.scale, ent.rotation * (MathHelper.Pi / 180));

                    // Test whether to replace the current area
                    if (!HasArea)
                    {
                        Area = ent.Area;
                        HasArea = true;

                        // Add the local area
                        LocalArea = GDMath.GetRectangleArea(LocalPoints);
                    }
                    // Union the area otherwise
                    else
                    {
                        Area = RectangleF.Union(Area, ent.Area);

                        // Unite with the local area
                        LocalArea = RectangleF.Union(LocalArea, GDMath.GetRectangleArea(LocalPoints));
                    }
                }
            }

            // Set the local area size
            if (HasArea)
            {
                LocalPoints[0].X = LocalArea.X;
                LocalPoints[0].Y = LocalArea.Y;
                
                LocalPoints[1].X = LocalArea.X + LocalArea.Width;
                LocalPoints[1].Y = LocalArea.Y;

                LocalPoints[2].X = LocalArea.X + LocalArea.Width;
                LocalPoints[2].Y = LocalArea.Y + LocalArea.Height;
                
                LocalPoints[3].X = LocalArea.X;
                LocalPoints[3].Y = LocalArea.Y + LocalArea.Height;
            }

            Dirty = false;
        }

        /// <summary>
        /// Calculates the area of this Entity
        /// </summary>
        public virtual void CalculateArea()
        {
            // Recurse the refresh
            Area.Reset();
            LocalArea.Reset();
            HasArea = false;

            // Try using the collision shape if it's available
            if (HasCollisionShape)
            {
                Area = CollisionRecMod;
                LocalArea = CollisionRec;
                HasArea = true;
            }
        }

        /// <summary>
        /// Refreshes the collision shape
        /// </summary>
        protected virtual void RefreshCollision()
        {
            // Calculate the collision shape
            CollisionShape = GDMath.GetRectangle(absolutePosition + GDMath.Rotate(CollisionRec.Position * absoluteScale, absoluteRotation * (float)(Math.PI / 180)), CollisionRec.Size, absoluteOffset, absoluteScale, absoluteRotation * (float)(Math.PI / 180));

            // Load a RectangleF representing the collision boundaries
            RectangleF box = GDMath.GetBoundingBox(CollisionShape);

            // Resize the box
            box.Width -= box.X;
            box.Height -= box.Y;

            // Set the modulated collision rectangle
            CollisionRecMod = box;
        }

        /// <summary>
        /// Adds the area of a child into this entity's own area
        /// </summary>
        /// <param name="rectangle">The area to add</param>
        private void AreaChild(RectangleF rectangle)
        {
            if (Area.IsEmpty)
                Area = rectangle;
            else
                Area = RectangleF.Union(Area, rectangle);
        }

        /// <summary>
        /// Reset this Entity's Area rectangle
        /// </summary>
        public void AreaReset()
        {
            HasArea = false;
            Area.Reset();
        }

        /// <summary>
        /// Returns the Area rectangle, which is the combined area of this entity, and all children entity areas
        /// </summary>
        /// <param name="refreshIfDirty">Whether to force a refresh operation when fetching the area if the entity is marked dirty</param>
        /// <returns>The Area rectangle</returns>
        public RectangleF GetArea(bool refreshIfDirty = false)
        {
            // Refresh the area if the display is dirty
            if (refreshIfDirty && Dirty)
            {
                ApplyMatrixRecursive();
                RefreshArea();
            }

            return Area;
        }

        /// <summary>
        /// Returns the LocalArea rectangle, which is the combined local area of this entity, and all children entity local areas
        /// </summary>
        /// <param name="refreshIfDirty">Whether to force a refresh operation when fetching the area if the entity is marked dirty</param>
        /// <returns>The LocalArea rectangle</returns>
        public RectangleF GetLocalArea(bool refreshIfDirty = false)
        {
            // Refresh the area if the display is dirty
            if (refreshIfDirty && Dirty)
            {
                ApplyMatrixRecursive();
                RefreshArea();
            }

            return LocalArea;
        }

        /// <summary>
        /// Returns the points that represent the oriented Area rectangle
        /// </summary>
        /// <returns>The point set that represents oriented Area rectangle</returns>
        public Vector2[] GetPoints()
        {
            return Points;
        }

        /// <summary>
        /// Returns the local points that represent the oriented LocalArea rectangle
        /// </summary>
        /// <returns>The local point set that represents oriented LocalArea rectangle</returns>
        public Vector2[] GetLocalPoints()
        {
            return LocalPoints;
        }

        /// <summary>
        /// Returns the modified collision rectangle, that is equal to the absolute
        /// representation of this entitiy's collision rectangle onscreen
        /// </summary>
        /// <returns>The modified collision rectangle</returns>
        public RectangleF GetModifiedCollisionRec()
        {
            return CollisionRecMod;
        }

        /// <summary>
        /// Sets the entity chain as dirty, affecting children and parents recursively
        /// </summary>
        public void SetDirty()
        {
            RecurseParentDirty();
            RecurseDirty();
        }
        
        /// <summary>
        /// Returns where the area of this Entity is dirty, and needs recalculating
        /// </summary>
        /// <returns>Whether this Entity is dirty</returns>
        public bool IsDirty()
        {
            return Dirty;
        }

        /// <summary>
        /// Recursively set the area as dirty, setting the parents as dirty as well
        /// </summary>
        public void RecurseParentDirty()
        {
            if (Parent != null/* && !Parent.Dirty*/)
                Parent.RecurseParentDirty();
            
            Dirty = true;
        }

        /// <summary>
        /// Recursively set the area as dirty, setting the children as dirty as well
        /// </summary>
        public void RecurseDirty()
        {
            Dirty = true;

            foreach (GDEntity child in Children)
            {
                //if(!child.Dirty)
                    child.RecurseDirty();
            }
        }

        /// <summary>
        /// Reset the area recursively, resetting the parents' areas as well
        /// </summary>
        public void RecurseParentAreaReset()
        {
            if (Parent != null)
                Parent.RecurseParentAreaReset();
            else
                RefreshArea();
        }

        /// <summary>
        /// Reset the area recursively, resetting the children's areas as well
        /// </summary>
        public void RecursedAreaReset()
        {
            AreaReset();

            foreach (GDEntity Child in Children)
            {
                Child.RecursedAreaReset();
            }
        }

        /// <summary>
        /// Apply the transformation matrix recursively througout all the children entities
        /// </summary>
        public virtual void ApplyMatrixRecursive()
        {
            // Reset the absolute values
            absoluteScale = Scale;
            relativeScale = Vector2.One;
            absolutePosition = Vector2.Zero;
            absoluteOffset = Origin;
            absoluteRotation = rotation;
            absoluteTint = Tint;
            absoluteAlpha = Alpha;

            absoluteTransform = Matrix.Identity;

            Vector2 newPos = Position;

            // Apply parent transformations
            if (Parent != null)
            {
                absoluteScale = Parent.absoluteScale * absoluteScale;
                relativeScale *= Parent.absoluteScale;

                absoluteRotation += Parent.absoluteRotation;
                absolutePosition += Parent.absolutePosition;
                newPos -= Parent.absoluteOffset;

                // Tint:
                absoluteTint.R = (byte)(absoluteTint.ToVector4().X * Parent.absoluteTint.ToVector4().X * 255);
                absoluteTint.G = (byte)(absoluteTint.ToVector4().Y * Parent.absoluteTint.ToVector4().Y * 255);
                absoluteTint.B = (byte)(absoluteTint.ToVector4().Z * Parent.absoluteTint.ToVector4().Z * 255);
                absoluteTint.A = (byte)(absoluteTint.ToVector4().W * Parent.absoluteTint.ToVector4().W * 255);

                absoluteAlpha *= Parent.Alpha;

                absolutePosition += GDMath.Rotate((Position - Parent.absoluteOffset) * relativeScale, Parent.absoluteRotation * (float)(Math.PI / 180));
            }
            else
            {
                absolutePosition += Position;
            }

            absoluteTransform *= Matrix.CreateRotationZ(MathHelper.ToRadians(Rotation));
            absoluteTransform *= Matrix.CreateTranslation(new Vector3(newPos * relativeScale, 0));

            absoluteTint.A = (byte)(absoluteTint.A * absoluteAlpha);

            // Apply transformation of the parent
            if (Parent != null)
            {
                absoluteTransform *= Parent.absoluteTransform;
            }

            // Apply the transformations to the children entities too
            foreach (GDEntity entity in Children)
            {
                entity.ApplyMatrixRecursive();
            }
        }

        // Private Members:
        /// <summary>
        /// This entity's X coordinate
        /// </summary>
        protected float X;
        /// <summary>
        /// This entity's Y coordinate
        /// </summary>
        protected float Y;
        /// <summary>
        /// This entity's X origin
        /// </summary>
        protected float XOrigin;
        /// <summary>
        /// This entity's Y origin
        /// </summary>
        protected float YOrigin;
        /// <summary>
        /// This entity's Width
        /// </summary>
        protected float Width;
        /// <summary>
        /// This entitiy's heiht
        /// </summary>
        protected float Height;
        /// <summary>
        /// This entity's X scale
        /// </summary>
        protected float ScaleX;
        /// <summary>
        /// This entity's Y scale
        /// </summary>
        protected float ScaleY;

        /// <summary>
        /// This entity's rotation
        /// </summary>
        protected float Rotation;

        /// <summary>
        /// This entity's tint used to tint this and any child entity
        /// </summary>
        protected Color Tint;

        /// <summary>
        /// This entity's alpha used to make this entity transparent
        /// </summary>
        protected float Alpha;

        /// <summary>
        /// This entity's original width before scaling
        /// </summary>
        protected float OriginalWidth;
        /// <summary>
        /// This entity's original height before scaling
        /// </summary>
        protected float OriginalHeight;

        /// <summary>
        /// Rectangle that will be used to mask this entity's draw call on screen
        /// </summary>
        protected Rectangle MaskRectangle = new Rectangle(0, 0, -1, -1);

        /// <summary>
        /// This entity's collision rectangle
        /// </summary>
        protected RectangleF CollisionRec;
        /// <summary>
        /// This entity's collision rectangle modified to account for transformations applied to this entity
        /// </summary>
        protected RectangleF CollisionRecMod;
        /// <summary>
        /// This entity's collision shape (vector of points representing the modified CollisionRec)
        /// </summary>
        protected Vector2[] CollisionShape;

        /// <summary>
        /// The collision method used to test this entity against other entities
        /// </summary>
        protected CollisionShape CollisionMethod;

        /// <summary>
        /// Whether this entity has a collision shape
        /// </summary>
        protected bool HasCollisionShape;


        /// <summary>
        /// The internal reference to the Area rectangle
        /// </summary>
        internal RectangleF Area;
        /// <summary>
        /// The internal reference to the points that compose the Area rectangle
        /// </summary>
        internal Vector2[] Points;

        /// <summary>
        /// The internal reference to the local area rectangle
        /// </summary>
        internal RectangleF LocalArea;
        /// <summary>
        /// The internal reference to the points that compose the LocalArea rectangle
        /// </summary>
        internal Vector2[] LocalPoints;

        /// <summary>
        /// Whether this entity has an area that can be added to the parent entity
        /// </summary>
        internal bool HasArea = false;

        /// <summary>
        /// Whether this entity is being disposed
        /// </summary>
        protected bool Disposing = false;

        /// <summary>
        /// Whether this entity's area is dirty and needs recalculating
        /// </summary>
        protected bool Dirty;

        /// <summary>
        /// Whether this entity is unscaled
        /// </summary>
        protected bool Unscaled;

        /// <summary>
        /// The number of child entities on this entity
        /// </summary>
        protected int NumChild;

        /// <summary>
        /// The index of this entity on the parent entity (-1 if no parent entity)
        /// </summary>
        protected int ChildIndex;

        /// <summary>
        /// A static copy to a SpriteBatch object (no use right now)
        /// </summary>
        protected SpriteBatch Graphics;

        // VECTORS:

        /// <summary>
        /// This entity's rotation, in radians
        /// </summary>
        public float Radians;

        /// <summary>
        /// Position vector
        /// </summary>
        protected Vector2 Position;

        /// <summary>
        /// Origin vector
        /// </summary>
        protected Vector2 Origin;

        /// <summary>
        /// Scale vector
        /// </summary>
        protected Vector2 Scale;

        /// <summary>
        /// Engine reference.
        /// </summary>
        internal GDMainEngine Engine;

        /// <summary>
        /// Reference to the screen this entity is currently on
        /// </summary>
        internal GDScreen Screen;

        /// <summary>
        /// This entitiy's parent
        /// </summary>
        protected GDEntity Parent;

        /// <summary>
        /// This entitiy's root
        /// </summary>
        protected GDEntity Root;

        // Public Members:

        /// <summary>
        /// The absolute transformation matrix for this entity
        /// </summary>
        public Matrix absoluteTransform = Matrix.Identity;
        /// <summary>
        /// This entity's absolute position on the screen
        /// </summary>
        public Vector2 absolutePosition = Vector2.One;
        /// <summary>
        /// This entity's absolute offset on the screen
        /// </summary>
        public Vector2 absoluteOffset = Vector2.One;
        /// <summary>
        /// This entity's absolute scale on the screen
        /// </summary>
        public Vector2 absoluteScale = Vector2.One;
        /// <summary>
        /// This entity's relative scale to the parent entity
        /// </summary>
        public Vector2 relativeScale = Vector2.One;
        /// <summary>
        /// This entity's absolute tint
        /// </summary>
        public Color absoluteTint = Color.White;
        /// <summary>
        /// This entity's absolute alpha
        /// </summary>
        public float absoluteAlpha = 1;
        /// <summary>
        /// This entity's absolute rotation on the screen
        /// </summary>
        public float absoluteRotation = 0;

        /// <summary>
        /// Gets or sets the Vector that represents the position of this entity
        /// </summary>
        public Vector2 position
        {
            get { return Position; }
            set
            {
                if(!Position.Equals(value))
                {
                    Position = value;
                    X = Position.X;
                    Y = Position.Y;
                    AfterMove();
                }
            }
        }

        /// <summary>
        /// Gets or sets the X position of this entity
        /// </summary>
        public float x
        {
            get { return X; }
            set { X = value; AfterMove();}
        }

        /// <summary>
        /// Gets or sets the Y position of this entity
        /// </summary>
        public float y
        {
            get { return Y; }
            set { Y = value; AfterMove(); }
        }

        /// <summary>
        /// Gets or sets the Vector representing the origin of this object
        /// </summary>
        public Vector2 origin
        {
            get { return Origin; }
            set
            {
                if (Origin != value)
                {
                    XOrigin = value.X;
                    YOrigin = value.Y;
                    AfterMove();
                }
            }
        }

        /// <summary>
        /// Gets or sets the X-origin
        /// </summary>
        public float xOrigin
        {
            get { return XOrigin; }
            set { if (XOrigin != value) { XOrigin = value; AfterMove(); } }
        }

        /// <summary>
        /// Gets or sets the Y-origin
        /// </summary>
        public float yOrigin
        {
            get { return YOrigin; }
            set { if (YOrigin != value) { YOrigin = value; AfterMove(); } }
        }


        /// <summary>
        /// Gets or sets the width size of this entity
        /// </summary>
        public float width
        {
            get { return Math.Abs(Width); }
            set { if (Width != value) { Width = value; ScaleX = (OriginalWidth > 0 ? Math.Abs(Width / OriginalWidth) : 1); AfterScale(); } }
        }

        /// <summary>
        /// Gets or sets the height size of this entity
        /// </summary>
        public float height
        {
            get { return Math.Abs(Height); }
            set { if (Height != value) { Height = value; ScaleY = (OriginalHeight > 0 ? Math.Abs(Height / OriginalHeight) : 1); AfterScale(); } }
        }

        /// <summary>
        /// Gets or sets the Vector that represents the size (width and height) of this entity
        /// </summary>
        public Vector2 size
        {
            get { return new Vector2(Width, Height); }
            set
            {
                if (Width != value.X || Height != value.Y)
                {
                    Width = value.X;
                    Height = value.Y;

                    ScaleX = Width / OriginalWidth;
                    ScaleY = Height / OriginalHeight;

                    AfterScale();
                }
            }
        }

        /// <summary>
        /// Gets the Vector that represents the original size (width and height) of this entity before absolute scaling
        /// </summary>
        public Vector2 originalSize
        {
            get { return new Vector2(OriginalWidth, OriginalHeight); }
        }

        /// <summary>
        /// Gets the starting width size of this entity
        /// </summary>
        public float originalWidth
        {
            get { return OriginalWidth; }
        }

        /// <summary>
        /// Gets the starting height size of this entity
        /// </summary>
        public float originalHeight
        {
            get { return OriginalHeight; }
        }

        /// <summary>
        /// Gets or sets the scale the Entity
        /// </summary>
        public Vector2 scale
        {
            get { return Scale; }

            set
            {
                if (Scale != value)
                {
                    Scale = value;

                    ScaleX = value.X;
                    ScaleY = value.Y;

                    Width = OriginalWidth * ScaleX;
                    Height = OriginalHeight * ScaleY;

                    AfterScale();
                }
            }
        }

        /// <summary>
        /// Gets or sets the X-scale of this entity
        /// </summary>
        public float scaleX
        {
            get { return ScaleX; }
            set { if (ScaleX != value) { ScaleX = value; Width = OriginalWidth * ScaleX; AfterScale(); } }
        }

        /// <summary>
        /// Gets or sets the Y-scale of this entity
        /// </summary>
        public float scaleY
        {
            get { return ScaleY; }
            set { if (ScaleY != value) { ScaleY = value; Height = OriginalHeight * ScaleY; AfterScale(); } }
        }

        /// <summary>
        /// Gets or sets the rotation of this entity
        /// </summary>
        public float rotation
        {
            get { return Rotation; }
            set { if (Rotation != value) { Rotation = value; AfterRotate(); } }
        }

        /// <summary>
        /// Gets or sets the mask rectangle that will be used to clip this entity on screen
        /// </summary>
        public Rectangle maskRectangle
        {
            get { return MaskRectangle; }
            set { MaskRectangle = value; }
        }

        /// <summary>
        /// The collision Bitmask, used when filtering queries and HitTests
        /// </summary>
        public int collisionBitmask = 0xFFFF;

        /// <summary>
        /// Gets or sets the collision mask rectangle used for custom collision areas
        /// </summary>
        public RectangleF collision
        {
            get { return CollisionRec; }
            set
            {
                if (CollisionRec != value)
                {
                    CollisionRec = value;
                    HasCollisionShape = (value != null);
                    HasArea = (value != null);
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Gets the total area of the collision points
        /// </summary>
        public RectangleF collisionAligned
        {
            get { return CollisionRecMod; }
        }

        /// <summary>
        /// Gets the projected collision points, transformed according to the inherited transformation
        /// </summary>
        public Vector2[] collisionPoints
        {
            get { return CollisionShape; }
        }

        /// <summary>
        /// Gets whether or not this entity has a collision shape
        /// </summary>
        public bool hasCollisionShape
        {
            get { return HasCollisionShape; }
        }

        /// <summary>
        /// Gets or sets the color tint of this entity
        /// </summary>
        public Color tint
        {
            get { return Tint; }
            set { Tint = value; Alpha = (Tint.A / 255.0f); }
        }

        /// <summary>
        /// The alpha color of this entity
        /// 1.0 is fully opaque, 0.0 is fully transparent
        /// </summary>
        public float alpha
        {
            get { return Alpha; }
            set { Alpha = MathHelper.Clamp(value, 0.0f, 1.0f); Tint.A = (byte)(255 * Alpha); }
        }

        /// <summary>
        /// Gets or sets whether the entity should be visible on screen
        /// </summary>
        public bool visible = true;

        /// <summary>
        /// Gets whether this entity is unscaled (scaleX and scaleY are 1)
        /// </summary>
        public bool uncaled
        {
            get { return Unscaled; }
        }

        /// <summary>
        /// Gets the number of children entities linked to this entity
        /// </summary>
        public int numChildren
        {
            get { return NumChild; }
        }

        /// <summary>
        /// Gets the index at which this entity sits in the parent entity's Children list. 
        /// It returns -1 if this entity has no parent
        /// </summary>
        public int childIndex
        {
            get { return ChildIndex; }
        }

        /// <summary>
        /// Gets the parent this entity is linked with
        /// </summary>
        public GDEntity parent
        {
            get { return Parent; }
        }

        /// <summary>
        /// Gets the root this entity is linked with
        /// </summary>
        public GDEntity root
        {
            get { return Root; }
        }

        /// <summary>
        /// Gets the graphical drawing layer of this entity
        /// </summary>
        public SpriteBatch graphics
        {
            get { return Graphics; }
        }

        /// <summary>
        /// Gets or sets this entity's name
        /// </summary>
        public string name;

        /// <summary>
        /// Gets the list of children entities
        /// </summary>
        public GDDisplayLinkedList Children;

        /// <summary>
        /// Link that represents this GDEntity
        /// </summary>
        public GDDisplayLinkedListLink Link;

        /// <summary>
        /// Gets a child entity based on an index
        /// </summary>
        /// <param name="index">The index between 0 and numChildren-1 of the child to get</param>
        /// <returns>A child entity at the given index</returns>
        public GDEntity this[int index]
        {
            get
            {
                if (index > Children.Count - 1 || index < 0)
                    throw new IndexOutOfRangeException("Index " + index + " is out of bounds of the children list");
                
                return Children[index].Entity;
            }
        }

        /// <summary>
        /// Gets a child entity based on a name
        /// </summary>
        /// <param name="name">The name of the entity to grab</param>
        /// <returns>A child entity witht he given name, or null if not found</returns>
        public GDEntity this[string name]
        {
            get
            {
                foreach (GDEntity child in Children)
                {
                    if (child.name == name)
                        return child;
                }

                return null;
            }
        }

        /// <summary>
        /// Whether this entity is the root of the drawing engine. Don't mess with this! It
        /// can screw with the drawing process
        /// </summary>
        public bool isRoot = false;

        /// <summary>
        /// Whether this entity was freed from memory.
        /// </summary>
        public bool Freed = false;

        /// <summary>
        /// The device owning this entity
        /// </summary>
        public GraphicsDevice Device;

        /// <summary>
        /// Gets the engine this entity is assigned to
        /// </summary>
        public GDMainEngine engine
        {
            get { return Engine; }
            set { Engine = value; RecurseEngine(value); }
        }

        /// <summary>
        /// Gets the screen this entity is currently on
        /// </summary>
        public GDScreen screen
        {
            get { return Screen; }
            set { Screen = value; }
        }

        /// <summary>
        /// Custom object that can be set by the user
        /// </summary>
        public Object UserData;
    }

    /// <summary>
    /// Data structure used to represent a result of a raycast on an entity
    /// </summary>
    public struct RaycastResult
    {
        /// <summary>
        /// The position the ray hit the entity
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// A float value between [0 - 1] that specifies the magnitude at which the ray ended after the cast
        /// </summary>
        public float Ratio;

        /// <summary>
        /// The closest GDEntity to the casting position the ray hit
        /// </summary>
        public GDEntity Entity;

        /// <summary>
        /// Whether the raycast hit something
        /// </summary>
        public bool Hit;
    }
}