namespace Assets.Contracts.Infrastructure
{
    public interface IBuilding : IConstruction
    {
        BuildingType BuildingType { get; }
    }
}
