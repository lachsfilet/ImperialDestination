﻿namespace Assets.Scripts.Research
{
    public class Railway : IInvention
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
