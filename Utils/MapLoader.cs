using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom;
using Phantom.Core;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Phantom.Misc;
using System.Globalization;

#if TOUCH
using Trace = System.Console;
#endif

namespace Phantom.Utils
{
    public static class MapLoader
    {
        public static List<PCNComponent> TileList;
        public static Dictionary<string, PCNComponent> EntityList;
        public static float TileSize;
        //private static string filename;

        public static Dictionary<string, string> EmbeddedFiles;


        public static void Initialize(float tileSize)
        {
            TileSize = tileSize;
            TileList = new List<PCNComponent>();
            EntityList = new Dictionary<string, PCNComponent>();
            EmbeddedFiles = new Dictionary<string,string>();
            PhantomGame.Game.Console.Register("savemap", "Saves the current map to file.", delegate(string[] argv)
            {
                if (argv.Length < 2)
                    Trace.WriteLine("Filename expected!");
                else
                    MapLoader.SaveMap(argv[1]);
            });
            PhantomGame.Game.Console.Register("storemap", "Saves the current map to memory.", delegate(string[] argv)
            {
                if (argv.Length < 2)
                    Trace.WriteLine("Filename expected!");
                else
                    MapLoader.StoreMap(argv[1]);
            });
            PhantomGame.Game.Console.Register("openmap", "Loads the current map to file.", delegate(string[] argv)
            {
                if (argv.Length < 2)
                    Trace.WriteLine("Filename expected!");
                if (argv.Length == 2)
                    MapLoader.OpenMap(argv[1], null);
                else
                    MapLoader.OpenMap(argv[1], argv[2]);
            });
        }

        public static void SaveMap(string filename)
        {
            GameState state = GetState();
            //Reset map first
            state.HandleMessage(Messages.MapReset, null);

            string data = GetMapData();
            File.WriteAllText(filename, data);
            Trace.WriteLine("Map saved.");

            if (state.Properties == null)
                state.Properties = new PropertyCollection();
            state.Properties.Objects["filename"] = filename;
        }

        public static void StoreMap(string filename)
        {
            GameState state = GetState();
            string data = GetMapData();
            EmbeddedFiles[filename] = data;
            Trace.WriteLine("Map stored.");

            if (state.Properties == null)
                state.Properties = new PropertyCollection();
            state.Properties.Objects["filename"] = filename;
        }

        public static void OpenMap(string filename)
        {
            OpenMap(filename, null);
        }

        public static void OpenMap(string filename, string option)
        {
            string data = null;
            if (option != "file" && EmbeddedFiles.ContainsKey(filename))
            {
                data = EmbeddedFiles[filename];
                Trace.WriteLine("Map recovered fromm memory.");
            }
            if (option != "stored")
            {
                if (System.IO.File.Exists(filename))
                    data = System.IO.File.ReadAllText(filename);
                Trace.WriteLine("Map opened from file.");
            }
            if (data != null)
            {
                ParseData(data);
                GameState state = GetState();
                state.HandleMessage(Messages.MapLoaded, null);
                if (state.Properties == null)
                    state.Properties = new PropertyCollection();
                state.Properties.Objects["filename"] = filename;
            }
            else
                Trace.WriteLine("Map not found!");
        }

        private static GameState GetState()
        {
            GameState result = PhantomGame.Game.CurrentState;
            if (result is Editor.EditorState)
                result = ((Editor.EditorState)result).Editor.Editing;
            return result;
        }

        private static string GetMapData()
        {
            string data = "";
            GameState map = GetState();
            foreach (Component component in map.Components)
            {
                if (component.Properties != null && component.Properties.GetString("editable", null) != null)
                {
                    data += component.Properties.GetString("editable", "") + "("+PhantomComponentNotation.PropertiesToPCNString(component.Properties) + ")\n";

                    if (component is EntityLayer)
                    {
                        foreach (KeyValuePair<string, PCNComponent> entityType in EntityList)
                            data += "entityType " + entityType.Key + " " + entityType.Value.ToString() + "\n";

                        data += GetEntityData((EntityLayer)component) + "\n";
                    }
                }
            }
            return data;
        }

        public static Entity[] ConstructTileMap(EntityLayer entityLayer)
        {
            int width = (int)Math.Ceiling(entityLayer.Bounds.X/TileSize);
            int height = (int)Math.Ceiling(entityLayer.Bounds.Y/TileSize);
            Entity[] result = new Entity[width*height];

            foreach (Component component in entityLayer.Components)
            {
                Entity entity = component as Entity;
                if (entity != null)
                {
                    if (entity.Properties.GetInt("isTile", 0) > 0)
                    {
                        int x = (int)MathHelper.Clamp((int)Math.Floor(entity.Position.X / TileSize), 0, width-1);
                        int y = (int)MathHelper.Clamp((int)Math.Floor(entity.Position.Y / TileSize), 0, height - 1);
                        result[x + y * width] = entity;
                    }
                }
            }

            return result;

        }

