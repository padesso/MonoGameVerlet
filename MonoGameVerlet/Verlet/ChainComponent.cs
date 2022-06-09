using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Text;

namespace MonoGameVerlet.Verlet
{
    public class ChainComponent
    {
        public VerletComponent[] Links;
        public float TargetDist;

        public ChainComponent(int numLinks, Vector2 startPosition, Vector2 endPosition, int radius, float targetDist)
        {
            TargetDist = targetDist;

            if(numLinks < 3)
                numLinks = 3;

            Links = new VerletComponent[numLinks];

            for (int i = 0; i < numLinks; i++)
            {
                if (i == 0)
                {
                    Links[i] =new VerletComponent(startPosition, radius, true);
                }
                else if(i > 0 && i < numLinks - 1)
                {
                    //TODO: interpolate between start and end positions and add accurately
                    Links[i] = new VerletComponent(new Vector2(startPosition.X + radius, startPosition.Y), radius, false);
                }
                else
                {
                    Links[i] = new VerletComponent(endPosition, radius, true);
                }
            }
        }

        public void Update(float dt)
        {
            for(int i = 0; i < Links.Length - 1; i++)
            {
                VerletComponent component1 = Links[i];
                VerletComponent component2 = Links[i + 1];
                Vector2 axis = component1.PositionCurrent - component2.PositionCurrent;
                float dist = axis.Length();
                Vector2 n = axis / dist;
                float delta = TargetDist - dist;

                if(!component1.IsStatic)
                    component1.PositionCurrent += 0.5f * delta * n;

                if(!component2.IsStatic)
                    component2.PositionCurrent -= 0.5f * delta * n;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice g)
        {
            for(int i = 0; i < Links.Length; i++)
            {
                spriteBatch.Begin();
                ShapeExtensions.DrawCircle(spriteBatch, Links[i].PositionCurrent, Links[i].Radius, 36, Color.White);
                spriteBatch.End();
            }
        }
    }
}
