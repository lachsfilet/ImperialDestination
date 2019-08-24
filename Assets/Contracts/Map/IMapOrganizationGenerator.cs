using Assets.Contracts.Organization;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Contracts.Map
{
    public interface IMapOrganizationGenerator
    {
        void GenerateCountryOnMap(ICountry country, IList<IProvince> regions, IHexMap map, int regionCount, Color color, float step);
    }
}