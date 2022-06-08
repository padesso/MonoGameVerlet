using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.ImGui;
using MonoGame.ImGui.Standard;
using MonoGameVerlet.Verlet;
using System;

namespace MonoGameVerlet
{
    public class Game1 : Game
    {
        private FrameCounter frameCounter = new FrameCounter();

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private VerletSolver verletSolver;
        private double spawnDelay = 250; //ms
        private double spawnTime = 0;

        SpriteFont debugFont;
        public ImGUIRenderer GuiRenderer; 

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
            debugFont = Content.Load<SpriteFont>("Debug");
            verletSolver = new VerletSolver(spriteBatch, new Vector2(960, 540f), 500, this, 5);           
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            spawnTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (spawnTime > spawnDelay && verletSolver.NumberVerletComponents < 2000)
            {
                verletSolver.AddVerletComponent(new Vector2(540, 300), (float)(new Random().NextDouble() * 10));
                verletSolver.AddVerletComponent(new Vector2(750, 300), (float)(new Random().NextDouble() * 10));
                verletSolver.AddVerletComponent(new Vector2(960, 300), (float)(new Random().NextDouble() * 10));
                verletSolver.AddVerletComponent(new Vector2(1170, 300), (float)(new Random().NextDouble() * 10));
                verletSolver.AddVerletComponent(new Vector2(1380, 300), (float)(new Random().NextDouble() * 10));
                spawnTime = 0;
            }

            verletSolver.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            verletSolver.Draw(gameTime);

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            frameCounter.Update(deltaTime);
            var fps = string.Format("FPS: {0}", frameCounter.AverageFramesPerSecond);

            spriteBatch.Begin();
            spriteBatch.DrawString(debugFont, fps, new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(debugFont, "Number objects: " + verletSolver.NumberVerletComponents, new Vector2(10, 30), Color.White);
            spriteBatch.DrawString(debugFont, "Substeps: " + verletSolver.SubSteps, new Vector2(10, 50), Color.White);
            spriteBatch.End();

            GuiRenderer.BeginLayout(gameTime);

            //Insert Your ImGui code
            ImGui.Text("Test");

            GuiRenderer.EndLayout();

            base.Draw(gameTime);
        }
    }
}
