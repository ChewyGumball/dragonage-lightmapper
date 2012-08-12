using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Text;
using System.IO;
using System.Linq;

using Bioware.Structs;
using Bioware.Files;
using Bioware.IO;

using Geometry;
using Ben;
using OpenTK;

namespace DALightmapper
{
    class XMLScene : Scene
    {
        private Dictionary<String, ModelInstance> modelDictionary;
        private Dictionary<String, Light> lightDictionary;

        private Dictionary<String, Dictionary<String, String>> lightmapJobs;
        private Dictionary<String, Dictionary<String, String>> shadowmapJobs;
        private Dictionary<String, Dictionary<String, String>> ambientJobs;

        private string atlasFile;

        //Directories to place lightmaps
        private String uncompressedDirectory;
        private String uncompressedAmbientDirectory;
        private String combinedDirectory;
        private String compressedDirectory;
        private String atlasDirectory;

        //-- Scene interface implementations --//
        public List<ModelInstance> lightmapModels { get { return modelDictionary.Values.ToList(); } }
        public List<Light> lights { get { return lightDictionary.Values.ToList(); } }
        public void exportLightmaps(List<LightMap> lightmaps)
        {
            Settings.stream.WriteText("Creating light map textures . . . ");
            outputLightmaps(lightmaps);
            Settings.stream.WriteLine("Done");

            Settings.stream.WriteText("Combining lightmaps . . . ");
            combineLightMaps(uncompressedDirectory, uncompressedAmbientDirectory, combinedDirectory);
            Settings.stream.WriteLine("Done");

            Settings.stream.WriteText("Building lightmap atlas . . . ");
            buildAtlas(combinedDirectory, atlasDirectory, compressedDirectory, atlasFile);
            Settings.stream.WriteLine("Done");

            Settings.stream.WriteText("Compressing lightmap atlas . . . ");
            compressAtlas(atlasDirectory, compressedDirectory);
            Settings.stream.WriteLine("Done");
        }

        public XMLScene(String path)
        {
            //This is the directory in which all job descriptions are found
            String basePath = Path.GetDirectoryName(path);
            if (Settings.atlasFile == "")
            {
                Settings.atlasFile = basePath + "\\job_atlas.xml";
            }
            atlasFile = Settings.atlasFile;
            Settings.stream.WriteLine(Verbosity.TESTING, "AtlasFile = " + atlasFile);

            if (Settings.uncompressedDirectory == "")
            {
                Settings.uncompressedDirectory = basePath + "\\TGA";
            }
            uncompressedDirectory = Settings.uncompressedDirectory;
            Settings.stream.WriteLine(Verbosity.TESTING, "uncompressedDir = " + uncompressedDirectory);

            if (Settings.ambientDirectory == "")
            {
                Settings.ambientDirectory = basePath + "\\..\\CachedScene\\AO";
            }
            uncompressedAmbientDirectory = Settings.ambientDirectory;
            Settings.stream.WriteLine(Verbosity.TESTING, "uncompressedAmbient = " + uncompressedAmbientDirectory);

            if (Settings.combinedDirectory == "")
            {
                Settings.combinedDirectory = basePath + "\\..\\PreAtlas";
            }
            combinedDirectory = Settings.combinedDirectory;
            Settings.stream.WriteLine(Verbosity.TESTING, "combined = " + combinedDirectory);

            if (Settings.compressedDirectory == "")
            {
                Settings.compressedDirectory = basePath + "\\..\\CombinedMaps";
            }
            compressedDirectory = Settings.compressedDirectory;
            Settings.stream.WriteLine(Verbosity.TESTING, "compressed = " + compressedDirectory);


            if (Settings.atlasedDirectory == "")
            {
                Settings.atlasedDirectory = basePath + "\\Atlased";
            }
            atlasDirectory = Settings.atlasedDirectory;
            Settings.stream.WriteLine(Verbosity.TESTING, "atlassed = " + atlasDirectory);

            lightmapJobs = new Dictionary<string, Dictionary<string, string>>();
            shadowmapJobs = new Dictionary<string, Dictionary<string, string>>();
            ambientJobs = new Dictionary<string, Dictionary<string, string>>();

            modelDictionary = new Dictionary<string, ModelInstance>();
            lightDictionary = new Dictionary<string, Light>();
            
            
            readScene(basePath);
            readJobs(basePath);

        }

