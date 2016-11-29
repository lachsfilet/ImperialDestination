namespace Assets.Scripts.Infrastructure
{
    public interface IConstruction
    {
        string Name { get; set; }

        Tile Location { get; set; }
    }
}
