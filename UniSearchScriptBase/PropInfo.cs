using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniSearchScriptBase
{
    public struct PropInfo
    {
        public PropInfo(Func<object, object> step, Type type, bool isList, bool isDict) {
            this.Step = step;
            this.Type = type;
            this.IsList = isList;
            this.IsDict = isDict;
        }
        public Func<object, object> Step { get; }
        public Type Type { get; }
        public bool IsList { get; }
        public bool IsDict { get; }
    }
}
