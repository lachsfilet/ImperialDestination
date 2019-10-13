using Assets.Contracts.Map;
using System;
using System.Collections.Generic;

namespace Assets.Contracts.Utilities
{
    public class PositionComparer : IComparer<Position>
    {
        private readonly int _height;

        public PositionComparer(int height)
        {
            _height = height;
        }

        public int Compare(Position x, Position y)
        {
            if (x is null)
                throw new ArgumentNullException(nameof(x));

            if (y is null)
                throw new ArgumentNullException(nameof(y));

            var a = (int)Math.Pow(10, _height.CountDigits()) * x.Y + x.X;
            var b = (int)Math.Pow(10, _height.CountDigits()) * y.Y + y.X;

            if (a < b)
                return -1;
            if (a == b)
                return 0;
            return 1;
        }
    }
}