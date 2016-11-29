using System;

namespace Assets.Scripts.Organization
{
    [Serializable]
    public class ContinentInfo : IEquatable<ContinentInfo>
    {
        public string Name { get; set; }

        public bool Equals(ContinentInfo other)
        {
            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
