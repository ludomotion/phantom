﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Phantom.Graphics;

namespace Phantom.Physics
{
    /// <summary>
    /// A TiledIntegrator class is responsible for updating the physics of its entities, and detecting and handling collisions between them.
    /// It divides the world in a number of square tiles to optimize the speed of collision detection. The optimal tile size is the same as the average 
    /// of the lareger game entities. For entities larger than twice the tile size or for two entities that are both larger than 1.5 times the tile size,
    /// the checks become inaccurate.
    /// </summary>
    public class TiledIntegrator : Integrator
    {
        public static int DefaultArraySize = 1;
        public static int DefaultArrayIncrease = 4;

        internal class Tile
        {
            public int X;
            public int Y;

            public int entitiesIndex;
            public Entity[] entities;

            public Tile(int x, int y)
            {
                this.X = x;
                this.Y = y;
                this.entitiesIndex = -1;
                this.entities = new Entity[DefaultArraySize];
            }
        }

        /// <summary>
        /// This array indicates the order in which tiles are checked. For the best effects, the entities on the same tile are checked first
        /// then the tile below, above, to the right and to the left. The diagonal neighbors are checked last.
        /// </summary>
        private readonly int[] neighbors = {
                 0,  0,
                 0,  1,
                 0, -1,
                 1,  0,
                -1,  0,
                 1,  1,
                -1,  1,
                 1, -1,
                -1, -1
            };

        private Layer layer;

        private Dictionary<Entity, TilePosition> positions;

        private float tileSize;
        private float invTileSize;

        /// <summary>
        /// The set tile size
        /// </summary>
        public float TileSize { get { return tileSize; } set { } }
        private Tile[] tiles;
        private int tilesX;
        private int tilesY;
        private int tileCount;

        
        /// <summary>
        /// Creates a new tiledIntegrator
        /// </summary>
        /// <param name="physicsExecutionCount">The number of integration step each frame. For fast paced games with many physics, 4 is good value</param>
        /// <param name="tileSize">The tile size (all tiles are squares), smaller tile perform better, but the collision checks become inaccurate if the entities grow larger than 150% of the tile size.</param>
        public TiledIntegrator(int physicsExecutionCount, float tileSize) : base(physicsExecutionCount)
        {
            this.tileSize = tileSize;
            this.invTileSize = (tileSize == 0f) ? 0f : 1.0f / tileSize;
        }

        /// <summary>
        /// An array of tiles is created when the integrator is added to a layer.
        /// </summary>
        /// <param name="parent"></param>
        public override void OnAdd(Component parent)
        {
#if DEBUG
            if (!(parent is Layer))
            {
                throw new Exception("Please add the TiledIntegrater to a Layer.");
            }
#endif
            this.layer = parent as Layer;
            float w = this.layer.Bounds.X;
            float h = this.layer.Bounds.Y;

            this.tilesX = (int)Math.Ceiling(w * invTileSize);
            this.tilesY = (int)Math.Ceiling(h * invTileSize);
            this.tileCount = this.tilesX * this.tilesY;

            this.tiles = new Tile[this.tileCount];
            for (int i = 0; i < this.tileCount; i++)
                this.tiles[i] = new Tile(i % this.tilesX, i / this.tilesX);

            this.positions = new Dictionary<Entity, TilePosition>();

            base.OnAdd(parent);
        }

        internal override void ChangeSize(Vector2 bounds, bool destroyEntities)
        {
            base.ChangeSize(bounds, destroyEntities);
            float w = bounds.X;
            float h = bounds.Y;

            this.tilesX = (int)Math.Ceiling(w * invTileSize);
            this.tilesY = (int)Math.Ceiling(h * invTileSize);
            this.tileCount = this.tilesX * this.tilesY;

            this.tiles = new Tile[this.tileCount];
            for (int i = 0; i < this.tileCount; i++)
                this.tiles[i] = new Tile(i % this.tilesX, i / this.tilesX);

            for (int i = 0; i < entities.Count; i++)
            {
                entities[i].Integrate(0);
            }
        }


