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
        public static bool abort { get; set; }

        //Event to signal asynchronous lightmapping has finished
        public delegate void FinishedLightMappingEventHandler(FinishedLightMappingEventArgs args);
        public static event FinishedLightMappingEventHandler FinishedLightMapping;

        // Runs the light map process
        public static void runLightmaps(Level level)
        {

            PatchInstance[] patches;
            PatchInstance[][] visibleSets;
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
            visibleSets = new PatchInstance[patches.Length][];
            coefficients = new double[patches.Length][];
            for (int i = 0; i < patches.Length; i++)
            {
                visibleSets[i] = makeVisibleSet(castingModels, patches[i], patches);
                coefficients[i] = calculateCoefficients(patches[i], visibleSets[i]);
            }

            //Initialize the loop counter so message can say how many bounces were rendered
            int currentBounce = 0;

            //Start the lightmapping process off by just rendering light sources
            renderLights(level.lights,patches,castingModels); 

            //Do some lightmapping
            for(abort = false; currentBounce < Settings.numBounces && false; currentBounce ++)
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
                propogate(patches[highestIndex],visibleSets[highestIndex],coefficients[highestIndex]);
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

        //Makes lightmaps for the input model instances
        private static void makeLightmaps(ModelInstance[] models, out LightMap[] maps, out PatchInstance[] patches)
        {
            List<PatchInstance> patchList = new List<PatchInstance>();
            List<LightMap> lightmapList = new List<LightMap>();
            //Make lightmaps
            foreach (ModelInstance m in models)
            {
                //for each mesh in the model instance
                for (int i = 0; i < m.baseModel.meshes.Length; i++)
                {
                    //Make the lightmap
                    LightMap temp = new LightMap(m, i);
                    //For each patch instance in the lightmap
                    foreach (PatchInstance p in temp.patches)
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

            maps = lightmapList.ToArray();
            patches = patchList.ToArray();
        }

        //Makes a list of visible sets for each patch based on the input models
        private static PatchInstance[] makeVisibleSet(ModelInstance[] models, PatchInstance patch, PatchInstance[] patchList)
        {
            List<PatchInstance> visiblePatches = new List<PatchInstance>();
            List<ModelInstance> intersectionTests = new List<ModelInstance>();

            foreach (PatchInstance p in patchList)
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

        private static double[] calculateCoefficients(PatchInstance patch, PatchInstance[] visibleSet)
        {
            return new double[visibleSet.Length];
        }

        //Propogates light from a to a's visible set
        private static void propogate(PatchInstance a, PatchInstance[] visibleSet, double[] coefficients)
        {
        }

        //Initializes patches to the colour received only from visible lights to start the light mapping process
        private static void renderLights(Light[] lights, PatchInstance[] patches, ModelInstance[] castingModels)
        {

            for (int i = 0; i < lights.Length; i++)
            {
                Patch lightPatch = new Patch(new Vector2(), lights[i].position,new Vector3(), lights[i].intensity * lights[i].colour, new Vector3());
                PatchInstance currentLight = new PatchInstance(lightPatch, new Vector3(), new Quaternion());
                List<ModelInstance> intersectionTests = new List<ModelInstance>();
                List<PatchInstance> visiblePatches = new List<PatchInstance>();

                bool visible = true;

                foreach (PatchInstance p in patches)
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

                PatchInstance[] visibleArray = visiblePatches.ToArray();
                propogate(currentLight,visibleArray,calculateLightCoefficients(lights[i],visibleArray));
            }        
        }

        private static double[] calculateLightCoefficients(Light light, PatchInstance[] visibleArray)
        {
            double[] coefficients = new double[visibleArray.Length];
            for (int i = 0; i < coefficients.Length; i++)
            {
                //THIS SHOULD TAKE INTO ACCOUNT PATCH NORMAL OR SOMETHING
                coefficients[i] = light.influence(visibleArray[i].position);
            }
            return coefficients;
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

            length = l.width;
            width = l.height;

            for (int x = halfStride; x < length - halfStride; x += stride)
            {
                for (int y = halfStride; y < width - halfStride; y += stride)
                {
                    //If the x is in bounds calculate the row pixels
                    if (x < length)
                    {
                        //tempY = y - halfStride;

                        //a = l.lightmap[x + halfstride, tempy].incidentlight;
                        //b = l.lightMap[x - halfStride, tempY].incidentLight;

                        //ratioVector = ratio(a, b);

                        //if (ratioVector.X < Settings.renderThreshold || ratioVector.Y < Settings.renderThreshold || ratioVector.Z < Settings.renderThreshold)
                        //    lock (pointsToBeRendered)
                        //        pointsToBeRendered.Add(new Vector2(x, tempY));
                        //else
                        //    l.lightMap[x, tempY].incidentLight = 0.5f * (a + b);
                    }
                    //if the y is in bounds calculate the column pixels
                    if (y < width)
                    {
                        //tempX = x - halfStride;

                        //a = l.lightMap[tempX, y + halfStride].incidentLight;
                        //b = l.lightMap[tempX, y - halfStride].incidentLight;

                        //ratioVector = ratio(a, b);

                        //if (ratioVector.X < Settings.renderThreshold || ratioVector.Y < Settings.renderThreshold || ratioVector.Z < Settings.renderThreshold)
                        //    lock (pointsToBeRendered)
                        //        pointsToBeRendered.Add(new Vector2(tempX, y));
                        //else
                        //    l.lightMap[tempX, y].incidentLight = 0.5f * (a + b);
                    }
                }
            }

            //Calculate the center pixels
            for (int x = halfStride; x < length - halfStride; x += stride)
            {
                for (int y = halfStride; y < width - halfStride; y += stride)
                {

                    //a = l.lightMap[x, y + halfStride].incidentLight;
                    //b = l.lightMap[x, y - halfStride].incidentLight;
                    //c = l.lightMap[x + halfStride, y].incidentLight;
                    //d = l.lightMap[x - halfStride, y].incidentLight;

                    //ratioVector = ratio(a, b, c, d);

                    //if (ratioVector.X < Settings.renderThreshold || ratioVector.Y < Settings.renderThreshold || ratioVector.Z < Settings.renderThreshold)
                    //    lock (pointsToBeRendered)
                    //        pointsToBeRendered.Add(new Vector2(x, y));
                    //else
                    //    l.lightMap[x, y].incidentLight = 0.25f * (a + b + c + d);
                }
            }
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

    }
}
