namespace Assets.Scripts.Research
{
    public interface IInventionNode : IInvention
    {
        IInvention Precondition { get; set; }
    }
}
