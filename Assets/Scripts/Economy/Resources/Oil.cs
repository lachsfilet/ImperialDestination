using Assets.Scripts.Map;
using System.Collections.Generic;
using System;

namespace Assets.Scripts.Economy.Resources
{
    public class Oil : IResource
    {
        private List<TileTerrainType> _possibleTerrainTypes;

        public Oil()
        {
            _possibleTerrainTypes = new List<TileTerrainType>
            {
                TileTerrainType.Marsh,
                TileTerrainType.Desert,
                TileTerrainType.Tundra
            };
        }

        public int Modificator { get; set; }

        public string Name
        {
            get
            {
                return "Oil";
            }
        }

        public IEnumerable<TileTerrainType> PossibleTerrainTypes
        {
            get
            {
                return _possibleTerrainTypes;
            }
        }

        public decimal Price { get; set; }
    }
}
