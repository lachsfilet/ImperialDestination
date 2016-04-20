using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Economy
{
    public interface IResource : ICommodity
    {
        int Modificator { get; set; }
    }
}
