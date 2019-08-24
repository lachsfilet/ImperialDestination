using Assets.Contracts.Research;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Organization
{
    [Serializable]
    public class CountryInfo : IEquatable<CountryInfo>
    {
        public string Name { get; set; }

        public List<ITechnology> Technologies { get; set; }

        public bool Equals(CountryInfo other)
        {
            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
