using System;

namespace Assets.Scripts.Utilities
{
    public static class IntExtensions
    {
        public static int CountDigits(this int number) =>
            number == 0 ? 1 : (int)Math.Floor(Math.Log10(Math.Abs(number)) + 1);
    }
}