using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK;

using Bioware.Files;
using Bioware.Structs;

namespace Bioware.IO
{
    public class ResourceManager
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
        public static void clearFilePaths()
        {
            filePathList.Clear();
        }
        public static void clearERFs()
        {
            erfFileList.Clear();
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

        //Creates an empty directory with the input path, deleting previous directories if they exist
        public static void createDirectory(String path)
        {
            if(Directory.Exists(path))
            {
                Directory.Delete(path,true);
            }
            Directory.CreateDirectory(path);
        }

        //Returns a string describing the struct definitions of the GFF file
        public static String getGFFLayout(GFF file)
        {
            StringBuilder layout = new StringBuilder(); 
            for (int i = 0; i < file.structs.Length; i++)
            {
                layout.AppendFormat("{0}. {1}{2}", i, file.structs[i].definition.ToString(), Environment.NewLine);
                for (int j = 0; j < file.structs[i].fields.Length; j++)
                {
                    layout.AppendFormat("\t{0}. {1}{2}", j, file.structs[i].fields[j].ToString(), Environment.NewLine);
                }
            }

            return layout.ToString();
        }

        //Returns a string listing all the contents of the ERF file with sizes and offsets
        public static String getERFContents(ERF file)
        {
            StringBuilder contents = new StringBuilder();

            contents.AppendFormat("{0} contains {1} files.{2}", file.path, file.resourceCount,Environment.NewLine);
            for (int i = 0; i < file.resourceCount; i++)
            {
                contents.AppendFormat("\t{0}: {1} at offset {2}{3}", file.resourceNames[i], IOUtilities.ToByteString(file.resourceLengths[i]), file.resourceOffsets[i],Environment.NewLine);
            }

            return contents.ToString();
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
