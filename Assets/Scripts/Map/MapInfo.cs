using Assets.Scripts.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Map
{
    [Serializable]
    public class MapInfo
    {
        public Vector3[,] MapGrid { get; set; }
        public TileTerrainType[,] Map { get; set; }
        public IList<CountryInfo> Countries { get; set; }
        public IList<ProvinceInfo> Provinces { get; set; }   
    }
}
