using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoGameVerlet.Verlet
{
    /// <summary>
    /// Port of Verlet simulation: https://www.youtube.com/watch?v=lS_qeBy3aQI&t=72
    /// </summary>
    public class VerletComponent
    {
        public float Radius;

        public Vector2 PositionCurrent = Vector2.Zero;
        private Vector2 positionOld = Vector2.Zero;
        private Vector2 acceleration = Vector2.Zero;
        
        public RectangleF Bounds;

        public VerletComponent(Vector2 initialPosition, float radius = 15f)
        {
            positionOld = initialPosition;
            PositionCurrent = initialPosition;
            Radius = radius;
            Bounds = new RectangleF(initialPosition.X, initialPosition.Y, (radius * 2), (radius * 2));
        }

        public void Update(float dt)
        {
            Vector2 velocity = PositionCurrent - positionOld;
            
            //Save current position
            positionOld = PositionCurrent;

            //Perform Verlet Integration
            PositionCurrent = PositionCurrent + velocity + Vector2.Multiply(acceleration, dt * dt);

            //Reset acceleration
            acceleration = Vector2.Zero;

            Bounds = new RectangleF(PositionCurrent.X, PositionCurrent.Y, (Radius * 2), (Radius * 2));
        }

        public void Accelerate(Vector2 acc)
        {
            acceleration += acc;
        }
    }
}
