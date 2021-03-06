﻿using System.Collections.Generic;
using Assets.Contracts.Economy;
using Assets.Contracts.Map;

namespace Assets.Scripts.Economy.Resources
{
    public class IronOre : IResource
    {
        private List<TileTerrainType> _possibleTerrainTypes;

        public IronOre()
        {
            _possibleTerrainTypes = new List<TileTerrainType>
            {
                TileTerrainType.Hills,
                TileTerrainType.Mountains
            };
        }

        public int Modificator { get; set; }

        public string Name
        {
            get
            {
                return "Iron ore";
            }
        }

        public IEnumerable<TileTerrainType> PossibleTerrainTypes
        {
            get
            {
                return _possibleTerrainTypes;
            }
        }

        public int Price { get; set; }
    }
}
