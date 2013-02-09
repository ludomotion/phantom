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

namespace Phantom.Utils
{
    public class MapLoader
    {
        public static Type[] TileList;
        public static Type[] EntityList;
        public static float TileSize;
        //private static string filename;

        public static Dictionary<string,string> EmbeddedFiles;


        public static void Initialize(float tileSize, Type[] tiles, Type[] entities)
        {
            MapLoader.EntityList = entities;
            MapLoader.TileList = tiles;

            MapLoader.TileSize = tileSize;
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
                    data += component.Properties.GetString("editable", "") + GetProperties(component.Properties) + '\n';

                    if (component is EntityLayer)
                        data += GetEntityData((EntityLayer)component) + '\n';
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
                data += "tiles";
                for (int i = 0; i < TileList.Length; i++)
                    data += " " + ShortTypeName(TileList[i]);
                data += "\n";

                Entity[] tileMap = ConstructTileMap(entityLayer);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int tile = -1;
                        int index = x + y * width;
                        if (tileMap[index] != null)
                        {
                            for (int i = 0; i < TileList.Length; i++)
                            {
                                if (TileList[i] == tileMap[index].GetType())
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
            string e = "entity " + ShortTypeName(entity.GetType()) + " " + Math.Round(entity.Position.X) + " " + Math.Round(entity.Position.Y) + " " + Math.Round(MathHelper.ToDegrees(entity.Orientation));
            e += GetProperties(entity.Properties);
            return e;
        }

        private static string GetProperties(PropertyCollection properties)
        {
            string result = "";
            foreach (KeyValuePair<string, int> property in properties.Ints)
            {
                if (property.Key[0] >= 'A' && property.Key[0] <= 'Z')
                    result += " " + property.Key + "=" + property.Value.ToString();
            }
            foreach (KeyValuePair<string, float> property in properties.Floats)
            {
                if (property.Key[0] >= 'A' && property.Key[0] <= 'Z')
                    result += " " + property.Key + "=" + property.Value.ToString()+"f";
            }
            foreach (KeyValuePair<string, object> property in properties.Objects)
            {
                if (property.Key[0] >= 'A' && property.Key[0] <= 'Z')
                {
                    //TODO Escape spaces in a string
                    if (property.Value is string)
                        result += " " + property.Key + "=\"" + (string)property.Value + "\"";
                    //TODO Creates Hex Representation?
                    if (property.Value is Color)
                        result += " " + property.Key + "=#" + ((Color)property.Value).A.ToString("X2") +((Color)property.Value).R.ToString("X2") +((Color)property.Value).G.ToString("X2") +((Color)property.Value).B.ToString("X2"); 
                }
            }
            return result;
        }

        private static void ParseProperties(Component component, string[] values, int firstValue)
        {
            for (int i = firstValue; i < values.Length; i++)
            {
                string[] member = values[i].Split('=');
                if (member.Length >= 2)
                {
                    if (member[1].EndsWith("f"))
                    {
                        //float
                        float f = 0;
                        float.TryParse(member[1].Substring(0, member[1].Length - 1), out f);
                        component.Properties.Floats[member[0]] = f;
                    }
                    else if (member[1].StartsWith("\""))
                    {
                        //string
                        //TODO Unescape spaces
                        component.Properties.Objects[member[0]] = member[1].Substring(1, member[1].Length - 2);
                    }
                    else if (member[1].StartsWith("#"))
                    {
                        //color
                        int c = 0;
                        int.TryParse(member[1].Substring(1, member[1].Length - 1), NumberStyles.HexNumber, null, out c);
                        component.Properties.Objects[member[0]] = c.ToColor();
                    }
                    else
                    {
                        //int
                        int j = 0;
                        int.TryParse(member[1], out j);
                        component.Properties.Ints[member[0]] = j;
                    }
                }
            }

            if (values.Length >= firstValue)
                component.HandleMessage(Messages.PropertiesChanged, null);

        }

        private static void ParseData(string data)
        {
            string[] lines = data.Split('\n', '\r');
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
                else if (lines[i].StartsWith("entity ") && entities!=null)
                {
                    ParseEntity(lines[i], entities);
                    i++;
                }
                else
                {
                    string[] values = lines[i].Split(' ');
                    if (values.Length > 1)
                    {
                        for (int j = 0; j < state.Components.Count; j++)
                        {
                            if (state.Components[j].Properties != null && state.Components[j].Properties.GetString("editable", "") == values[0])
                            {
                                ParseProperties(state.Components[j], values, 1);
                                if (state.Components[j] is EntityLayer)
                                    entities = state.Components[j] as EntityLayer;
                            }
                        }
                    }
                    i++;
                }
            }
        }

        private static void ParseTiles(string[] lines, ref int index, EntityLayer entities)
        {
            string[] parameters = lines[index].Split(' ');
            index++;

            Dictionary<int, Type> types = new Dictionary<int, Type>();
            for (int i = 1; i < parameters.Length; i++)
            {
                for (int j =0; j<TileList.Length; j++)
                    if (ShortTypeName(TileList[j]) == parameters[i])
                    {
                        types[i-1] = TileList[j];
                        break;
                    }
            }

            int width = (int)Math.Ceiling(entities.Bounds.X / TileSize);
            int height = (int)Math.Ceiling(entities.Bounds.Y / TileSize);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    while (lines[index].Length == 0) index++;
                    string tile = lines[index].Substring(x * 4, 3);
                    int t = -1;
                    if (int.TryParse(tile, out t) && types.ContainsKey(t))
                        AddTile(x, y, types[t], entities);
                }
                index++;
            }
        }

        private static void AddTile(int x, int y, Type tile, EntityLayer entities)
        {
            Vector2 position = new Vector2((x + 0.5f) * TileSize, (y + 0.5f) * TileSize);

            Entity entity = (Entity)Activator.CreateInstance(tile, position);
            entity.Properties.Ints["isTile"] = 1;
            entities.AddComponent(entity);
        }

        private static void ParseEntity(string line, EntityLayer entities)
        {
            string[] parameters = line.Split(' ');
            if (parameters.Length<5) {
                Trace.WriteLine("Entity requires at least 4 parameters (type x y orientation).");
                return;
            }
            Vector2 position = new Vector2();
            if (!float.TryParse(parameters[2], out position.X))
            {
                Trace.WriteLine("Error parsing entity x.");
                return;
            }
            if (!float.TryParse(parameters[3], out position.Y))
            {
                Trace.WriteLine("Error parsing entity y."); 
                return;
            }
            float orientation = 0;
            if (!float.TryParse(parameters[4], out orientation))
            {
                Trace.WriteLine("Error parsing entity orientation.");
                return;
            }
            int type = -1;
            for (int i = 0; i < EntityList.Length; i++)
            {
                if (ShortTypeName(EntityList[i]) == parameters[1])
                {
                    type =i;
                    break;
                }
            }

            if (type < 0)
            {
                Trace.WriteLine("Could not find entity type "+parameters[1]+".");
                return;
            }

            Entity entity = (Entity)Activator.CreateInstance(EntityList[type], position);
            entity.Orientation = MathHelper.ToRadians(orientation);
            entities.AddComponent(entity);
            ParseProperties(entity, parameters, 5);
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

        public static string ShortTypeName(Type type)
        {
            if (type == null)
                return "<null>";
            string result = type.ToString();
            result = result.Substring(result.LastIndexOf('.') + 1);
            return result;
        }

        


    }
}
