using System.Collections.Generic;

namespace Assets.Contracts.Organization
{
    public interface IContinent
    {
        string Name { get; set; }

        int TileCount { get; }

        ICollection<ICountry> Countries { get; set; }

        void AddCountry(ICountry country);
    }
}