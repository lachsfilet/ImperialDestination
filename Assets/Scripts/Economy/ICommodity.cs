using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Economy
{
    public interface ICommodity
    {
        string Name { get; }

        decimal Price { get; set; }
    }
}
