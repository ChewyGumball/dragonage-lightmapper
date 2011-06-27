using System;
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
        public static float renderThreshold = 0.5f; //The threshold above which a pixel will be rendered instead of lerped
        public static int lerpStartStride = 4;  //The stride to start lerping with
        public static int pixelsPerUnit = 10;   //Number of pixels per unit length to use when making lightmap textures

        //--Paths to Required Files--//
        public static String toolsetPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\dragon age origins\\tools";  //The path to the toolset
        public static String mmhERFPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\dragon age origins\\packages\\core\\data\\modelhierarchies.erf";   //The path to the core mmh erf file
        public static String mshERFPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\dragon age origins\\packages\\core\\data\\modelmeshdata.erf";   //The path to the core msh erf file
        public static String overrideFolderPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\dragon age origins\\packages\\core\\override";   //The path to the override folder in which to look for files if they aren't in the erf

        //--Path to temp files--//
        public static String tempPath = Application.StartupPath + "\\temp";
        
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
        public static void initializeSettings(String fileName)
        {
            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);

            Directory.CreateDirectory(tempPath);

            _modelERF = new ERF(mmhERFPath);
            _modelERF.readKeyData();
            _meshERF = new ERF(mshERFPath);
            _meshERF.readKeyData();
        }
    }
}
