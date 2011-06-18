using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;

using Bioware.Files;
using Bioware.Structs;

using Ben;

namespace DALightmapper
{

    class Level
    {
        //Event to signal asynchronous reads have finished 
        public event FinishedReadingEventHandler FinishedReading;

        ModelInstance[] _lightmapModels = null;
        BiowareModel[] _models = null;
        Light[] _lights = null;

        ERF diskFile;
        GFF headerFile;

        public String name
        {
            get { return diskFile.path; }
        }
        public ModelInstance[] lightmapModels
        {
            get { return _lightmapModels; }
        }
        public BiowareModel[] models
        {
            get { return _models; }
        }
        public Light[] lights
        {
            get { return _lights; }
        }

        #region Index Values

        //Struct index values (mostly list indecies)
        private static readonly int TOP_LEVEL_STRUCT_INDEX = 0;
        private static readonly int TERRAIN_CHUNK_LIST_INDEX = 3;
        private static readonly int ROOM_LIST_INDEX = 1;
        private static readonly int ROOM_OBJECT_LIST_INDEX = 2;
        private static readonly int ENVIRONMENT_LIST_INDEX = 1;

        //Light index values (within struct)
        private static readonly int LIGHT_POSITION_INDEX = 0;
        private static readonly int LIGHT_ROTATION_INDEX = 1;
        private static readonly int LIGHT_NAME_INDEX = 4;
        private static readonly int LIGHT_COLOUR_INDEX = 5;
        private static readonly int LIGHT_TYPE_INDEX = 6;
        private static readonly int LIGHT_EFFECT_INDEX = 9;
        private static readonly int LIGHT_INANGLE_INDEX = 11;
        private static readonly int LIGHT_OUTANGLE_INDEX = 12;
        private static readonly int LIGHT_DISTANCE_INDEX = 13;

        //Light constant values
        private const int LIGHT_STATIC = 2;
        private const int LIGHT_BAKED = 0;
        private const int LIGHT_POINT = 0;
        private const int LIGHT_AMBIENT = 1;
        private const int LIGHT_SPOT = 2;

        //Model index values (within struct)
        private static readonly int MODEL_POSITION_INDEX = 0;
        private static readonly int MODEL_ROTATION_INDEX = 1;
        private static readonly int MODEL_PROPERTY_INDEX = 2;
        private static readonly int MODEL_ID_INDEX = 4;

        //Model index values (within property list)
        private static readonly int MODEL_FILENAME_INDEX = 0;
        private static readonly int MODEL_LIGHTMAPVALUE_INDEX = 13;

        //Model static constant values
        private static readonly int MODEL_LIGHTMAPPABLE = -1;
        private static readonly int MODEL_NOTLIGHTMAPPABLE = 0;

        //Property index values (within struct)
        private static readonly int PROPERTY_VALUE_INDEX = 1;
        private static readonly int PROPERTY_CHILDREN_INDEX = 2;

        #endregion

        private BiowareStruct roomStruct = null;
        private BiowareStruct areaStruct = null;
        private BiowareStruct modelStruct = null;
        private BiowareStruct lightStruct = null;
        private BiowareStruct environmentStruct = null;
        private BiowareStruct terrainChunkStruct = null;
        private BiowareStruct propertyStruct = null;

        public Level(String filePath)
        {
            // Initialize the erf (lvl) file in memory
            diskFile = new ERF(filePath);
            diskFile.readKeyData();

            //find the index of the header file in the erf
            int headerIndex = diskFile.indexOf("header.gff");

            if (headerIndex < 0)
            {
                throw(new Exception("Could not find the header file in "+filePath+"! Please make sure it is a level file."));
            }

            //Initialize the header file
            headerFile = new GFF(filePath, diskFile.resourceOffsets[headerIndex]);
            //Set up the struct definitions (for sanity!)
            setStructDefinitions();
        }

