namespace Assets.Scripts.Economy
{
    public interface ICommodity
    {
        string Name { get; }

        int Price { get; set; }
    }
}
