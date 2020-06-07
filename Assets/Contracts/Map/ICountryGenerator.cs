using Assets.Contracts.Organization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Contracts.Map
{
    public interface ICountryGenerator
    {
        void GenerateCountries(ICollection<IProvince> provinces, IHexMap map, int majorCountryCount, int minorCountryCount, int provincesMajorCountries, int provincesMinorCountries, ICollection<string> majorCountryNames, ICollection<string> minorCountryNames, Func<GameObject, GameObject> instantiate, GameObject original, ICollection<Color> countryColors);
    }
}