namespace Assets.Scripts.Infrastructure
{
    public interface IBuilding : IConstruction
    {
        BuildingType BuildingType { get; }
    }
}
