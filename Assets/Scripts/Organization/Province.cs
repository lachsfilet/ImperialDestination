using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Organization
{
    public class Province
    {
        private List<Tile> _hexTiles;

        public Province()
        {
            _hexTiles = new List<Tile>();
        }

        public string Name { get; set; }

        public Country Owner { get; set; }

        public IEnumerable<Tile> HexTiles
        {
            get
            {
                return _hexTiles;
            }
        }

        public void AddHexTile(Tile hexTile)
        {
            if (hexTile == null)
                return;

            _hexTiles.Add(hexTile);
            hexTile.Province = this;
        }
    }
}
