namespace Assets.Scripts.Research
{
    public interface ITechnology
    {
       bool IsInvented { get; set; }

       int Price { get; }

       int OccurrenceYear { get; }
    }
}
