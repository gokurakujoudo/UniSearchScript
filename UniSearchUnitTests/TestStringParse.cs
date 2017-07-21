using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniSearchScriptBase;

namespace UniSearchUnitTests
{
    [TestClass]
    public class TestStringParse
    {
        [TestMethod]
        public void ValueParse()
        {
            var r = new Random();

            var ints = Enumerable.Range(0, 100).Select(i => r.Next()).ToArray();
            var intStr = ints.Select(i => i.ToString()).ToArray();
            var intParse = intStr.Select(x => Convert.ToInt32(x)).ToArray();
            var intResult = ints.Select((num, id) => num == intParse[id]).All(b=>b);
            Assert.IsTrue(intResult, "int parses");

            var intsStr = string.Format("[{0}]", string.Join(", ", ints));
            Assert.IsTrue(FilterBuilder.TryParseList<int>(intsStr, out var intsParse));
            var intsResult = ints.OrderBy(a => a).SequenceEqual(intsParse.OrderBy(a => a));
            Assert.IsTrue(intsResult, "int list parse");

            var doubles = Enumerable.Range(0, 100).Select(i => r.NextDouble()).ToArray();
            var doubleStr = doubles.Select(i => i.ToString()).ToArray();
            var doubleParse = doubleStr.Select(Convert.ToDouble).ToArray();
            var doubleResult = doubles.Select((num, id) => Math.Abs(num - doubleParse[id]) <= 1e-7).All(b => b);
            Assert.IsTrue(doubleResult, "double parses");

            var doublesStr = string.Format("[{0}]", string.Join(", ", doubles));
            Assert.IsTrue(FilterBuilder.TryParseList<double>(doublesStr, out var doublesParse));
            var doublesResult = doubles.Select((num, id) => Math.Abs(num - doublesParse[id]) <= 1e-7).All(b => b);
            Assert.IsTrue(doublesResult, "double list parse");
        }

    }
}
