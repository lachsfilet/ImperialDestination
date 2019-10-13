using Assets.Contracts;
using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Contracts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Map
{
    public class HeightMapGenerator : IHeightMapGenerator
    {
        private Func<int, int, int> _random;

        public HeightMapGenerator() : this(UnityEngine.Random.Range)
        {
        }

        public HeightMapGenerator(Func<int, int, int> random)
        {
            _random = random;
        }

        public void GenerateHeightMap(IHexMap hexMap, int ratio)
        {
            var heightMap = new Dictionary<TileBase, TileTerrainType>();
            var continents = hexMap.Where(t => t.TileTerrainType == TileTerrainType.Water).Select(t => t.Province.Owner.Continent).Distinct();

            foreach (var continent in continents)
            {
                var sectors = SliceContinentSectors(continent);

                 var index = _random(0, 4);
                var startSector = sectors[index];
                sectors.Remove(startSector);

                index = _random(0, 3);
                var endSector = sectors[index];

                // Determine start of mountains
                index = _random(0, startSector.Count);
                var startTile = startSector[index];

                // Determine end of mountains
                index = _random(0, endSector.Count);
                var endTile = endSector[index];

                var mountains = hexMap.DrawLine(startTile.Position, endTile.Position).ToList();
                var mountainTiles = mountains.Select(m => hexMap.GetTile(m))
                    .Where(t => t.TileTerrainType != TileTerrainType.Water).ToList();
                foreach (var mountain in mountainTiles)
                    mountain.TileTerrainType = TileTerrainType.Mountains;

                var hills = GenerateSurroundingHills(mountainTiles, hexMap);
                foreach (var hill in hills)
                    hill.TileTerrainType = TileTerrainType.Hills;
            }
        }

        private List<List<TileBase>> SliceContinentSectors(IContinent continent)
        {
            var tiles = continent.Countries.SelectMany(c => c.Provinces.SelectMany(p => p.HexTiles)).ToList();

            var west = tiles.Min(t => t.Position.X);
            var east = tiles.Max(t => t.Position.X);

            var north = tiles.Max(t => t.Position.Y);
            var south = tiles.Min(t => t.Position.Y);

            var centerX = west / 2 + east / 2;
            var centerY = north / 2 + south / 2;

            var comparer = new PositionComparer(east);
            var sectors = new List<List<TileBase>>
                {
                    tiles.Where(t => t.Position.X <= centerX && t.Position.Y > centerY).OrderBy(p=>p.Position, comparer).ToList(),
                    tiles.Where(t => t.Position.X > centerX && t.Position.Y > centerY).OrderBy(p=>p.Position, comparer).ToList(),
                    tiles.Where(t => t.Position.X <= centerX && t.Position.Y <= centerY).OrderBy(p=>p.Position, comparer).ToList(),
                    tiles.Where(t => t.Position.X > centerX && t.Position.Y <= centerY).OrderBy(p=>p.Position, comparer).ToList()
                };
            return sectors;
        }

        private static ICollection<TileBase> GenerateSurroundingHills(ICollection<TileBase> mountains, IHexMap map)
        {
            var range = 2;

            var result = new List<TileBase>();

            foreach (var mountain in mountains)
            {
                var fringes = new List<List<TileBase>>
                {
                    new List<TileBase> { mountain }
                };

                for (var i = 1; i <= range; i++)
                {
                    foreach (var tile in fringes[i - 1])
                    {
                        fringes.Add(new List<TileBase>());

                        foreach (var neighbour in map.GetNeighbours(tile)
                            .Where(n => n.TileTerrainType != TileTerrainType.Water
                                && !mountains.Contains(n)
                                && !result.Contains(n)))
                        {
                            result.Add(neighbour);
                            fringes[i].Add(neighbour);
                        }
                    }
                }
            }
            return result;
        }

        /*
            function hex_reachable(start, movement):
            var visited = set() # set of hexes
            add start to visited
            var fringes = [] # array of arrays of hexes
            fringes.append([start])

            for each 1 < k ≤ movement:
                fringes.append([])
                for each hex in fringes[k-1]:
                    for each 0 ≤ dir < 6:
                        var neighbor = hex_neighbor(hex, dir)
                        if neighbor not in visited and not blocked:
                            add neighbor to visited
                            fringes[k].append(neighbor)

            return visited
         */
    }
}