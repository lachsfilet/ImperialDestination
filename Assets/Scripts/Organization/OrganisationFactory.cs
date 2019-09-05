using Assets.Contracts.Organization;
using UnityEngine;

namespace Assets.Scripts.Organization
{
    public class OrganisationFactory
    { 
        private static OrganisationFactory _instance;

        private OrganisationFactory()
        {
        }

        public static OrganisationFactory Instance => _instance ?? (_instance = new OrganisationFactory());

        public Country CreateCountry(GameObject countryContainer, string name, CountryType countryType, Color color)
        {
            var country = countryContainer.GetComponent<Country>(); 
            country.Name = name;
            country.CountryType = countryType;
            country.Color = color;
            return country;
        }

        public Province CreateProvince(GameObject provinceContainer, string name)
        {
            var province = provinceContainer.GetComponent<Province>();
            province.Name = name;
            return province;
        }
    }
}