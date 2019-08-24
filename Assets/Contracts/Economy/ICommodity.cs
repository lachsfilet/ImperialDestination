namespace Assets.Contracts.Economy
{
    public interface ICommodity
    {
        string Name { get; }

        int Price { get; set; }
    }
}
