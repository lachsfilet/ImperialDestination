using Assets.Contracts.Map;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Map
{
    public class TerrainGenerator : ITerrainGenerator
    {
        private int _equator;

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
                
        public TileTerrainTypeMap CreateMap(IHexMap hexMap)
        {
            _equator = (int)Math.Round(hexMap.Height / 2m);
                        
            var terrainMap = new TileTerrainTypeMap(hexMap.Width, hexMap.Height);
            
            var terrainTypes = new Dictionary<TileTerrainType, double>
            {
                { TileTerrainType.Bosk, 0.1 },
                { TileTerrainType.Forest, 0.1 },
                { TileTerrainType.Marsh, 0.02 },
                { TileTerrainType.GrainField, 0.2 },
                { TileTerrainType.Orchard, 0.05 },
                { TileTerrainType.CattleMeadows, 0.05 },
                { TileTerrainType.CottonField, 0.05 },
                { TileTerrainType.SheepMeadows, 0.05 },
                { TileTerrainType.StudFarm, 0.01 }
            };

            _heightMapGenerator.GenerateHeightMap(hexMap, 5);

            var random = new Random();

            foreach(var tile in hexMap)
            {
                var x = tile.Position.X;
                var y = tile.Position.Y;
                
                if (tile.Province.IsWater)
                {
                    terrainMap.Add(TileTerrainType.Water, x, y);
                    continue;
                }

                if (IsWithinDesertBelt(y))
                {
                    terrainMap.Add(TileTerrainType.Desert, x, y);
                    continue;
                }

                if (IsWithinPoleBelt(y, hexMap.Height))
                {
                    terrainMap.Add(TileTerrainType.Tundra, x, y);
                    continue;
                }

                var plainTerrain = TileTerrainType.Plain;

                // Generate busk, forest and agriculture tiles 
                foreach (var terrainType in terrainTypes.Keys)
                {
                    var randomValue = random.NextDouble();
                    if (randomValue > terrainTypes[terrainType])
                        continue;

                    plainTerrain = terrainType;
                    break;
                }                
                terrainMap.Add(plainTerrain, x, y);
            }
            return terrainMap;
        }            

        private bool IsWithinDesertBelt(int y)
        {
            var upper = _equator + DesertBelt / 2;
            var lower = _equator - DesertBelt / 2;
            return (y < upper && y > lower);
        }

        private bool IsWithinPoleBelt(int y, int height)
        {
            return (y < PoleBelt || y >= height - PoleBelt);
        }      
    }
}
