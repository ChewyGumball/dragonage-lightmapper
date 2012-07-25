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

    public class LightmappingAbortedException : Exception
    {
        public LightmappingAbortedException() : base() { }
        public LightmappingAbortedException(String message) : base(message) { }
    }

    public static class Lightmapper
    {
        public static bool abort { get; set; }

        // Runs the light map process
        public static List<LightMap> runLightmaps(List<ModelInstance> models, List<Light> lights)
        {
            //Reset abort
            abort = false;       

            //-- Get the geometry we need for lightmapping --//

            //The list of triangles which will be casting shadows, we don't care which models they come from
            List<Triangle> castingTriangles = new List<Triangle>();

            //The list of models that need lightmaps, not all models receive lightmaps
            List<ModelInstance> receivingModels = new List<ModelInstance>();

            //Find all the lightmapped models and casting triangles
            foreach (ModelInstance m in models)
            {
                if (m.baseModel.isLightmapped)
                    receivingModels.Add(m);
                if (m.baseModel.castsShadows)
                    castingTriangles.AddRange(m.tris);
            }



            //-- Do the lightmapping --//

            //Make the lightmaps
            List<LightMap> maps = makeLightmaps(receivingModels);
            receivingModels.Clear();

            //Make the triangle partitioner
            Settings.stream.WriteText("Partitioning level . . . ");
            Partitioner partition = new Octree(castingTriangles);
            castingTriangles.Clear();
            Settings.stream.WriteLine("Done");

            //Shoot the photons
            Settings.stream.WriteText("Firing photons with {0} threads, {1} photons per light . . . ", Settings.maxThreads, Settings.numPhotonsPerLight);
            List<Photon> photons = firePhotons(lights, partition);            
            Settings.stream.WriteLine("Done");

            //Make the photon map
            Settings.stream.WriteText("Making photon map with {0} photons . . . ", photons.Count);
            Partitioner photonMap = new Octree(photons);
            photons.Clear();
            Settings.stream.WriteLine("Done");

            //Gather the photons for each patch in each map
            Settings.stream.WriteText("Gathering photons for lightmaps . . . ");
            gatherPhotons(maps,photonMap, partition);
            photonMap.Clear();
            Settings.stream.WriteLine("Done");

            //Do Ambient Occlusion
            Settings.stream.WriteText("Calculating ambient occlusion . . . ");
            //calculateAmbientOcclusion(maps,partition);
            Settings.stream.WriteLine("Done");

            //Do Shadow Maps
            Settings.stream.WriteText("Calculating shadows . . . ");
            //calculateShadows(maps, lights, partition);
            Settings.stream.WriteLine("Done");

            partition.Clear();

            return maps;
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
            double reflectProbability = 0.3;

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
                        int bounces = 0;
                        Vector3 direction = l.generateRandomDirection();
                        Triangle t = scene.firstIntersection(l.position, l.position + (direction * 1000));
                        if (t != null)
                        {
                            Vector3 intersection = t.lineIntersectionPoint(l.position, l.position + (direction * 1000));
                            
                            //I don't actually care if its unique between threads, threadsafty on this random number is not that big of a deal
                            while (random.NextDouble() > reflectProbability)
                            {
                                direction = Vector3.Transform(direction * -1, Matrix4.CreateFromAxisAngle(t.normal, (float)Math.PI));
                                t = scene.firstIntersection(intersection, intersection + (direction * 1000));
                                if (t == null)
                                    break;
                                intersection = t.lineIntersectionPoint(intersection, intersection + (direction * 1000));
                                bounces += 1;
                            }
                            if (l.influence(intersection) > 0)
                            {
                                lock (photons)
                                {
                                    photons.Add(new Photon(intersection, l.colour * (float)Math.Pow(Math.E, -bounces) * l.influence(intersection)));
                                }
                            }
                        }
                        Settings.stream.UpdateProgress();
                    });

                    if (abort)
                    {
                        throw new LightmappingAbortedException("Aborted lightmapping while firing photons.");
                    }
                }
            }
            return photons;
        }
        private static void gatherPhotons(List<LightMap> maps, Partitioner photonMap, Partitioner scene)
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
                    photonMap.getWithinDistance(p.position, Settings.gatherRadius, ref gather);
                    //photonMap.getNearest(p.position, 20, ref gather);
                    int unobstructedPhotons = 0;
                    foreach (Photon photon in gather)
                    {
                        if(scene.lineIsUnobstructed(photon.position, p.position))
                        {
                            unobstructedPhotons++;
                            p.incidentLight += photon.colour;
                        }
                    }
                    if (unobstructedPhotons > 0)
                    {
                        p.incidentLight /= unobstructedPhotons;
                    }

                    //p.incidentLight *= 100;
                    Settings.stream.UpdateProgress();
                });

                if (abort)
                {
                    throw new LightmappingAbortedException("Aborted lightmapping while gathering photons.");
                }
            }
        }

        private static void calculateAmbientOcclusion(List<LightMap> maps, Partitioner scene)
        {
            int numPatches = 0;
            foreach (LightMap l in maps)
            {
                numPatches += l.patches.Count;
            }
            Settings.stream.SetProgressBarMaximum(numPatches);

            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = Settings.maxThreads;

            Random generator = new Random();

            foreach (LightMap l in maps)
            {
                Parallel.For(0, l.patches.Count, opts, delegate(int i, ParallelLoopState state)
                {
                    if (abort)
                    {
                        state.Stop();
                    }

                    Random localGenerator;
                    lock (generator)
                    {
                        localGenerator = new Random(generator.Next());
                    }

                    Patch p = l.patches[i];

                    for (int sample = 0; sample < 64 /*Settings.ambientOcclusionSamples*/; sample++)
                    {
                        double randomA = localGenerator.NextDouble();
                        double randomB = localGenerator.NextDouble();

                        //http://mathworld.wolfram.com/SpherePointPicking.html
                        double theta = 2 * Math.PI * randomA;
                        double phi = Math.Acos(2*randomB - 1);

                        Vector3 randomDirection = new Vector3(  Settings.ambientRayLength * (float)(Math.Cos(theta) * Math.Sin(phi)),
                                                                Settings.ambientRayLength * (float)(Math.Sin(theta) * Math.Sin(phi)),
                                                                Settings.ambientRayLength * (float)Math.Cos(phi));

                        if (Vector3.Dot(p.normal, randomDirection) < 0)
                        {
                            randomDirection = -randomDirection;
                        }

                        if (scene.lineIsUnobstructed(p.position, p.position + randomDirection))
                        {
                            p.ambient += 4;
                        }
                    }
                    Settings.stream.UpdateProgress();
                });

                if (abort)
                {
                    throw new LightmappingAbortedException("Aborted lightmapping while gathering photons.");
                }
            }
        }

        private static void calculateShadows(List<LightMap> maps, List<Light> lights, Partitioner scene)
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

                    foreach (Light light in lights)
                    {
                        foreach (Patch p in l.patches)
                        {
                            p.inShadow = scene.lineIsUnobstructed(light.position, p.position);
                        }
                    }
                    Settings.stream.UpdateProgress();
                });

                if (abort)
                {
                    throw new LightmappingAbortedException("Aborted lightmapping while gathering photons.");
                }
            }
        }
    }
}
