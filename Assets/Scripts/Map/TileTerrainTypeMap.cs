using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.Map
{
    public class TileTerrainTypeMap : IEnumerable<TileTerrainType>
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


        IEnumerator IEnumerable.GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        public IEnumerator<TileTerrainType> GetEnumerator()
        {
            var iterator = _map.GetEnumerator();
            while (iterator.MoveNext())
            {
                yield return (TileTerrainType)iterator.Current;
            }
        }
    }
}
