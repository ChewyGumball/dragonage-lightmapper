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
            Patch[][] visibleSets;
            double[][] coefficients;
            LightMap[] maps;

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
            Settings.stream.AppendFormatLine("There are {0} patches in {1} lightmaps.", patches.Length, maps.Length);
            //Make visible set for each patch
            visibleSets = new Patch[patches.Length][];
            coefficients = new double[patches.Length][];
            
            /*
            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = Settings.maxThreads;
            Parallel.For(0, patches.Length,opts, delegate(int i)
            {
                visibleSets[i] = makeVisibleSet(patches[i], patches, scene);
                if ((i % 100) == 0)
                {
                    System.GC.Collect();
                    Settings.stream.AppendFormatLine("Done making visible set for {0} patches.", i);
                }

            }
            );
            //*/
            /*
            Task<Patch[]>[] calculations = new Task<Patch[]>[patches.Length];
            for (int i = 0; i < patches.Length; i++)
            {
                Patch patch = patches[i];
                calculations[i] = new Task<Patch[]>(() => makeVisibleSet(patch,patches,scene));
                calculations[i].Start();
            }

            for (int i = 0; i < patches.Length; i++)
            {
                visibleSets[i] = calculations[i].Result;
                if((i % 100) == 0)
                    Settings.stream.AppendFormatLine("Done making visible set for {0} patches.", Verbosity.Medium, i);
            }
            // */
            
            
            
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
