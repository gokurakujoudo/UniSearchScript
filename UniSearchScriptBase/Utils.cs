using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UniSearchScriptBase {
    
    public static class Utils {
        private static readonly Random R = new Random();

        public static T Rnd<T>(this T[] array) => array[R.Next(array.Length)];

        public static IEnumerable<string> RndStrs(this int num, string[] from) => new bool[num].Select(x => from.Rnd());
        public static IEnumerable<int> RndInts(this int num, int max = 100) => new bool[num].Select(x => R.Next(max));
        public static IEnumerable<bool> RndBools(this int num) => new bool[num].Select(x => R.NextDouble() > 0.5);

        public static T To<T>(this object e) => (T) e;

        public static int ToInt(this string str, int def = default(int)) =>
            int.TryParse(str, out var num) ? num : def;

        public static double ToDouble(this string str, double def = default(double)) =>
            double.TryParse(str, out var num) ? num : def;

        public static IEnumerable<string> ToStrs(this string str, string split = ",",
                                                     bool trim = true, bool dropna = true, bool unique = true) {
            var strs = str.Split(split.ToCharArray());
            var list = trim ? strs.Select(s => s.Trim()) : strs;
            list = dropna ? list.Where(s => !string.IsNullOrWhiteSpace(s)) : list;
            list = unique ? list.Distinct() : list;
            return list;
        }

        public static IEnumerable<int> ToInts(this IEnumerable<object> ie) =>
            ie.Select(Convert.ToInt32);

        public static IEnumerable<T> ToTs<T>(this object o) => o.To<IEnumerable>().Cast<T>();
    }
}