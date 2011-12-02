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

            List<ModelInstance> castingModelsList = new List<ModelInstance>();
            List<ModelInstance> receivingModelsList = new List<ModelInstance>();

            foreach (ModelInstance m in level.lightmapModels)
            {
                if (m.baseModel.isLightmapped)
                    receivingModelsList.Add(m);
                if (m.baseModel.castsShadows)
                    castingModelsList.Add(m);
            }

            ModelInstance[] receivingModels = receivingModelsList.ToArray();
            ModelInstance[] castingModels = castingModelsList.ToArray();

            //Make the lightmaps and patch instances
            makeLightmaps(receivingModels, out maps, out patches);
            System.Console.WriteLine("There are {0} patches in {1} lightmaps.", patches.Length, maps.Length);

            //Make visible set for each patch
            visibleSets = new Patch[patches.Length][];
            coefficients = new double[patches.Length][];

            Task<Patch[]>[] calculations = new Task<Patch[]>[patches.Length];
            /*
            Parallel.For(0, patches.Length, delegate(int i)
            {
                visibleSets[i] = makeVisibleSet(castingModels, patches[i], patches);
            }
            );
            //*/
            //*
            ThreadPool.SetMaxThreads(4, 4);
            for (int i = 0; i < 1000; i++)
            {
                Patch patch = patches[i];
                calculations[i] = new Task<Patch[]>(() => makeVisibleSet(castingModels,patch,patches));
                calculations[i].Start();
            }

            for (int i = 0; i < 1000; i++)
            {
                visibleSets[i] = calculations[i].Result;
                if((i % 100) == 0)
                    System.Console.WriteLine("Done with {0}.", i);
            }
            // */
            System.Console.WriteLine("Done making coeffients.");
            Environment.Exit(0);
            //Initialize the loop counter so message can say how many bounces were rendered
            int currentBounce = 0;

            //Start the lightmapping process off by just rendering light sources
            renderLights(level.lights,patches,castingModels);
            System.Console.WriteLine("Done rendering lights.");
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
                coefficients[highestIndex] = calculateCoefficients(patches[highestIndex], visibleSets[highestIndex]);
                propogate(patches[highestIndex],visibleSets[highestIndex],coefficients[highestIndex]);
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
                            //If its an active patch add it to the list of patches, otherwise ignore
                            if (p.isActive)
                            {
                                patchList.Add(p);
                            }
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
        private static Patch[] makeVisibleSet(ModelInstance[] models, Patch patch, Patch[] patchList)
        {
            List<Patch> visiblePatches = new List<Patch>();
            List<ModelInstance> intersectionTests = new List<ModelInstance>();

            foreach (Patch p in patchList)
            {
                //Ignore the patch we are making the set for
                if(p != patch)
                {
                    bool visible = true;
                    intersectionTests.Clear();
                    //Do simple facing test, < 0 means the direction to the patch is on the side of the patch we want
                    Vector3 vectorToPatch = p.position - patch.position;
                    if (Vector3.Dot(patch.normal, vectorToPatch) > 0)
                    {
                        //Do complicated test, go through all the models and see if something is in the way

                        //First do bounding box check
                        foreach (ModelInstance model in models)
                        {
                            foreach (BoundingBox bounds in model.bounds)
                            {
                                if (bounds.containsLine(patch.position, p.position) || bounds.lineIntersects(patch.position, p.position))
                                {
                                    intersectionTests.Add(model);
                                }
                            }
                        }
                        //Then do triangle by triangle
                        for (int a = 0;a < intersectionTests.Count && visible; a++) 
                        {
                            ModelInstance model = intersectionTests[a];
                            for (int i = 0; i < model.getNumMeshes() && visible; i++)
                            {
                                for (int j = 0; j < model.baseModel.meshes[i].getNumTris() && visible; j++)
                                {
                                    if (model.getTri(i, j).lineIntersects(patch.position, p.position))
                                    {
                                        visible = false;
                                    }
                                }
                            }
                        }
                        if (visible)
                        {
                            visiblePatches.Add(p);
                        }
                    }
                }
            }
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
        private static void propogate(Patch a, Patch[] visibleSet, double[] coefficients)
        {
            for (int i = 0; i < visibleSet.Length; i++)
            {
                visibleSet[i].excidentLight += Vector3.Multiply(a.excidentLight, (float)coefficients[i] * 30);
            }
            a.excidentLight = new Vector3();
        }

        //Initializes patches to the colour received only from visible lights to start the light mapping process
        private static void renderLights(Light[] lights, Patch[] patches, ModelInstance[] castingModels)
        {

            for (int i = 0; i < lights.Length; i++)
            {
                Patch lightPatch = new Patch(new Vector2(), lights[i].position, new Vector3(), lights[i].intensity * lights[i].colour, new Vector3());
                Patch currentLight = new Patch(lightPatch, new Vector3(), new Quaternion());
                List<ModelInstance> intersectionTests = new List<ModelInstance>();
                List<Patch> visiblePatches = new List<Patch>();

                bool visible = true;

                foreach (Patch p in patches)
                {
                    //First do bounding box check
                    foreach (ModelInstance model in castingModels)
                    {
                            foreach (BoundingBox bounds in model.bounds)
                            {
                                if (bounds.containsLine(currentLight.position, p.position) || bounds.lineIntersects(currentLight.position, p.position))
                                {
                                    intersectionTests.Add(model);
                                }
                            }
                    }
                    //Then do triangle by triangle
                    for (int a = 0; a < intersectionTests.Count && visible; a++)
                    {
                        ModelInstance model = intersectionTests[a];
                        for (int d = 0; d < model.getNumMeshes() && visible; d++)
                        {
                            for (int j = 0; j < model.baseModel.meshes[d].getNumTris() && visible; j++)
                            {
                                if (model.getTri(d, j).lineIntersects(currentLight.position, p.position))
                                {
                                    visible = false;
                                }
                            }
                        }
                    }
                    if (visible)
                    {
                        visiblePatches.Add(p);
                    }
                }

                Patch[] visibleArray = visiblePatches.ToArray();
                propogate(currentLight,visibleArray,calculateLightCoefficients(lights[i],visibleArray));
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