        internal override void OnComponentAddedToLayer(Component component)
        {
            base.OnComponentAddedToLayer(component);
            Entity e = component as Entity;
            if (e != null)
            {
                TilePosition tp = e.GetComponentByType<TilePosition>();
                if (tp == null)
                    e.AddComponent(tp = new TilePosition(this));
                else
                    tp.SetIntegrater(this);

                // ************** START CONTAINS IMPLEMENTATION **************
                // Check if tile contains entity
                bool contains = false;

                // Loop over all the entities
                for (int i = tp.Tile.entitiesIndex; i > -1; i--)
                {
                    if (tp.Tile.entities[i] == e)
                    {
                        contains = true;
                        break;
                    }
                }
                // ************** END CONTAINS IMPLEMENTATION **************

                if (!contains)
                {
                    // ************** START ADD IMPLEMENTATION **************
                    tp.Tile.entitiesIndex++;
                    if (tp.Tile.entitiesIndex >= tp.Tile.entities.Length)
                        Array.Resize(ref tp.Tile.entities, tp.Tile.entities.Length * DefaultArrayIncrease);
                    tp.Tile.entities[tp.Tile.entitiesIndex] = e;
                    // ************** END ADD IMPLEMENTATION **************
                }

                this.positions[e] = tp;
            }
        }

        internal override void OnComponentRemovedToLayer(Component component)
        {

            base.OnComponentRemovedToLayer(component);
            Entity e = component as Entity;
            if (e != null)
            {
                TilePosition tp = e.GetComponentByType<TilePosition>();
                if (tp != null) {

                    // ************** START REMOVE IMPLEMENTATION **************
                    // Loop from back to front
                    for (int i = tp.Tile.entitiesIndex; i > -1; i--)
                    {
                        // We found it
                        if (tp.Tile.entities[i] == e)
                        {
                            // Move all elements down
                            for (int j = i; j < tp.Tile.entitiesIndex; j++)
                                tp.Tile.entities[j] = tp.Tile.entities[j + 1];

                            // Remove last element
                            tp.Tile.entities[tp.Tile.entitiesIndex] = null;

                            // Decrement size of array
                            tp.Tile.entitiesIndex--;

                            // Break out of loop
                            break;
                        }
                    }
                    // ************** END REMOVE IMPLEMENTATION **************

                    this.positions.Remove(e);
                }
                this.positions[e] = tp;
            }
        }


        protected override void CheckEntityCollision(Entity e)
        {
            if (e.Shape == null || !e.InitiateCollision)
                return;

            TilePosition tp = this.positions[e];
            Tile t = tp.Tile;
            int minX = Math.Max(t.X - 1, 0);
            int maxX = Math.Min(t.X + 1, this.tilesX-1);
            int minY = Math.Max(t.Y - 1, 0);
            int maxY = Math.Min(t.Y + 1, this.tilesY-1);

            for (int i = 0; i < neighbors.Length; i += 2)
            {
                int x = t.X + neighbors[i];
                int y = t.Y + neighbors[i+1];
                if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                {
                    Tile tt = this.tiles[y * this.tilesX + x];
                    for (int j = 0; j < tt.entitiesIndex + 1; j++)
                    {
                        Entity o = tt.entities[j];
                        if (e != o && !o.Destroyed && o.Shape != null && (!o.InitiateCollision || e.ID > o.ID || o.UpdateBehaviour == Entity.UpdateBehaviours.NeverUpdate))
                            this.CheckCollisionBetween(e, o);
                    }
                }

            }
        }
        
        /*/
        /// <summary>
        /// Renders the tile grid
        /// </summary>
        /// <param name="info"></param>
        public override void Render(Graphics.RenderInfo info)
        {
            base.Render(info);
            Canvas c = info.Canvas;
            if (c == null)
                return;
            for (int y = 0; y < this.tilesY; y++)
            {
                c.StrokeColor = Color.White;
                c.LineWidth = 1;
                c.FillColor = Color.White;
                c.FillColor.A = 128;
                for (int x = 0; x < this.tilesX; x++)
                {
                    c.Begin();
                    c.MoveTo(x * tileSize, y * tileSize);
                    c.LineTo((x + 1) * tileSize, y * tileSize);
                    c.LineTo((x + 1) * tileSize, (y + 1) * tileSize);
                    c.LineTo((x) * tileSize, (y + 1) * tileSize);
                    c.LineTo(x * tileSize, y * tileSize);
                    if (this.tiles[y * this.tilesX + x].Entities.Count > 0)
                        c.Fill();
                    c.Stroke();
                }
            }
        }
        //*/

