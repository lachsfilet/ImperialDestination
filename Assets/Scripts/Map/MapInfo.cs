using System;
using System.Collections.Generic;

namespace Assets.Scripts.Map
{
    [Serializable]
    public class MapInfo
    {
        public TileTerrainType[,] Map { get; set; }
        public IList<TileInfo> Tiles { get; set; }
    }
}
