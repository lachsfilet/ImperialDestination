using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Map
{
    [Serializable]
    public class MapInfo
    {
        public HexGrid HexGrid { get; set; }
        public HexMap Map { get; set; }
        public TileTerrainTypeMap TerrainMap { get; set; }
    }
}
