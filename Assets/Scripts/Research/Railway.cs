using System;

namespace Assets.Scripts.Research
{
    [Serializable]
    public class Railway : ITechnology
    {
        public bool IsInvented
        {
            get; set;
        }

        public int OccurrenceYear
        {
            get
            {
                return 1815;
            }
        }

        public int Price
        {
            get
            {
                return 100;
            }
        }
    }
}
