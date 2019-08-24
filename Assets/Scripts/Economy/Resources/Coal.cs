using System.Collections.Generic;
using Assets.Contracts.Economy;
using Assets.Contracts.Map;

namespace Assets.Scripts.Economy.Resources
{
    public class Coal : IResource
    {
        private List<TileTerrainType> _possibleTerrainTypes;

        public Coal()
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
                return "Coal";
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
