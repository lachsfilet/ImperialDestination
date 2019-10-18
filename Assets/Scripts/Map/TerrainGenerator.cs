using Assets.Contracts.Map;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Map
{
    public class TerrainGenerator : ITerrainGenerator
    {
        private int _desertBeltUpper;
        private int _desertBeltLower;

        private readonly IHeightMapGenerator _heightMapGenerator;

        public int FilterCount { get; set; }

        public int MinHillsValue { get; set; }

        public int MinMountainsValue { get; set; }

        public int DesertBelt { get; set; }

        public int PoleBelt { get; set; }

        public TerrainGenerator(IHeightMapGenerator heightMapGenerator)
        {
            _heightMapGenerator = heightMapGenerator;
        }
                
        public void GenerateTerrain(IHexMap hexMap)
        {
            CalculateDesertArea(hexMap);

            var terrainTypes = new Dictionary<TileTerrainType, int>
            {
                { TileTerrainType.Bosk, 10 },
                { TileTerrainType.Forest, 10 },
                { TileTerrainType.Marsh, 2 },
                { TileTerrainType.GrainField, 2 },
                { TileTerrainType.Orchard, 5 },
                { TileTerrainType.CattleMeadows, 5 },
                { TileTerrainType.CottonField, 5 },
                { TileTerrainType.SheepMeadows, 5 },
                { TileTerrainType.StudFarm, 1 }
            };

            _heightMapGenerator.GenerateHeightMap(hexMap, 5);

            foreach(var tile in hexMap)
            {
                var y = tile.Position.Y;
                
                if (tile.Province.IsWater)
                {
                    tile.TileTerrainType = TileTerrainType.Water;
                    continue;
                }

                if (tile.TileTerrainType == TileTerrainType.Mountains || tile.TileTerrainType == TileTerrainType.Hills)
                    continue;

                if (IsWithinDesertBelt(y))
                {
                    tile.TileTerrainType = TileTerrainType.Desert;
                    continue;
                }

                if (IsWithinPoleBelt(y, hexMap.Height))
                {
                    tile.TileTerrainType = TileTerrainType.Tundra;
                    continue;
                }
                        
                tile.TileTerrainType = RandomizeTerrain(terrainTypes);
            }
        }            

        private void CalculateDesertArea(IHexMap hexMap)
        {
            var equator = (int)Math.Round(hexMap.Height / 2m);

            _desertBeltUpper = equator + DesertBelt / 2;
            _desertBeltLower = equator - DesertBelt / 2;
        }

        private bool IsWithinDesertBelt(int y) 
            => DesertBelt == 0 ? false : y <= _desertBeltUpper && y >= _desertBeltLower;

        private bool IsWithinPoleBelt(int y, int height)
        {
            return (y < PoleBelt || y >= height - PoleBelt);
        }
        
        private TileTerrainType RandomizeTerrain(IDictionary<TileTerrainType, int> terrainTypes)
        {
            var random = UnityEngine.Random.Range(1, 101);

            var sum = 0;
            var terrainTable = terrainTypes.ToDictionary(t => sum += t.Value, t => t.Key);

            terrainTable.Add(100, TileTerrainType.Plain);

            var key = terrainTable.Keys.Where(k => k >= random).Min();
            return terrainTable[key];
        }
    }
}