        private void readScene(String path)
        {

            BinaryReader sceneReader = new BinaryReader(new FileStream(path + "\\job_scene.xml", FileMode.Open, FileAccess.Read, FileShare.Read));
            String sceneXML = Encoding.UTF8.GetString(sceneReader.ReadBytes((int)sceneReader.BaseStream.Length));
            sceneReader.Close();

            XmlDocument sceneSpecification = new XmlDocument();
            sceneSpecification.LoadXml(sceneXML);

            XmlNode scene = sceneSpecification.SelectSingleNode("RenderFarmScene");
            foreach (XmlNode model in scene.ChildNodes)
            {
                String modelName = model.Attributes.GetNamedItem("Source").Value;
                String pathName = Path.GetDirectoryName(modelName);
                String filename = Path.GetFileName(modelName).ToLower();
                modelName = pathName + "\\" + filename;

                ResourceManager.addFilePath(pathName);

                if (Path.GetFileName(modelName) == "prp_fertblsm_01.mmh")
                    continue;
                GFF hierarchy = ResourceManager.findFile<GFF>(modelName);
                ModelHierarchy modelHierarchy = new ModelHierarchy(hierarchy);

                Model modelMesh = modelHierarchy.mesh.toModel();

                foreach (XmlNode instance in model.ChildNodes)
                {
                    String instanceName = instance.Attributes.GetNamedItem("Name").Value;
                    Matrix4 transform = xmlToMatrix(instance.SelectSingleNode("Transform").Attributes);
                    modelDictionary.Add(instanceName, new ModelInstance(instanceName, modelMesh, transform));
                }
            }
        }
        private void readJobs(string basePath)
        {
            List<String> jobFiles = new List<String>();

            jobFiles.AddRange(Directory.GetFiles(basePath, "job_lm*"));
            jobFiles.AddRange(Directory.GetFiles(basePath, "job_sm*"));
            jobFiles.AddRange(Directory.GetFiles(basePath, "job_ao*"));

            foreach (string path in jobFiles)
            {
                processJobFile(path);
            }
        }

        private void processJobFile(string path)
        {
            BinaryReader sceneReader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
            String sceneXML = Encoding.UTF8.GetString(sceneReader.ReadBytes((int)sceneReader.BaseStream.Length));
            sceneReader.Close();

            XmlDocument sceneSpecification = new XmlDocument();
            sceneSpecification.LoadXml(sceneXML);

            List<string> shadowList = new List<string>();
            List<string> lightList = new List<string>();

            Dictionary<string, Dictionary<string, string>> jobDictionary;
            List<string> jobList;
            
            XmlNode topNode = sceneSpecification.SelectSingleNode("RenderFarmOutput");

            switch(topNode.SelectSingleNode("Output").Attributes.GetNamedItem("Type").Value)
            {
                case "LightMap": jobDictionary = lightmapJobs; jobList = lightList; break;
                case "ShadowMap": jobDictionary = shadowmapJobs; jobList = shadowList; break;
                case "AmbientOcclusion": jobDictionary = ambientJobs; jobList = lightList; break;
                default: throw new LightmappingAbortedException("Unknown job type (" + topNode.SelectSingleNode("Output").Attributes.GetNamedItem("Type").Value + ")");
            }

            foreach (XmlNode node in topNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Light":
                        processLightNode(node, jobList);
                        break;
                    case "RenderTarget":
                        processRenderTargetNode(node, jobDictionary);
                        break;
                    case "RenderOptions":
                        processRenderOptions(node);
                        break;
                    default: break; //Ignore other nodes
                }
            }

            foreach (string s in lightList)
            {
                if (lightDictionary.Keys.Contains(s))
                {
                    lightDictionary[s].inLightMap = true;
                }
            }
            foreach (string s in shadowList)
            {
                if (lightDictionary.Keys.Contains(s))
                {
                    lightDictionary[s].inShadowMap = true;
                }
            }
        }

