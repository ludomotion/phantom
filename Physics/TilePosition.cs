using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Physics
{
    /// <summary>
    /// A special class that keeps track of the tile a game entity is on in a tileIntegrator.
    /// </summary>
    internal class TilePosition : EntityComponent
    {
        private TiledIntegrator integrator;

        /// <summary>
        /// The current tile
        /// </summary>
        public TiledIntegrator.Tile Tile;

        /// <summary>
        /// Create an instance for the TilePosition
        /// </summary>
        /// <param name="integrator"></param>
        public TilePosition(TiledIntegrator integrator)
        {
            this.SetIntegrater(integrator);
        }

        /// <summary>
        /// Changes the integrator
        /// </summary>
        /// <param name="integrator"></param>
        public void SetIntegrater(TiledIntegrator integrator)
        {
            this.integrator = integrator;
        }

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            this.SetTile(this.integrator.GetTile(this.Entity.Position));
        }

        public override void OnRemove()
        {
            base.OnRemove();
            if (this.Tile != null)
            {
                // ************** START REMOVE IMPLEMENTATION **************
                // Loop from back to front
                for (int i = this.Tile.entitiesIndex; i > -1; i--)
                {
                    // We found it
                    if (this.Tile.entities[i] == this.Entity)
                    {
                        // Move all elements down
                        for (int j = i; j < this.Tile.entitiesIndex; j++)
                            this.Tile.entities[j] = this.Tile.entities[j + 1];

                        // Remove last element
                        this.Tile.entities[this.Tile.entitiesIndex] = null;

                        // Decrement size of array
                        this.Tile.entitiesIndex--;

                        // Break out of loop
                        break;
                    }
                }
                // ************** END REMOVE IMPLEMENTATION **************
            }
            this.Tile = null;

        }

        /// <summary>
        /// Changes the tile and updates the new and old tiles' entity list
        /// </summary>
        /// <param name="tile"></param>
        public void SetTile(TiledIntegrator.Tile tile)
        {
            if (this.Tile == tile)
                return;
            if (this.Tile != null)
            {
                // ************** START REMOVE IMPLEMENTATION **************
                // Loop from back to front
                for (int i = this.Tile.entitiesIndex; i > -1; i--)
                {
                    // We found it
                    if (this.Tile.entities[i] == this.Entity)
                    {
                        // Move all elements down
                        for (int j = i; j < this.Tile.entitiesIndex; j++)
                            this.Tile.entities[j] = this.Tile.entities[j + 1];

                        // Remove last element
                        this.Tile.entities[this.Tile.entitiesIndex] = null;

                        // Decrement size of array
                        this.Tile.entitiesIndex--;

                        // Break out of loop
                        break;
                    }
                }
                // ************** END REMOVE IMPLEMENTATION **************
            }
            this.Tile = tile;
            if( this.Tile != null )
            {
                // ************** START ADD IMPLEMENTATION **************
                this.Tile.entitiesIndex++;
                if (this.Tile.entitiesIndex >= this.Tile.entities.Length)
                    Array.Resize(ref this.Tile.entities, this.Tile.entities.Length * TiledIntegrator.DefaultArrayIncrease);
                this.Tile.entities[this.Tile.entitiesIndex] = this.Entity;
                // ************** END ADD IMPLEMENTATION **************
            }
        }

        public override void Integrate(float elapsed)
        {
            this.SetTile(this.integrator.GetTile(this.Entity.Position));
            base.Integrate(elapsed);
        }
    }
}
