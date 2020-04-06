using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Contracts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Map
{
    public class MapOrganizationGenerator : IMapOrganizationGenerator
    {
        private Func<int, int, int> _random;

        private IOrganisationFactory _organisationFactory;

        private ICollection<string> _countryNames;

        public MapOrganizationGenerator(IOrganisationFactory organisationFactory) : this(organisationFactory, UnityEngine.Random.Range)
        {
        }

        public MapOrganizationGenerator(IOrganisationFactory organisationFactory, Func<int, int, int> random)
        {
            _organisationFactory = organisationFactory;
            _random = random;
            _countryNames = SettingsLoader.Instance.GetCountryNames();
        }

        public void GenerateCountries(ICollection<IProvince> provinces, IHexMap map, int majorCountryCount, int minorCountryCount, int provincesMajorCountries, int provincesMinorCountries, Func<GameObject, GameObject> instantiate, GameObject original, ICollection<Color> countryColors)
        {
            if (countryColors.Count != majorCountryCount)
                throw new ArgumentException($"{majorCountryCount} and {countryColors.Count} must be equal");

            var regions = provinces.Where(p => !p.HexTiles.Any(h => h.Position.X == 0 || h.Position.Y == 0 || h.Position.X == map.Width - 1 || h.Position.Y == map.Height - 1)).ToList();

            var majorCountries = Enumerable.Range(1, majorCountryCount).Select(n => new { number = n, isMajor = true });
            var minorCountries = Enumerable.Range(1, minorCountryCount).Select(n => new { number = n, isMajor = false });
            var countries = majorCountries.Concat(minorCountries).Shuffle().ToList();
            var step = 1f / countries.Count;

            var count = 0;
            foreach (var countryInfo in countries)
            {
                var regionCount = countryInfo.isMajor ? provincesMajorCountries : provincesMinorCountries;
                var prefix = countryInfo.isMajor ? "Major" : "Minor";
                var countryContainer = instantiate(original);
                var country = _organisationFactory.CreateCountry(
                    countryContainer,
                    count < _countryNames.Count ? _countryNames.ElementAt(count) : $"{prefix} Country {count}",
                    countryInfo.isMajor ? CountryType.Major : CountryType.Minor,
                    countryInfo.isMajor ? countryColors.ElementAt(count) : Color.grey);

                GenerateCountryOnMap(country, regions, map, regionCount, step);
                count++;
            }
        }

        public void GenerateCountryOnMap(ICountry country, IList<IProvince> regions, IHexMap map, int regionCount, float step)
        {
            var index = _random(regionCount - 1, regions.Count - (regionCount - 1));
            var region = regions[index];

            var countryStep = step * (1f / regionCount);

            for (var i = 0; i < regionCount; i++)
            {
                if (region == null)
                    Debug.LogError($"Invalid index {index} for regions of count {regions.Count}");

                country.AddProvince(region);
                region.IsWater = false;

                Debug.Log($"Province: {region.Name}");

                foreach (var tile in region.HexTiles)
                {
                    tile.TileTerrainType = TileTerrainType.Plain;
                }
                region.SetCapital(map);
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
                            Debug.LogWarning($"Surrounded sea province found for {region.Name}");
                            found = false;
                            region.IsWater = true;
                        }
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
            }
            country.SetCapital(map);
            country.DrawBorder(map);
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