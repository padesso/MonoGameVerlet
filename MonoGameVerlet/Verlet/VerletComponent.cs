using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoGameVerlet.Verlet
{
    public class VerletComponent
    {
        public float Radius;

        public Vector2 PositionCurrent = Vector2.Zero;
        public Vector2 PositionOld = Vector2.Zero;
        private Vector2 acceleration = Vector2.Zero;
        
        public Rectangle Bounds;
        public Color Color;

        public bool IsStatic;

        public float Temperature;
        private const float MAX_TEMPERATURE = 1f;
        private const float MIN_TEMPERATURE = 0f;
        private Vector2 tempVelcityModifier = new Vector2(0, -2000);

        public VerletComponent(Vector2 initialPosition, float radius = 15f, bool isStatic = false, int initialTemperature = 0)
        {
            PositionOld = initialPosition;
            PositionCurrent = initialPosition;
            Radius = radius;
            Bounds = new Rectangle((int)initialPosition.X, (int)initialPosition.Y, (int)(radius * 2), (int)(radius * 2));
            IsStatic = isStatic;
            Temperature = initialTemperature;

            //TODO: color by applied temperature
            Random random = new Random();
            Color = ApplyTemperatureColor();
        }

        public void Update(float dt)
        {
            if (!IsStatic)
            {
                Vector2 velocity = PositionCurrent - PositionOld;

                //Save current position
                PositionOld = PositionCurrent;

                //Perform Verlet Integration
                PositionCurrent = PositionCurrent + velocity + Vector2.Multiply(acceleration, dt * dt);

                //Reset acceleration
                acceleration = Vector2.Zero;
            }

            //Decay the temp
            ApplyTemperature(-.001f);

            Bounds = new Rectangle((int)PositionCurrent.X, (int)PositionCurrent.Y, (int)(Radius * 2), (int)(Radius * 2));
            Color = ApplyTemperatureColor();
        }

        public void Accelerate(Vector2 acc)
        {
            float accY = Temperature / MAX_TEMPERATURE;
            acc.Y += -accY * 5000; //TODO: apply to a acc factor or perhaps move this up to solver to have access to gravity value
            acceleration += acc;
        }

        private Color ApplyTemperatureColor()
        {
            //TODO: scale these values via maths

            if(Temperature <= MIN_TEMPERATURE)
            {
                return new Color(.3f, 0f, 0f);
            }
            else if(Temperature > MIN_TEMPERATURE && Temperature <= .2 * MAX_TEMPERATURE)
            {
                return Color.DarkRed;
            }
            else if (Temperature > .2 * MAX_TEMPERATURE && Temperature <= .4 * MAX_TEMPERATURE)
            {
                return Color.Red;
            }
            else if (Temperature > .4 * MAX_TEMPERATURE && Temperature <= .6 * MAX_TEMPERATURE)
            {
                return Color.Orange;
            }
            else if (Temperature > .6 * MAX_TEMPERATURE && Temperature <= .8 * MAX_TEMPERATURE)
            {
                return Color.Yellow;
            }
            else if (Temperature > .8 * MAX_TEMPERATURE && Temperature <= MAX_TEMPERATURE)
            {
                return Color.White;
            }
            else if (Temperature > MAX_TEMPERATURE)
            {
                return Color.CornflowerBlue;
            }

            return Color.Green; //We should never get here!
        }

        public void ApplyTemperature(float tempChange)
        {
            

            if(Temperature + tempChange < MIN_TEMPERATURE)
            {
                Temperature = MIN_TEMPERATURE;
            }
            else if(Temperature + tempChange > MAX_TEMPERATURE)
            {
                Temperature = MAX_TEMPERATURE;
            }
            else
            {
                Temperature += tempChange;
            }
        }
    }
}