        /// <summary>
        /// Find and return the tile at the specified position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        internal Tile GetTile(Vector2 position)
        {
            //TODO: escape is for debug purposes. Needs to be removed 
            if (float.IsNaN(position.X) || float.IsNaN(position.Y))
                return this.tiles[0];
            
            int x = (int)MathHelper.Clamp(position.X * this.invTileSize, 0, this.tilesX - 1);
            int y = (int)MathHelper.Clamp(position.Y * this.invTileSize, 0, this.tilesY - 1);
            return this.tiles[y * this.tilesX + x];
        }

        public override Entity GetEntityAt(Vector2 position)
        {
            int tX = (int)(position.X * this.invTileSize);
            int tY = (int)(position.Y * this.invTileSize);
            int minX = Math.Max(tX - 1, 0);
            int maxX = Math.Min(tX + 1, this.tilesX - 1);
            int minY = Math.Max(tY - 1, 0);
            int maxY = Math.Min(tY + 1, this.tilesY - 1);

            for (int i = 0; i < neighbors.Length; i += 2)
            {
                int x = tX + neighbors[i];
                int y = tY + neighbors[i + 1];
                if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                {
                    Tile tt = this.tiles[y * this.tilesX + x];

                    for (int j = 0; j < tt.entitiesIndex + 1; j++)
                    {
                        Entity o = tt.entities[j];
                        if (!o.Destroyed && o.Shape != null && o.Shape.InShape(position))
                            return o;
                    }
                }

            }
            return null;
        }

        public override List<Entity> GetEntitiesAt(Vector2 position)
        {
            List<Entity> result = new List<Entity>();
            int tX = (int)(position.X * this.invTileSize);
            int tY = (int)(position.Y * this.invTileSize);
            int minX = Math.Max(tX - 1, 0);
            int maxX = Math.Min(tX + 1, this.tilesX - 1);
            int minY = Math.Max(tY - 1, 0);
            int maxY = Math.Min(tY + 1, this.tilesY - 1);

            for (int i = 0; i < neighbors.Length; i += 2)
            {
                int x = tX + neighbors[i];
                int y = tY + neighbors[i + 1];
                if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                {
                    Tile tt = this.tiles[y * this.tilesX + x];

                    for (int j = 0; j < tt.entitiesIndex + 1; j++)
                    {
                        Entity o = tt.entities[j];
                        if (!o.Destroyed && o.Shape != null && o.Shape.InShape(position))
                            result.Add(o);
                    }
                }
            }
            return result;
        }

        public override IEnumerable<Entity> GetEntitiesInRect(Vector2 topLeft, Vector2 bottomRight, bool partial)
        {
            int tX1 = (int)(topLeft.X * this.invTileSize);
            int tY1 = (int)(topLeft.Y * this.invTileSize);
            int tX2 = (int)(bottomRight.X * this.invTileSize);
            int tY2 = (int)(bottomRight.Y * this.invTileSize);
            int minX = Math.Max(tX1 - 1, 0);
            int maxX = Math.Min(tX2 + 1, this.tilesX - 1);
            int minY = Math.Max(tY1 - 1, 0);
            int maxY = Math.Min(tY2 + 1, this.tilesY - 1);

			for (int y = minY; y <= maxY; y++)
			{
				for (int x = minX; x <= maxX; x++)
				{
					Tile tt = this.tiles[y * this.tilesX + x];
                    for (int i = 0; i < tt.entitiesIndex + 1; i++)
                    {
                        Entity o = tt.entities[i];
                        if (!o.Destroyed && o.Shape != null && o.Shape.InRect(topLeft, bottomRight, partial))
                            yield return o;
                    }
				}
			}
        }

