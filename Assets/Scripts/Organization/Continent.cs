using Assets.Contracts.Organization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Continent : MonoBehaviour, IContinent
{
    public string Name { get; set; }

    public int TileCount => Countries.SelectMany(c => c.Provinces.SelectMany(p => p.HexTiles)).Count();

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
        country.SetParent(transform);

        var tiles = country.Provinces.SelectMany(p => p.HexTiles).ToList();
        tiles.ForEach(t => t.transform.SetParent(transform));
    }
}