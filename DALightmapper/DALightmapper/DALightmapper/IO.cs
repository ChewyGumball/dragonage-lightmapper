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
        String _message;
        Boolean _successful;
        Level _level;

        public String message
        {
            get { return _message; }
        }
        public Boolean successful
        {
            get { return _successful; }
        }
        public Level level
        {
            get { return _level; }
        }
        
        public FinishedReadingEventArgs(Boolean successful, String message, Level level)
        {
            _successful = successful;
            _message = message;
            _level = level;
        }
    }

    public class IO
    {
        //If set to true, reading should be aborted and donereading event should fire false
        static Boolean _abort;
        public static Boolean abort
        {
            get { return _abort; }
            set { _abort = value; }
        }

        //List of temporary files created during IO
        static List<BiowareFile> tempFiles = new List<BiowareFile>();
        public static int numTempFiles
        {
            get { return tempFiles.Count; }
        }

        //Reads in the data needed for a level
        //Currently reads more than lvl files for testing purposes NEED TO CLEAN UP 
        public static Level readLevel(String path)
        {
            Level levelFile = null;

            _abort = false;

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
                    temp.Close();
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
                    thing.readKeyData();
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

        public static void addTempFile(BiowareFile fileObject)
        {
            if (fileObject != null)
            {
                tempFiles.Add(fileObject);
            }
        }

        public static void cleanUpTempFiles()
        {
            for (int i = 0; i < tempFiles.Count; i++)
            {
                tempFiles[i].Close();
            }

            Settings.stream.AppendFormatLine(Verbosity.Medium, "Closed {0} files.", tempFiles.Count);
            tempFiles.Clear();
        }

        //Returns the gff file with the name filename, looking in the erfs and folders saved in the settings classs.
        //  RETURNS NULL IF THE INPUT FILENAME DOES NOT EXIST IN ONE OF THOSE PLACES
        public static GFF findGFFFile(String filename)
        {
            GFF tempGFF = null;

            //Look in temp directory
            String tempPath = Settings.tempDirectory + "\\" + filename;
            if (File.Exists(tempPath))
            {
                tempGFF = new GFF(tempPath);
            }
            else
            {
                //Look in the erfs
                foreach (ERF erf in Settings.erfFiles)
                {
                    if (erf.isInERF(filename))
                    {
                        tempGFF = new GFF(erf.path, erf.resourceOffsets[erf.indexOf(filename)]);
                    }
                }
                //Look in the folders
                foreach (String path in Settings.filePaths)
                {
                    String fullPath = path + "\\" + filename;
                    if (File.Exists(fullPath))
                    {
                        tempGFF = new GFF(fullPath);
                    }
                }
            }
            addTempFile(tempGFF);
            return tempGFF;
        }
    }
}
