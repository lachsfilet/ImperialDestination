using System;
using System.Collections.Generic;
using Assets.Contracts.Economy;
using Assets.Contracts.Infrastructure;
using Assets.Contracts.Map;
using Assets.Contracts.Research;

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

        public ITechnology Precondition
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int Price
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        Position IConstruction.Location
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void SetProduction(params ICommodity[] input)
        {

        }
    }
}
