﻿using OpenTK;
using System;

//using Geometry;

namespace Geometry
{
    public enum LightType { Static, Baked }

    public abstract class Light
    {
        public float intensity { get; set; } //Intensity of the light
        public Vector3 colour { get; set; } //Colour for the light maps (RGB)
        public Vector3 shadowColour { get; set; } //The colour for the shadow maps
        public Vector3 position { get; set; }  //Position of the light
        public Vector3 attenuation { get; set; } //Attenuation vector (constant,linear,quadratic)
        public Boolean shootsPhotons { get; set; } //Whether to shoot photons from this light or not 
        public Boolean inLightMap { get; set; }
        public Boolean inShadowMap { get; set; }

        public Light(Vector3 pos, Vector3 col, Vector3 shadowCol, float intense, Boolean shoots)
        {
            position = pos;
            colour = col * 255;
            shadowColour = shadowCol * 255;
            intensity = intense;
            //By default all lights use 1/d^2 as their falloff. If Settings.useTrueAttenuation = false this vector
            //      will be overwritten when the lights are created
            attenuation = new Vector3(1, 0, 0);
            shootsPhotons = shoots;
            inLightMap = false;
            inShadowMap = false;
        }

        //Returns a float value representing the amount of light this light would have on 
        //      input point ignoring obstructions
        public abstract float influence(float distance);

        //Returns a random vector along which a photon from this light could travel
        public abstract Vector3 generateRandomDirection();


        //-- For creating randomly directed rays for lights --//
        private static Random random = new Random();
        private static Boolean haveNextNextGaussian = false;
        private static double nextNextGaussian;

        protected static double nextRandom()
        {
            lock (random)
            {
                return random.NextDouble();
            }
        }

        protected static double nextGaussian()
        {
            if (haveNextNextGaussian)
            {
                haveNextNextGaussian = false;
                return nextNextGaussian;
            }
            else
            {
                double v1, v2, s;
                do
                {
                    v1 = 2 * random.NextDouble() - 1;   // between -1.0 and 1.0
                    v2 = 2 * random.NextDouble() - 1;   // between -1.0 and 1.0
                    s = v1 * v1 + v2 * v2;
                } while (s >= 1 || s == 0);
                double multiplier = System.Math.Sqrt(-2 * System.Math.Log(s) / s);
                nextNextGaussian = v2 * multiplier;
                haveNextNextGaussian = true;
                return v1 * multiplier;
            }
        }
    }
}
