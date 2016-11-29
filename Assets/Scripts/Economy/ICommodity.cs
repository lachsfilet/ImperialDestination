namespace Assets.Scripts.Economy
{
    public interface ICommodity
    {
        string Name { get; }

        decimal Price { get; set; }
    }
}
