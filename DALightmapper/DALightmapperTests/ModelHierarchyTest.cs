using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.IO;

using Bioware.Files;
using DALightmapper;

namespace DALightmapperTests
{
    [TestFixture]
    public class ModelHierarchyTest
    {
        [TestCase]
        public void SettingsERFNotNull()
        {
            Settings.initializeSettings();
            foreach(ERF e in Settings.erfFiles)
            {
                Assert.NotNull(e);
                Console.WriteLine(e.path);
                for (int i = 0; i < e.resourceCount; i++)
                {
                    GFF temp = IO.findFile<GFF>(e.resourceNames[i]);
                    Assert.NotNull(temp);
                }
            }
        }

        [TestCase]
        public void ModelLoads()
        {
            Settings.initializeSettings();
            String modelERFPath = "E:\\Program Files (x86)\\Steam\\steamapps\\common\\dragon age origins\\packages\\core\\data\\modelhierarchies.erf";
            
            ERF models = new ERF(modelERFPath);
            models.readKeyData();
            Assert.Greater(models.resourceCount, 0);
            int failures = 0;
            for (int i = 0; i < models.resourceCount; i++)
            {
                GFF temp = IO.findFile<GFF>(models.resourceNames[i]);
                Assert.NotNull(temp, "Not found: |" + models.resourceNames[i] +"|" + i);
                if (Path.GetExtension(models.resourceNames[i]) == ".mmh")
                {
                    try
                    {
                        ModelHierarchy tempH = new ModelHierarchy(temp);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(models.resourceNames[i]);
                        failures++;
                    }
                }
            }
            Assert.AreEqual(0, failures);
        }
        
        [TestCase]
        public void MeshLoads()
        {
            Settings.initializeSettings();
            String meshERFPath = "E:\\Program Files (x86)\\Steam\\steamapps\\common\\dragon age origins\\packages\\core\\data\\modelmeshdata.erf";

            ERF meshes = new ERF(meshERFPath);
            meshes.readKeyData();
            Assert.Greater(meshes.resourceCount, 0);
            int failures = 0;
            for (int i = 0; i < meshes.resourceCount; i++)
            {
                GFF temp = IO.findFile<GFF>(meshes.resourceNames[i]);
                Assert.NotNull(temp, "Not found: |" + meshes.resourceNames[i] + "|" + i);
                if (Path.GetExtension(meshes.resourceNames[i]) == ".msh")
                {
                    try
                    {
                        ModelMesh tempH = new ModelMesh(temp);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(meshes.resourceNames[i]);
                        failures++;
                    }
                }
            }
            Assert.AreEqual(0, failures);
        }             
    }
}
