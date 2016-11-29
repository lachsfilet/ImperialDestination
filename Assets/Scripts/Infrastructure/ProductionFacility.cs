using System.Collections.Generic;
using Assets.Scripts.Economy;

namespace Assets.Scripts.Infrastructure
{
    public class ProductionFacility : IBuilding
    {
        public BuildingType BuildingType
        {
            get
            {
                return BuildingType.Economic;
            }
        }

        public Tile Location { get; set; }

        public string Name { get; set; }

        List<ICommodity> Input { get; set; }

        List<ICommodity> Output { get; set; }

        public int Capacity { get; set; }

        public void SetProduction(params ICommodity[] input)
        {

        }
    }
}
