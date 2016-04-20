using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Economy
{
    public class ProductionOrder
    {
        public ICommodity Commodity { get; set; }

        public int Quantity { get; set; }
    }
}