        public void readObjectsAsync()
        {
            int reference;      //used for storing the reference to things in files
            BinaryReader file = headerFile.getReader();

            //If the level is outdoors, read the models and the terrain mesh, otherwise just read in the models
            //      the file is layed out differently for outdoors and indoor environments
            List<BiowareModel> modelList = new List<BiowareModel>();
            List<Light> lightList = new List<Light>();;
            GenericList objectList;

            //If this is an outdoor level read in the terrain
            if(environmentStruct.type == GFFSTRUCTTYPE.ENV_WORLD_TERRAIN){
                modelList.AddRange(readTerrainModels());                
            }

            //then make the generic list for lights and models

            //get to the beginning of the data block
            file.BaseStream.Seek(headerFile.dataOffset, SeekOrigin.Begin);

            //seek to the reference to the terrain world reference (first field in the struct) then go there
            file.BaseStream.Seek(headerFile.structs[TOP_LEVEL_STRUCT_INDEX].fields[0].index, SeekOrigin.Current);

            //seek to the list of objects in the environment struct and go there
            file.BaseStream.Seek(environmentStruct.fields[ENVIRONMENT_LIST_INDEX].index, SeekOrigin.Current);
            reference = file.ReadInt32();
            file.BaseStream.Seek(headerFile.dataOffset + reference, SeekOrigin.Begin);

            //make the list of objects
            objectList = new GenericList(file);

            //If this is an outdoor level this is the list of models and lights
            if(environmentStruct.type == GFFSTRUCTTYPE.ENV_WORLD_TERRAIN){
                modelList.AddRange(readPropModels(objectList));
                lightList.AddRange(readLights(objectList));
            }

            //if its an indoor room we have to go farther into the data
            else if (environmentStruct.type == GFFSTRUCTTYPE.ENV_WORLD_ROOM)
            {
                //get to the area struct data
                file.BaseStream.Seek(headerFile.dataOffset + objectList[0],SeekOrigin.Begin);

                //now get the reference to the room list and make it
                file.BaseStream.Seek(areaStruct.fields[1].index,SeekOrigin.Current);
                reference = file.ReadInt32();
                file.BaseStream.Seek(headerFile.dataOffset + reference, SeekOrigin.Begin);
                GenericList roomList = new GenericList(file);

                //for each room in the list
                for (int i = 0; i < roomList.length; i++)
                {
                    //seek to the object list
                    file.BaseStream.Seek(headerFile.dataOffset + roomList[i] + roomStruct.fields[ROOM_OBJECT_LIST_INDEX].index, SeekOrigin.Begin);
                    reference = file.ReadInt32();
                    file.BaseStream.Seek(headerFile.dataOffset + reference, SeekOrigin.Begin);
                    //Make the list of objects in the room
                    objectList = new GenericList(file);
                    //Add the models and lights to the lists
                    modelList.AddRange(readPropModels(objectList));
                    lightList.AddRange(readLights(objectList));
                }
            }

            //Make the models array
            _models = modelList.ToArray();
            //Complete the models with model hierarchies
            createMeshHierarchies();

            ModelInstance[] mInstances = new ModelInstance[_models.Length];
            for (int i = 0; i < mInstances.Length; i++)
            {
                BiowareModel mb = _models[i];



            }
            //Make the lights array
            _lights = lightList.ToArray();

            FinishedReading.BeginInvoke(new FinishedReadingEventArgs(true,"Finished Reading Level Data.",this),null,null);
        }

