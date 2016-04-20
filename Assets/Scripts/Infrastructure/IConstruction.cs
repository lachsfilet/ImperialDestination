using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Infrastructure
{
    public interface IConstruction
    {
        string Name { get; set; }

        Tile Location { get; set; }
    }
}