        private void outputLightmaps(List<LightMap> lightmaps)
        {
            Settings.stream.SetProgressBarMaximum(lightmaps.Count);
            foreach (LightMap l in lightmaps)
            {
                
                int[,] boxFilter = {
                                    {1,1,1},
                                    {1,0,1},
                                    {1,1,1}
                                   };
                /*
                int[,] gaussFilter = {
                                    {0,1,2,1,0},
                                    {1,2,4,2,1},
                                    {2,4,8,4,2},
                                    {1,2,4,2,1},
                                    {0,1,2,1,0}
                                   };
                */
                int[,] gaussFilter = {
                                         {1,2,1},
                                         {2,4,2},
                                         {1,2,1}
                                   };

                String lightmapPath = lightmapJobs[l.model.name][l.mesh.name];
                Targa lightMap = l.makeLightMapTexture(Path.GetDirectoryName(lightmapPath), Path.GetFileName(lightmapPath));
                /*lightMap.grow(boxFilter);
                lightMap.grow(boxFilter);
                lightMap.grow(boxFilter);
                lightMap.grow(boxFilter);
                lightMap.grow(boxFilter);
                lightMap.grow(boxFilter);*/
                lightMap.applyFilter(gaussFilter);
                lightMap.writeToFile();

                String ambientPath = ambientJobs[l.model.name][l.mesh.name];
                l.makeAmbientOcclusionTexture(Path.GetDirectoryName(ambientPath), Path.GetFileName(ambientPath)).writeToFile();

                String shadowmapPath = shadowmapJobs[l.model.name][l.mesh.name];
                Targa shadowMap = l.makeShadowMapTexture(Path.GetDirectoryName(shadowmapPath), Path.GetFileName(shadowmapPath));
                
                /*
                shadowMap.grow(boxFilter);
                shadowMap.grow(boxFilter);
                shadowMap.grow(boxFilter);
                shadowMap.grow(boxFilter);
                shadowMap.grow(boxFilter);
                shadowMap.grow(boxFilter);
                shadowMap.grow(boxFilter);
                shadowMap.grow(boxFilter);
                shadowMap.applyFilter(gaussFilter);
                 //*/
                shadowMap.applyFilter(gaussFilter);
                shadowMap.writeToFile();

                Settings.stream.UpdateProgress();
            }
        }

        private void processLightNode(XmlNode lightNode, List<string> lightList)
        {
            string type = lightNode.Attributes.GetNamedItem("Type").Value;
            bool castsShadows = lightNode.Attributes.GetNamedItem("CastShadows").Value == "true";

            float brightness = Convert.ToSingle(lightNode.SelectSingleNode("Brightness").InnerText);
            Vector3 colour = new Vector3(Convert.ToSingle(lightNode.SelectSingleNode("Colour").Attributes.GetNamedItem("R").Value),
                                            Convert.ToSingle(lightNode.SelectSingleNode("Colour").Attributes.GetNamedItem("G").Value),
                                            Convert.ToSingle(lightNode.SelectSingleNode("Colour").Attributes.GetNamedItem("B").Value));

            Vector3 shadowColour = new Vector3(Convert.ToSingle(lightNode.SelectSingleNode("ShadowColour").Attributes.GetNamedItem("R").Value),
                                            Convert.ToSingle(lightNode.SelectSingleNode("ShadowColour").Attributes.GetNamedItem("G").Value),
                                            Convert.ToSingle(lightNode.SelectSingleNode("ShadowColour").Attributes.GetNamedItem("B").Value));

            string guid = lightNode.SelectSingleNode("GUID").Attributes.GetNamedItem("A").Value + lightNode.SelectSingleNode("GUID").Attributes.GetNamedItem("B").Value +
                            lightNode.SelectSingleNode("GUID").Attributes.GetNamedItem("C").Value + lightNode.SelectSingleNode("GUID").Attributes.GetNamedItem("D").Value;
            Matrix4 transform = xmlToMatrix(lightNode.SelectSingleNode("Transform").Attributes);
            
            switch (type)
            {
                case "Point":
                    {
                        float radius = Convert.ToSingle(lightNode.SelectSingleNode("Radius").InnerText);
                        if (!lightDictionary.ContainsKey(guid))
                        {
                            lightDictionary.Add(guid, new PointLight(transform.Row3.Xyz, colour, shadowColour, brightness, radius, true));
                        }
                    }
                    break;
                case "Spot":
                    {
                        throw new NotImplementedException("SpotLights are not implemented");
                        //newLight = new SpotLight(transform,,colour,brightness,
                    }
                    break;
                case "Ambient":
                    {
                        if (!lightDictionary.ContainsKey(guid))
                        {
                            lightDictionary.Add(guid, new AmbientLight(transform.Row3.Xyz, colour, shadowColour, brightness, false));
                        }
                    }
                    break;
                default: break; //ignore unknown light types
            }

            lightList.Add(guid);
        }
        private void processRenderTargetNode(XmlNode renderTargetNode, Dictionary<String, Dictionary<String, String>> jobDictionary)
        {
            string modelName = renderTargetNode.Attributes.GetNamedItem("Model").Value;
            string partName = renderTargetNode.Attributes.GetNamedItem("Part").Value;
            int width = Convert.ToInt32(renderTargetNode.Attributes.GetNamedItem("SizeX").Value);
            int height = Convert.ToInt32(renderTargetNode.Attributes.GetNamedItem("SizeY").Value);
            string outputName = renderTargetNode.Attributes.GetNamedItem("OutputFile").Value;

            if (modelDictionary.ContainsKey(modelName))
            {
                modelDictionary[modelName].baseModel.meshes.First(m => m.name == partName).generatePatches(width, height);
            }

            if (!jobDictionary.Keys.Contains(modelName))
            {
                jobDictionary.Add(modelName,new Dictionary<string,string>());
            }
            jobDictionary[modelName].Add(partName, outputName);
        }
        private void processRenderOptions(XmlNode options)
        {
            XmlNode ambientNode = options.FirstChild;
            Settings.ambientSamples = Convert.ToInt32(ambientNode.SelectSingleNode("MaxSamples").InnerText);
            Settings.ambientRayLength = Convert.ToSingle(ambientNode.SelectSingleNode("MaxRayLength").InnerText);
        }

