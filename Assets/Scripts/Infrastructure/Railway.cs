using Assets.Contracts.Infrastructure;
using Assets.Contracts.Map;
using Assets.Contracts.Research;

namespace Assets.Scripts.Infrastructure
{
    public class Railway : IConstruction
    {
        public Railway(Research.Railway precondition)
        {
            Precondition = precondition;
        }

        public int Price { get; set; }

        public RailwayDirection Direction { get; set; }

        public string Name { get; set; }

        public Position Location { get; set; }

        public ITechnology Precondition { get; private set; }
    }
}
