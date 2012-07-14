using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

using Bioware.Structs;
using Bioware.Files;
using Bioware.IO;

using Geometry;

namespace DALightmapper
{
    public class LevelScene : Scene
    {
        //-- Scene interface implementations --//
        public List<ModelInstance> lightmapModels
        {
            get { return level.lightmapModels; }
        }
        public List<Light> lights
        {
            get { return level.lights; }
        }
        public void exportLightmaps(List<LightMap> lightmaps)
        {
            Settings.stream.WriteText("Creating light map textures . . . ");
            outputLightmaps(uncompressedDirectory, uncompressedAmbientDirectory, lightmaps);
            Settings.stream.WriteLine("Done");

            Settings.stream.WriteText("Compressing lightmaps . . . ");
            compressLightMaps(uncompressedDirectory, uncompressedAmbientDirectory, compressedDirectory);
            Settings.stream.WriteLine("Done");

            Settings.stream.WriteText("Building lightmap atlas . . . ");
            buildAtlas(compressedDirectory, atlasDirectory);
            Settings.stream.WriteLine("Done");
        }


        private Level level;

        //Directories to place lightmaps
        private String uncompressedDirectory;
        private String uncompressedAmbientDirectory;
        private String compressedDirectory;
        private String atlasDirectory;

        public LevelScene(String path)
        {
            Settings.stream.WriteText("Loading level . . . ");
            //Level level = ResourceManager.readLevel(path);
            Settings.stream.WriteLine("GET AMBIENT SETTINGS~!~!~!~!~!");
            level = new Level(path);
            if (level == null)
            {
                throw new LightmappingAbortedException("The file " + path + " was not found.");
            }
            Settings.stream.WriteLine("Done");


            foreach(Light l in lights)
            {
                Settings.stream.WriteLine("{0}: {1}", l.position, l.intensity);
            }

            //-- Set up directories for lightmap files --//

            if (Settings.allPathsSpecified)
            {
                uncompressedDirectory = Settings.uncompressedDirectory;
                uncompressedAmbientDirectory = Settings.ambientDirectory;
                compressedDirectory = Settings.compressedDirectory;
                atlasDirectory = Settings.atlasedDirectory;
            }
            else
            {
                //Create the directory to store the lightmaps in
                String lightmapDirectory = Settings.tempDirectory + "\\" + Path.GetFileName(level.name);
                ResourceManager.createDirectory(lightmapDirectory);

                //Create the subdirectory where we store uncompressed lightmaps
                uncompressedDirectory = lightmapDirectory + "\\uncompressed";
                ResourceManager.createDirectory(uncompressedDirectory);

                //Ambient Occlusion directory is the same as the uncompressed directory
                uncompressedAmbientDirectory = uncompressedDirectory;

                //Create the subdirectory where we store compressed lightmaps
                compressedDirectory = lightmapDirectory + "\\compressed";
                ResourceManager.createDirectory(compressedDirectory);

                //Create the subdirectory where we store the atlas textures
                atlasDirectory = lightmapDirectory + "\\atlas";
                ResourceManager.createDirectory(atlasDirectory);
            }
        }

        private static void outputLightmaps(String uncompressedPath, String ambientPath, List<LightMap> lightmaps)
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

                l.makeAmbientOcclusionTexture(ambientPath).writeToFile();
                l.makeShadowMapTexture(uncompressedPath).writeToFile();

                Settings.stream.UpdateProgress();
            }
        }
        private static void compressLightMaps(String uncompressedPath, String ambientPath, String compressedPath)
        {
            String arguments = String.Format("-in_lm \"{0}\" -in_sm \"{0}\" -in_ao \"{1}\" -out \"{2}\"", uncompressedPath, ambientPath, compressedPath);
            ProcessStartInfo info = new ProcessStartInfo(Settings.lightmappingToolsDirectory + "\\BakedMapProcessor.exe", arguments);
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            Process.Start(info);
        }
        private static void buildAtlas(String compressedPath, String atlasPath)
        {
            String arguments = String.Format("-in \"{0}\" -out \"{1}\" -in_width {2} -in_height {3}", compressedPath, atlasPath, Settings.atlasWidth, Settings.atlasHeight);
            if (Settings.atlasFile != "")
            {
                arguments += " -file " + Settings.atlasFile;
            }
            ProcessStartInfo info = new ProcessStartInfo(Settings.lightmappingToolsDirectory + "\\CreateAtlas.exe", arguments);
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            Process.Start(info);
        }
    }
}
