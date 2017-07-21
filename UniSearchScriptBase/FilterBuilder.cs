using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UniSearchScriptBase {
    public enum EnmAgg {
        None,
        All,
        Any,
        Count
    }


    public static class FilterBuilder {
        private const string NORM_RX =
            @"^\$(?<prop>[/\w]+(\.[/\w]+)*)\s*((?<op>=|>=|<=|:|->|~)\s*(?<value>[^$~&|()]+))*$";

        private const string RELAX_RX =
            @"^(?<init>\$)*(?<prop_rx>\S*)\s*(?<op_rx>\S+)*\s*(?<value_rx>.*)$";

        private const string PROP_RX = @"^(?<name>\w+)*(?<agg>/(?<aggmethod>\w+)*)*$";

        public static readonly Regex NormRx = new Regex(NORM_RX);
        public static readonly Regex RelaxRx = new Regex(RELAX_RX);
        public static readonly Regex PropRx = new Regex(PROP_RX);

        public static readonly Dictionary<string, EnmAgg> AggDict = new Dictionary<string, EnmAgg> {
            {"all", EnmAgg.All},
            {"any", EnmAgg.Any},
            {"count", EnmAgg.Count},
        };

        public static IEnumerable<T> Where<T>(this IEnumerable<T> ie, Filter<T> filter) =>
            ie.Where(filter.Apply);

        public static string[] ReplaceProp(string propStr) {
            var sb = new StringBuilder(propStr.Trim().ToLower());
            sb.Replace(@".all", @"/all");
            sb.Replace(@".any", @"/any");
            sb.Replace(@".count", @"/count");
            return sb.ToString().Split('.');
        }

        internal static (string Name, EnmAgg Agg) ParsePropName(string propStr) {
            var match = PropRx.Match(propStr);
            var name = match.Groups["name"].Value;
            var agg = match.Groups["agg"].Success;
            if (!agg) return (name, EnmAgg.None);
            var aggM = match.Groups["aggmethod"].Value;
            if (string.IsNullOrWhiteSpace(name)) name = "/";
            return (name, AggDict.TryGetValue(aggM, out var method) ?  method : EnmAgg.All);
        }

        public static Func<object, bool> BuildRhs(string opStr, string valueStr, (Type type, bool isList) type) {
            if (!type.isList) {
                if (type.type == typeof(int)) {
                    if (int.TryParse(valueStr, out var intValue)) {
                        if (opStr == "=") return obj => Convert.ToInt32(obj) == intValue;
                        if (opStr == ">") return obj => Convert.ToInt32(obj) > intValue;
                        if (opStr == "<") return obj => Convert.ToInt32(obj) < intValue;
                        if (opStr == ">=") return obj => Convert.ToInt32(obj) >= intValue;
                        if (opStr == "<=") return obj => Convert.ToInt32(obj) <= intValue;
                        throw new ArgumentException($"Unknown operator {opStr} to int");
                    }
                    if (TryParseList<int>(valueStr, out var intList)) {
                        if (opStr == "~")
                            return obj => Convert.ToInt32(obj) >= intList.Min() &&
                                          Convert.ToInt32(obj) <= intList.Max();
                        if (opStr == "->") return obj => intList.Contains(Convert.ToInt32(obj));
                        throw new ArgumentException($"Unknown operator {opStr} to int[]");
                    }
                    if (string.IsNullOrWhiteSpace(opStr) && string.IsNullOrWhiteSpace(valueStr)) {
                        return obj => Convert.ToInt32(obj) > 0;
                    }
                    throw new ArgumentException($"Cannot convert {valueStr} to {type.type}");
                }
                if (type.type == typeof(double)) {
                    if (double.TryParse(valueStr, out var doubleValue)) {
                        if (opStr == "=") return obj => Math.Abs(Convert.ToDouble(obj) - doubleValue) < 1e-7;
                        if (opStr == ">") return obj => Convert.ToDouble(obj) > doubleValue;
                        if (opStr == "<") return obj => Convert.ToDouble(obj) < doubleValue;
                        if (opStr == ">=") return obj => Convert.ToDouble(obj) >= doubleValue;
                        if (opStr == "<=") return obj => Convert.ToDouble(obj) <= doubleValue;
                        throw new ArgumentException($"Unknown operator {opStr} to double");
                    }
                    if (TryParseList<double>(valueStr, out var doubleList)) {
                        if (opStr == "~")
                            return obj => Convert.ToDouble(obj) >= doubleList.Min() &&
                                          Convert.ToDouble(obj) <= doubleList.Max();
                        if (opStr == "->") return obj => doubleList.Contains(Convert.ToDouble(obj));
                        throw new ArgumentException($"Unknown operator {opStr} to double[]");
                    }
                    if (string.IsNullOrWhiteSpace(opStr) && string.IsNullOrWhiteSpace(valueStr)) {
                        return obj => Convert.ToDouble(obj) > 0;
                    }
                    throw new ArgumentException($"Cannot convert {valueStr} to {type.type}");
                }
                if (type.type == typeof(bool)) {
                    if (string.IsNullOrWhiteSpace(opStr) && string.IsNullOrWhiteSpace(valueStr))
                        // ReSharper disable once ConvertClosureToMethodGroup
                        return obj => Convert.ToBoolean(obj);
                    throw new ArgumentException($"Unknown operator {opStr} {valueStr} to bool");
                }
                //string
                if (opStr == "=") return obj => obj.ToString() == valueStr;
                if (opStr == ":") return obj => obj.ToString().Contains(valueStr);
                if (opStr == "->") {
                    if (TryParseList<string>(valueStr, out var valList))
                        return obj => valList.Contains(obj.ToString());
                    throw new ArgumentException($"Cannot convert {valueStr} to string[]");
                }
                throw new ArgumentException($"Unknown operator {opStr} to {type.type}");
            }
            // TODO
            // List



            throw new NotImplementedException();
        }


        private const string LIST_RX = @"\[(?<value>.+)\]";
        private static readonly Regex ListRx = new Regex(LIST_RX);

        public static bool TryParseList<T>(string str, out T[] result) {
            try {
                var strs = ListRx.Match(str).Groups["value"].Value.ToStrs().ToArray();
                if(strs.Length==0)
                    throw new ArgumentNullException();
                if(typeof(T) == typeof(bool))
                    result = strs.Select(Convert.ToBoolean).ToArray().To<T[]>();
                else if(typeof(T) == typeof(int))
                    result = strs.Select(i => Convert.ToInt32(i)).ToArray().To<T[]>();
                else if(typeof(T) == typeof(float))
                    result = strs.Select(Convert.ToSingle).ToArray().To<T[]>();
                else if(typeof(T) == typeof(double))
                    result = strs.Select(Convert.ToDouble).ToArray().To<T[]>();
                else if(typeof(T) == typeof(int))
                    result = strs.Select(i => Convert.ToInt32(i)).ToArray().To<T[]>();
                else if(typeof(T) == typeof(DateTime))
                    result = strs.Select(Convert.ToDateTime).ToArray().To<T[]>();
                else
                    result = strs.To<T[]>();
                return true;
            }
            catch (Exception e) {
                //Console.WriteLine(e.Message);
                result = null;
                return false;
            }
        }

    }

    public static class FilterBuilder<T> {
        private static readonly Type T_ = typeof(T);

        public static bool TryParse(string str, out Filter<T> filter) {
            try {
                filter = Parse(str);
                return true;
            }
            catch (Exception e) {
                Console.WriteLine(e);
                filter = default(Filter<T>);
                return false;
            }
        }

        public static Filter<T> Parse(string str) {
            while (true) {
                var depth = 0;
                str = str.Trim();
                if (str.Length <= 3)
                    throw new ArgumentException("Filter string is too short");

                if (str[0] == '(' && str[str.Length - 1] == ')')
                    str = str.Substring(1, str.Length - 2).Trim();

                for (var i = str.Length - 1; i >= 0; i--)
                    switch (str[i]) {
                        case '(':
                            depth += 1;
                            break;
                        case ')':
                            depth -= 1;
                            break;
                        case '&':
                            if (depth != 0) break;
                            return Parse(str.Substring(0, i)) & Parse(str.Substring(i + 1));
                        case '|':
                            if (depth != 0) break;
                            return Parse(str.Substring(0, i)) | Parse(str.Substring(i + 1));
                    }

                var isReverse = false;
                var revCount = 0;
                foreach (var t in str) {
                    if (t != '~') break;
                    isReverse = !isReverse;
                    revCount++;
                }

                if (isReverse)
                    return ~Parse(str.Substring(revCount));
                if (revCount > 0) {
                    str = str.Substring(revCount);
                    continue;
                }

                var filter = ParseSingle(str);
                return filter;
            }
        }

        public static Filter<T> ParseSingle(string str) {

            var match = FilterBuilder.NormRx.Match(str);
            if (match.Length == 0)
                throw new ArgumentException("Invalid Sentence");

            var propStrs = FilterBuilder.ReplaceProp(match.Groups["prop"].Value);
            var opStr = match.Groups["op"].Value;
            var valueStr = match.Groups["value"].Value;

            var types = GetPropType(propStrs);
            var rhs = FilterBuilder.BuildRhs(opStr, valueStr, types.Last());
            var filter = ParseSingle(propStrs, types, rhs, str);
            return filter;
        }

        public static List<(Type type, bool isList)> GetPropType(string[] propStrs) {
            var curType = T_;
            var curList = false;
            var lst = new List<(Type type, bool isList)>{(curType, false) };

            foreach (var propStr in propStrs) {
                var (propName, aggMethod) = FilterBuilder.ParsePropName(propStr);

                if (propName == "/") {
                    // /*.
                    if (aggMethod == EnmAgg.Count)
                        curType = typeof(int);
                    else {
                        curType = TypeHelper.RealType(curType);
                        curList = true;
                    }
                    lst.Add((curType, curList));
                    continue;
                }

                if (TypeHelper.Override.TryGetValue((curType, propName), out var over)) {
                    if (aggMethod == EnmAgg.Count)
                        curType = typeof(int);
                    else {
                        curType = over.Type;
                        curList |= over.IsList;
                    }
                    lst.Add((curType, curList));
                    continue;
                }

                var prop = TypeHelper.GetPropDict(curType, propName);
                if (aggMethod == EnmAgg.Count)
                    curType = typeof(int);
                else {
                    curType = prop.Type;
                    curList |= prop.IsList;
                }
                lst.Add((curType, curList));
            }
            return lst;
        }

        private static Filter<T> ParseSingle(string[] propStrs, List<(Type type, bool isList)> types,
                                             Func<object, bool> rhs, string str) {

            var filters = new Func<object, bool>[propStrs.Length + 1];
            filters[propStrs.Length] = rhs;
            for (var i = propStrs.Length - 1; i >= 0; i--) {
                var propStr = propStrs[i];
                var (propName, aggMethod) = FilterBuilder.ParsePropName(propStr);

                if (string.IsNullOrWhiteSpace(propName)) {
                    // /.
                    switch (aggMethod)
                    {
                        case EnmAgg.Any:
                            filters[i] = ts => ts.To<IEnumerable<object>>().Any(filters[1]);
                            break;
                        case EnmAgg.None:
                            filters[i] = ts => filters[1](ts);
                            break;
                        case EnmAgg.Count:
                            filters[i] = ts => filters[1](ts.To<IEnumerable<object>>().Count());
                            break;
                        case EnmAgg.All:
                            filters[i] = ts => ts.To<IEnumerable<object>>().All(filters[1]);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
       
                var typeBefore = types[i];
                var prop = TypeHelper.Override.TryGetValue((typeBefore.type, propName), out var over)
                    ? over
                    : TypeHelper.TypeDict[(typeBefore.type, propName)];
                var step = prop.Step;
                var singleStep = !prop.IsList;

                var localI = i;
                if (singleStep)
                {
                    // single out
                    filters[i] = obj => filters[localI + 1](step(obj));
                    continue;
                }
                // multi out
                switch (aggMethod)
                {
                    case EnmAgg.Any:
                        filters[i] = obj => step(obj).ToTs<object>().Any(filters[localI + 1]);
                        break;
                    case EnmAgg.None:
                        filters[i] = obj => filters[localI + 1](step(obj));
                        break;
                    case EnmAgg.Count:
                        filters[i] = obj => filters[localI + 1](step(obj).ToTs<object>().Count());
                        break;
                    case EnmAgg.All:
                        filters[i] = obj => step(obj).ToTs<object>().All(filters[localI + 1]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new Filter<T>(t => filters[0](t), str);
        }
    }
}
