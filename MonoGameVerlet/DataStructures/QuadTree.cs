using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoGameVerlet.DataStructures
{
    public class QuadTree
    {
        public QuadTree NorthWest;
        public QuadTree NorthEast;
        public QuadTree SouthWest;
        public QuadTree SouthEast;
        public Vector2? Position;
        public AxisAlignedBoundingBox Bounds;

        public QuadTree(AxisAlignedBoundingBox bounds)
        {
            this.Bounds = bounds;
        }

        /// <summary>
        /// Create the quads as four equal regions in reference to this quad.
        /// </summary>
        private void Subdivide()
        {
            NorthWest = new QuadTree(
                new AxisAlignedBoundingBox(new Vector2(Bounds.CenterPoint.X - 0.5f * Bounds.HalfWidth, Bounds.CenterPoint.Y - 0.5f * Bounds.HalfHeight),
                    Bounds.HalfWidth, Bounds.HalfHeight));

            NorthEast = new QuadTree(
                new AxisAlignedBoundingBox(new Vector2(Bounds.CenterPoint.X + 0.5f * Bounds.HalfWidth, Bounds.CenterPoint.Y - 0.5f * Bounds.HalfHeight),
                    Bounds.HalfWidth, Bounds.HalfHeight));

            SouthWest = new QuadTree(
                new AxisAlignedBoundingBox(new Vector2(Bounds.CenterPoint.X - 0.5f * Bounds.HalfWidth, Bounds.CenterPoint.Y + 0.5f * Bounds.HalfHeight),
                    Bounds.HalfWidth, Bounds.HalfHeight));

            SouthEast = new QuadTree(
                new AxisAlignedBoundingBox(new Vector2(Bounds.CenterPoint.X + 0.5f * Bounds.HalfWidth, Bounds.CenterPoint.Y + 0.5f * Bounds.HalfHeight),
                Bounds.HalfWidth, Bounds.HalfHeight));
        }

        /// <summary>
        /// Add a position to the quadtree
        /// </summary>
        /// <param name="position">A Vector2 to add to the quadtree<./param>
        /// <returns>True if the insertion was successful.</returns>
        public bool Insert(Vector2? position)
        {
            // Ignore objects that do not belong in this quad tree
            if (!this.Bounds.Contains(position))
                return false; // object cannot be added to this quad

            //If the quad is not full and it is an external node, fill it
            if (this.Position == null && NorthWest == null)
            {
                this.Position = position;
                return true;
            }

            // Otherwise, subdivide and shift down current position.
            if (NorthWest == null)
                Subdivide();

            //Shift down the current position as this is now an internal node
            if (NorthWest.Insert(this.Position))
            {
                this.Position = null;
            }
            else if (NorthEast.Insert(this.Position))
            {
                this.Position = null;
            }
            else if (SouthWest.Insert(this.Position))
            {
                this.Position = null;
            }
            else if (SouthEast.Insert(this.Position))
            {
                this.Position = null;
            }

            //Attempt to insert new node
            if (NorthWest.Insert(position))
                return true;

            if (NorthEast.Insert(position))
                return true;

            if (SouthWest.Insert(position))
                return true;

            if (SouthEast.Insert(position))
                return true;

            // The point cannot be inserted for  unknown reasons (how did we get here?
            return false;
        }

        /// <summary>
        /// Removes the first position from the quad tree at the position passed.
        /// </summary>
        /// <param name="pos">The position to be removed.</param>
        /// <returns>A true indicates a position was successfully removed.</returns>
        public bool Delete(Vector2 pos)
        {
            //TODO: Optimize this...
            List<Vector2?> allPositions = this.ToList();

            if (allPositions.Remove(pos))
            {
                Position = null;
                NorthWest = null;
                NorthEast = null;
                SouthWest = null;
                SouthEast = null;

                foreach (Vector2 position in allPositions)
                {
                    Insert(position);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Find all points within an axis aligned bounding box.
        /// </summary>
        /// <param name="bounds">An axis aligned bounding box to find all points in the quadtree.</param>
        /// <returns>A list of Vector2s representing all positions within the AABB passed.</returns>
        public List<Vector2?> QueryBounds(AxisAlignedBoundingBox bounds)
        {
            List<Vector2?> positionsInBounds = new List<Vector2?>();

            //Get out early if the bounds passed isn't in this quad (or a child quad)
            if (!this.Bounds.Intersect(bounds))
                return positionsInBounds;

            // Terminate here, if there are no children (external node)
            // We only need to check one since the subdivide method instantiates all sub-quads
            if (this.Position != null && NorthWest == null)
            {
                positionsInBounds.Add(this.Position);
                return positionsInBounds;
            }

            // Otherwise, add the positions from the children
            if (NorthWest != null)
                positionsInBounds.AddRange(NorthWest.QueryBounds(bounds));

            if (NorthEast != null)
                positionsInBounds.AddRange(NorthEast.QueryBounds(bounds));

            if (SouthWest != null)
                positionsInBounds.AddRange(SouthWest.QueryBounds(bounds));

            if (SouthEast != null)
                positionsInBounds.AddRange(SouthEast.QueryBounds(bounds));

            return positionsInBounds;
        }

        public List<Vector2?> ToList()
        {
            return this.QueryBounds(this.Bounds);
        } 
    }
}

