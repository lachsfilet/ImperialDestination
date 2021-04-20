using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Contracts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Map
{
    public class CountryGenerator : ICountryGenerator
    {
        private readonly IOrganisationFactory _organisationFactory;

        private readonly IMapOrganizationGenerator _mapOrganizationGenerator;

        private readonly Func<int, int, int> _random;


        public CountryGenerator(IOrganisationFactory organisationFactory, IMapOrganizationGenerator mapOrganizationGenerator) :
            this(organisationFactory, mapOrganizationGenerator, UnityEngine.Random.Range)
        {
        }

        public CountryGenerator(IOrganisationFactory organisationFactory, IMapOrganizationGenerator mapOrganizationGenerator, Func<int, int, int> random)
        {
            _organisationFactory = organisationFactory ?? throw new ArgumentNullException(nameof(organisationFactory));
            _mapOrganizationGenerator = mapOrganizationGenerator ?? throw new ArgumentNullException(nameof(mapOrganizationGenerator));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public void GenerateCountries(ICollection<IProvince> provinces, IHexMap map, int majorCountryCount, 
            int minorCountryCount, int provincesMajorCountries, int provincesMinorCountries,
            ICollection<string> majorCountryNames, ICollection<string> minorCountryNames,
            Func<GameObject, GameObject> instantiate, GameObject original, ICollection<Color> countryColors)
        {
            if (countryColors.Count != majorCountryCount)
                throw new ArgumentException($"{nameof(majorCountryCount)} ({majorCountryCount}) and {nameof(countryColors.Count)} ({countryColors.Count}) must be equal");

            var regions = provinces.Where(p => !p.HexTiles.Any(h => h.Position.X == 0 || h.Position.X == map.Width - 1)).ToList();

            var majorCountryNameQueue = new Queue<string>(majorCountryNames ?? Enumerable.Empty<string>());
            var minorCountryNameQueue = new Queue<string>(minorCountryNames ?? Enumerable.Empty<string>());

            var majorCountries = Enumerable.Range(1, majorCountryCount).Select(n => new { number = n, isMajor = true });
            var minorCountries = Enumerable.Range(1, minorCountryCount).Select(n => new { number = n, isMajor = false });
            var countries = majorCountries.Concat(minorCountries).Shuffle(_random).ToList();
            var step = 1f / countries.Count;

            var majorCount = 0;
            var minorCount = 0;
            foreach (var countryInfo in countries)
            {
                var regionCount = countryInfo.isMajor ? provincesMajorCountries : provincesMinorCountries;
                var countryName = GenerateName(countryInfo.isMajor, majorCountryNameQueue, minorCountryNameQueue, majorCount, minorCount);

                var countryContainer = instantiate(original);
                var country = _organisationFactory.CreateCountry(
                    countryContainer,
                    countryName,
                    countryInfo.isMajor ? CountryType.Major : CountryType.Minor,
                    countryInfo.isMajor ? countryColors.ElementAt(majorCount) : Color.grey);
                _mapOrganizationGenerator.GenerateCountryOnMap(country, regions, map, regionCount, step);
                
                if(countryInfo.isMajor)
                    majorCount++;
                else
                    minorCount++;
            }
        }

        private string GenerateName(bool isMajor, Queue<string> majorCountryNames, Queue<string> minorCountryNames, int majorCount, int minorCount)
        {
            if(isMajor)
            {
                if (majorCountryNames.Any())
                    return majorCountryNames.Dequeue();
                return $"Major {majorCount}";
            }
            if (minorCountryNames.Any())
                return minorCountryNames.Dequeue();
            return $"Minor {minorCount}";
        }
    }
}