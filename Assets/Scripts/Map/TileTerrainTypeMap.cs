using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.Map
{
    public class TileTerrainTypeMap : IEnumerable<TileTerrainType>
    {
        public TileTerrainType[,] Map { get; set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public TileTerrainTypeMap(int width, int height)
        {
            Map = new TileTerrainType[width, height];
            Width = width;
            Height = height;
        }


        public TileTerrainTypeMap(TileTerrainType[,] map)
        {
            Map = map;
            Width = map.GetLength(0);
            Height = map.GetLength(1);
        }

        public TileTerrainType? Get(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return Map[x, y];
            return null;
        }

        public void Add(TileTerrainType terrainType, int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                Map[x, y] = terrainType;
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return Map.GetEnumerator();
        }

        public IEnumerator<TileTerrainType> GetEnumerator()
        {
            var iterator = Map.GetEnumerator();
            while (iterator.MoveNext())
            {
                yield return (TileTerrainType)iterator.Current;
            }
        }
    }
}
