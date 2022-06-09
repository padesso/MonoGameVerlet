using System;
using System.Text;

namespace MonoGameVerlet.Verlet
{
    public class ChainComponent
    {
        private Link[] Links;
        private int radius;

        public ChainComponent(Link[] links, int radius)
        {
            Links = links;
            this.radius = radius;
        }

        public void Update(float dt)
        {
            //TODO:
            foreach(var link in Links)
            {
                link.Apply();
            }
        }
    }
}
