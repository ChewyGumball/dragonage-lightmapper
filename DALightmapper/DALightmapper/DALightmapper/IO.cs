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
    class FinishedReadingEventArgs : EventArgs
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

    class IO
    {

        //The stream to which updates should be sent
        StatusStream stream;

        //If set to true, reading should be aborted and donereading event should fire false
        static Boolean _abort;
        public static Boolean abort
        {
            get { return _abort; }
            set { _abort = value; }
        }

        //List of temporary files created during IO
        static List<BiowareFile> tempFiles = new List<BiowareFile>();
        public int numTempFiles
        {
            get { return tempFiles.Count; }
        }

        //Constructor
        public IO(StatusStream statStream)
        {
            stream = statStream;
        }

        //Reads in the data needed for a level
        //Currently reads more than lvl files for testing purposes NEED TO CLEAN UP 
        public void readLevelAsync(String path, FinishedReadingEventHandler handler)
        {
            Level levelFile;

            _abort = false;

            String fileName = path;
            String extention = Path.GetExtension(fileName);
            String directory = Path.GetDirectoryName(fileName);


            if (File.Exists(fileName))
            {
                if (extention == ".gff" || extention == ".msh" || extention == ".mmh" || extention == ".tmsh")
                {
                    stream.AppendText("Displaying GFF structs and fields:\n", Verbosity.Low);
                    GFF temp = new GFF(fileName, 0);
                    for (int i = 0; i < temp.structs.Length; i++)
                    {
                        stream.AppendLine(i+". "+temp.structs[i].definition.ToString(),Verbosity.Low);
                        stream.indent++;
                        for (int j = 0; j < temp.structs[i].fields.Length; j++)
                        {
                            stream.AppendLine(j+". "+temp.structs[i].fields[j].ToString(),Verbosity.Low);
                        }
                        stream.indent--;
                   }
                    temp.Close();
                }
                else if (extention == ".lvl")
                {
                    levelFile = new Level(fileName);
                    levelFile.FinishedReading += handler;
                    new Thread(levelFile.readObjectsAsync).Start();
                }
                else if (extention == ".erf")
                {
                    stream.AppendLine("This is an ERF, attempting to read key data",Verbosity.Warnings);
                    ERF thing = new ERF(fileName);
                    thing.readKeyData();
                    stream.AppendLine("Read in key data, " + thing.resourceCount + " files found", Verbosity.Warnings);
                    for (int i = 0; i < thing.resourceCount; i++)
                    {
                        stream.AppendLine("\t " + thing.resourceNames[i] + ": " + thing.resourceLengths[i] + " " + thing.resourceOffsets[i], Verbosity.Warnings);
                    }
                }
                else
                {
                    handler.Invoke(new FinishedReadingEventArgs(false, "File with extention \"" + extention + "\" is not a valid file type.", null));
                }
            }
            else
            {
                handler.Invoke(new FinishedReadingEventArgs(false, fileName + " is not a valid file.",null));
            }
        }

        public static void addTempFile(BiowareFile fileObject)
        {
            if (fileObject != null)
            {
                tempFiles.Add(fileObject);
            }
        }

        public void cleanUpTempFiles()
        {
            for (int i = 0; i < tempFiles.Count; i++)
            {
                tempFiles[i].Close();
            }

            stream.AppendText("Cleaned up " + tempFiles.Count + " files.\n", Verbosity.Medium);
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
                tempGFF = new GFF(tempPath, 0);
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
                        tempGFF = new GFF(fullPath, 0);
                    }
                }
            }
            addTempFile(tempGFF);
            return tempGFF;
        }
    }
}
