using Assets.Contracts.Map;
using Assets.Scripts.Organization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Map
{
    [Serializable]
    public class MapInfo
    {
        public static MapInfo Create(IHexMap hexMap, TileTerrainTypeMap terrainMap)
            => new MapInfo
            {
                Map = terrainMap.Map,
                Tiles = hexMap.Select(t => new TileInfo
                {
                    Position = t.Position,
                    TileTerrainType = t.TileTerrainType,
                    Resources = t.Resources,
                    ProvinceInfo = new ProvinceInfo
                    {
                        Name = t.Province.Name,
                        IsCapital = t.Province.IsCapital,
                        OwnerInfo = t.Province.Owner != null ? new CountryInfo { Name = t.Province.Owner.Name } : null,
                        ContinentInfo = t.Province.Owner != null ? new ContinentInfo { Name = t.transform.parent.name } : null
                    }
                }).ToList()
            };

        public TileTerrainType[,] Map { get; set; }
        public IList<TileInfo> Tiles { get; set; }
    }
}
