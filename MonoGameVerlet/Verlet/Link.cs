using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoGameVerlet.Verlet
{
    public class Link
    {
        public VerletComponent Component1;
        public VerletComponent Component2;
        public float TargetDist;

        public Link(VerletComponent component1, VerletComponent component2, float targetDist)
        {
            Component1 = component1;
            Component2 = component2;
            TargetDist = targetDist;
        }

        
    }
}
