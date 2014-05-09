using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;

namespace Phantom.Utils
{
    public static class EntityMap
    {
        private static float tileSize;
        private static int mapWidth;
        private static int mapHeight;
        private static Type[] classes;
        private static bool initialized = false;

        public static void Initialize(float tileSize, int mapWidth, int mapHeight, params Type[] classes)
        {
            EntityMap.tileSize = tileSize;
            EntityMap.mapWidth = mapWidth;
            EntityMap.mapHeight = mapHeight;
            EntityMap.classes = classes;
            EntityMap.initialized = true;
        }

        public static void PopulateEntityLayer(EntityLayer layer, int[] data)
        {
            if (!initialized)
                throw new Exception("EntityMap mus be initialized first!");

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    int i = x + y * mapWidth;
                    if (i < data.Length)
                        SpawnEntity(layer, data[i], x, y);
                }
            }
        }

        private static void SpawnEntity(EntityLayer layer, int entityIndex, int x, int y)
        {
            if (entityIndex < 0 || entityIndex >= classes.Length || classes[entityIndex] == null)
                return;
            Vector2 position = new Vector2((x + 0.5f) * tileSize, (y + 0.5f) * tileSize);
            Entity entity = (Entity)Activator.CreateInstance(classes[entityIndex], position);
            layer.AddComponent(entity);
        }
    }
}
