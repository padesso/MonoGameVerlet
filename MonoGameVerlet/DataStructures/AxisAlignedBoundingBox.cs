using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoGameVerlet.DataStructures
{
    public class AxisAlignedBoundingBox
    {
        private Vector2 centerPoint;
        private float width;
        private float height;

        public AxisAlignedBoundingBox(Vector2 centerPoint, float width, float height)
        {
            this.centerPoint = centerPoint;
            this.width = width;
            this.height = height;
        }

        public Vector2 TopLeft { get => new Vector2(centerPoint.X - HalfWidth, centerPoint.Y - HalfHeight); }
        public Vector2 CenterPoint { get => centerPoint; set => centerPoint = value; }
        public float Width { get => width; set => width = value; }
        public float Height { get => height; set => height = value; }

        public float HalfWidth { get => width / 2.0f; }
        public float HalfHeight { get => height / 2.0f; }

        /// <summary>
        /// Check if a position is within the box.
        /// </summary>
        /// <param name="position">Position to be checked.</param>
        /// <returns>Bool based on whether position Vector2 is within the AABB.</returns>
        public bool Contains(Vector2? position)
        {
            if (position == null)
                return false;

            if (position.Value.X >= (this.centerPoint.X - HalfWidth) &&
                position.Value.X < (this.centerPoint.X + HalfWidth) &&
                position.Value.Y >= (this.centerPoint.Y - HalfHeight) &&
                position.Value.Y < (this.centerPoint.Y + HalfHeight))
                return true;

            return false;
        }

        /// <summary>
        /// Check if two axis aligned bounding boxes intersect with one another
        /// </summary>
        /// <param name="bounds">The AABB to check against.</param>
        /// <returns>A bool based on if the AABB's in question intersect.</returns>
        public bool Intersect(AxisAlignedBoundingBox bounds)
        {
            if (this.CenterPoint.X + this.HalfWidth < bounds.CenterPoint.X - bounds.HalfWidth) return false; // left
            if (this.CenterPoint.X - this.HalfWidth > bounds.CenterPoint.X + bounds.HalfWidth) return false; // right
            if (this.CenterPoint.Y + this.HalfHeight < bounds.CenterPoint.Y - bounds.HalfHeight) return false; // above
            if (this.CenterPoint.Y - this.HalfHeight > bounds.CenterPoint.Y + bounds.HalfHeight) return false; // below
            return true; // overlap
        }
    }
}
