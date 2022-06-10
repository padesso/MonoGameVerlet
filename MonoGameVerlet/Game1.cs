using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.ImGui.Standard;
using MonoGameVerlet.Verlet;
using System;
using MonoGame.Extended.Input.InputListeners;

namespace MonoGameVerlet
{
    public class Game1 : Game
    {
        private readonly MouseListener mouseListener = new MouseListener();
        private readonly KeyboardListener keyboardListener = new KeyboardListener();

        private FrameCounter frameCounter = new FrameCounter();

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private VerletSolver verletSolver;
        private double spawnDelay = 125; //ms
        private double spawnTime = 0;

        private ChainComponent chain1;
        private ChainComponent chain2;

        public ImGUIRenderer GuiRenderer;
        private bool reset = false;

        private VerletComponent selectedVerletComponent;
        private bool clickedComponentStatic = false;

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
            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.ApplyChanges();

            GuiRenderer = new ImGUIRenderer(this).Initialize().RebuildFontAtlas();

            mouseListener.MouseDragStart += MouseListener_MouseDragStart;
            mouseListener.MouseDrag += MouseListener_MouseDrag;
            mouseListener.MouseDragEnd += MouseListener_MouseDragEnd;

            keyboardListener.KeyPressed += KeyboardListener_KeyPressed;

            base.Initialize();
        }

        private void KeyboardListener_KeyPressed(object sender, KeyboardEventArgs e)
        {
            if(e.Key == Microsoft.Xna.Framework.Input.Keys.Left)
            {
                verletSolver.Gravity.X -= 100;
            }
            else if(e.Key == Microsoft.Xna.Framework.Input.Keys.Right)
            {
                verletSolver.Gravity.X += 100;
            }
            else if(e.Key == Microsoft.Xna.Framework.Input.Keys.Up)
            {
                verletSolver.Gravity.Y -= 100;
            }
            else if(e.Key == Microsoft.Xna.Framework.Input.Keys.Down)
            {
                verletSolver.Gravity.Y += 100;
            }
        }

        private void MouseListener_MouseDrag(object sender, MouseEventArgs e)
        {
            selectedVerletComponent.PositionOld = new Vector2(e.Position.X - selectedVerletComponent.Radius, e.Position.Y - selectedVerletComponent.Radius);
            selectedVerletComponent.PositionCurrent = new Vector2(e.Position.X - selectedVerletComponent.Radius, e.Position.Y - selectedVerletComponent.Radius);
        }

        private void MouseListener_MouseDragEnd(object sender, MouseEventArgs e)
        {
            selectedVerletComponent.IsStatic = clickedComponentStatic;
            selectedVerletComponent = null;
        }

        private void MouseListener_MouseDragStart(object sender, MouseEventArgs e)
        {
            selectedVerletComponent = verletSolver.GetVerletComponent(new Vector2(e.Position.X, e.Position.Y));
            clickedComponentStatic = selectedVerletComponent.IsStatic; //store the state
            selectedVerletComponent.IsStatic = true; //make it static so it's not affected by physics during drag
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            verletSolver = new VerletSolver(spriteBatch, new Vector2(960, 540f), 500, this, 5);

            chain1 = new ChainComponent(10, new Vector2(800, 500), new Vector2(1200, 500), 20, 40);
            verletSolver.AddChain(chain1);

            chain2 = new ChainComponent(10, new Vector2(600, 400), 20, 40);
            verletSolver.AddChain(chain2);
        }

        protected override void Update(GameTime gameTime)
        {
            mouseListener.Update(gameTime);
            keyboardListener.Update(gameTime);

            if (reset)
            {
                verletSolver.Reset();
                reset = false;
            }

            spawnTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (spawnTime > spawnDelay && verletSolver.NumberVerletComponents < 500)
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
                chain1.Update(subDt);
                chain2.Update(subDt);
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
