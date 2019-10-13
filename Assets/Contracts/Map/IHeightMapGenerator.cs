using System.Collections.Generic;

namespace Assets.Contracts.Map
{
    public interface IHeightMapGenerator
    {
        void GenerateHeightMap(IHexMap hexMap, int ratio);
    }
}