using System;
using System.Threading;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Bioware.Files;
using Bioware.Structs;
using System.Drawing;
using System.Drawing.Imaging;

using Ben;

namespace DALightmapper
{
    class FinishedLightMappingEventArgs : EventArgs
    {
        String _message;

        public String message
        {
            get { return _message; }
        }

        public FinishedLightMappingEventArgs(String message)
        {
            _message = message;
        }
    }


    static class Lightmapper
    {
        static bool _abort;  //Used to abort the lightmapping process prematurely
        static Dictionary<String, int> _textures = new Dictionary<string, int>();

        //Holds the vector arrays representing a texture - use vectors as they are easier to work with than a OpenGL texture
        //  They are converted into textures after the mapping process is completed or aborted
        static List<LightMap> _maps = new List<LightMap>();

        //Holds the points which need to have a hemicube rendering because their neighbouring pixels are too dissimilar
        static List<Vector2> pointsToBeRendered = new List<Vector2>();

        public static bool abort
        {
            get { return _abort; }
            set { _abort = value; }
        }
        public static List<LightMap> maps
        {
            get { return _maps; }
        }


        //Event to signal asynchronous lightmapping has finished
        public delegate void FinishedLightMappingEventHandler(FinishedLightMappingEventArgs args);
        public static event FinishedLightMappingEventHandler FinishedLightMapping;

        //public static LightMapPreviewForm lightMapPreviewForm;

        // Runs the light map process
        public static void runLightmaps(Level level)
        {
            //Make patches
            List<Patch> patches = new List<Patch>();
            //Make lightmaps

            //Make PVS

            //Initialize the loop counter so message can say how many bounces were rendered
            int currentBounce = 0;
            int stride;

            //Start the lightmapping process off by just rendering light sources
            renderLights(); //THIS NEEDS TO BE CHANGED!!!!


            //Do some lightmapping
            for(abort = false; currentBounce < Settings.numBounces; currentBounce ++)
            {
                //Find patch with highest exident light
                Patch highest = patches[0];
                foreach (Patch p in patches)
                {
                    if(p.excidentLight.Length > highest.excidentLight.Length)
                    {
                        highest = p;
                    }
                }

                //Propogate it's light
                propogate(highest);
            }
            
            
            //If the loop exited early fire the event saying so
            if (abort)
            {
                FinishedLightMapping.BeginInvoke(new FinishedLightMappingEventArgs("Aborted lightmapping after " + 4/*currentBounce*/ + " bounces."), null, null);
            }
            //Otherwise fire the event saying lightmapping was finished completely
            else
            {
                FinishedLightMapping.BeginInvoke(new FinishedLightMappingEventArgs("Successfully finished light mapping."), null, null);
            }
        }

        //Propogates light from a to a's pvs
        private static void propogate(Patch a)
        {
        }

        //Initializes pixels to the colour received only from visible lights to start the light mapping process
        private static void renderLights()
        {
            
        }

        private static void makeIntoTexture(LightMap l)
        {
        }

        //Calculates the linear interpolation phase of the light mapping algorithm using the given stride
        private static void lerp(LightMap l, int stride)
        {
            int halfStride = stride / 2;
            int length, width, tempY, tempX;
            Vector3 a, b, c, d, ratioVector;

            length = l.lightMap.GetLength(0);
            width = l.lightMap.GetLength(1);

            for (int x = halfStride; x < length - halfStride; x += stride)
            {
                for (int y = halfStride; y < width - halfStride; y += stride)
                {
                    //If the x is in bounds calculate the row pixels
                    if (x < length)
                    {
                        tempY = y - halfStride;

                        a = l.lightMap[x + halfStride, tempY].incidentLight;
                        b = l.lightMap[x - halfStride, tempY].incidentLight;

                        ratioVector = ratio(a, b);

                        if (ratioVector.X < Settings.renderThreshold || ratioVector.Y < Settings.renderThreshold || ratioVector.Z < Settings.renderThreshold)
                            lock (pointsToBeRendered)
                                pointsToBeRendered.Add(new Vector2(x, tempY));
                        else
                            l.lightMap[x, tempY].incidentLight = 0.5f * (a + b);
                    }
                    //if the y is in bounds calculate the column pixels
                    if (y < width)
                    {
                        tempX = x - halfStride;

                        a = l.lightMap[tempX, y + halfStride].incidentLight;
                        b = l.lightMap[tempX, y - halfStride].incidentLight;

                        ratioVector = ratio(a, b);

                        if (ratioVector.X < Settings.renderThreshold || ratioVector.Y < Settings.renderThreshold || ratioVector.Z < Settings.renderThreshold)
                            lock (pointsToBeRendered)
                                pointsToBeRendered.Add(new Vector2(tempX, y));
                        else
                            l.lightMap[tempX, y].incidentLight = 0.5f * (a + b);
                    }
                }
            }

            //Calculate the center pixels
            for (int x = halfStride; x < length - halfStride; x += stride)
            {
                for (int y = halfStride; y < width - halfStride; y += stride)
                {

                    a = l.lightMap[x, y + halfStride].incidentLight;
                    b = l.lightMap[x, y - halfStride].incidentLight;
                    c = l.lightMap[x + halfStride, y].incidentLight;
                    d = l.lightMap[x - halfStride, y].incidentLight;

                    ratioVector = ratio(a, b, c, d);

                    if (ratioVector.X < Settings.renderThreshold || ratioVector.Y < Settings.renderThreshold || ratioVector.Z < Settings.renderThreshold)
                        lock (pointsToBeRendered)
                            pointsToBeRendered.Add(new Vector2(x, y));
                    else
                        l.lightMap[x, y].incidentLight = 0.25f * (a + b + c + d);
                }
            }
        }
        s
        //Used to find ratios between 2 vectors (component wise)
        private static Vector3 ratio(Vector3 a, Vector3 b)
        {
            float x, y, z;

            if (a.X == 0 && b.X == 0)
                x = 1;
            else if (a.X == 0 || b.X == 0)
                x = 0;
            else
                x = Math.Min(a.X, b.X) / Math.Max(a.X, b.X);


            if (a.Y == 0 && b.Y == 0)
                y = 1;
            else if (a.Y == 0 || b.Y == 0)
                y = 0;
            else
                y = Math.Min(a.Y, b.Y) / Math.Max(a.Y, b.Y);


            if (a.Z == 0 && b.Z == 0)
                z = 1;
            else if (a.Z == 0 || b.Z == 0)
                z = 0;
            else
                z = Math.Min(a.Z, b.Z) / Math.Max(a.Z, b.Z);

            return new Vector3(x, y, z);
        }
        //Finds the ratio vectors between a,b and c,d then returns the minimum of those component wise
        private static Vector3 ratio(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            Vector3 pair1 = ratio(a, b);
            Vector3 pair2 = ratio(c, d);
            return new Vector3(Math.Min(pair1.X, pair2.X), Math.Min(pair1.Y, pair2.Y), Math.Min(pair1.Z, pair2.Z));
        }

        //Returns the textureID for the input texture file name, making a new one if needed
        public static int addTexture(String name)
        {
            // load texture data and store it
            Bitmap bmp = new Bitmap(name);
            return addTexture(bmp, name);
        }
        public static int addTexture(Bitmap bmp, String name)
        {
            if (!_textures.ContainsKey(name))
            {
                int id = GL.GenTexture();
                _textures.Add(name, id);
            }

            GL.BindTexture(TextureTarget.Texture2D, _textures[name]);

            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
            bmp.UnlockBits(bmp_data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            return _textures[name];
        }
    }
}
