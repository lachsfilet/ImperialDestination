using System.Collections.Generic;
using Assets.Contracts.Economy;
using Assets.Contracts.Map;

namespace Assets.Scripts.Economy.Resources
{
    public class Wood : IResource
    {
        private List<TileTerrainType> _possibleTerrainTypes;

        public Wood()
        {
            _possibleTerrainTypes = new List<TileTerrainType>
            {
                TileTerrainType.Forest,
                TileTerrainType.Bosk
            };            
        }

        public int Modificator { get; set; }

        public string Name
        {
            get
            {
                return "Wood";
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
