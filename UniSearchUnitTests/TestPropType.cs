using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniSearchScriptBase;
using static UniSearchScriptBase.FilterBuilder<UniSearchUnitTests.TestPropType.CompCls>;

namespace UniSearchUnitTests
{
    [TestClass]
    public class TestPropType
    {
        public class CompCls : List<int> {
            public CompCls(string str, int i, double dbl, DateTime dt, bool bl, bool[] ls, List<CompCls> ar, string[] strs, params int[] values) {
                this.Str = str;
                this.Int = i;
                this.Dbl = dbl;
                this.Dt = dt;
                this.Bl = bl;
                this.Ls = ls;
                this.Ar = ar;
                this.Strs = strs;
                AddRange(values);
            }
            public string Str { get; }
            public string[] Strs { get; }
            public int Int { get; }
            public double Dbl { get; }
            public DateTime Dt { get; }
            public bool Bl { get; }
            public bool[] Ls { get; }
            public List<CompCls> Ar { get; }
        }

        private static readonly Random R = new Random();
        private static readonly string[] Strs = {"Mark", "Smith", "To m", "12_3"};
        private static CompCls GetC() {
            var cc= new CompCls(
                Strs.Rnd(),
                R.Next(20),
                R.NextDouble(),
                DateTime.Now,
                R.NextDouble()>0.5,
                10.RndBools().ToArray(),
                new List<CompCls>(),
                5.RndStrs(Strs).ToArray(),
                R.Next(20),
                R.Next(20),
                R.Next(20),
                R.Next(20));
            return cc;
        }


        [TestMethod]
        public void ParseSingleC() {
            const string strStr = @"str";
            var strs = FilterBuilder.ReplaceProp(strStr);
            var toStrs = GetPropType(strs);
            Assert.IsTrue(toStrs[0].type == typeof(CompCls), "toStrs[0].type == typeof(CompCls)");
            Assert.IsTrue(!toStrs[0].isList, "!toStrs[0].isList");
            Assert.IsTrue(toStrs[1].type == typeof(string), "toStrs[1].type == typeof(string)");
            Assert.IsTrue(!toStrs[1].isList, "!toStrs[1].isList");

            const string sl = @"/all";
            var slprop = FilterBuilder.ReplaceProp(sl);
            var toSl = GetPropType(slprop);
            Assert.IsTrue(toSl[1].type == typeof(int), "toSl[1].type == typeof(int)");
            Assert.IsTrue(toSl[1].isList, "toSl[1].isList");

            const string dbls = @"Ar.all.dbl";
            var dblsprop = FilterBuilder.ReplaceProp(dbls);
            var toDbls = GetPropType(dblsprop);
            Assert.IsTrue(toDbls[1].type == typeof(CompCls), "toDbls[1].type == typeof(CompCls)");
            Assert.IsTrue(toDbls[1].isList, "toDbls[1].isList");
            Assert.IsTrue(toDbls[2].type == typeof(double), "toDbls[2].type == typeof(double)");
            Assert.IsTrue(toDbls[2].isList, "toDbls[2].isList");

            const string count = @"/count";
            var countProp = FilterBuilder.ReplaceProp(count);
            var toCount = GetPropType(countProp);
            Assert.IsTrue(toCount[1].type == typeof(int), "toCount[1].type == typeof(int)");
            Assert.IsTrue(!toCount[1].isList, "toCount[1].isList");

            const string lsCount = @"ls.count";
            var lsCountProp = FilterBuilder.ReplaceProp(lsCount);
            var tolsCount = GetPropType(lsCountProp);
            Assert.IsTrue(tolsCount[1].type == typeof(int), "tolsCount[1].type == typeof(int)");
            Assert.IsTrue(!tolsCount[1].isList, "tolsCount[1].isList");
        }

        [TestMethod]
        public void FilterSingleC() {
            var c = GetC();

            // string 
            Assert.IsTrue(ParseSingle($@"$str={c.Str}").Apply(c));
            Assert.IsFalse(ParseSingle($@"$str=a{c.Str}b").Apply(c));

            Assert.IsTrue(ParseSingle($@"$str:{c.Str}").Apply(c));
            Assert.IsTrue(ParseSingle($@"$str:{c.Str[0]}").Apply(c));
            Assert.IsFalse(ParseSingle($@"$str:a{c.Str}").Apply(c));

            Assert.IsTrue(ParseSingle($@"$str->[{c.Str}]").Apply(c));
            Assert.IsTrue(ParseSingle($@"$str->[ {c.Str} , 123]").Apply(c));
            Assert.IsFalse(ParseSingle($@"$str->[a {c.Str} , 123]").Apply(c));

            Assert.ThrowsException<ArgumentException>(() => ParseSingle($@"$str->{c.Str}"));
            Assert.ThrowsException<ArgumentException>(() => ParseSingle($@"$str![{c.Str}]"));
            Assert.ThrowsException<ArgumentException>(() => ParseSingle($@"$str~[{c.Str}]"));
            Assert.ThrowsException<ArgumentException>(() => ParseSingle(@"$str->[]"));

            // bool
            Assert.AreEqual(ParseSingle(@"$bl").Apply(c), c.Bl);
            Assert.ThrowsException<ArgumentException>(() => ParseSingle(@"$bl ="));
            Assert.ThrowsException<ArgumentException>(() => ParseSingle(@"$bl =123"));

            // int
            Assert.IsTrue(ParseSingle($@"$int={c.Int}").Apply(c));

            // double
            Assert.IsTrue(ParseSingle($@"$dbl={c.Dbl}").Apply(c));

            // datetime
            Assert.IsTrue(ParseSingle($@"$dt={c.Dt}").Apply(c));

            // count
            Assert.IsTrue(ParseSingle($@"$ls.count={c.Ls.Length}").Apply(c));
            Assert.IsTrue(ParseSingle($@"$ar.count={c.Ar.Count}").Apply(c));
        }
    }
}
