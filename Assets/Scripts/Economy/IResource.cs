using Assets.Scripts.Map;
using System.Collections.Generic;

namespace Assets.Scripts.Economy
{
    public interface IResource : ICommodity
    {
        int Modificator { get; set; }

        IEnumerable<TileTerrainType> PossibleTerrainTypes { get; }
    }
}
