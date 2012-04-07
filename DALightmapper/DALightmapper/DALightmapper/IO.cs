using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using System.IO;
using OpenTK;

using Bioware.Files;
using Bioware.Structs;
using Ben;

namespace DALightmapper
{
    delegate void FinishedReadingEventHandler(FinishedReadingEventArgs e);
    public class FinishedReadingEventArgs : EventArgs
    {
        public String message {get; private set;}
        public Boolean successful {get; private set;}
        public Level level {get; private set;}
        
        public FinishedReadingEventArgs(Boolean s, String m, Level l)
        {
            successful = s;
            message = m;
            level = l;
        }
    }

    public class IO
    {
        private static List<String> filePathList = new List<String>();
        private static List<ERF> erfFileList = new List<ERF>();

        //Lists of places to look for files
        public static IEnumerable<String> filePaths { get { return filePathList; } }
        public static IEnumerable<ERF> erfFiles { get { return erfFileList; } }

        public static void addFilePath(String path)
        {
            if (!filePathList.Contains(path) && Directory.Exists(path))
            {
                filePathList.Add(path);
            }
        }
        private static void addERF(ERF erf)
        {
            erfFileList.Add(erf);
        }
        public static void addERF(String path)
        {
            if (File.Exists(path) && Path.GetExtension(path) == ".erf")
            {
                if(getERF(path) == null)
                {
                    addERF(new ERF(path));
                }
            }
        }
        public static void removeFilePath(String path)
        {
            filePathList.Remove(path);
        }
        public static void removeERF(ERF erf)
        {
            erfFileList.Remove(erf);
        }

        public static ERF getERF(String path)
        {
            ERF foundERF = null;
            foreach (ERF erf in erfFiles)
            {
                if (erf.path == path)
                {
                    foundERF = erf;
                    break;
                }
            }

            return foundERF;
        }

        //If set to true, reading should be aborted and donereading event should fire false
        public static Boolean abort {get; set;}

        //Reads in the data needed for a level
        //Currently reads more than lvl files for testing purposes NEED TO CLEAN UP 
        public static Level readLevel(String path)
        {
            Level levelFile = null;

            abort = false;

            String fileName = path;
            String extention = Path.GetExtension(fileName);
            String directory = Path.GetDirectoryName(fileName);


            if (File.Exists(fileName))
            {
                if (extention == ".gff" || extention == ".msh" || extention == ".mmh" || extention == ".tmsh")
                {
                    Settings.stream.AppendLine(Verbosity.Low, "Displaying GFF structs and fields:");
                    GFF temp = new GFF(fileName);
                    for (int i = 0; i < temp.structs.Length; i++)
                    {
                        Settings.stream.AppendFormatLine(Verbosity.Low,"{0}. {1}",i,temp.structs[i].definition.ToString());
                        Settings.stream.indent++;
                        for (int j = 0; j < temp.structs[i].fields.Length; j++)
                        {
                            Settings.stream.AppendFormatLine(Verbosity.Low, "{0}. {1}", j, temp.structs[i].fields[j].ToString());
                        }
                        Settings.stream.indent--;
                    }
                    if (extention == ".mmh")
                    {
                        ModelHierarchy m = new ModelHierarchy(temp);
                    }
                    else if (extention == ".msh")
                    {
                        ModelMesh m = new ModelMesh(temp);
                    }
                }
                else if (extention == ".lvl")
                {
                    levelFile = new Level(fileName);
                    levelFile.readObjects();
                }
                else if (extention == ".erf")
                {
                    Settings.stream.AppendLine(Verbosity.Low, "This is an ERF, attempting to read key data");
                    ERF thing = new ERF(fileName);
                    Settings.stream.AppendLine(Verbosity.Low, "Read in key data, " + thing.resourceCount + " files found");
                    Settings.stream.indent++;
                    for (int i = 0; i < thing.resourceCount; i++)
                    {
                        Settings.stream.AppendFormatLine(Verbosity.Low, "{0}: {1} at offset {2}", thing.resourceNames[i], IOUtilities.ToByteString(thing.resourceLengths[i]), thing.resourceOffsets[i]);
                    }
                    Settings.stream.indent--;
                }
                else
                {
                    Settings.stream.AppendFormatLine("{0} is not a valid extension ({1}).",extention,fileName);
                }
            }
            else
            {
                Settings.stream.AppendFormatLine("{0} is not a valid file.", fileName);
            }

            return levelFile;
        }
        
        //Returns the file with the name filename, looking in the erfs and folders saved in the settings classs.
        //  RETURNS NULL IF THE INPUT FILENAME DOES NOT EXIST IN ONE OF THOSE PLACES
        public static T findFile<T>(String filename) where T : class, FindableFile, new()
        {
            T file = null;
            foreach (ERF erf in erfFiles)
            {
                if (erf.isInERF(filename))
                {
                    file = new T();
                    int index = erf.indexOf(filename);
                    file.createFromPathWithOffset(erf.path, erf.resourceOffsets[index], (int)erf.resourceLengths[index]);
                }
            }

            //Look in the folders
            foreach (String path in filePaths)
            {
                String fullPath = path + "\\" + filename;
                if (File.Exists(fullPath))
                {
                    file = new T();
                    file.createFromPath(fullPath);
                }
            }
            
            //If the filename is actually a full path itself
            if (File.Exists(filename))
            {
                file = new T();
                file.createFromPath(filename);
            }
            
            return file;
        }
    }
}
