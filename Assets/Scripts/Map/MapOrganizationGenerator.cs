using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Map
{
    public class MapOrganizationGenerator : IMapOrganizationGenerator
    {
        private Func<int, int, int> _random;

        public MapOrganizationGenerator() : this(UnityEngine.Random.Range)
        {
        }

        public MapOrganizationGenerator(Func<int, int, int> random)
        {
            _random = random;
        }

        public void GenerateCountryOnMap(ICountry country, IList<IProvince> regions, IHexMap map, int regionCount, Color color, float step)
        {
            var index = _random(regionCount - 1, regions.Count - (regionCount - 1));
            var region = regions[index];

            var countryStep = step * (1f / regionCount);

            for (var i = 0; i < regionCount; i++)
            {
                if (region == null)
                    Debug.LogError($"Invalid index {index} for regions of count {regions.Count}");

                country.Provinces.Add(region);
                region.Owner = country;
                region.IsWater = false;

                Debug.Log($"Province: {region.Name}");

                foreach (var tile in region.HexTiles)
                {
                    tile.TileTerrainType = TileTerrainType.Plain;
                    tile.SetColor(color);
                }
                regions.Remove(region);

                Debug.Log($"Remaining provinces: {string.Join(", ", regions.Select(r => r.Name).OrderBy(n => n))}");

                var found = false;
                var tries = 20;
                do
                {
                    var neighbours = region.GetNeighbours(map);

                    if (!neighbours.Any())
                        throw new InvalidOperationException("No neighbour found!");

                    var freeNeighbours = neighbours.Where(n => regions.Contains(n)).ToList();
                    var countryNeighbours = neighbours.Where(n => country.Provinces.Contains(n)).ToList();

                    Debug.Log($"Remaining free provinces: {string.Join(", ", freeNeighbours.Select(r => r.Name).OrderBy(n => n))}");
                    Debug.Log($"Remaining country neighbours: {string.Join(", ", countryNeighbours.Select(r => r.Name).OrderBy(n => n))}");

                    if (freeNeighbours.Any())
                    {
                        var neighbourIndex = _random(0, freeNeighbours.Count);
                        region = freeNeighbours[neighbourIndex];
                        found = true;
                        region.IsWater = false;
                        Debug.Log($"Free neighbour province: {region.Name} with index {neighbourIndex} and {nameof(region.IsWater)} {region.IsWater}");
                        if (region.GetNeighbours(map).Any(n => CheckForSurroundedSeaProvince(n, map)))
                        {
                            Debug.Log($"Surrounded sea province found for {region.Name}");
                            found = false;
                            region.IsWater = true;
                        }
                        else
                            Debug.Log($"No surrounded sea province found for {region.Name}");
                    }
                    else
                    {
                        var neighbourIndex = _random(0, countryNeighbours.Count);
                        region = countryNeighbours[neighbourIndex];
                        Debug.Log($"Neighbour of another province: {region.Name} with index {neighbourIndex}");
                    }
                } while (!found && --tries > 0);
                if (!found)
                    throw new InvalidOperationException("No unset neighbour found!");

                color = new Color(color.r - countryStep, color.g - countryStep, color.b - countryStep, 1);
            }
        }

        /// <summary>
        /// Checks with a flood fill algorithm if any sea provinces are surrounded 
        /// by land provinces.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static bool CheckForSurroundedSeaProvince(IProvince start, IHexMap map)
        {
            if (!start.IsWater)
                return false;

            var steps = 10;
            var provinceStack = new Stack<IProvince>();
            var checkedSeaProvinces = new List<IProvince>();

            provinceStack.Push(start);
            
            while (provinceStack.Count > 0 && steps-- > 0)
            {
                var province = provinceStack.Pop();

                if (checkedSeaProvinces.Contains(province))
                    continue;

                checkedSeaProvinces.Add(province);

                foreach (var neighbour in province.GetNeighbours(map))
                {
                    if (neighbour.IsWater)
                    {
                        provinceStack.Push(neighbour);
                    }
                }
            }
            return steps > 0;
        }
    }
}
