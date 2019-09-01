using System.Collections.Generic;
using UnityEngine;

namespace Assets.Contracts.Organization
{
    public interface ICountry
    {
        string Name { get; set; }

        CountryType CountryType { get; set; }

        List<IProvince> Provinces { get; set; }

        Color Color { get; set; }
    }
}