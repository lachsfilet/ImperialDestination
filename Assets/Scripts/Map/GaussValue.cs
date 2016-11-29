namespace Assets.Scripts.Map
{
    public class GaussValue
    {
        public int Position { get; private set; }
        public double Value { get; private set; }

        public GaussValue(int position, double value)
        {
            Position = position;
            Value = value;
        }
    }
}
