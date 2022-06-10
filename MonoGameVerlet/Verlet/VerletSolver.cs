using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGameVerlet.DataStructures;
using MonoGameVerlet.Effects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoGameVerlet.Verlet
{
    public class VerletSolver : DrawableGameComponent
    {
        private BloomFilter bloomFilter;

        private Texture2D circleTexture;

        private SpriteBatch spriteBatch;
        public Vector2 Gravity = new Vector2(0f, 2000f);

        private List<VerletComponent> verletComponents;
        private QuadTree quadTree;

        private Vector2 constraintPosition;
        private float constraintRadius;
        
        public int NumberVerletComponents { get => verletComponents.Count; }
        public int SubSteps;

        public bool UseQuadTree = true;
        public bool DrawQuadTree = false;

        Random random;
        public bool UseBloomShader = true;

        public VerletSolver(SpriteBatch spriteBatch, Vector2 constraintPosition, float constraintRadius, Game game, int subSteps = 3) : base(game)
        {
            verletComponents = new List<VerletComponent>();
            quadTree = new QuadTree(0, new Rectangle((int)(constraintPosition.X - constraintRadius), (int)(constraintPosition.Y - constraintRadius), (int)(constraintRadius * 2), (int)(constraintRadius * 2)));

            this.spriteBatch = spriteBatch;
            this.constraintPosition = constraintPosition;
            this.constraintRadius = constraintRadius;
            this.SubSteps = subSteps;

            circleTexture = Game.Content.Load<Texture2D>("Circle");
            random = new Random();

            //Load our Bloomfilter!
            bloomFilter = new BloomFilter();
            bloomFilter.Load(GraphicsDevice, Game.Content, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            bloomFilter.BloomPreset = BloomFilter.BloomPresets.Focussed;
            bloomFilter.BloomStrengthMultiplier = .5f;
            bloomFilter.BloomThreshold = .8f;
        }

        internal VerletComponent GetVerletComponent(Vector2 position)
        {
            List<VerletComponent> clickedNeighbors = new List<VerletComponent>();
            quadTree.Retrieve(clickedNeighbors, new Rectangle((int)position.X, (int)position.Y, 1, 1));

            VerletComponent closestToClick = null;
            float clickDist = 0;
            for(int i = 0; i < clickedNeighbors.Count; i++)
            {
                if(i==0)
                {
                    closestToClick = clickedNeighbors[i];
                    clickDist = Vector2.Distance(position, clickedNeighbors[i].PositionCurrent);
                }
                else
                {
                    if (Vector2.Distance(position, clickedNeighbors[i].PositionCurrent) < clickDist)
                    {
                        closestToClick = clickedNeighbors[i];
                        clickDist = Vector2.Distance(position, clickedNeighbors[i].PositionCurrent);
                    }
                }
            }

            return closestToClick;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public void Update(float dt)
        {
            applyGravity();
            applyConstraint();
            quadTree.Update(verletComponents);
            solveCollisions();
            updatePositions(dt);
        }

        internal void Reset()
        {
            verletComponents.Clear();
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
            if (UseQuadTree)
            {
                Vector2 collisionAxis;
                Vector2 n;
                VerletComponent verletComponent1;
                VerletComponent verletComponent2;

                for (int i = 0; i < verletComponents.Count; i++)
                {
                    List<VerletComponent> collisions = new List<VerletComponent>();
                    verletComponent1 = verletComponents[i];
                    if (verletComponent1.IsStatic)
                        continue;

                    quadTree.Retrieve(collisions, verletComponent1.Bounds);

                    for (int k = 0; k < collisions.Count; k++)
                    {
                        if (verletComponents[i] == collisions[k])
                            continue;

                        verletComponent2 = collisions[k];
                        collisionAxis.X = verletComponent1.PositionCurrent.X - verletComponent2.PositionCurrent.X;
                        collisionAxis.Y = verletComponent1.PositionCurrent.Y - verletComponent2.PositionCurrent.Y;
                        float dist = Vector2.Distance(verletComponent1.PositionCurrent, verletComponent2.PositionCurrent);
                        float minDist = verletComponent1.Radius + verletComponent2.Radius;

                        if (dist < minDist)
                        {
                            n = collisionAxis / dist;
                            float delta = minDist - dist;
                            verletComponent1.PositionCurrent += 0.5f * delta * n;

                            if (!verletComponent2.IsStatic)
                            {
                                verletComponent2.PositionCurrent -= 0.5f * delta * n;
                            }
                        }
                    }
                }
            }
            else
            {
                Vector2 collisionAxis;
                Vector2 n;
                VerletComponent verletComponent1;
                VerletComponent verletComponent2;

                for (int i = 0; i < verletComponents.Count; i++)
                {
                    verletComponent1 = verletComponents[i];

                    for (int k = i + 1; k < verletComponents.Count; k++)
                    {
                        verletComponent2 = verletComponents[k];
                        collisionAxis.X = verletComponent1.PositionCurrent.X - verletComponent2.PositionCurrent.X;
                        collisionAxis.Y = verletComponent1.PositionCurrent.Y - verletComponent2.PositionCurrent.Y;
                        float dist = Vector2.Distance(verletComponent1.PositionCurrent, verletComponent2.PositionCurrent);
                        float minDist = verletComponent1.Radius + verletComponent2.Radius;

                        if (dist < minDist)
                        {
                            n = collisionAxis / dist;
                            float delta = minDist - dist;
                            verletComponent1.PositionCurrent += 0.5f * delta * n;
                            verletComponent2.PositionCurrent -= 0.5f * delta * n;
                        }
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            //Shader needs this spritebatch setup
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

            spriteBatch.DrawCircle(constraintPosition, constraintRadius, 100, Color.White);
            if (DrawQuadTree && UseQuadTree)
            {
                quadTree.Draw(spriteBatch, GraphicsDevice);
            }

            if (UseBloomShader)
            {
                Texture2D bloom = bloomFilter.Draw(circleTexture, GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Width / 2);
                Game.GraphicsDevice.SetRenderTarget(null);

                foreach (var verletComponent in verletComponents)
                {
                    spriteBatch.Draw(bloom, new Rectangle((int)(verletComponent.PositionCurrent.X - verletComponent.Radius),
                        (int)(verletComponent.PositionCurrent.Y - verletComponent.Radius),
                        (int)verletComponent.Radius * 4,
                        (int)verletComponent.Radius * 4),
                        verletComponent.Color * .85f);
                }
            }
            spriteBatch.End();
            
            //Change spritebatch so we can do transparency
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            foreach (var verletComponent in verletComponents)
            {
                spriteBatch.Draw(circleTexture,
                    new Rectangle((int)verletComponent.PositionCurrent.X,
                    (int)verletComponent.PositionCurrent.Y,
                    (int)verletComponent.Radius * 2,
                    (int)verletComponent.Radius * 2),
                    verletComponent.Color * .5f);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void AddVerletComponent(Vector2 position, float radius, bool isStatic = false)
        {
            verletComponents.Add(new VerletComponent(position, radius, isStatic));
        }

        public void AddChain(ChainComponent chain)
        {
            foreach(var link in chain.Links)
            {
                verletComponents.Add(link);
            }
        }
    }
}
