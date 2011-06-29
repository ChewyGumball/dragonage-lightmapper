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
                        stream.AppentLine(i+". "+temp.structs[i].definition.ToString(),Verbosity.Low);
                        stream.indent++;
                        for (int j = 0; j < temp.structs[i].fields.Length; j++)
                        {
                            stream.AppentLine(j+". "+temp.structs[i].fields[j].ToString(),Verbosity.Low);
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
                    stream.AppentLine("This is an ERF, attempting to read key data",Verbosity.Sparse);
                    ERF thing = new ERF(fileName);
                    thing.readKeyData();
                    stream.AppentLine("Read in key data, " + thing.resourceCount + " files found", Verbosity.Sparse);
                    for (int i = 0; i < thing.resourceCount; i++)
                    {
                        stream.AppentLine("\t " + thing.resourceNames[i] + ": " + thing.resourceLengths[i] + " " + thing.resourceOffsets[i], Verbosity.Sparse);
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
            tempFiles.Add(fileObject);
        }

        public void cleanUpTempFiles()
        {
            for (int i = 0; i < tempFiles.Count; i++)
            {
                if (Settings.verboseStatus == Verbosity.Medium)
                {
                    if (File.Exists(tempFiles[i].path))
                        stream.AppendText("Deleting " + tempFiles[i].path + "\n", Verbosity.Sparse);
                    else
                        stream.AppendText(tempFiles[i].path + " does not exist.\n", Verbosity.Sparse);
                }
                tempFiles[i].Close();
                File.Delete(tempFiles[i].path);
            }

            stream.AppendText("" + tempFiles.Count + " files deleted.\n", Verbosity.Sparse);

            tempFiles.Clear();
        }

        //Returns the gff file with the name filename, looking in the specified ERF file, the override folders, and the temp folder.
        //  RETURNS NULL IF THE INPUT FILENAME DOES NOT EXIST IN ONE OF THOSE PLACES
        public static GFF findGFFFile(String filename, ERF archiveFile)
        {
            GFF tempGFF = null;
            //Look in temp folder
            if (fileIsInTemp(filename))
            {
                //Look for it in the temp files list
                foreach (BiowareFile f in tempFiles)
                {
                    if (f.path == Settings.tempPath + "\\" + filename)
                        return f as GFF;
                }

                tempGFF = new GFF(Settings.tempPath + "\\" + filename, 0);
                addTempFile(tempGFF);
            }
            //Look in override folder
            else if (fileIsInOverride(filename))
            {
                tempGFF = new GFF(Settings.overrideFolderPath + "\\" + filename, 0);
            }
            //Look in erf
            else if (archiveFile.isInERF(filename))
            {
                archiveFile.separateFile(filename, Settings.tempPath);
                tempGFF = new GFF(Settings.tempPath + "\\" + filename, 0);
            }
            return tempGFF;
        }

        //Returns true if the input filename exists in one of the toolset override directories, false otherwise
        private static Boolean fileIsInOverride(String filename)
        {
            return File.Exists(Settings.overrideFolderPath + "\\" + filename);
        }
        //Returns true if the input filename exists in the temporary folder, false otherwise
        private static Boolean fileIsInTemp(String filename)
        {
            return File.Exists(Settings.tempPath + "\\" + filename);
        }
    }
}
