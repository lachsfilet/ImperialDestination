using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Map
{

    public static class EnumExtensions
    {
        public static IEnumerable<Direction> GetValues(this Direction foo) => Enum.GetValues(typeof(Direction)).Cast<Direction>();
    }
}