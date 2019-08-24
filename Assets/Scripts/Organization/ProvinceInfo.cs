using Assets.Contracts.Map;
using System;

namespace Assets.Scripts.Organization
{
    [Serializable]
    public class ProvinceInfo : IEquatable<ProvinceInfo>
    {
        public string Name { get; set; }

        public CountryInfo OwnerInfo { get; set; }

        public ContinentInfo ContinentInfo { get; set; }

        public Position Capital { get; set; }

        public bool IsCapital { get; set; }

        public bool Equals(ProvinceInfo other)
        {
            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
