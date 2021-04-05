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
        private GameObject _mapObject;

        private Func<int, int, int> _random;

        private IOrganisationFactory _organisationFactory;

        public MapOrganizationGenerator(GameObject mapObject, IOrganisationFactory organisationFactory)
            : this(mapObject, organisationFactory, UnityEngine.Random.Range)
        {
        }

        public MapOrganizationGenerator(GameObject mapObject, IOrganisationFactory organisationFactory, Func<int, int, int> random)
        {
            _mapObject = mapObject ?? throw new ArgumentNullException(nameof(mapObject));
            _organisationFactory = organisationFactory ?? throw new ArgumentNullException(nameof(organisationFactory));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public void GenerateCountryOnMap(ICountry country, IList<IProvince> regions, IHexMap map, int regionCount, float step)
        {
            for (var j = 0; j < 20; j++)
            {
                Debug.Log($"Try genereate provinces for country {country.Name} attempt {j}");
                if (!TryGenerateCountry(country, regions, map, regionCount, step))
                    continue;

                country.SetCapital(map);
                country.DrawBorder(map);
                return;
            }
            throw new InvalidOperationException($"Country {country.Name} could not be generated due to lack of space on the map.");
        }

        private bool TryGenerateCountry(ICountry country, IList<IProvince> regions, IHexMap map, int regionCount, float step)
        {
            Debug.Log($"From: {regionCount} - 1, {regions.Count} - ({regionCount} - 1)");           
            if (regionCount > regions.Count)
                throw new InvalidOperationException($"No regions left to generate country {country.Name}");
            
            var index = _random(regionCount - 1, regions.Count - (regionCount - 1));
            Debug.Log($"Index: {index}");
            Debug.Log($"Region count: {regions.Count}");
            var region = regions[index];
            var redoCache = new List<IProvince>();

            for (var i = 0; i < regionCount; i++)
            {
                if (region == null)
                    Debug.LogError($"Invalid index {index} for regions of count {regions.Count}");

                AddProvince(country, regions, map, region, redoCache);

                //Debug.Log($"Remaining provinces: {string.Join(", ", regions.Select(r => r.Name).OrderBy(n => n))}");

                if (TrySetNextProvince(country, regions, map, region, out region))
                    continue;

                Debug.Log($"Reset province {region?.Name} for country {country?.Name}");
                ResetRegions(redoCache, regions);
                return false;
            }
            return true;
        }

        private void AddProvince(ICountry country, IList<IProvince> regions, IHexMap map, IProvince region, List<IProvince> redoCache)
        {
            country.AddProvince(region);
            region.IsWater = false;

            Debug.Log($"Add Province: {region.Name}");

            foreach (var tile in region.HexTiles)
            {
                tile.TileTerrainType = TileTerrainType.Plain;
            }
            region.SetCapital(map);
            regions.Remove(region);
            redoCache.Add(region);
        }

        private void ResetRegions(List<IProvince> redoCache, IList<IProvince> regions)
        {
            redoCache.ForEach(r =>
            {
                Debug.Log($"Reset province {r.Name}");
                regions.Add(r);
                r.ResetProvince(_mapObject);
            });
            redoCache.Clear();
        }

        private bool TrySetNextProvince(ICountry country, IList<IProvince> regions, IHexMap map, IProvince region, out IProvince nextRegion)
        {
            var found = false;
            var tries = 20;
            nextRegion = null;
            do
            {
                var neighbours = region.GetNeighbours(map);

                if (!neighbours.Any())
                    throw new InvalidOperationException("No neighbour found!");

                var freeNeighbours = neighbours.Where(n => regions.Contains(n)).ToList();
                var countryNeighbours = neighbours.Where(n => country.Provinces.Contains(n)).ToList();

                //Debug.Log($"Remaining free provinces: {string.Join(", ", freeNeighbours.Select(r => r.Name).OrderBy(n => n))}");
                //Debug.Log($"Remaining country neighbours: {string.Join(", ", countryNeighbours.Select(r => r.Name).OrderBy(n => n))}");

                if (freeNeighbours.Any())
                {
                    var neighbourIndex = _random(0, freeNeighbours.Count);
                    region = freeNeighbours[neighbourIndex];
                    found = true;
                    region.IsWater = false;
                    //Debug.Log($"Free neighbour province: {region.Name} with index {neighbourIndex} and {nameof(region.IsWater)} {region.IsWater}");
                    if (region.GetNeighbours(map).Any(n => CheckForSurroundedSeaProvince(n, map)))
                    {
                        //Debug.LogWarning($"Surrounded sea province found for {region.Name}");
                        found = false;
                        region.IsWater = true;
                    }
                }
                else
                {
                    var neighbourIndex = _random(0, countryNeighbours.Count);
                    region = countryNeighbours[neighbourIndex];
                    //Debug.Log($"Neighbour of another province: {region.Name} with index {neighbourIndex}");
                }
                if (found)
                {
                    nextRegion = region;
                    return true;
                }
            } while (--tries > 0);
            return false;
        }

        /// <summary>
        /// Checks with a flood fill algorithm if any sea provinces are surrounded
        /// by land provinces.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        private static bool CheckForSurroundedSeaProvince(IProvince start, IHexMap map)
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

        public void GenerateContinentsList(Func<GameObject, GameObject> instantiate, GameObject original, ICollection<IProvince> provinces, IHexMap map, GameObject parent)
        {
            var continents = new List<IContinent>();
            var landProvinces = provinces.Where(p => !p.IsWater && p.Owner != null).ToList();

            var count = 0;
            while (landProvinces.Any())
            {
                var provinceQueue = new Queue<IProvince>();
                provinceQueue.Enqueue(landProvinces.First());

                var continent = _organisationFactory.CreateContinent(instantiate(original), $"Continent {count++}", parent);
                continents.Add(continent);

                while (provinceQueue.Any())
                {
                    var province = provinceQueue.Dequeue();
                    var countryParent = province.Owner.GetParent();
                    if (countryParent == null)
                        continent.AddCountry(province.Owner);

                    landProvinces.Remove(province);

                    foreach (var neighbour in province.GetNeighbours(map).Where(n => !n.IsWater
                    && !provinceQueue.Contains(n)
                    && landProvinces.Contains(n)))
                    {
                        provinceQueue.Enqueue(neighbour);
                    }
                }
            }
        }
    }
}