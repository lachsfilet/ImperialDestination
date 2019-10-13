namespace Assets.Scripts.Map
{
    public interface IMapGenerator
    {
        int[,] Generate(int width, int height);
    }
}
