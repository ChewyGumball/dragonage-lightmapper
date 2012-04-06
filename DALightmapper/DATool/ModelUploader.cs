using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

using Bioware.Structs;
using Bioware.Files;

using DALightmapper;

namespace DATool
{
    class ModelUploader : UploadableObject
    {
        ModelMesh model;

        public ModelUploader() { model = new ModelMesh(); }
        public ModelUploader(ModelMesh m) { model = m; }

        public void upload(ref List<VBO> vboList, ref Dictionary<String, int> textureList)
        {
            //Make VBOs of each chunk
            foreach (MeshChunk m in model.chunks)
            {
                int vb, eb, tb = 0;
                GL.GenBuffers(1, out vb);
                GL.GenBuffers(1, out eb);
                //find texture buffer
                //*
                if (m.materialObjectName != "")
                {
                    MaterialObject mao = IO.findFile<MaterialObject>(m.materialObjectName);
                    if (mao != null)
                    {
                        if (mao.textures.ContainsKey(TextureType.Diffuse))
                        {
                            //if we havent already added it to the dictionary
                            if (!textureList.ContainsKey(mao.textures[TextureType.Diffuse]))
                            {
                                int buffer = 0;
                                //find the file and then make a buffer out of it then add it to the dictionary
                                DDS dds = IO.findFile<DDS>(mao.textures[TextureType.Diffuse]);
                                if (dds != null)
                                {
                                    DDSUploader temp = new DDSUploader(dds);
                                    buffer = temp.uploadTexture();
                                }
                                else
                                {
                                    Console.WriteLine("Couldn't find texture {0}", mao.textures[TextureType.Diffuse]);
                                }

                                textureList.Add(mao.textures[TextureType.Diffuse], buffer);
                            }
                            tb = textureList[mao.textures[TextureType.Diffuse]];
                        }
                        else
                        {
                            Console.WriteLine("Material object {0} doesn't have a diffuse texture", m.materialObjectName);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Couldn't find material object {0}", m.materialObjectName);
                    }
                }
                //*/
                VBO curVBO = new VBO(vb, eb, tb, 8, m.indices.Length);
                vboList.Add(curVBO);

                GL.BindBuffer(BufferTarget.ArrayBuffer, curVBO.vertexBuffer);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, curVBO.elementBuffer);

                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(m.verts.Length * sizeof(float)), m.verts, BufferUsageHint.StaticDraw);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(curVBO.indexElementCount * sizeof(uint)), m.indices, BufferUsageHint.StaticDraw);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            }
        }
    }
}
