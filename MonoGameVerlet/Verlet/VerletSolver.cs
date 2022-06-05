﻿using Microsoft.Xna.Framework;
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
    public class VerletSolver : DrawableGameComponent
    {
        private SpriteBatch spriteBatch;
        public Vector2 Gravity = new Vector2(0f, 1000f);

        private List<VerletComponent> verletComponents;

        private int subSteps;

        private Vector2 constraintPosition;
        private float constraintRadius;
        
        public int NumberVerletComponents { get => verletComponents.Count; }
        public int SubSteps { get => subSteps; }

        public VerletSolver(SpriteBatch spriteBatch, Vector2 constraintPosition, float constraintRadius, Game game, int subSteps = 3) : base(game)
        {
            verletComponents = new List<VerletComponent>();
            this.spriteBatch = spriteBatch;
            this.constraintPosition = constraintPosition;
            this.constraintRadius = constraintRadius;
            this.subSteps = subSteps;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            float subDt = (float)(gameTime.ElapsedGameTime.TotalSeconds / subSteps);
            for (int subStep = subSteps; subStep > 0; subStep--)
            {
                applyGravity();
                applyConstraint();
                solveCollisions();
                updatePositions(subDt);
            }

            base.Update(gameTime);
        }

        private void updatePositions(float dt)
        {
            foreach (var verletComponent in verletComponents)
            {
                verletComponent.Update(dt);
            }
        }

        private void applyGravity()
        {
            foreach (var verletComponent in verletComponents)
            {
                verletComponent.Accelerate(Gravity);
            }
        }

        private void applyConstraint()
        {
            Vector2 toComponent;
            Vector2 n;

            foreach (var verletComponent in verletComponents)
            {
                toComponent.X = verletComponent.PositionCurrent.X - constraintPosition.X;
                toComponent.Y = verletComponent.PositionCurrent.Y - constraintPosition.Y;
                float dist = toComponent.Length();

                if(dist > constraintRadius - verletComponent.Radius)
                {
                    n = Vector2.Divide(toComponent, dist);                    
                    verletComponent.PositionCurrent = constraintPosition + Vector2.Multiply(n, constraintRadius - verletComponent.Radius);
                }
            }
        }

        private void solveCollisions()
        {
            Vector2 collisionAxis;
            Vector2 n;
            VerletComponent verletComponent1;
            VerletComponent verletComponent2;

            for (int i = 0; i < verletComponents.Count; i++)
            {
                verletComponent1 = verletComponents[i];
                
                for(int k = i + 1; k < verletComponents.Count; k++)
                {
                    verletComponent2 = verletComponents[k];
                    collisionAxis.X = verletComponent1.PositionCurrent.X - verletComponent2.PositionCurrent.X;
                    collisionAxis.Y = verletComponent1.PositionCurrent.Y - verletComponent2.PositionCurrent.Y;
                    float dist = Vector2.Distance(verletComponent1.PositionCurrent, verletComponent2.PositionCurrent);
                    float minDist = verletComponent1.Radius + verletComponent2.Radius;

                    if(dist < minDist)
                    {
                        n = collisionAxis / dist;
                        float delta = minDist - dist;
                        verletComponent1.PositionCurrent += 0.5f * delta * n;
                        verletComponent2.PositionCurrent -= 0.5f * delta * n;
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            ShapeExtensions.DrawCircle(spriteBatch, constraintPosition, constraintRadius, 100, Color.White);
            spriteBatch.End();

            foreach (var verletComponent in verletComponents)
            {
                spriteBatch.Begin();
                ShapeExtensions.DrawCircle(spriteBatch, verletComponent.PositionCurrent, verletComponent.Radius, 10, Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        public void AddVerletComponent(Vector2 position, float radius)
        {
            verletComponents.Add(new VerletComponent(position, radius));
        }
    }
}
