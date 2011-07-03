using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Ben;
using Bioware.Files;

namespace DALightmapper
{
    class Settings
    {
        //--Scene Variables--//
        public static int worldScale = 10;  //How much to scale up the world on import
        public static bool useTrueAttenuation = true;   //Whether to use d^-2 or a more linear fall off specified per light for lighting

        //--Housekeeping Variables--//
        public static Verbosity verboseStatus = Verbosity.Low;   //The level of verbosity messages are checked against for display
        public static Boolean cleanUpTempFiles = true;  //Whether to clean up the temp files created

        //--Light Mapping Variables--//
        public static int numBounces = 5;   //Number of bounces to calculate light for
        public static double minimumEnergy = 0.05; //The minimum energy required for lightmapping to continue
        public static Boolean useNumBounces = false;
        
        public static float renderThreshold = 0.5f; //The threshold above which a pixel will be rendered instead of lerped
        public static int lerpStartStride = 4;  //The stride to start lerping with
        public static int pixelsPerUnit = 10;   //Number of pixels per unit length to use when making lightmap textures

        //--Paths to Required Files--// 
        public static String tempDirectory;
        public static String workingDirectory;
        public static String toolsetPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\dragon age origins\\tools";  //The path to the toolset
        public static String overrideFolderPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\dragon age origins\\packages\\core\\override";   //The path to the override folder in which to look for files if they aren't in the erf

        public static List<String> filePaths = new List<String>();
        public static List<ERF> erfFiles = new List<ERF>();
       
        
        //--Required ERF files--//
        private static ERF _modelERF, _meshERF;

        public static ERF modelERF
        {
            get { return _modelERF; }
        }
        public static ERF meshERF
        {
            get { return _meshERF; }
        }
        
        //Initializes the variables saved in an ini file leaving the others at default value
        public static void initializeSettings()
        {
            workingDirectory = Properties.Settings.Default.workingDirectory;
            tempDirectory = Properties.Settings.Default.tempDirectory;
            foreach (String s in Properties.Settings.Default.filePaths)
            {
                filePaths.Add(s);
            }
            foreach (String s in Properties.Settings.Default.erfFiles)
            {
                ERF temp = new ERF(s);
                temp.readKeyData();
                erfFiles.Add(temp);
            }

            numBounces = Properties.Settings.Default.numBounces;
            verboseStatus = (Verbosity)Properties.Settings.Default.verbosity;
            minimumEnergy = Properties.Settings.Default.minEnergy;
            useTrueAttenuation = Properties.Settings.Default.trueAttenuation;
            cleanUpTempFiles = Properties.Settings.Default.clearTempFiles;
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

            Properties.Settings.Default.numBounces = numBounces;
            Properties.Settings.Default.verbosity = (int)verboseStatus;
            Properties.Settings.Default.minEnergy = minimumEnergy;
            Properties.Settings.Default.trueAttenuation = useTrueAttenuation;
            Properties.Settings.Default.clearTempFiles = cleanUpTempFiles;

            Properties.Settings.Default.Save();
        }
    }
}
