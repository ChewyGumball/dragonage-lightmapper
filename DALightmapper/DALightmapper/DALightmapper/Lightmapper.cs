using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Bioware.Files;
using Bioware.Structs;
using Bioware.IO;

using Ben;

using Geometry;

namespace DALightmapper
{
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

            //Make the triangle partitioner
            Settings.stream.WriteText("Partitioning level . . . ");
            TrianglePartitioner partition = new Octree(castingTriangles);
            Settings.stream.WriteLine("Done");

            //Shoot the photons
            Settings.stream.WriteText("Firing photons with {0} threads, {1} photons per light . . . ", Settings.maxThreads, Settings.numPhotonsPerLight);
            List<Photon> photons = firePhotons(lights, partition);
            Settings.stream.WriteLine("Done");

            if (photons.Count > 0)
            {
                //Make the photon map
                Settings.stream.WriteText("Making photon map with {0} photons . . . ", photons.Count);
                PhotonPartitioner photonMap = new KDTree(photons);
                Settings.stream.WriteLine("Done");

                //Gather the photons for each patch in each map
                Settings.stream.WriteText("Gathering photons for lightmaps . . . ");
                gatherPhotons(maps, photonMap, partition, lights);
                Settings.stream.WriteLine("Done");
            }
            else
            {
                Settings.stream.WriteText("No lights affect the lightmaps, skipping to ambient occlusion.");
            }
            //Do Ambient Occlusion
            Settings.stream.WriteText("Calculating ambient occlusion . . . ");
            //calculateAmbientOcclusion(maps,partition);
            Settings.stream.WriteLine("Done");

            return maps;
        }

        //Makes lightmaps for the input model instances
        private static List<LightMap> makeLightmaps(List<ModelInstance> models)
        {
            List<LightMap> lightmapList = new List<LightMap>();
            foreach (ModelInstance model in models)
            {
                foreach (Mesh mesh in model.meshes)
                {
                    if (mesh.isLightmapped)
                    {
                        lightmapList.Add(new LightMap(model, mesh));
                    }
                }
            }

            return lightmapList;
        }

        private static List<Photon> firePhotons(List<Light> lights, TrianglePartitioner scene)
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
                        List<Photon> newPhotons = new List<Photon>();
                        Vector3 direction = l.generateRandomDirection();
                        Triangle intersectedTriangle;
                        float distance = scene.intersection(l.position, direction, out intersectedTriangle);
                        if (distance > 0)
                        {
                            Vector3 intersection = l.position + (distance * direction);
                            newPhotons.Add(new Photon(intersection, direction, l, 1.0f));
                            float totalDistance = distance;

                            //I don't actually care if its unique between threads, threadsafty on this random number is not that big of a deal
                            while (random.NextDouble() > reflectProbability)
                            {
                                direction = Vector3.Transform(direction * -1, Matrix4.CreateFromAxisAngle(intersectedTriangle.normal, (float)Math.PI));
                                distance = scene.intersection(intersection, direction, out intersectedTriangle);
                                //If we don't get a valid intersection, we are done bouncing
                                if (distance <= 0) break;

                                intersection = intersection + (distance * direction);
                                totalDistance += distance;
                                newPhotons.Add(new Photon(intersection, direction, l, 1.0f /*l.influence(totalDistance) / Settings.numPhotonsPerLight*/));
                            }

                            lock (photons)
                            {
                                photons.AddRange(newPhotons);
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
        private static void gatherPhotons(List<LightMap> maps, PhotonPartitioner photonMap, TrianglePartitioner scene, List<Light> lights)
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
                Parallel.ForEach(l.patches, opts, delegate(Patch p, ParallelLoopState state)
                {
                    if (abort)
                    {
                        state.Stop();
                    }

                    List<Photon> gather = photonMap.nearest(Settings.neighbourCount, p.position);
                    foreach (Photon photon in gather)
                    {
                        Triangle throwAway;
                        if (scene.intersection(photon.position, p.position - photon.position, out throwAway) < 1.0f)
                        {
                            p.absorbPhoton(photon);
                        }
                    }

                    foreach (Light light in lights)
                    {
                        if (!light.shootsPhotons)
                        {
                            p.ambientLight += light.influence((p.position - light.position).Length) * light.colour;
                        }
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

        private static void calculateAmbientOcclusion(List<LightMap> maps, TrianglePartitioner scene)
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
                Parallel.ForEach(l.patches, opts, delegate(Patch p, ParallelLoopState state)
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

                    for (int sample = 0; sample < 64 /*Settings.ambientOcclusionSamples*/; sample++)
                    {
                        double randomA = localGenerator.NextDouble();
                        double randomB = localGenerator.NextDouble();

                        //http://mathworld.wolfram.com/SpherePointPicking.html
                        double theta = 2 * Math.PI * randomA;
                        double phi = Math.Acos(2 * randomB - 1);

                        Vector3 randomDirection = new Vector3(Settings.ambientRayLength * (float)(Math.Cos(theta) * Math.Sin(phi)),
                                                                Settings.ambientRayLength * (float)(Math.Sin(theta) * Math.Sin(phi)),
                                                                Settings.ambientRayLength * (float)Math.Cos(phi));

                        if (Vector3.Dot(p.normal, randomDirection) < 0)
                        {
                            randomDirection = -randomDirection;
                        }

                        Triangle throwAway;
                        if (scene.intersection(p.position, randomDirection, out throwAway) > 0)
                        {
                            p.ambientOcclusion += 4;
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
