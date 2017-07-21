using System;
using System.Collections.Generic;

namespace UniSearchTestWpf {
    internal class TestCompClass {
        public Level1 L1 { get; set; }

        public Level1[] L1S { get; set; }

        public List<Level1> L1L { get; set; }
    }

    internal class Level1 {
        public Level2 L2 { get; set; }

        public Level2[] L2S { get; set; }

        public List<Level2> L2L { get; set; }
    }

    internal struct Level2
    {
        public Level3 L3 { get; set; }

        public Level3[] L3S { get; set; }

        public List<Level3> L3L { get; set; }
    }

    internal class Level3 {
        public string Str { get; set; }

        public int[] Nums { get; set; }

        public List<DateTime> Times { get; set; }
    }
}