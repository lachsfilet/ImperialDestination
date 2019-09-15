using Assets.Contracts.Map;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Contracts.Organization
{
    public interface ICountry
    {
        string Name { get; set; }

        CountryType CountryType { get; set; }

        List<IProvince> Provinces { get; set; }

        IContinent Continent { get; set; }

        Color Color { get; set; }

        void AddProvince(IProvince province);

        void DrawBorder(IHexMap map);

        Transform GetParent();

        void SetParent(Transform transform);
    }
}