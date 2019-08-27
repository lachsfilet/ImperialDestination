using Assets.Contracts;
using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Map
{
    public class MapOrganizationGenerator : IMapOrganizationGenerator
    {
        public void GenerateCountryOnMap(ICountry country, IList<IProvince> regions, IHexMap map, int regionCount, Color color, float step)
        {
            var index = UnityEngine.Random.Range(regionCount - 1, regions.Count - (regionCount - 1));
            var region = regions[index];

            var countryStep = step * (1f / regionCount);

            for (var i = 0; i < regionCount; i++)
            {
                country.Provinces.Add(region);
                region.Owner = country;
                if (region == null)
                    Debug.LogError($"Invalid index {index} for regions of count {regions.Count}");

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

                    if(!neighbours.Any())
                        throw new InvalidOperationException("No neighbour found!");

                    var freeNeighbours = neighbours.Where(n => regions.Contains(n)).ToList();
                    var countryNeighbours = neighbours.Where(n => country.Provinces.Contains(n)).ToList();

                    Debug.Log($"Remaining free provinces: {string.Join(", ", freeNeighbours.Select(r => r.Name).OrderBy(n => n))}");
                    Debug.Log($"Remaining country neighbours: {string.Join(", ", countryNeighbours.Select(r => r.Name).OrderBy(n => n))}");

                    if (freeNeighbours.Any())
                    {
                        var neighbourIndex = UnityEngine.Random.Range(0, freeNeighbours.Count);
                        region = freeNeighbours[neighbourIndex];
                        Debug.Log($"Free neighbour province: {region.Name}");
                        found = true;
                    }
                    else
                    {
                        var neighbourIndex = UnityEngine.Random.Range(0, countryNeighbours.Count);
                        region = countryNeighbours[neighbourIndex];
                        Debug.Log($"Neighbour of another province: {region.Name}");
                    }
                } while (!found && --tries > 0);
                if (!found)
                    throw new InvalidOperationException("No unset neighbour found!");

                color = new Color(color.r - countryStep, color.g - countryStep, color.b - countryStep, 1);
            }
        }
    }
}
