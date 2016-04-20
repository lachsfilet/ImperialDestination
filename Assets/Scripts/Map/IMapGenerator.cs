using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Map
{
    public interface IMapGenerator
    {
        int[,] Generate(int width, int height);
    }
}
