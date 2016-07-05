using System;
using System.Collections.Generic;

namespace Assets.Scripts.Map
{
    public class MapFactory
    {
        private int _equator;

        public IMapGenerator MapGenerator { get; set; }

        public int FilterCount { get; set; }

        public int MinHillsValue { get; set; }

        public int MinMountainsValue { get; set; }

        public int DesertBelt { get; set; }

        public int PoleBelt { get; set; }

        public MapFactory(IMapGenerator mapGenerator)
        {
            MapGenerator = mapGenerator;
        }

        public TileTerrainTypeMap CreateMap(int width, int height)
        {
            if (MapGenerator == null)
                return null;

            _equator = (int)Math.Round(height / 2m);

            var map = MapGenerator.Generate(width, height);

            var terrainMap = new TileTerrainTypeMap(width, height);

            var gaussianBlur = new GaussianBlur();
            int[,] blurredMap = map;
            for (var i = 0; i < FilterCount; i++)
            {
                blurredMap = gaussianBlur.Filter(blurredMap);
            }

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

            var random = new System.Random();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var value = blurredMap[x, y];

                    // In the map border regions only use water
                    if (value == 0 || (x < 2 || y < 2 || y > height - 3 || x > width - 3))
                    {
                        terrainMap.Add(TileTerrainType.Water, x, y);
                        continue;
                    }

                    if (value >= MinHillsValue && value < MinMountainsValue)
                    {
                        terrainMap.Add(TileTerrainType.Hills, x, y);
                        continue;
                    }

                    if (value >= MinMountainsValue)
                    {
                        terrainMap.Add(TileTerrainType.Mountains, x, y);
                        continue;
                    }

                    if (IsWithinDesertBelt(y))
                    {
                        terrainMap.Add(TileTerrainType.Desert, x, y);
                        continue;
                    }

                    if (IsWithinPoleBelt(y, height))
                    {
                        terrainMap.Add(TileTerrainType.Tundra, x, y);
                        continue;
                    }

                    TileTerrainType plainTerrain = TileTerrainType.Plain;

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
