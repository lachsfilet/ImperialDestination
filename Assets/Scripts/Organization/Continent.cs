using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Organization;
using Assets.Contracts.Organization;

public class Continent : MonoBehaviour, IContinent {

    public string Name { get; set; }

    public int TileCount { get; set; }

    public ICollection<ICountry> Countries { get; set; }

    public Continent()
    {
        Countries = new List<ICountry>();
    }

    public void AddCountry(ICountry country)
    {
        if (Countries.Contains(country))
            return;

        Countries.Add(country);
        country.Continent = this;
    }
}
