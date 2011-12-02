using OpenTK;

namespace DALightmapper
{
    public enum LightType { Static, Baked }

    abstract class Light
    {

        float _intensity;       //Intensity of the light
        Vector3 _colour;        //Colour of the light (RGB)
        Vector3 _position;      //Position of the light
        LightType _type;        //Type of light (enum)
        Vector3 _attenuation;   //Attenuation vector (constant,linear,quadratic)

        public float intensity
        {
            get { return _intensity; }
            set { _intensity = value; }
        }
        public Vector3 colour
        {
            get { return _colour; }
            set { _colour = value; }
        }
        public Vector3 position
        {
            get { return _position; }
            set { _position = value; }
        }
        public LightType type
        {
            get { return _type; }
            set { _type = value; }
        }
        public Vector3 attenuation
        {
            get { return _attenuation; }
            set { _attenuation = value; }
        }

        public Light(Vector3 pos, Vector3 col, float intense, LightType t)
        {
            position = pos;
            colour = col;
            intensity = intense;
            //By default all lights use 1/d^2 as their falloff. If Settings.useTrueAttenuation = false this vector
            //      will be overwritten when the lights are created
            attenuation = new Vector3(1, 1, 1);
            type = t;
        }

        //Returns a float value representing the amount of light this light would have on 
        //      input point ignoring obstructions
        public abstract float influence(Patch patch);
    }
}