        private static string GetEntityData(EntityLayer entityLayer)
        {
            string data = "";
            if (entityLayer.Properties.GetBoolean("tiles", true))
            {
                int width = (int)Math.Ceiling(entityLayer.Bounds.X / TileSize);
                int height = (int)Math.Ceiling(entityLayer.Bounds.Y / TileSize);
                data += "tiles [";
                for (int i = 0; i < TileList.Count; i++)
                {
                    if (i > 0) 
                        data += " ";
                    data += TileList[i].ToString();
                }
                data += "]\n";

                Entity[] tileMap = ConstructTileMap(entityLayer);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int tile = -1;
                        int index = x + y * width;
                        if (tileMap[index] != null)
                        {
                            for (int i = 0; i < TileList.Count; i++)
                            {
                                if (TileList[i].Name == tileMap[index].GetType().Name)
                                {
                                    tile = i;
                                    break;
                                }

                            }
                        }
                        string t = tile.ToString();
                        while (t.Length < 3) t = " " + t;
                        data += t + " ";
                    }
                    data += '\n';
                }
            }

            foreach (Component component in entityLayer.Components)
            {
                Entity entity = component as Entity;
                if (entity != null && entity.Properties.GetInt("isTile", 0) == 0)
                        data += EntityString(entity)+ "\n";
            }
            return data;
        }

        public static string EntityString(Entity entity)
        {
            string e = "entity " + EntityFactory.InstanceToPCNString(entity);
            return e;
        }

        private static void ParseProperties(Component component, PCNComponent description, int firstValue)
        {
            for (int i = firstValue; i < description.Members.Count; i++)
            {
                if (description.Members[i].Value is int)
                    component.Properties.SetInt(description.Members[i].Name, (int)description.Members[i].Value);
                else if (description.Members[i].Value is float)
                    component.Properties.SetFloat(description.Members[i].Name, (float)description.Members[i].Value);
                else
                    component.Properties.SetObject(description.Members[i].Name, description.Members[i].Value);
            }

            if (description.Members.Count >= firstValue)
                component.HandleMessage(Messages.PropertiesChanged, null);
        }

        private static void ParseData(string data)
        {
            string[] lines = data.Split('\n');
            ClearState();
            int i = 0;
            GameState state = GetState();
            EntityLayer entities = null;
            while (i < lines.Length)
            {
                if (lines[i].StartsWith("tiles ") && entities != null)
                {
                    ParseTiles(lines, ref i, entities);
                }
                else if (lines[i].StartsWith("entity ") && entities != null)
                {
                    ParseEntity(lines[i].Substring(7), entities);
                    i++;
                }
                else if (lines[i].StartsWith("entityType "))
                {
                    ParseEntityType(lines[i].Substring(11));
                    i++;
                }
                else if (lines[i].StartsWith("component "))
                {
                    PCNComponent component = new PCNComponent(lines[i].Substring(10));
                    for (int j = 0; j < state.Components.Count; j++)
                    {
                        if (state.Components[j].Properties != null && state.Components[j].Properties.GetString("editable", "") == component.Name)
                        {
                            ParseProperties(state.Components[j], component, 0);
                            if (state.Components[j] is EntityLayer)
                                entities = state.Components[j] as EntityLayer;
                        }
                    }
                    i++;
                }
                else
                {
                    Trace.WriteLine("WARNING: MapLoader did not understand: " + lines[i]);
                    i++;
                }
            }
        }

        private static void ParseTiles(string[] lines, ref int index, EntityLayer entities)
        {
            //string[] parameters = lines[index].Split(' ');
            PCNComponent tileDefs = new PCNComponent(lines[index]);
            index++;

            TileList = tileDefs.Components;

            int width = (int)Math.Ceiling(entities.Bounds.X / TileSize);
            int height = (int)Math.Ceiling(entities.Bounds.Y / TileSize);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    while (lines[index].Length == 0) index++;
                    string tile = lines[index].Substring(x * 4, 3);
                    int t = -1;
                    if (int.TryParse(tile, out t) && t>=0 && t<TileList.Count)
                        AddTile(x, y, TileList[t], entities);
                }
                index++;
            }
        }

        private static void AddTile(int x, int y, PCNComponent blueprint, EntityLayer entities)
        {
            Vector2 position = new Vector2((x + 0.5f) * TileSize, (y + 0.5f) * TileSize);
            Entity entity = EntityFactory.AssembleEntity(blueprint);
            entity.Position = position;
            entity.Properties.Ints["isTile"] = 1;
            entities.AddComponent(entity);
        }

        private static void ParseEntity(string description, EntityLayer entities)
        {
            PCNComponent instance = new PCNComponent(description);
            PCNComponent blueprint = EntityList[instance.Name];
            Entity entity = EntityFactory.BuildInstance(blueprint, instance);
            entities.AddComponent(entity);
        }

        private static void ParseEntityType(string description)
        {
            int p = description.IndexOf(' ');
            if (p < 0)
            {
                PCNComponent blueprint = new PCNComponent(description);
                EntityList[blueprint.Name] = blueprint;
            }
            else
            {
                PCNComponent blueprint = new PCNComponent(description.Substring(p+1));
                EntityList[description.Substring(0, p)] = blueprint;

            }
        }



        private static void ClearState()
        {
            GameState state = GetState();
            foreach (Component c in state.Components)
            {
                if (c is EntityLayer)
                    ((EntityLayer)c).ClearComponents();
            }
        }

    }
}
