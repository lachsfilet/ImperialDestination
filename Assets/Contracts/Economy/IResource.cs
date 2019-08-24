using Assets.Contracts.Map;
using System.Collections.Generic;

namespace Assets.Contracts.Economy
{
    public interface IResource : ICommodity
    {
        int Modificator { get; set; }

        IEnumerable<TileTerrainType> PossibleTerrainTypes { get; }
    }
}
