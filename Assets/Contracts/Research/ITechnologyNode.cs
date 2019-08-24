using Assets.Contracts.Research;

namespace Assets.Scripts.Research
{
    public interface ITechnologyNode : ITechnology
    {
        ITechnology Precondition { get; set; }
    }
}
