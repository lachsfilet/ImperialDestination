using Assets.Contracts.Map;
using Assets.Contracts.Research;

namespace Assets.Contracts.Infrastructure
{
    public interface IConstruction
    {
        ITechnology Precondition { get; }

        int Price { get; set; }

        string Name { get; set; }

        Position Location { get; set; }
    }
}
