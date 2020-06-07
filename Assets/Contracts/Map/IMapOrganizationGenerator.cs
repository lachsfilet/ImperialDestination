using Assets.Contracts.Organization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Contracts.Map
{
    public interface IMapOrganizationGenerator
    {
        void GenerateCountryOnMap(ICountry country, IList<IProvince> regions, IHexMap map, int regionCount, float step);

        void GenerateContinentsList(Func<GameObject, GameObject> instantiate, GameObject original, ICollection<IProvince> provinces, IHexMap map, GameObject parent);
    }
}