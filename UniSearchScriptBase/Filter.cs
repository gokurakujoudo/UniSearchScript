using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UniSearchScriptBase {

    public struct Filter<T> {
        public Filter(Func<T, bool> func, string str) {
            _str = str;
            this.Apply = func;
        }

        public Func<T, bool> Apply { get; }

        private readonly string _str;
        public override string ToString() => _str;

        #region operator

        public static Filter<T> operator &(Filter<T> f1, Filter<T> f2) =>
            new Filter<T>(obj => f1.Apply(obj) & f2.Apply(obj), $"({f1} & {f2})");

        public static Filter<T> operator |(Filter<T> f1, Filter<T> f2) =>
            new Filter<T>(obj => f1.Apply(obj) | f2.Apply(obj), $"({f1} | {f2})");

        public static Filter<T> operator ~(Filter<T> f) =>
            new Filter<T>(obj => !f.Apply(obj), $"~({f})");

        #endregion

    }

}
