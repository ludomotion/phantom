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
    /// <summary>
    /// Static class that handles loading and saving of maps from and to PCN format.
    /// </summary>
    public static class MapLoader
    {
        /// <summary>
        /// A dictionary containing dictionaries for entityLists
        /// </summary>
        public static Dictionary<string, Dictionary<string, PCNComponent>> EntityLists;

        private static char[] lineSplit = new char[] { '\n', '\r' };

        public static void Initialize()
        {
            EntityLists = new Dictionary<string, Dictionary<string, PCNComponent>>();

            PhantomGame.Game.Console.Register("savemap", "Saves the current map to file.", delegate(string[] argv)
            {
                if (argv.Length < 2)
                    Trace.WriteLine("Filename expected!");
                else
                    MapLoader.SaveMap(argv[1]);
            });
            PhantomGame.Game.Console.Register("openmap", "Loads the current map to file.", delegate(string[] argv)
            {
                if (argv.Length < 2)
                    Trace.WriteLine("Filename expected!");
                if (argv.Length >= 2)
                    MapLoader.OpenMap(argv[1]);
            });
        }

        public static void OpenEntityList(string listname, string filename)
        {
            string data = null;
            if (System.IO.File.Exists(filename))
                data = System.IO.File.ReadAllText(filename);
            if (data != null)
            {
                Dictionary<string, PCNComponent> entities = new Dictionary<string, PCNComponent>();
                string[] lines = data.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    int p = lines[i].IndexOf(' ');
                    if (p > 0)
                    {
                        string name = lines[i].Substring(0, p);
                        entities[name] = new PCNComponent(lines[i].Substring(p + 1));
                    }
                }
                EntityLists[listname] = entities;
            }
            else
                Trace.WriteLine("WARNING List not found! "+filename);
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


        
        public static void OpenMap(string filename)
        {
            string data = null;
            if (System.IO.File.Exists(filename))
            {
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
                    data += "component "+component.Properties.GetString("editable", "") + "("+PhantomComponentNotation.PropertiesToPCNString(component.Properties) + ")\n";

                    if (component is EntityLayer)
                    {
                        data += GetEntityData((EntityLayer)component) + "\n";
                    }
                }
            }
            return data;
        }

        public static Entity[] ConstructTileMap(EntityLayer entityLayer)
        {
            int tileSize = entityLayer.Properties.GetInt("tileSize", 0);
            if (tileSize <= 0)
                return new Entity[] {};
            int width = (int)Math.Ceiling(entityLayer.Bounds.X / tileSize);
            int height = (int)Math.Ceiling(entityLayer.Bounds.Y / tileSize);
            Entity[] result = new Entity[width*height];

            foreach (Component component in entityLayer.Components)
            {
                Entity entity = component as Entity;
                if (entity != null)
                {
                    if (entity.Properties.GetInt("isTile", 0) > 0)
                    {
                        int x = (int)MathHelper.Clamp((int)Math.Floor(entity.Position.X / tileSize), 0, width-1);
                        int y = (int)MathHelper.Clamp((int)Math.Floor(entity.Position.Y / tileSize), 0, height - 1);
                        result[x + y * width] = entity;
                    }
                }
            }

            return result;

        }

        private static string GetEntityData(EntityLayer entityLayer)
        {
            string data = "";
            string list = entityLayer.Properties.GetString("tileList", "");
            int tileSize = entityLayer.Properties.GetInt("tileSize", 0);
            if (list != "" && tileSize>0)
            {
                int width = (int)Math.Ceiling(entityLayer.Bounds.X / tileSize);
                int height = (int)Math.Ceiling(entityLayer.Bounds.Y / tileSize);
                data += "tiles [";
                Dictionary<string, int> tileList = new Dictionary<string, int>();
                int c = 0;
                foreach(KeyValuePair<string, PCNComponent> tile in EntityLists[list])
                {
                    if (c > 0) 
                        data += ", ";
                    data += tile.Key;
                    tileList[tile.Key] = c;
                    c++;
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
                            string n = tileMap[index].Properties.GetString(EntityFactory.PROPERTY_NAME_BLUEPRINT, "");
                            if (tileList.ContainsKey(n))
                                tile = tileList[n];
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
            string[] lines = data.Split(lineSplit, StringSplitOptions.RemoveEmptyEntries);
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
                    if (lines[i].Length>0) 
                        Trace.WriteLine("WARNING: MapLoader did not understand: " + lines[i]);
                    i++;
                }
            }
        }

        private static void ParseTiles(string[] lines, ref int index, EntityLayer entities)
        {
            string tileListName = entities.Properties.GetString("tileList", "");
            Dictionary<string, PCNComponent> tileList = EntityLists[tileListName];
            PCNComponent tileDefs = new PCNComponent(lines[index]);
            index++;

            int tileSize = entities.Properties.GetInt("tileSize", 0);
            if (tileSize <= 0)
                return;

            int width = (int)Math.Ceiling(entities.Bounds.X / tileSize);
            int height = (int)Math.Ceiling(entities.Bounds.Y / tileSize);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    while (lines[index].Length == 0) index++;
                    string tile = lines[index].Substring(x * 4, 3);
                    int t = -1;
                    if (int.TryParse(tile, out t) && t >= 0 && t < tileDefs.Components.Count)
                        AddTile(x, y, tileSize, tileList[tileDefs.Components[t].Name], entities, tileDefs.Components[t].Name);
                }
                index++;
            }
        }

        private static void AddTile(int x, int y, int tileSize, PCNComponent blueprint, EntityLayer entities, string blueprintName)
        {

            Vector2 position = new Vector2((x + 0.5f) * tileSize, (y + 0.5f) * tileSize);
            Entity entity = EntityFactory.AssembleEntity(blueprint, blueprintName);
            entity.Position = position;
            entity.Properties.Ints["isTile"] = 1;
            entities.AddComponent(entity);
        }

        private static void ParseEntity(string description, EntityLayer entities)
        {
            string listName = entities.Properties.GetString("entityList", "");
            PCNComponent instance = new PCNComponent(description);
            PCNComponent blueprint = EntityLists[listName][instance.Name];
            Entity entity = EntityFactory.BuildInstance(blueprint, instance, instance.Name);
            if (entity.Properties.GetBoolean("use_blueprint", false))
                entity.Properties.SetBoolean("use_blueprint", false);
            entities.AddComponent(entity);
        }

        private static void ClearState()
        {
            GameState state = GetState();
            foreach (Component c in state.Components)
            {
                if (c is EntityLayer)
                {
                    for (int i = c.Components.Count - 1; i >= 0; i--)
                    {
                        if (c.Components[i] is Entity)
                            c.RemoveComponent(c.Components[i]);
                    }
                }
            }
        }

    }
}
