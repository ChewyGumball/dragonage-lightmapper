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
    public class Photon
    {
        public Vector3 position { get; set; }
        public Vector3 colour { get; set; }
        public Photon(Vector3 p, Vector3 c)
        {
            position = p;
            colour = c;
        }
    }
    public class FinishedLightMappingEventArgs : EventArgs
    {
        public String message { get; private set; }
        public bool successful { get; private set; }

        public FinishedLightMappingEventArgs(String m, bool success)
        {
            message = m;
            successful = success;
        }
    }
    public class LightmappingAbortedException : Exception
    {
        public LightmappingAbortedException() : base() { }
    }

    public static class Lightmapper
    {
        public static bool abort { get; set; }

        //Event to signal asynchronous lightmapping has finished
        public delegate void FinishedLightMappingEventHandler(FinishedLightMappingEventArgs args);
        public static event FinishedLightMappingEventHandler FinishedLightMapping;

        // Runs the light map process
        public static void runLightmaps(String path)
        {
            Level level = IO.readLevel(path);
            if (level == null)
            {
                FinishedLightMapping.BeginInvoke(new FinishedLightMappingEventArgs("Aborted lightmapping, no level file.", false), null, null);
                return;
            }

            List<LightMap> maps;
            List<Photon> photons;

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

            //We keep an index in the lightmap into this array so we can identify which map goes to which model
            ModelInstance[] receivingModels = receivingModelsList.ToArray();

            //Make the lightmaps
            maps = makeLightmaps(receivingModels);
            //Make the triangle partitioner
            Partitioner scene = new Octree(castingTriangles);

            //Shoot the photons
            Settings.stream.AppendFormatText("Firing photons with {0} threads, {1} photons per light . . . ", Settings.maxThreads, Settings.numPhotonsPerLight);
            photons = firePhotons(level.lights, scene);            
            Settings.stream.AppendLine("Done");

            //Make the photon map
            Settings.stream.AppendFormatText("Making photon map with {0} photons . . . ", photons.Count);
            Partitioner photonMap = new Octree(photons);
            Settings.stream.AppendLine("Done");

            //Gather the photons for each patch in each map
            Settings.stream.AppendFormatText("Gathering photons for lightmaps . . . ");
            gatherPhotons(maps,photonMap);
            Settings.stream.AppendLine("Done");

            //Make the lightmaps
            Settings.stream.SetProgressBarMaximum(maps.Count);
            Settings.stream.AppendText("Creating light map textures . . . ");
            foreach (LightMap l in maps)
            {
                l.makeIntoTexture(Settings.tempDirectory + "\\lightmaps").writeToFile();
                Settings.stream.UpdateProgress();
            }
            Settings.stream.AppendLine("Done");

            //Fire the event saying lightmapping was finished completely
            FinishedLightMapping.BeginInvoke(new FinishedLightMappingEventArgs("Successfully finished light mapping.", true), null, null);
        }

        //Makes lightmaps for the input model instances
        private static List<LightMap> makeLightmaps(ModelInstance[] models)
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
                        //Add the lightmap to the list of lightmaps
                        lightmapList.Add(new LightMap(m, i));
                    }
                }
            }

            return lightmapList;
        }

        private static List<Photon> firePhotons(Light[] lights, Partitioner scene)
        {
            int numPhotons = 0;
            foreach (Light l in lights)
            {
                if (l.shootsPhotons)
                {
                    numPhotons += Settings.numPhotonsPerLight;
                }
            }
            Settings.stream.SetProgressBarMaximum(numPhotons);

            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = Settings.maxThreads;

            List<Photon> photons = new List<Photon>();
            double reflectProbability = 0.8;

            foreach (Light l in lights)
            {
                if (l.shootsPhotons)
                {
                    Parallel.For(0, Settings.numPhotonsPerLight, opts, delegate(int i, ParallelLoopState state)
                    {
                        if (abort)
                        {
                            state.Stop();
                        }
                        Vector3 direction = l.generateRandomDirection();
                        Triangle t = scene.firstIntersection(l.position, l.position + (direction * 1000));
                        if (t != null)
                        {
                            Vector3 intersection = t.lineIntersectionPoint(l.position, l.position + (direction * 1000));
                            while (Ben.MathHelper.nextRandom() > reflectProbability)
                            {
                                direction = Vector3.Transform(direction * -1, Matrix4.CreateFromAxisAngle(t.normal, (float)Math.PI));
                                t = scene.firstIntersection(intersection, intersection + (direction * 1000));
                                if (t == null)
                                    break;
                                intersection = t.lineIntersectionPoint(intersection, intersection + (direction * 1000));
                            }

                            lock (photons)
                            {
                                photons.Add(new Photon(intersection, l.colour));
                            }
                        }
                        Settings.stream.UpdateProgress();
                    });

                    if (abort)
                    {
                        FinishedLightMapping.BeginInvoke(new FinishedLightMappingEventArgs("Aborted lightmapping while firing photons.", false), null, null);
                        throw new LightmappingAbortedException();
                    }
                }
            }
            return photons;
        }
        private static void gatherPhotons(List<LightMap> maps, Partitioner photonMap)
        {
            int numPatches = 0;
            foreach (LightMap l in maps)
            {
                numPatches += l.patches.Count;
            }
            Settings.stream.SetProgressBarMaximum(numPatches);

            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = Settings.maxThreads;

            foreach (LightMap l in maps)
            {
                Parallel.For(0, l.patches.Count, opts, delegate(int i, ParallelLoopState state)
                {
                    if (abort)
                    {
                        state.Stop();
                    }

                    Patch p = l.patches[i];
                    List<Photon> gather = photonMap.getWithinDistanceSquared(p.position, Settings.gatherRadius * Settings.gatherRadius);
                    foreach (Photon photon in gather)
                    {
                        p.incidentLight += photon.colour;
                    }
                    if (gather.Count > 0)
                    {
                        p.incidentLight /= gather.Count;
                    }
                    Settings.stream.UpdateProgress();
                });

                if (abort)
                {
                    FinishedLightMapping.BeginInvoke(new FinishedLightMappingEventArgs("Aborted lightmapping while gathering photons.", false), null, null);
                    throw new LightmappingAbortedException();
                }
            }
        }
    }
}
