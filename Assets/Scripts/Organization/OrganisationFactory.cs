using Assets.Contracts.Organization;
using UnityEngine;

namespace Assets.Scripts.Organization
{
    public class OrganisationFactory : IOrganisationFactory
    {                   
        public ICountry CreateCountry(GameObject countryContainer, string name, CountryType countryType, Color color)
        {
            var country = countryContainer.GetComponent<Country>();
            country.Name = name;
            country.CountryType = countryType;
            country.Color = color;
            return country;
        }

        public IProvince CreateProvince(GameObject provinceContainer, string name)
        {
            var province = provinceContainer.GetComponent<Province>();
            province.Name = name;
            return province;
        }

        public IContinent CreateContinent(GameObject continentContainer, string name, GameObject parent)
        {
            var continent = continentContainer.GetComponent<Continent>();
            continent.Name = name;
            continent.transform.SetParent(parent.transform);
            return continent;
        }
    }
}