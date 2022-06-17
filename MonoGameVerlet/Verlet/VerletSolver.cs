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
        public QuadTree QuadTree;

        private Vector2 constraintPosition;
        private float constraintRadius;
        
        public int NumberVerletComponents { get => verletComponents.Count; }
        public int SubSteps;

        public bool UseQuadTree = true;
        public bool DrawQuadTree = false;

        Random random;
        public bool UseBloomShader = true;

        private Rectangle heatSource;
        public bool HeatEnabled = false;
        public float HeatAmount = .1f;

        public VerletSolver(SpriteBatch spriteBatch, Vector2 constraintPosition, float constraintRadius, Game game, int subSteps = 3) : base(game)
        {
            verletComponents = new List<VerletComponent>();
            QuadTree = new QuadTree(0, new Rectangle((int)(constraintPosition.X - constraintRadius), (int)(constraintPosition.Y - constraintRadius), (int)(constraintRadius * 2), (int)(constraintRadius * 2)));

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
            
            heatSource = new Rectangle((int)(constraintPosition.X - constraintRadius), 1000, (int)(constraintRadius * 2), 50); 
        }

        internal VerletComponent GetVerletComponent(Vector2 position)
        {
            List<VerletComponent> clickedNeighbors = new List<VerletComponent>();
            QuadTree.Retrieve(clickedNeighbors, new Rectangle((int)position.X, (int)position.Y, 1, 1));

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

        internal List<VerletComponent> GetVerletComponents(Vector2 position, Vector2 size)
        {
            List<VerletComponent> components = new List<VerletComponent>();
            QuadTree.Retrieve(components, new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y));

            return components;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public void Update(float dt)
        {
            
            applyGravity();
            applyConstraint();
            QuadTree.Update(verletComponents);
            if (HeatEnabled)
            {
                applyHeat();
            }
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
                if(!verletComponent.IsStatic)
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

        private void applyHeat()
        {
            foreach (var verletComponent in verletComponents)
            {
                //TODO: handle multiple heat sources
                if (heatSource.Contains(verletComponent.PositionCurrent))
                {
                    verletComponent.ApplyTemperature(HeatAmount);
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

                    QuadTree.Retrieve(collisions, verletComponent1.Bounds);

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

                            //Temp advection
                            var tempDelta = verletComponent1.Temperature - verletComponent2.Temperature;

                            if (tempDelta > 0)
                            {
                                verletComponent1.ApplyTemperature(-.01f);
                                verletComponent2.ApplyTemperature(.01f);
                            }
                            else if (tempDelta < 0)
                            {
                                verletComponent1.ApplyTemperature(.01f);
                                verletComponent2.ApplyTemperature(-.01f);
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

                            //Temp advection
                            var tempDelta = verletComponent1.Temperature - verletComponent2.Temperature;

                            if (tempDelta > 0)
                            {
                                verletComponent1.ApplyTemperature(-.01f);
                                verletComponent2.ApplyTemperature(.01f);
                            }
                            else if (tempDelta < 0)
                            {
                                verletComponent1.ApplyTemperature(.01f);
                                verletComponent2.ApplyTemperature(-.01f);
                            }
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
                QuadTree.Draw(spriteBatch, GraphicsDevice);
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

            //Heatsource
            ShapeExtensions.DrawRectangle(spriteBatch, heatSource, Color.Red);

            //foreach (var verletComponent in verletComponents)
            //{
            //    spriteBatch.Draw(circleTexture,
            //        new Rectangle((int)verletComponent.PositionCurrent.X,
            //        (int)verletComponent.PositionCurrent.Y,
            //        (int)verletComponent.Radius * 2,
            //        (int)verletComponent.Radius * 2),
            //        verletComponent.Color);
            //}

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void AddVerletComponent(Vector2 position, float radius, bool isStatic = false, int temperature = 0)
        {
            verletComponents.Add(new VerletComponent(position, radius, isStatic, temperature));
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
