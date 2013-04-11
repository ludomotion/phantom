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

        /// <summary>
        /// Changes the tile and updates the new and old tiles' entity list
        /// </summary>
        /// <param name="tile"></param>
        public void SetTile(TiledIntegrator.Tile tile)
        {
            if (this.Tile == tile)
                return;
            if (this.Tile != null)
                this.Tile.Entities.Remove(this.Entity);
            this.Tile = tile;
            if( this.Tile != null )
                this.Tile.Entities.Add(this.Entity);
        }

        public override void Integrate(float elapsed)
        {
            this.SetTile(this.integrator.GetTile(this.Entity.Position));
            base.Integrate(elapsed);
        }

    }
}
