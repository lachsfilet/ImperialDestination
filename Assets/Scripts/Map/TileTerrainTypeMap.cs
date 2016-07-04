using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Map
{
    public class TileTerrainTypeMap
    {
        private TileTerrainType[,] _map;

        public int Width { get; private set; }

        public int Height { get; private set; }

        public TileTerrainTypeMap(int width, int height)
        {
            _map = new TileTerrainType[width, height];
            Width = width;
            Height = height;
        }

        public TileTerrainType? Get(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return _map[x, y];
            return null;
        }

        public void Add(TileTerrainType terrainType, int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                _map[x, y] = terrainType;
        }
    }
}