        private List<Light> readLights(GenericList objectList)
        {
            List<Light> lights = new List<Light>();
            BinaryReader file = headerFile.getReader();
            long currentPosition;   //position of begining of light struct (for offsets within struct)

            int type;       //point = 0, ambient = 1, spot = 2                      -The type of the light
            int effect;     //baked = 0, static = 2,  animated = 3, negative = 4;   -How light effects environment
            LightType lightEffect;                                                  //enum for above (used in constructor)
            float intensity = 1;                                                    //intensity of light (colour multiplier)
            Vector3 position;                                                       //position of the light
            Quaternion rotation;                                                    //rotation of the light (only used in spot lights)
            Vector3 colour;                                                         //colour of the light
            float inAngle, outAngle, distance;                                      //spot light values


            //for all the objects in the list
            for (int i = 0; i < objectList.length; i++)
            {
                //if its a light object process it
                if (headerFile.structs[(int)objectList.type[i].id].type == GFFSTRUCTTYPE.LIGHT)
                {
                    //seek to the light struct data
                    file.BaseStream.Seek(headerFile.dataOffset + objectList[i], SeekOrigin.Begin);
                    currentPosition = file.BaseStream.Position;
                    
                    //get the position
                    file.BaseStream.Seek(currentPosition + lightStruct.fields[LIGHT_POSITION_INDEX].index,SeekOrigin.Begin);
                    position = new Vector3(file.ReadSingle(), file.ReadSingle(), file.ReadSingle());

                    //get the rotation
                    file.BaseStream.Seek(currentPosition + lightStruct.fields[LIGHT_ROTATION_INDEX].index, SeekOrigin.Begin);
                    rotation = new Quaternion(file.ReadSingle(), file.ReadSingle(), file.ReadSingle(), file.ReadSingle());

                    //get the colour
                    file.BaseStream.Seek(currentPosition + lightStruct.fields[LIGHT_COLOUR_INDEX].index, SeekOrigin.Begin);
                    colour = new Vector3(file.ReadSingle(), file.ReadSingle(), file.ReadSingle());

                    //get the type
                    file.BaseStream.Seek(currentPosition + lightStruct.fields[LIGHT_TYPE_INDEX].index, SeekOrigin.Begin);
                    type = file.ReadInt32();

                    //get the effect
                    file.BaseStream.Seek(currentPosition + lightStruct.fields[LIGHT_EFFECT_INDEX].index, SeekOrigin.Begin);
                    effect = file.ReadByte();

                    //set the lightEffect variable for easy constructing
                    if (effect == LIGHT_BAKED)
                        lightEffect = LightType.Baked;
                    else if (effect == LIGHT_STATIC)
                        lightEffect = LightType.Static;
                    else
                        continue;

                    //get the in angle
                    file.BaseStream.Seek(currentPosition + lightStruct.fields[LIGHT_INANGLE_INDEX].index, SeekOrigin.Begin);
                    inAngle = file.ReadSingle();

                    //get the out angle
                    file.BaseStream.Seek(currentPosition + lightStruct.fields[LIGHT_OUTANGLE_INDEX].index, SeekOrigin.Begin);
                    outAngle = file.ReadSingle();

                    //get the distance
                    file.BaseStream.Seek(currentPosition + lightStruct.fields[LIGHT_DISTANCE_INDEX].index, SeekOrigin.Begin);
                    distance = file.ReadSingle();

                    //add the light to the lights list
                    switch (type)
                    {
                        case LIGHT_AMBIENT:
                            lights.Add(new AmbientLight(position, colour, intensity, lightEffect)); break;
                        case LIGHT_SPOT:
                            lights.Add(new SpotLight(position, rotation, colour, intensity, inAngle, outAngle, distance, lightEffect)); break;
                        case LIGHT_POINT:
                            lights.Add(new PointLight(position, colour, intensity, lightEffect)); break;
                    }
                }
            }
            return lights;
        }

