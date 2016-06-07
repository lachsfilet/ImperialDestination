using Assets.Scripts.Map;
using System.Collections.Generic;

namespace Assets.Scripts.Economy.Resources
{
    public class Gemstone : IResource
    {
        private List<TileTerrainType> _possibleTerrainTypes;

        public Gemstone()
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
                return "Gemstone";
            }
        }

        public decimal Price { get; set; }

        public IEnumerable<TileTerrainType> PossibleTerrainTypes
        {
            get
            {
                return _possibleTerrainTypes;
            }
        }
    }
}
