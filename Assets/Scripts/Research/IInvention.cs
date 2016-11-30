namespace Assets.Scripts.Research
{
    public interface IInvention
    {
       bool IsInvented { get; set; }

       int Price { get; }

       int OccurrenceYear { get; }
    }
}
