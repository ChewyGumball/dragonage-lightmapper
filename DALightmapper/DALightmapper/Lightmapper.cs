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
        static Level _level;
        static bool _finished;  //Used to abort the lightmapping process prematurely
        static Dictionary<String, int> _textures = new Dictionary<string, int>();

        //Holds the vector arrays representing a texture - use vectors as they are easier to work with than a OpenGL texture
        //  They are converted into textures after the mapping process is completed or aborted
        static List<LightMap> _maps = new List<LightMap>();

        //Holds the points which need to have a hemicube rendering because their neighbouring pixels are too dissimilar
        static List<Vector2> pointsToBeRendered = new List<Vector2>();

        public static Level level
        {
            get { return _level; }
            set { _level = value; }
        }
        public static bool finished
        {
            get { return _finished; }
            set { _finished = value; }
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
        public static void runLightmaps()
        {
            //Clear the points to be rendered
            pointsToBeRendered.Clear();

            //Make all the maps
            foreach(BiowareModel m in level.models)
            {
                foreach(MeshChunk mc in m.mesh.meshChunks)
                {
                    if (mc.receives)
                    {
                        _maps.Add(new LightMap(m, mc));
                    }
                }
            }

            int counter;
            //Fill in the maps
            foreach (LightMap l in _maps)
            {
                counter = 0;
                for (int i = 0; i < l.dimension; i++)
                {
                    for (int j = 0; j < l.dimension; j++)
                    {
                        Vector2 currentPlace = new Vector2((float)i/l.dimension, (float)j/l.dimension);
                        l.lightMap[i, j] = new Patch();
                        for (int k = 0; k < l.meshChunk.tris.Length; k++)
                        {
                            Triangle t = l.meshChunk.tris[k];
                            if (t.isUVOnThisTriangle(currentPlace))
                            {
                                l.lightMap[i, j] = new Patch(t.uvTo3d(currentPlace), t.normal, new Vector3(0, 0, 0), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                                counter++;
                                break;
                            }
                        }
                    }
                }
                Console.WriteLine("For lightmap {0} {1} there were {2} active patches.", l.model.modelFileName, l.meshChunk.name, counter);
            }


            //Initialize the loop counter so message can say how many bounces were rendered
            int currentBounce = 0;
            int stride;

            //Start the lightmapping process off by just rendering light sources
            renderLights();
            if (false)
            {
                //Do some more lightmapping
                for (finished = false; currentBounce < Settings.numBounces && !finished; currentBounce++)
                {
                    //go through all the maps
                    foreach (LightMap l in maps)
                    {
                        //Render the initial pixels 
                        renderInitialPixels(l);

                        //Start lerping pixels, rendering those which need renders, then halving the stride and doing it again
                        //  until all the pixels are filled or the lightmapping is aborted early
                        for (stride = Settings.lerpStartStride; stride > 1 && !finished; stride /= 2)
                        {
                            lerp(l, stride);
                            renderPoints(l);
                        }
                    }
                    foreach (LightMap l in maps)
                    {
                        makeIntoTexture(l);
                    }
                }
            }
            //If the loop exited early fire the event saying so
            if (finished)
            {
                FinishedLightMapping.BeginInvoke(new FinishedLightMappingEventArgs("Aborted lightmapping after " + 4/*currentBounce*/ + " bounces."), null, null);
            }
            //Otherwise fire the event saying lightmapping was finished completely
            else
            {
                FinishedLightMapping.BeginInvoke(new FinishedLightMappingEventArgs("Successfully finished light mapping."), null, null);
            }
        }

        //Initializes pixels to the colour received only from visible lights to start the light mapping process
        private static void renderLights()
        {
            foreach (LightMap l in _maps)
            {
                //For each patch in the lightmap
                for (int i = 0; i < l.dimension; i++)
                {
                    for (int j = 0; j < l.dimension; j++)
                    {
                        //incident light at (i,j) = (numIntensities/totalIntensity) * (sum from 1 to numIntensities (intensityI * colourI))
                        float totalIntensity = 0;
                        int numIntensities = 0;

                        Vector2 currentPlace = new Vector2(i, j);
                        //Check all the lights to see if they influence the patch
                        foreach (Light light in _level.lights)
                        {
                            float influence = light.influence(l.lightMap[i,j].position + l.model.position);
                            //Check if the influence is > 0
                            if (influence > 0)
                            {
                                //If there are no other triangles in front of it
                                //if()()()()
                                //fill in the pixel
                                l.lightMap[i, j].incidentLight += influence * light.colour;

                                //add the intensity to the total
                                totalIntensity += influence;
                                // Increase the number of intensities
                                numIntensities++;
                            }
                        }
                        //Find the other factor in the colour calculation
                        totalIntensity = (float)numIntensities / totalIntensity;
                        //Multiply it into the colour
                        l.lightMap[i, j].incidentLight *= totalIntensity;
                    }
                }
                makeIntoTexture(l);
            }
            makeIntoTexture(maps[0]);
        }

        private static void makeIntoTexture(LightMap l)
        {
            Vector3 currentPixel;
            Bitmap newBitmap = new Bitmap(l.lightMap.GetLength(0), l.lightMap.GetLength(1),System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            for(int i = 0;i<l.dimension;i++)
                for (int j = 0; j < l.dimension; j++)
                {
                    currentPixel = l.lightMap[i,j].incidentLight;
                    newBitmap.SetPixel(i, j, Color.FromArgb((int)currentPixel.X, (int)currentPixel.Y, (int)currentPixel.Z));
                }
            
            l.textureID = addTexture(newBitmap, l.model.modelID + l.meshChunk.name);
        }

        //Renders pixels to give the lerp process a starting point
        private static void renderInitialPixels(LightMap l)
        {
            for (int x = 0; x < l.dimension; x += Settings.lerpStartStride)
            {
                for (int y = 0; y < l.dimension; y += Settings.lerpStartStride)
                {
                    renderPoint(new Vector2(x, y),l);
                }
            }
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

        //Calculates and saves the value of pixels which need hemicubes rendered
        private static void renderPoints(LightMap l)
        {
            while (pointsToBeRendered.Count > 0)
            {
                renderPoint(pointsToBeRendered[0],l);
                pointsToBeRendered.RemoveAt(0);
            }
        }

        //Renders a hemicube at the specified point on the light map and stores the combined colour value at that pixel
        private static void renderPoint(Vector2 point, LightMap l)
        {
        }

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
