﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Contracts.Utilities
{
    public static class EnumExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> values)
        {
            var list = values.ToList();
            var random = new Random();
            while (list.Count() > 0)
            {
                var i = random.Next(0, list.Count());
                yield return list[i];
                list.RemoveAt(i);
            }
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> values, Func<int, int, int> random)
        {
            var list = values.ToList();
            while (list.Count() > 0)
            {
                var i = random(0, list.Count());
                yield return list[i];
                list.RemoveAt(i);
            }
        }
    }
}