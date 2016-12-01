using Assets.Scripts.Map;
using Assets.Scripts.Research;

namespace Assets.Scripts.Infrastructure
{
    public interface IConstruction
    {
        ITechnology Precondition { get; }

        int Price { get; set; }

        string Name { get; set; }

        Position Location { get; set; }
    }
}
