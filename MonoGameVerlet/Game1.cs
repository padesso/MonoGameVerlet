using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.ImGui;
using MonoGame.ImGui.Standard;
using MonoGameVerlet.Verlet;
using System;
using System.Collections.Generic;

namespace MonoGameVerlet
{
    public class Game1 : Game
    {
        private FrameCounter frameCounter = new FrameCounter();

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private VerletSolver verletSolver;
        private double spawnDelay = 125; //ms
        private double spawnTime = 0;

        private ChainComponent chain;

        public ImGUIRenderer GuiRenderer;
        private bool reset = false;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.ApplyChanges();

            GuiRenderer = new ImGUIRenderer(this).Initialize().RebuildFontAtlas();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            verletSolver = new VerletSolver(spriteBatch, new Vector2(960, 540f), 500, this, 5);

            chain = new ChainComponent(10, new Vector2(800, 500), new Vector2(1200, 500), 20, 40); //TODO: do better
            verletSolver.AddChain(chain);
        }

        protected override void Update(GameTime gameTime)
        {
            if(reset)
            {
                verletSolver.Reset();
                reset = false;
            }

            spawnTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (spawnTime > spawnDelay && verletSolver.NumberVerletComponents < 1000)
            {
                verletSolver.AddVerletComponent(new Vector2(540, 300), (float)(new Random().NextDouble() * 10 + 5));
                verletSolver.AddVerletComponent(new Vector2(750, 300), (float)(new Random().NextDouble() * 10 + 5));
                verletSolver.AddVerletComponent(new Vector2(960, 300), (float)(new Random().NextDouble() * 10 + 5));
                verletSolver.AddVerletComponent(new Vector2(1170, 300), (float)(new Random().NextDouble() * 10 + 5));
                verletSolver.AddVerletComponent(new Vector2(1380, 300), (float)(new Random().NextDouble() * 10 + 5));
                spawnTime = 0;
            }

            float subDt = (float)(gameTime.ElapsedGameTime.TotalSeconds / verletSolver.SubSteps);
            for (int subStep = verletSolver.SubSteps; subStep > 0; subStep--)
            {
                verletSolver.Update(subDt);
                chain.Update(subDt);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            verletSolver.Draw(gameTime);

            //chain.Draw(spriteBatch, GraphicsDevice);

            //Debug info
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            frameCounter.Update(deltaTime);
            var fps = string.Format("FPS: {0}", frameCounter.AverageFramesPerSecond);

            //ImGUI
            GuiRenderer.BeginLayout(gameTime);

            ImGui.Begin("Debug Settings");
            ImGui.SetWindowPos(new System.Numerics.Vector2(10, 10));
            ImGui.SetWindowSize(new System.Numerics.Vector2(250, 200));
            ImGui.Text(fps);
            ImGui.Text("Object Count: " + verletSolver.NumberVerletComponents);
            ImGui.SliderInt("Substeps", ref verletSolver.SubSteps, 1, 10);
            ImGui.SliderFloat("Gravity X", ref verletSolver.Gravity.X, -5000, 5000);
            ImGui.SliderFloat("Gravity Y", ref verletSolver.Gravity.Y, -5000, 5000);
            ImGui.Checkbox("Use QuadTree?", ref verletSolver.UseQuadTree);
            ImGui.Checkbox("Draw QuadTree?", ref verletSolver.DrawQuadTree);
            reset = ImGui.Button("Reset");

            GuiRenderer.EndLayout();

            base.Draw(gameTime);
        }
    }
}
