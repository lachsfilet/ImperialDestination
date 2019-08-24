using Assets.Contracts.Economy;
using Assets.Contracts.Map;
using System.Collections.Generic;

namespace Assets.Scripts.Economy.Resources
{
    public class Gold : IResource
    {
        private List<TileTerrainType> _possibleTerrainTypes;

        public Gold()
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
                return "Gold";
            }
        }

        public int Price { get; set; }

        public IEnumerable<TileTerrainType> PossibleTerrainTypes
        {
            get
            {
                return _possibleTerrainTypes;
            }
        }
    }
}
