using System;

namespace Assets.Scripts.Organization
{
    [Serializable]
    public class CountryInfo : IEquatable<CountryInfo>
    {
        public string Name { get; set; }

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
