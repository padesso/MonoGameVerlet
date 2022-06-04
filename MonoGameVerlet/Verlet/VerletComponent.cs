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
        private SpriteBatch spriteBatch;

        public float Radius;

        public Vector2 PositionCurrent = Vector2.Zero;
        private Vector2 positionOld = Vector2.Zero;
        private Vector2 acceleration = Vector2.Zero;

        public VerletComponent(Vector2 initalPosition, SpriteBatch spriteBatch, Game game, float radius = 15f)
        {
            this.spriteBatch = spriteBatch;
            positionOld = initalPosition;
            PositionCurrent = initalPosition;
            Radius = radius;
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
        }

        public void Accelerate(Vector2 acc)
        {
            acceleration += acc;
        }
    }
}
