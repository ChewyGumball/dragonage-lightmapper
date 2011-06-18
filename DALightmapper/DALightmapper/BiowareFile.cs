using System;
using System.IO;
using Ben;

namespace Bioware.Files
{
    class BiowareFile
    {
        bool isOpen;
        protected BinaryReader file;
        protected FileStream fs;
        protected String _path;

        public String path
        {
            get { return _path; }
        }

        public BiowareFile(String fileName)
        {
            _path = fileName;
            isOpen = false;
        }

        public virtual void OpenR()
        {
            if (!isOpen)
            {
                try
                {
                    fs = new FileStream(_path, FileMode.Open,FileAccess.Read);
                    file = new BinaryReader(fs);
                    isOpen = true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                file.BaseStream.Seek(0, SeekOrigin.Begin);
            }
        }
        public void Close()
        {
            if (isOpen)
            {
                file.Close();
                fs.Close();
                isOpen = false;
            }
        }
        public BinaryReader getReader()
        {
            if (!isOpen)
                OpenR();
            return file;
        }
    }
}
