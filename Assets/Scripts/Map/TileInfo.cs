using Assets.Contracts.Economy;
using Assets.Contracts.Map;
using Assets.Scripts.Economy;
using Assets.Scripts.Organization;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Map
{
    [Serializable]
    public class TileInfo
    {
        public Position Position { get; set; }
        
        public TileTerrainType TileTerrainType { get; set; }

        public IList<IResource> Resources { get; set; }

        public ProvinceInfo ProvinceInfo { get; set; }
    }
}