        public override List<Entity> GetEntitiesInRectAsList(Vector2 topLeft, Vector2 bottomRight, bool partial)
        {
            List<Entity> listOfEntitiesInRect = GetListFromPool();
            int tX1 = (int)(topLeft.X * this.invTileSize);
            int tY1 = (int)(topLeft.Y * this.invTileSize);
            int tX2 = (int)(bottomRight.X * this.invTileSize);
            int tY2 = (int)(bottomRight.Y * this.invTileSize);
            int minX = Math.Max(tX1 - 1, 0);
            int maxX = Math.Min(tX2 + 1, this.tilesX - 1) + 1;
            int minY = Math.Max(tY1 - 1, 0);
            int maxY = Math.Min(tY2 + 1, this.tilesY - 1) + 1;

            // Initialize variables at top just to be sure
            Tile tile;
            Entity entity;

            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    tile = this.tiles[y * this.tilesX + x];
                    
                    for (int i = 0; i < tile.entitiesIndex + 1; i++)
                    {
                        entity = tile.entities[i];
                        if (!(entity.Destroyed || entity.Shape == null || !entity.Shape.InRect(topLeft, bottomRight, partial)))
                            listOfEntitiesInRect.Add(entity);
                    }
                }
            }
            return listOfEntitiesInRect;
        }

        public ReadOnlySpan<Entity> GetEntitiesInRectAsReadOnlySpan(Vector2 topLeft, Vector2 bottomRight, bool partial)
        {
            //List<Entity> listOfEntitiesInRect = GetListFromPool();
            int tX1 = (int)(topLeft.X * this.invTileSize);
            int tY1 = (int)(topLeft.Y * this.invTileSize);
            int tX2 = (int)(bottomRight.X * this.invTileSize);
            int tY2 = (int)(bottomRight.Y * this.invTileSize);
            int minX = Math.Max(tX1 - 1, 0);
            int maxX = Math.Min(tX2 + 1, this.tilesX - 1) + 1;
            int minY = Math.Max(tY1 - 1, 0);
            int maxY = Math.Min(tY2 + 1, this.tilesY - 1) + 1;

            // We have no idea how many results we will have
            // But since this function is called so much
            // We will use a default size of 64 to avoid resizing
            // And multiply by 16 if there are more results
            Entity[] lst = new Entity[64];
            int index = -1;

            // Initialize variables at top just to be sure
            Tile tile;
            Entity entity;

            // Loop over results
            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    // Get the tile
                    tile = this.tiles[y * this.tilesX + x];

                    // Loop over the entities
                    for (int i = 0; i < tile.entitiesIndex + 1; i++)
                    {
                        entity = tile.entities[i];
                        if(!(entity.Destroyed || entity.Shape == null || !entity.Shape.InRect(topLeft, bottomRight, partial)))
                        {
                            // ************** START ADD IMPLEMENTATION **************
                            index++;
                            if (index >= lst.Length)
                                Array.Resize(ref lst, lst.Length * 16);
                            lst[index] = entity;
                            // ************** END ADD IMPLEMENTATION **************
                        }
                    }
                }
            }

            // Return result
            return new ReadOnlySpan<Entity>(lst, 0, index + 1);
        }

        public override Entity GetEntityCloseTo(Vector2 position, float distance)
        {
            int tX = (int)(position.X * this.invTileSize);
            int tY = (int)(position.Y * this.invTileSize);
            int minX = Math.Max(tX - 1, 0);
            int maxX = Math.Min(tX + 1, this.tilesX - 1);
            int minY = Math.Max(tY - 1, 0);
            int maxY = Math.Min(tY + 1, this.tilesY - 1);

            for (int i = 0; i < neighbors.Length; i += 2)
            {
                int x = tX + neighbors[i];
                int y = tY + neighbors[i + 1];
                if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                {
                    Tile tt = this.tiles[y * this.tilesX + x];
                    for (int j = 0; j < tt.entitiesIndex + 1; j++)
                    {
                        Entity o = tt.entities[j];
                        if (!o.Destroyed && o.Shape != null && o.Shape.DistanceTo(position).LengthSquared() < distance * distance)
                            return o;
                    }
                }
            }
            return null;
        }

    }
}
