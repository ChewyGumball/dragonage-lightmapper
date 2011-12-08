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
            Settings.stream.AppendLine("Done making visible sets.");
            //Initialize the loop counter so message can say how many bounces were rendered
            int currentBounce = 0;

            //Start the lightmapping process off by just rendering light sources
            renderLights(level.lights,patches,scene);
            Settings.stream.AppendLine("Done rendering lights.");
            //Do some lightmapping
            for(abort = false; currentBounce < Settings.numBounces; currentBounce ++)
            {
                //Find patch with highest exident light
                int highestIndex = 0;

                for (int i = 1; i < patches.Length; i++)
                {
                    if (patches[i].excidentLight.Length > patches[highestIndex].excidentLight.Length)
                    {
                        highestIndex = i;
                    }
                }
                //Propogate it's light
                propogate(patches[highestIndex], patches,scene);
            }
            
            
            //If the loop exited early fire the event saying so
            if (abort)
            {
                FinishedLightMapping.BeginInvoke(new FinishedLightMappingEventArgs("Aborted lightmapping after " + currentBounce + " bounces."), null, null);
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

        //Makes a list of visible sets for each patch based on the input models
        private static Patch[] makeVisibleSet(Patch patch, Patch[] patchList, Partitioner scene)
        {
            List<Patch> visiblePatches = new List<Patch>();
            //List<ModelInstance> intersectionTests = new List<ModelInstance>();

            foreach (Patch p in patchList)
            {
                //Ignore the patch we are making the set for
                if(p != patch)
                {
                    //Do simple facing test, < 0 means the direction to the patch is on the side of the patch we want
                    Vector3 vectorToPatch = p.position - patch.position;
                    if (Vector3.Dot(patch.normal, vectorToPatch) > 0)
                    {
                        if (scene.lineIsUnobstructed(patch.position, p.position))
                        {
                            visiblePatches.Add(p);
                        }
                    }
                }
            }

            //Patch[] visibles = new Patch[visiblePatches.Count];
            //visiblePatches.CopyTo(visibles);
            visiblePatches.TrimExcess();
            return visiblePatches.ToArray();
        }

        private static double[] calculateCoefficients(Patch patch, Patch[] visibleSet)
        {
            double[] coeffieients = new double[visibleSet.Length];
            for (int i = 0; i < visibleSet.Length; i++)
            {
                Vector3 ray = visibleSet[i].position - patch.position;
                //cosine law for emitted energy
                double cosineStart = Math.Cos(Vector3.CalculateAngle(ray, patch.normal));

                //falloff due to distance
                double falloff = 1 / (Math.Pow(ray.Length, 2));

                //cosine law for received energy
                ray = patch.position - visibleSet[i].position;
                double cosineEnd = Math.Cos(Vector3.CalculateAngle(ray, visibleSet[i].normal));
                
                coeffieients[i] = cosineStart * falloff * cosineEnd;
            }

            return coeffieients;
        }

        //Propogates light from a to a's visible set
        private static void propogate(Patch a, Patch[] patchList, Partitioner scene)
        {
            Patch[] visibleSet = makeVisibleSet(a, patchList, scene);
            double[] coefficients = calculateCoefficients(a, visibleSet);
            for (int i = 0; i < visibleSet.Length; i++)
            {
                visibleSet[i].incidentLight += Vector3.Multiply(a.excidentLight, (float)coefficients[i]);
            }
            a.incidentLight = new Vector3();
        }

        //Initializes patches to the colour received only from visible lights to start the light mapping process
        private static void renderLights(Light[] lights, Patch[] patches, Partitioner scene)
        {

            for (int i = 0; i < lights.Length; i++)
            {
                Patch lightPatch = new Patch(lights[i].position, new Vector3(), lights[i].intensity * lights[i].colour, new Vector3());
                List<Patch> visiblePatches = new List<Patch>();

                foreach (Patch p in patches)
                {
                    Vector3 vectorToPatch = lightPatch.position - p.position;
                    if (Vector3.Dot(p.normal, vectorToPatch) > 0)
                    {
                        if (scene.lineIsUnobstructed(lightPatch.position, p.position))
                        {
                            visiblePatches.Add(p);
                        }
                    }
                }

                Patch[] visibleArray = visiblePatches.ToArray();
                propogate(lightPatch,visibleArray,scene);
            }        
        }

        private static double[] calculateLightCoefficients(Light light, Patch[] visibleArray)
        {
            double[] coefficients = new double[visibleArray.Length];
            for (int i = 0; i < coefficients.Length; i++)
            {
                //THIS SHOULD TAKE INTO ACCOUNT PATCH NORMAL OR SOMETHING
                coefficients[i] = light.influence(visibleArray[i]);
            }
            return coefficients;
        }
    
        private static void makeIntoTexture(LightMap l)
        {
            l.makeIntoTexture(Settings.tempDirectory + "\\lightmaps").writeToFile();
        }
    }
}
