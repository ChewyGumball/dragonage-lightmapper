using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Ben;
using Bioware.Files;

namespace DALightmapper
{
    public class Settings
    {
        //--Scene Variables--//
        public static int worldScale = 10;  //How much to scale up the world on import
        public static bool useTrueAttenuation = true;   //Whether to use d^-2 or a more linear fall off specified per light for lighting

        //--Output Stream Variables--//
        public static Verbosity verboseStatus = Verbosity.Low;   //The level of verbosity messages are checked against for display
        public static StatusStream stream = new StatusStream();
        
        //-- Housekeeping Variables --//
        public static Boolean cleanUpTempFiles = true;  //Whether to clean up the temp files created

        //-- Multi-threading Variables --//
        public static int maxThreads = 2;

        //--Light Mapping Variables--//
        public static int numPhotonsPerLight = 10000;   //Number of photons to shoot per light
        public static double gatherRadius = 0.2;         //Radius of sphere to sample photons for a patch
        public static Boolean useNumBounces = false;
        
        public static int pixelsPerUnit = 10;   //Number of pixels per unit length to use when making lightmap textures

        //--Paths to Required Files--// 
        public static String tempDirectory;
        public static String workingDirectory;

        public static List<String> filePaths = null;
        public static List<ERF> erfFiles = null;
               
        //Initializes the variables saved in an ini file leaving the others at default value
        public static void initializeSettings()
        {
            workingDirectory = Properties.Settings.Default.workingDirectory;
            tempDirectory = Properties.Settings.Default.tempDirectory;

            filePaths = new List<String>();
            erfFiles = new List<ERF>();

            foreach (String s in Properties.Settings.Default.filePaths)
            {
                if (Directory.Exists(s))
                {
                    filePaths.Add(s);
                }
                else
                {
                    stream.AppendFormatLine("Could not find directory \"{0}\", it has been removed from the path list.", s);
                }
            }

            foreach (String s in Properties.Settings.Default.erfFiles)
            {
                if (File.Exists(s))
                {
                    ERF temp = new ERF(s);
                    temp.readKeyData();
                    erfFiles.Add(temp);
                }
                else
                {
                    stream.AppendFormatLine("Could not find ERF \"{0}\", It has been removed from the ERF list.", s);
                }
            }

            numPhotonsPerLight = Properties.Settings.Default.numPhotons;
            gatherRadius = Properties.Settings.Default.gatherRadius;
            verboseStatus = (Verbosity)Properties.Settings.Default.verbosity;
            useTrueAttenuation = Properties.Settings.Default.trueAttenuation;
            cleanUpTempFiles = Properties.Settings.Default.clearTempFiles;
            maxThreads = Properties.Settings.Default.maxThreads;
        }

        public static void saveSettings()
        {
            System.Collections.Specialized.StringCollection saveFilePaths = new System.Collections.Specialized.StringCollection();
            System.Collections.Specialized.StringCollection saveErfFiles = new System.Collections.Specialized.StringCollection();

            foreach (String s in filePaths)
            {
                saveFilePaths.Add(s);
            }
            foreach (ERF erf in erfFiles)
            {
                saveErfFiles.Add(erf.path);
            }

            Properties.Settings.Default.erfFiles = saveErfFiles;
            Properties.Settings.Default.filePaths = saveFilePaths;
            Properties.Settings.Default.workingDirectory = workingDirectory;
            Properties.Settings.Default.tempDirectory = tempDirectory;

            Properties.Settings.Default.numPhotons = numPhotonsPerLight;
            Properties.Settings.Default.gatherRadius = gatherRadius;
            Properties.Settings.Default.verbosity = (int)verboseStatus;
            Properties.Settings.Default.trueAttenuation = useTrueAttenuation;
            Properties.Settings.Default.clearTempFiles = cleanUpTempFiles;
            Properties.Settings.Default.maxThreads = maxThreads;

            Properties.Settings.Default.Save();
        }
    }
}
