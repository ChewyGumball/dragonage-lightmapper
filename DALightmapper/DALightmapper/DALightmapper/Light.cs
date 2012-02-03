using OpenTK;
using System;

namespace DALightmapper
{
    public enum LightType { Static, Baked }

    public abstract class Light
    {
        public float intensity { get; set; } //Intensity of the light
        public Vector3 colour { get; set; } //Colour of the light (RGB)
        public Vector3 position { get; set; }  //Position of the light
        public LightType type { get; set; } //Type of light (enum)
        public Vector3 attenuation { get; set; } //Attenuation vector (constant,linear,quadratic)
        public Boolean shootsPhotons { get; set; } //Whether to shoot photons from this light or not 

        public Light(Vector3 pos, Vector3 col, float intense, LightType t, Boolean shoots)
        {
            position = pos;
            colour = col;
            intensity = intense;
            //By default all lights use 1/d^2 as their falloff. If Settings.useTrueAttenuation = false this vector
            //      will be overwritten when the lights are created
            attenuation = new Vector3(1, 1, 1);
            type = t;
            shootsPhotons = shoots;
        }

        //Returns a float value representing the amount of light this light would have on 
        //      input point ignoring obstructions
        public abstract float influence(Patch patch);

        //Returns a random vector along which a photon from this light could travel
        public abstract Vector3 generateRandomDirection();
    }
}
