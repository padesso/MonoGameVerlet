using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Text;

namespace MonoGameVerlet.Verlet
{
    public class ChainComponent
    {
        private Link[] Links;

        public ChainComponent(int numLinks, Vector2 startPosition, Vector2 endPosition, int radius)
        {
            if(numLinks < 3)
                numLinks = 3;

            Links = new Link[numLinks];

            //TODO: create this properly
            for (int i = 0; i < numLinks; i++)
            {
                if (i == 0)
                {
                    Links[i] = new Link(new VerletComponent(startPosition, radius, true), new VerletComponent(startPosition, radius, false), radius * 2);
                }
                else if(i > 0 && i < numLinks - 1)
                {
                    Links[i] = new Link(new VerletComponent(startPosition, radius, false), new VerletComponent(startPosition, radius, false), radius * 2);
                }
                else
                {
                    Links[i] = new Link(new VerletComponent(endPosition, radius, false), new VerletComponent(endPosition, radius, true), radius * 2);
                }
            }
        }

        public void Update(float dt)
        {
            //TODO: something?
            foreach (var link in Links)
            {
                link.Apply();
            }
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice g)
        {
            for(int i = 0; i < Links.Length; i++)
            {
                spriteBatch.Begin();
                ShapeExtensions.DrawCircle(spriteBatch, Links[i].Component1.PositionCurrent, Links[i].Component1.Radius, 36, Color.White);
                ShapeExtensions.DrawCircle(spriteBatch, Links[i].Component2.PositionCurrent, Links[i].Component2.Radius, 36, Color.White);
                spriteBatch.End();
            }
        }
    }
}
