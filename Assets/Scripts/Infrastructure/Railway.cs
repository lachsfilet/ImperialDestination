using Assets.Scripts.Map;
using Assets.Scripts.Research;

namespace Assets.Scripts.Infrastructure
{
    public class Railway : IConstruction
    {
        public int Price { get; set; }

        public RailwayDirection Direction { get; set; }

        public string Name { get; set; }

        public Position Location { get; set; }

        public IInvention Precondition { get; set; }
    }
}
