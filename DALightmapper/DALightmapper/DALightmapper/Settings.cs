using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using Ben;

using Bioware.IO;
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
        public static float gatherRadius = 0.1f;         //Radius of sphere to sample photons for a patch
        public static Boolean useNumBounces = false;

        public static int pixelsPerUnit = 10;   //Number of pixels per unit length to use when making lightmap textures

        //--Ambient Occlusion Variables--//
        public static int ambientSamples = 256;
        public static float ambientRayLength = 5.0f;

        //--Paths to Required Files--// 
        public static String tempDirectory = "";
        public static String lightmappingToolsDirectory = "";

        //--Commandline arguments--// NOT SAVED
        public static Boolean commandline = false;
        public static Boolean showWindow = true;
        public static List<String> scenes = new List<String>();

        public static String compressedDirectory = "";
        public static String atlasedDirectory = "";
        public static String combinedDirectory = "";
        public static String ambientDirectory = "";
        public static String uncompressedDirectory = "";
        public static Boolean allPathsSpecified = false;

        public static String atlasFile = "";
        public static int atlasWidth = 512;
        public static int atlasHeight = 512;


        //Initializes the variables saved in an ini file leaving the others at default value
        public static void initializeSettings(String[] arguments)
        {
            loadDefaultSettings();
            processCommandLineArguments(arguments);
        }

        public static void loadDefaultSettings()
        {
            lightmappingToolsDirectory = Properties.Settings.Default.lightmappingToolsDirectory;
            tempDirectory = Properties.Settings.Default.tempDirectory;

            foreach (String s in Properties.Settings.Default.filePaths)
            {
                if (Directory.Exists(s))
                {
                    ResourceManager.addFilePath(s);
                }
                else
                {
                    stream.WriteLine(Verbosity.Warnings, "Could not find directory \"{0}\", it has been removed from the path list.", s);
                }
            }

            foreach (String s in Properties.Settings.Default.erfFiles)
            {
                if (File.Exists(s))
                {
                    ResourceManager.addERF(s);
                }
                else
                {
                    stream.WriteLine(Verbosity.Warnings, "Could not find ERF \"{0}\", It has been removed from the ERF list.", s);
                }
            }

            numPhotonsPerLight = Properties.Settings.Default.numPhotons;
            gatherRadius = Properties.Settings.Default.gatherRadius;
            verboseStatus = (Verbosity)Properties.Settings.Default.verbosity;
            useTrueAttenuation = Properties.Settings.Default.trueAttenuation;
            cleanUpTempFiles = Properties.Settings.Default.clearTempFiles;
            maxThreads = Properties.Settings.Default.maxThreads;
        }

        public static void processCommandLineArguments(String[] arguments)
        {
            if (arguments.Length > 0)
            {
                commandline = true;
                Boolean atLeastOnePathSepcified = false;

                Boolean overridePaths = false;
                int currentIndex = 1;
                List<String> filePaths = new List<String>();
                List<String> erfPaths = new List<String>();

                while (currentIndex < arguments.Length)
                {
                    string command = arguments[currentIndex];
                    currentIndex++; //move to the argument
                    try
                    {
                        switch (command)
                        {
                            //--TOOLSET OPTIONS--//

                            case "--inputDir":
                                scenes.Add(arguments[currentIndex] + "\\job_scene.xml");
                                break;
                            //Interpreted as number of threads
                            case "--numSubJobs":
                                maxThreads = convertFromString("numSubJobs", arguments[currentIndex], 1, Environment.ProcessorCount, Convert.ToInt32,
                                                          "You specified " + arguments[currentIndex] + " subjobs, however this machine can only run between 1 and " + Environment.ProcessorCount + " threads at once. Only one job will be done at a time.");
                                break;
                            case "--outputTGADir":
                                uncompressedDirectory = arguments[currentIndex].TrimEnd('\\', ' ');
                                atLeastOnePathSepcified = true;
                                break;
                            case "--outputAoTGADir":
                                ambientDirectory = arguments[currentIndex].TrimEnd('\\', ' ');
                                atLeastOnePathSepcified = true;
                                break;
                            case "--combinedDDSDir":
                                combinedDirectory = arguments[currentIndex].TrimEnd('\\', ' ');
                                atLeastOnePathSepcified = true;
                                break;
                            case "--atlasedDDSDir":
                                atlasedDirectory = arguments[currentIndex].TrimEnd('\\', ' ');
                                atLeastOnePathSepcified = true;
                                break;
                            case "--compressedDDSDir":
                                compressedDirectory = arguments[currentIndex].TrimEnd('\\', ' ');
                                atLeastOnePathSepcified = true;
                                break;
                            case "--atlasFile":
                                atlasFile = arguments[currentIndex];
                                break;
                            case "--in_width":
                                atlasWidth = convertFromString("in_width", arguments[currentIndex], 32, Int32.MaxValue, Convert.ToInt32);
                                break;
                            case "--in_height":
                                atlasHeight = convertFromString("in_height", arguments[currentIndex], 32, Int32.MaxValue, Convert.ToInt32);
                                break;

                            //--NONTOOLSET OPTIONS--//
                            case "-level":
                                scenes.Add(arguments[currentIndex]);
                                break;
                            case "-noWindow":
                                stream.WriteLine(Verbosity.Warnings, "-noWindow was specified. It will be ignored because this functionality is not implemented yet.");
                                //showWindow = false;
                                currentIndex--; //This command has no argument, so make sure it doesn't skip the next command
                                break;
                            case "-worldScale":
                                worldScale = convertFromString("worldScale", arguments[currentIndex], 1, Int32.MaxValue, Convert.ToInt32);
                                break;
                            case "-maxThreads":
                                maxThreads = convertFromString("maxThreads", arguments[currentIndex], 1, Environment.ProcessorCount, Convert.ToInt32,
                                                          "You specified " + arguments[currentIndex] + " threads should be used, however this machine can only run between 1 and " + Environment.ProcessorCount + " threads at once. Only one thread will be used.");
                                break;
                            case "-photonsPerLight":
                                numPhotonsPerLight = convertFromString("photonsPerLight", arguments[currentIndex], 1, Int32.MaxValue, Convert.ToInt32);
                                break;
                            case "-gatherRadius":
                                gatherRadius = convertFromString("gatherRadius", arguments[currentIndex], 0.01f, 0.5f, Convert.ToSingle);
                                break;
                            case "-pixelsPerUnit":
                                pixelsPerUnit = convertFromString("pixelsPerUnit", arguments[currentIndex], 1, Int32.MaxValue, Convert.ToInt32);
                                break;
                            case "-ambientSamples":
                                ambientSamples = convertFromString("ambientSamples", arguments[currentIndex],1, Int32.MaxValue, Convert.ToInt32);
                                break;
                            case "-ambientRayLength":
                                ambientRayLength = convertFromString("ambientRayLength", arguments[currentIndex], 0.01f, Single.MaxValue, Convert.ToSingle);
                                break;
                            case "-verbosity":
                                String argument = arguments[currentIndex].ToUpper();
                                switch (argument)
                                {
                                    case "WARNING":
                                        verboseStatus = Verbosity.Warnings;
                                        break;
                                    case "LOW":
                                        verboseStatus = Verbosity.Low;
                                        break;
                                    case "MEDIUM":
                                        verboseStatus = Verbosity.Medium;
                                        break;
                                    case "HIGH":
                                        verboseStatus = Verbosity.High;
                                        break;
                                    case "TESTING":
                                        verboseStatus = Verbosity.TESTING;
                                        break;
                                    default:
                                        stream.WriteLine(Verbosity.Warnings, "The argument to the verbosity option ({0}) is not valid.", arguments[currentIndex]);
                                        break;
                                }
                                break;
                            case "-toolsDir":
                                lightmappingToolsDirectory = arguments[currentIndex];
                                break;
                            case "-outputDir":
                                //outputDirectory = arguments[currentIndex];
                                break;
                            case "-directory":
                                filePaths.Add(arguments[currentIndex]);
                                break;
                            case "-erf":
                                erfPaths.Add(arguments[currentIndex]);
                                break;
                            case "-trueAttenuation":
                                useTrueAttenuation = true;
                                currentIndex--; //This command has no argument, so make sure it doesn't skip the next command
                                break;
                            case "-override":
                                overridePaths = true;
                                currentIndex--; //This command has no argument, so make sure it doesn't skip the next command
                                break;
                            default:
                                stream.WriteLine(Verbosity.Warnings, "Unknown option {0}.", arguments[currentIndex]);
                                break;
                        }
                        currentIndex++;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        stream.WriteLine(Verbosity.Warnings, "The argument list is improperly formed . . . you may be missing an argument to the last option {0}.", arguments[arguments.Length - 1]);
                    }
                }

                allPathsSpecified = compressedDirectory != "" && atlasedDirectory != "" && combinedDirectory != "" && ambientDirectory != "" && uncompressedDirectory != "";

                if (!allPathsSpecified && atLeastOnePathSepcified)
                {
                    stream.WriteLine("Not all required paths were specified so they will be ignored. -outputTGADir -outputAoTGADir -combinedDDSDir -atlasedDDSDir -compressedDDSDir must all be specified.");
                }

                if (overridePaths)
                {
                    ResourceManager.clearERFs();
                    ResourceManager.clearFilePaths();
                }

                foreach (String s in filePaths)
                {
                    ResourceManager.addFilePath(s);
                }
                foreach (String s in erfPaths)
                {
                    ResourceManager.addERF(s);
                }

            }
        }

        public static void saveSettings()
        {
            System.Collections.Specialized.StringCollection saveFilePaths = new System.Collections.Specialized.StringCollection();
            System.Collections.Specialized.StringCollection saveErfFiles = new System.Collections.Specialized.StringCollection();

            foreach (String s in ResourceManager.filePaths)
            {
                saveFilePaths.Add(s);
            }
            foreach (ERF erf in ResourceManager.erfFiles)
            {
                saveErfFiles.Add(erf.path);
            }

            Properties.Settings.Default.erfFiles = saveErfFiles;
            Properties.Settings.Default.filePaths = saveFilePaths;
            Properties.Settings.Default.lightmappingToolsDirectory = lightmappingToolsDirectory;
            Properties.Settings.Default.tempDirectory = tempDirectory;

            Properties.Settings.Default.numPhotons = numPhotonsPerLight;
            Properties.Settings.Default.gatherRadius = gatherRadius;
            Properties.Settings.Default.verbosity = (int)verboseStatus;
            Properties.Settings.Default.trueAttenuation = useTrueAttenuation;
            Properties.Settings.Default.clearTempFiles = cleanUpTempFiles;
            Properties.Settings.Default.maxThreads = maxThreads;

            Properties.Settings.Default.Save();
        }

        private static T convertFromString<T>(String command, String argument, T lowerBound, T upperBound, Func<String, T> conversionFunction) where T : IComparable
        {
            string outOfBoundsMessage = command + " must be at least " + lowerBound + " and no greater than " + upperBound + ", you specified " + argument + ", The value of this option will be set to " + lowerBound + ".";
            return convertFromString(command, argument, lowerBound, upperBound, conversionFunction, outOfBoundsMessage);
        }
        private static T convertFromString<T>(String command, String argument, T lowerBound, T upperBound, Func<String, T> conversionFunction, String outOfBoundsMessage) where T : IComparable
        {
            string conversionErrorMessage = "The argument to the " + command + " option (" + argument + ") is not a valid " + typeof(T).ToString() + ".";
            return convertFromString(argument, lowerBound, upperBound, conversionFunction, outOfBoundsMessage, conversionErrorMessage);
        }
        private static T convertFromString<T>(String argument, T lowerBound, T upperBound, Func<String, T> conversionFunction, String outOfBoundsMessage, String conversionErrorMessage) where T: IComparable
        {
            T option = lowerBound;
            try
            {
                option = conversionFunction(argument);
                if (option.CompareTo(lowerBound) < 0 || option.CompareTo(upperBound) > 0)
                {
                    stream.WriteLine(Verbosity.Warnings, outOfBoundsMessage);
                    option = lowerBound;
                }
            }
            catch (FormatException)
            {
                stream.WriteLine(Verbosity.Warnings, conversionErrorMessage);
            }

            return option;
        }
    }
}
