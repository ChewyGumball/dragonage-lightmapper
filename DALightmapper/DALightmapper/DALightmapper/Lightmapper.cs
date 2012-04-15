using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Bioware.Files;
using Bioware.Structs;
using Bioware.IO;

using Ben;

using Geometry;

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
            //Reset abort
            abort = false;

            Settings.stream.AppendText("Loading level . . . ");
            //Level level = ResourceManager.readLevel(path);
            Level level = new Level(path);
            if (level == null)
            {
                FinishedLightMapping.BeginInvoke(new FinishedLightMappingEventArgs("Aborted lightmapping, could not find level file " +path+".", false), null, null);
                return;
            }
            Settings.stream.AppendLine("Done");



            //-- Set up directories for lightmap files --//

            //Create the directory to store the lightmaps in
            String lightmapDirectory = Settings.tempDirectory + "\\" + Path.GetFileName(level.name);
            ResourceManager.createDirectory(lightmapDirectory);

            //Create the subdirectory where we store uncompressed lightmaps
            String uncompressedDirectory = lightmapDirectory + "\\uncompressed";
            ResourceManager.createDirectory(uncompressedDirectory);

            //Create the subdirectory where we store compressed lightmaps
            String compressedDirectory = lightmapDirectory + "\\compressed";
            ResourceManager.createDirectory(compressedDirectory);

            //Create the subdirectory where we store the atlas textures
            String atlasDirectory = lightmapDirectory + "\\atlas";
            ResourceManager.createDirectory(atlasDirectory);
                        


            //-- Get the geometry we need for lightmapping --//

            //The list of triangles which will be casting shadows, we don't care which models they come from
            List<Triangle> castingTriangles = new List<Triangle>();

            //The list of models that need lightmaps, not all models receive lightmaps
            List<ModelInstance> receivingModels = new List<ModelInstance>();

            //Find all the lightmapped models and casting triangles
            foreach (ModelInstance m in level.lightmapModels)
            {
                if (m.baseModel.isLightmapped)
                    receivingModels.Add(m);
                if (m.baseModel.castsShadows)
                    castingTriangles.AddRange(m.tris);
            }



            //-- Do the lightmapping --//

            //Make the lightmaps
            List<LightMap> maps = makeLightmaps(receivingModels);

            //Make the triangle partitioner
            Settings.stream.AppendText("Partitioning level . . . ");
            Partitioner scene = new Octree(castingTriangles);
            Settings.stream.AppendLine("Done");

            //Shoot the photons
            Settings.stream.AppendFormatText("Firing photons with {0} threads, {1} photons per light . . . ", Settings.maxThreads, Settings.numPhotonsPerLight);
            List<Photon> photons = firePhotons(level.lights, scene);            
            Settings.stream.AppendLine("Done");

            //Make the photon map
            Settings.stream.AppendFormatText("Making photon map with {0} photons . . . ", photons.Count);
            Partitioner photonMap = new Octree(photons);
            Settings.stream.AppendLine("Done");

            //Gather the photons for each patch in each map
            Settings.stream.AppendFormatText("Gathering photons for lightmaps . . . ");
            gatherPhotons(maps,photonMap);
            Settings.stream.AppendLine("Done");



            //-- Create the lightmap files --//

            Settings.stream.AppendText("Creating light map textures . . . ");
            outputLightmaps(uncompressedDirectory, maps);
            Settings.stream.AppendLine("Done");

            Settings.stream.AppendText("Compressing lightmaps . . . ");
            compressLightMaps(uncompressedDirectory, compressedDirectory);
            Settings.stream.AppendLine("Done");

            Settings.stream.AppendText("Building lightmap atlas . . . ");
            buildAtlas(compressedDirectory, atlasDirectory);
            Settings.stream.AppendLine("Done");




            //Fire the event saying lightmapping was finished completely
            FinishedLightMapping.BeginInvoke(new FinishedLightMappingEventArgs("Successfully finished light mapping.", true), null, null);
        }

        //Makes lightmaps for the input model instances
        private static List<LightMap> makeLightmaps(List<ModelInstance> models)
        {
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

        private static List<Photon> firePhotons(List<Light> lights, Partitioner scene)
        {
            Random random = new Random();
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
                            double randomNumber;

                            lock (random)
                            {
                                randomNumber = random.NextDouble();
                            }

                            while (randomNumber > reflectProbability)
                            {
                                direction = Vector3.Transform(direction * -1, Matrix4.CreateFromAxisAngle(t.normal, (float)Math.PI));
                                t = scene.firstIntersection(intersection, intersection + (direction * 1000));
                                if (t == null)
                                    break;
                                intersection = t.lineIntersectionPoint(intersection, intersection + (direction * 1000));

                                lock (random)
                                {
                                    randomNumber = random.NextDouble();
                                }
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
                    List<Photon> gather = new List<Photon>();
                    photonMap.getWithinDistanceSquared(p.position, (float)(Settings.gatherRadius * Settings.gatherRadius), ref gather);
                    foreach (Photon photon in gather)
                    {
                        p.incidentLight += photon.colour;
                    }
                    if (gather.Count > 0)
                    {
                        p.incidentLight /= gather.Count;
                    }

                    //p.incidentLight *= 100;
                    Settings.stream.UpdateProgress();
                });

                if (abort)
                {
                    FinishedLightMapping.BeginInvoke(new FinishedLightMappingEventArgs("Aborted lightmapping while gathering photons.", false), null, null);
                    throw new LightmappingAbortedException();
                }
            }
        }

        private static void outputLightmaps(String uncompressedPath, List<LightMap> lightmaps)
        {
            Settings.stream.SetProgressBarMaximum(lightmaps.Count);
            foreach (LightMap l in lightmaps)
            {
                /* for reference
                int[,] boxFilter = {
                                    {1,1,1},
                                    {1,1,1},
                                    {1,1,1}
                                   };
                 */
                int[,] gaussFilter = {
                                    {1,2,1},
                                    {2,4,2},
                                    {1,2,1}
                                   };

                Targa lightMap = l.makeLightMapTexture(uncompressedPath);
                lightMap.applyFilter(gaussFilter);
                lightMap.writeToFile();

                l.makeAmbientOcclutionTexture(uncompressedPath).writeToFile();
                l.makeShadowMapTexture(uncompressedPath).writeToFile();

                Settings.stream.UpdateProgress();
            }
        }
        private static void compressLightMaps(String uncompressedPath, String compressedPath)
        {
            String arguments = String.Format("-in_lm \"{0}\" -in_sm \"{0}\" -in_ao \"{0}\" -out \"{1}\"", uncompressedPath, compressedPath);

            ProcessStartInfo info = new ProcessStartInfo(Settings.lightmappingToolsDirectory + "\\BakedMapProcessor.exe", arguments);
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            Process.Start(info);
        }
        private static void buildAtlas(String compressedPath, String atlasPath)
        {
            String arguments = String.Format("-in \"{0}\" -out \"{1}\"", compressedPath, atlasPath);

            ProcessStartInfo info = new ProcessStartInfo(Settings.lightmappingToolsDirectory + "\\CreateAtlas.exe", arguments);
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            Process.Start(info);
        }
    }
}
