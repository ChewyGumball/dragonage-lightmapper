using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Bioware.IO;

namespace Bioware.Files
{
    public enum TextureType { Diffuse, Normal, Specular, Tint, Null };
    public class MaterialObject : FindableFile
    {
        private long beginningOffset;

        public String path { get; private set; }
        public String name { get; private set; }
        public Dictionary<TextureType, String> textures { get; private set; }


        public MaterialObject()
        {
            beginningOffset = 0;
            path = "";
            name = "UNUSED";
            textures = new Dictionary<TextureType, String>();
        }

        public MaterialObject(String filename, long offset, int length) { createFromPathWithOffset(filename, offset, length); }
        public MaterialObject(String filename) : this(filename, 0, -1) { }

        public void createFromPathWithOffset(String filename, long offset, int length)
        {
            path = filename;
            beginningOffset = offset;
            textures = new Dictionary<TextureType, String>();

            //The xml libraries don't like reading directly from ERFs so read the file in and store in a string instead
            BinaryReader file = openReader();
            String xml;
            if (length >= 0)
            {
                xml = Encoding.UTF8.GetString(file.ReadBytes(length));
            }
            else
            {
                xml = Encoding.UTF8.GetString(file.ReadBytes((int)file.BaseStream.Length));
            }
            file.Close();

            XmlTextReader reader = new XmlTextReader(new StringReader(xml));
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);

            foreach (XmlNode node in doc.ChildNodes)
            {
                if (node.Name == "MaterialObject")
                {
                    //Get the name of the mao
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name == "Name")
                        {
                            name = att.Value;
                        }
                    }

                    //Get the resource names
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.Name == "Texture")
                        {
                            TextureType name = TextureType.Null;
                            String resource = "";
                            foreach (XmlAttribute att in child.Attributes)
                            {
                                if (att.Name == "Name")
                                {
                                    switch (att.Value)
                                    {
                                        case "mml_tDiffuse": name = TextureType.Diffuse; break;
                                        case "mml_tNormalMap": name = TextureType.Normal; break;
                                        case "mml_tSpecularMask": name = TextureType.Specular; break;
                                        case "mml_tTintMask": name = TextureType.Tint; break;
                                        default: name = TextureType.Null; break;
                                    }
                                }
                                else if (att.Name == "ResName")
                                {
                                    resource = att.Value;
                                }
                            }
                            if (name != TextureType.Null && resource != "")
                            {
                                textures.Add(name, resource);
                            }
                        }
                    }
                }
            }

            reader.Close();
        }
        public void createFromPath(String filename) { createFromPathWithOffset(filename, 0, -1); }

        public BinaryReader openReader()
        {
            try
            {
                BinaryReader file = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
                file.BaseStream.Seek(beginningOffset, SeekOrigin.Begin);
                return file;
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                    Console.WriteLine("{0} \n Inner: {1}.", e.StackTrace, e.InnerException.Message);
                else
                    Console.WriteLine("{0}", e.StackTrace);

                throw e;
            }
        }
    }
}
