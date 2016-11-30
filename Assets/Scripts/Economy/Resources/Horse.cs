using System.Collections.Generic;
using Assets.Scripts.Map;

namespace Assets.Scripts.Economy.Resources
{
    public class Horse : IResource
    {
        private List<TileTerrainType> _possibleTerrainTypes;

        public Horse()
        {
            _possibleTerrainTypes = new List<TileTerrainType>
            {
                TileTerrainType.StudFarm
            };            
        }

        public int Modificator { get; set; }

        public string Name
        {
            get
            {
                return "Horse";
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
