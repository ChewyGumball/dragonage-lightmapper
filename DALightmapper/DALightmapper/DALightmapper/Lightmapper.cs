using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Bioware.Files;
using Bioware.Structs;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

using Ben;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace DALightmapper
{
    class Photon
    {
        public Vector3 position { get; set; }
        public Vector3 colour { get; set; }
        public Photon(Vector3 p, Vector3 c)
        {
            position = p;
            colour = c;
        }
    }
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
        public static bool abort { get; set; }

        //Event to signal asynchronous lightmapping has finished
        public delegate void FinishedLightMappingEventHandler(FinishedLightMappingEventArgs args);
        public static event FinishedLightMappingEventHandler FinishedLightMapping;

        // Runs the light map process
        public static void runLightmaps(Level level)
        {

            Patch[] patches;
            LightMap[] maps;
            List<Photon> photons = new List<Photon>(); ;

            //The list of triangles which will be casting shadows
            //  This comes from the models which cast shadows, we don't care which models they come from
            List<Triangle> castingTriangles = new List<Triangle>();

            //The list of models that need lightmaps
            List<ModelInstance> receivingModelsList = new List<ModelInstance>();

            foreach (ModelInstance m in level.lightmapModels)
            {
                if (m.baseModel.isLightmapped)
                    receivingModelsList.Add(m);
                if (m.baseModel.castsShadows)
                    castingTriangles.AddRange(m.tris);
            }

            ModelInstance[] receivingModels = receivingModelsList.ToArray();

            //Make the lightmaps and patch instances
            makeLightmaps(receivingModels, out maps, out patches);
            Partitioner scene = new Octree(castingTriangles);
            Settings.stream.AppendFormatLine("There are {0} patches in {1} lightmaps with {2}/{3} tris in octree.", patches.Length, maps.Length,Settings.tris,castingTriangles.Count);

            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = Settings.maxThreads;

            double reflectProbability = 0.5;

            foreach (Light l in level.lights)
            {
                if (l.shootsPhotons)
                {
                    for (int i = 0; i < Settings.numPhotonsPerLight; i++)
                    //Parallel.For(0, Settings.numPhotonsPerLight, delegate(int i)
                    {
                        Vector3 direction = l.generateRandomDirection();
                        //Settings.stream.AppendFormatLine("{0},{1},{2}", direction.X, direction.Y, direction.Z);
                        Triangle t = scene.firstIntersection(l.position, l.position + (direction * 1000));
                        if (t != null)
                        {
                            Settings.stream.AppendFormatLine("{0}", i);
                            Vector3 intersection = t.lineIntersectionPoint(l.position, l.position + (direction * 1000));
                            while (Ben.MathHelper.nextRandom() > reflectProbability)
                            {
                                direction = Vector3.Transform(direction * -1, Matrix4.CreateFromAxisAngle(t.normal, (float)Math.PI));
                                t = scene.firstIntersection(intersection, direction);
                                if (t == null)
                                    break;
                                intersection = t.lineIntersectionPoint(intersection, intersection + (direction * 1000));
                            }

                            lock (photons)
                            {
                                photons.Add(new Photon(intersection, l.colour * (l.intensity / Settings.numPhotonsPerLight)));
                            }
                        }
                    }//);
                }
            }
                       
            
            //If the loop exited early fire the event saying so
            if (abort)
            {
                FinishedLightMapping.BeginInvoke(new FinishedLightMappingEventArgs("Aborted lightmapping early."), null, null);
            }
            //Otherwise fire the event saying lightmapping was finished completely
            else
            {
                foreach (LightMap l in maps)
                {
                    makeIntoTexture(l);
                }
                FinishedLightMapping.BeginInvoke(new FinishedLightMappingEventArgs("Successfully finished light mapping."), null, null);
            }
        }
        private static int GetObjectSize(object TestObject)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            byte[] Array;
            bf.Serialize(ms, TestObject);
            Array = ms.ToArray();
            return Array.Length;
        }
        //Makes lightmaps for the input model instances
        private static void makeLightmaps(ModelInstance[] models, out LightMap[] maps, out Patch[] patches)
        {
            List<Patch> patchList = new List<Patch>();
            List<LightMap> lightmapList = new List<LightMap>();
            //Make lightmaps
            foreach (ModelInstance m in models)
            {
                //for each mesh in the model instance
                for (int i = 0; i < m.baseModel.meshes.Length; i++)
                {
                    if (m.baseModel.meshes[i].isLightmapped)
                    {
                        //Make the lightmap
                        LightMap temp = new LightMap(m, i);
                        //For each patch instance in the lightmap
                        foreach (Patch p in temp.patches)
                        {
                            patchList.Add(p);
                        }
                        //Add the lightmap to the list of lightmaps
                        lightmapList.Add(temp);
                    }
                }
            }

            maps = lightmapList.ToArray();
            patches = patchList.ToArray();
        }

        
    
        private static void makeIntoTexture(LightMap l)
        {
            l.makeIntoTexture(Settings.tempDirectory + "\\lightmaps").writeToFile();
        }
    }
}
