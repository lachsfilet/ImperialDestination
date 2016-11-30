using System.Collections.Generic;
using Assets.Scripts.Map;

namespace Assets.Scripts.Economy.Resources
{
    public class Fruit : IResource
    {
        private List<TileTerrainType> _possibleTerrainTypes;

        public Fruit()
        {
            _possibleTerrainTypes = new List<TileTerrainType>
            {
                TileTerrainType.Orchard
            };            
        }

        public int Modificator { get; set; }

        public string Name
        {
            get
            {
                return "Fruit";
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
