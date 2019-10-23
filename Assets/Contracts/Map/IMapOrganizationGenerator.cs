﻿using Assets.Contracts.Organization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Contracts.Map
{
    public interface IMapOrganizationGenerator
    {
        void GenerateCountryOnMap(ICountry country, IList<IProvince> regions, IHexMap map, int regionCount, Color color, float step);

        void GenerateContinentsList(Func<GameObject, GameObject> instantiate, GameObject original, ICollection<IProvince> provinces, IHexMap map, GameObject parent);

        void GenerateCountries(ICollection<IProvince> provinces, IHexMap map, int majorCountryCount, int minorCountryCount, int provincesMajorCountries, int provincesMinorCountries, Func<GameObject, GameObject> instantiate, GameObject original);
    }
}