        private List<BiowareModel> readTerrainModels()
        {

            BinaryReader file = headerFile.getReader();

            int numSectors, reference;
            long startOfList;

            //Seek to the reference to the terrain chunk list and get it
            file.BaseStream.Seek(headerFile.dataOffset + headerFile.structs[TOP_LEVEL_STRUCT_INDEX].fields[TERRAIN_CHUNK_LIST_INDEX].index, SeekOrigin.Begin);
            reference = file.ReadInt32();
            //Seek to the list
            file.BaseStream.Seek(headerFile.dataOffset + reference, SeekOrigin.Begin);
            //Get the lenght of the list
            numSectors = file.ReadInt32();

            //Make a list for the terrain meshes
            List<BiowareModel> terrainModels = new List<BiowareModel>(numSectors);

            int currentSectorFileIndex,currentSectorID = 0;
            uint currentSectorModelID = 1;
            GFF currentSectorFile;
            TerrainMesh currentTerrainMesh;

            startOfList = file.BaseStream.Position;
            for (int i = 0; i < numSectors; i++)
            {
                //Seek to the next struct in the list and get the model id and sector id
                file.BaseStream.Seek(startOfList + (i * terrainChunkStruct.structSize), SeekOrigin.Begin);
                //THIS IS UNSAFE ACCESS TO STRUCT FIELDS!!!!!
                currentSectorModelID = file.ReadUInt32();
                currentSectorID = file.ReadInt32();

                //Get the index of the next sector file
                currentSectorFileIndex = diskFile.indexOf(String.Format("sector{0:0000}.tmsh",currentSectorID));
                //If its not there something is wrong
                if(currentSectorFileIndex < 0)
                {
                    Console.WriteLine("Could not find file \"sector00"+i+".tmsh\" :(");
                }
                //Otherwise read it in
                else
                {
                    currentSectorFile = new GFF(diskFile.path, diskFile.resourceOffsets[currentSectorFileIndex]);
                    currentTerrainMesh = new TerrainMesh(currentSectorFile);
                    terrainModels.Add(currentTerrainMesh.toModel(currentSectorModelID));
                }
            }


            return terrainModels;
        }

        private List<BiowareModel> readPropModels(GenericList objectList)
        {
            List<BiowareModel> models = new List<BiowareModel>();
            BinaryReader file = headerFile.getReader();
            long currentPosition;   //position of beginning of model struct (for offsets within struct

            GenericList propertyList;   //to hold the list of properties of the model
            int reference;              //for referencing structs

            Vector3 position;
            Quaternion rotation;
            String modelFileName;
            int lightmapValue;
            uint modelID;

            //for all the objects in the list
            for (int i = 0; i < objectList.length; i++)
            {
                //if its a model object process it
                if (headerFile.structs[(int)objectList.type[i].id].type == GFFSTRUCTTYPE.MODEL)
                {
                    //seek to the model struct data
                    file.BaseStream.Seek(headerFile.dataOffset + objectList[i], SeekOrigin.Begin);
                    currentPosition = file.BaseStream.Position;

                    //get position
                    file.BaseStream.Seek(currentPosition + modelStruct.fields[MODEL_POSITION_INDEX].index, SeekOrigin.Begin);
                    position = new Vector3(file.ReadSingle(), file.ReadSingle(), file.ReadSingle());

                    //get the rotation
                    file.BaseStream.Seek(currentPosition + modelStruct.fields[MODEL_ROTATION_INDEX].index, SeekOrigin.Begin);
                    rotation = new Quaternion(file.ReadSingle(), file.ReadSingle(), file.ReadSingle(), file.ReadSingle());

                    //get the property List reference
                    file.BaseStream.Seek(currentPosition + modelStruct.fields[MODEL_PROPERTY_INDEX].index + propertyStruct.fields[PROPERTY_CHILDREN_INDEX].index, SeekOrigin.Begin);
                    reference = file.ReadInt32();
                    //seek to the children field  and make the children list
                    file.BaseStream.Seek(headerFile.dataOffset + reference, SeekOrigin.Begin);
                    propertyList = new GenericList(file);
                    
                    // get the reference to the model file name string
                    file.BaseStream.Seek(headerFile.dataOffset + propertyList[MODEL_FILENAME_INDEX] + propertyStruct.fields[PROPERTY_VALUE_INDEX].index, SeekOrigin.Begin);
                    modelFileName = IOUtilities.readECString(file, headerFile.dataOffset + file.ReadInt32()) + ".mmh";

                    //get the lightmap value
                    file.BaseStream.Seek(headerFile.dataOffset + propertyList[MODEL_LIGHTMAPVALUE_INDEX] + propertyStruct.fields[PROPERTY_VALUE_INDEX].index, SeekOrigin.Begin);

                    lightmapValue = Int32.Parse(IOUtilities.readECString(file, headerFile.dataOffset + file.ReadInt32()));

                    //get the ID value
                    file.BaseStream.Seek(currentPosition + modelStruct.fields[MODEL_ID_INDEX].index,SeekOrigin.Begin);
                    modelID = file.ReadUInt32();

                    //add the model to the models list (as normal models)
                    models.Add(new BiowareModel(position,rotation,modelFileName,modelID, false));
                }
            }
            return models;
        }

