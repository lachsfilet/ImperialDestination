using UnityEngine;

namespace Assets.Contracts.Organization
{
    public interface IOrganisationFactory
    {
        IContinent CreateContinent(GameObject continentContainer, string name, GameObject parent);

        ICountry CreateCountry(GameObject countryContainer, string name, CountryType countryType, Color color);

        IProvince CreateProvince(GameObject provinceContainer, string name);
    }
}