        private static Matrix4 xmlToMatrix(XmlAttributeCollection matrix)
        {
            return new Matrix4(Convert.ToSingle(matrix[0].Value), Convert.ToSingle(matrix[1].Value), Convert.ToSingle(matrix[2].Value), Convert.ToSingle(matrix[3].Value),
                                Convert.ToSingle(matrix[4].Value), Convert.ToSingle(matrix[5].Value), Convert.ToSingle(matrix[6].Value), Convert.ToSingle(matrix[7].Value),
                                Convert.ToSingle(matrix[8].Value), Convert.ToSingle(matrix[9].Value), Convert.ToSingle(matrix[10].Value), Convert.ToSingle(matrix[11].Value),
                                Convert.ToSingle(matrix[12].Value), Convert.ToSingle(matrix[13].Value), Convert.ToSingle(matrix[14].Value), Convert.ToSingle(matrix[15].Value));
        }

        private static void combineLightMaps(String uncompressedPath, String ambientPath, String combinedPath)
        {
            String arguments = String.Format("-in_lm \"{0}\" -in_sm \"{0}\" -in_ao \"{1}\" -out \"{2}\"", uncompressedPath, ambientPath, combinedPath);
            Settings.stream.WriteLine(Verbosity.TESTING, "BakedMapProcessor " + arguments);

            String output = runProcess(Settings.lightmappingToolsDirectory + "\\BakedMapProcessor.exe", arguments);
            Settings.stream.WriteLine(Verbosity.TESTING, output);
        }
        private static void buildAtlas(String combinedPath, String atlasPath, String compressedPath, String atlasFile)
        {
            String arguments = String.Format("-in \"{0}\" -out \"{1}\" -width 1024 -height 1024 -gutter 0 -halftexel -in_width {2} -in_height {3} -file {4}", combinedPath, atlasPath, Settings.atlasWidth, Settings.atlasHeight, atlasFile);
            Settings.stream.WriteLine(Verbosity.TESTING, "CreateAtlas " + arguments);

            String output = runProcess(Settings.lightmappingToolsDirectory + "\\CreateAtlas.exe", arguments);
            Settings.stream.WriteLine(Verbosity.TESTING, output);

            Settings.stream.WriteLine(Verbosity.TESTING, "There are " + Directory.GetFiles(atlasPath, "*dat.xml").Count() + " dat files");
            foreach (String s in Directory.GetFiles(atlasPath, "*dat.xml"))
            {
                Settings.stream.WriteLine(Verbosity.TESTING, "Copying {0} to {1}\\{2}", s, compressedPath, Path.GetFileName(s));
                File.Copy(s, compressedPath + "\\" + Path.GetFileName(s), true);
            }
        }
        private static void compressAtlas(String atlasPath, String compressedPath)
        {
            String arguments = String.Format("-dxt5 -nomipmap -input_directory \"{0}\" -output_directory \"{1}\"", atlasPath, compressedPath);
            Settings.stream.WriteLine(Verbosity.TESTING, "TextureProcessor " + arguments);

            String output = runProcess(Settings.lightmappingToolsDirectory + "\\TextureProcessor.exe", arguments);
            Settings.stream.WriteLine(Verbosity.TESTING, output);

            Settings.stream.WriteLine(Verbosity.TESTING, "There are " + Directory.GetFiles(atlasPath, "*.meta").Count() + " meta files");
            foreach (String s in Directory.GetFiles(atlasPath, "*.meta"))
            {
                File.Copy(s, compressedPath + "\\" + Path.GetFileName(s), true);
                Settings.stream.WriteLine(Verbosity.TESTING, "Copying {0} to {1}.", s, compressedPath + "\\" + Path.GetFileName(s));
            }
        }
        private static String runProcess(String executable, String arguments)
        {
            Process p = new Process();
            p.StartInfo.Arguments = arguments;
            p.StartInfo.FileName = executable;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;

            p.Start();

            String output = p.StandardOutput.ReadToEnd();
            String error = p.StandardError.ReadToEnd();

            p.WaitForExit();
            p.Close();
            return "OUTPUT: " + output + "\n ERROR: " + error;
        }
    }

}