        private void createMeshHierarchies()
        {
            //Get all the (unique) names for the model files (.mmh)
            HashSet<String> modelNames = new HashSet<String>();
            foreach (BiowareModel m in models)
            {
                if(!m.isTerrain)
                    modelNames.Add(m.modelFileName);
            }

            //Create a list of model hierarchies
            List<ModelHierarchy> modelHierarchies = new List<ModelHierarchy>();

            GFF tempGFF;    //temporary gff file for reading purposes

            //For each of the names find the model files in the erf(or wherever)
            foreach(String name in modelNames)
            {
                //Find the mmh file
                tempGFF = IO.findGFFFile(name, Settings.modelERF);
                //If the file was not found
                if (tempGFF == null)
                {
                    //Print an error and throw an exception
                    Console.WriteLine("Could not find model file \"{0}\".", name);
                    throw new Exception("COULD NOT FIND MODEL FILE, LOOK AT CONSOLE!!!!!!");
                }

                //Add the model hierarchy to the list
                modelHierarchies.Add(new ModelHierarchy(tempGFF));
            }

            //These models well be used for lightmapping, they are simplified and cleaner 
            Model[] realModels = new Model[modelHierarchies.Count];
            //These are the model instances of the above models
            _lightmapModels = new ModelInstance[models.Length];

            //Create the models
            for (int i = 0; i < modelHierarchies.Count; i++)
            {
                realModels[i] = modelHierarchies[i].mesh.toModel();
            }

            //Go through the hierarchies
            for(int i = 0;i<modelHierarchies.Count;i++)
            {
                ModelHierarchy mh = modelHierarchies[i];
                //And for each model
                for(int j = 0;j<models.Length;j++)
                {
                    BiowareModel m = models[j];

                    //if the model is terrain and it hasn't been set yet
                    if (m.isTerrain && m == null)
                    {
                        //Create the terrain lightmap model, the hierarchy has already been set
                        _lightmapModels[j] = new ModelInstance(m.modelFileName, m.hierarchy.mesh.toModel(), m.position, m.rotation, m.modelID);
                    }
                    //Else if the model is a prop model, hasn't been set yet because each has a unique mmh name
                    else if (m.modelFileName == mh.mmhName)
                    {
                        //Set the hierarchy in the bioware model
                        m.hierarchy = mh;
                        //Create the lightmap model
                        _lightmapModels[j] = new ModelInstance(m.modelFileName, realModels[i], m.position, m.rotation,m.modelID);
                    }
                }
            }
        }
        private void setStructDefinitions()
        {
            for (int i = 0; i < headerFile.structs.Length; i++)
            {
                switch (headerFile.structs[i].type)
                {
                    case GFFSTRUCTTYPE.EVN_ROOM:
                        roomStruct = headerFile.structs[i]; break;
                    case GFFSTRUCTTYPE.ENV_WORLD_ROOM:
                        environmentStruct = headerFile.structs[i]; break;
                    case GFFSTRUCTTYPE.ENV_WORLD_TERRAIN:
                        environmentStruct = headerFile.structs[i]; break;
                    case GFFSTRUCTTYPE.ENV_AREA:
                        areaStruct = headerFile.structs[i]; break;
                    case GFFSTRUCTTYPE.LIGHT:
                        lightStruct = headerFile.structs[i]; break;
                    case GFFSTRUCTTYPE.MODEL:
                        modelStruct = headerFile.structs[i]; break;
                    case GFFSTRUCTTYPE.TERRAIN_CHUNK:
                        terrainChunkStruct = headerFile.structs[i]; break;
                    case GFFSTRUCTTYPE.TS_PROPERTY:
                        propertyStruct = headerFile.structs[i]; break;
                }
            }
        }
    }
}
