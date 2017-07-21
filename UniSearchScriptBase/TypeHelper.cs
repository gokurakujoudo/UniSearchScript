using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UniSearchScriptBase {
    public static class TypeHelper {
        public static readonly Dictionary<(Type Base, string Name), PropInfo>
            TypeDict = new Dictionary<(Type Base, string Name), PropInfo>();

        private static readonly HashSet<Type> SavedType = new HashSet<Type>();

        private static Func<string, Type> _remote;
        public static void Init(Func<string, Type> remote) => _remote = remote;

        public static readonly Dictionary<(Type Base, string Name), PropInfo>
            Override = new Dictionary<(Type Base, string Name), PropInfo>();


        public static PropInfo GetPropDict(Type t, string name) {
            if (SavedType.Contains(t)) return TypeDict[(t, name)];
            var props = t.GetProperties().ToList();
            props.ForEach(prop => TypeDict[(t, prop.Name.ToLower())] = BuildProp(prop));
            SavedType.Add(t);
            return TypeDict[(t, name)];
        }

        private static PropInfo BuildProp(PropertyInfo prop) {
            var propPropertyType = prop.PropertyType;

            var isList = !(propPropertyType == typeof(string)) &&
                         propPropertyType.GetInterfaces().Any(t => t == typeof(IEnumerable));

            var isDict = propPropertyType.GetInterfaces().Any(t => t == typeof(IDictionary));

            var realType = isList || isDict ? RealType(propPropertyType) : propPropertyType;

            return new PropInfo(prop.GetValue, realType, isList, isDict);
        }

        public static Type RealType(Type wrapType) {
            if (wrapType.BaseType != typeof(Array))
                if (wrapType.GenericTypeArguments.Length != 0)
                    return wrapType.GenericTypeArguments.Last();
                else
                    return RealType(wrapType.BaseType);
            var name = wrapType.FullName.Replace("[]", string.Empty);
            if (_remote is null)
                return Type.GetType(name);
            var realType = _remote(name);
            return realType;
        }
    }
}