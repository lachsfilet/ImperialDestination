using System.Collections.Generic;
using Assets.Scripts.Map;

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

        public decimal Price { get; set; }
    }
}
