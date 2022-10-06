using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EntityWrapperGenerator
{
    public static class ExtensionMethods
    {
        public static readonly string NullableTypeName = "Nullable`1";
        private static readonly string Question = "?";
        private static readonly string System = "System";
        public static readonly Dictionary<string, string> TypesAliasList = new Dictionary<string, string>
        {
            { "String", "string" },
            { "Boolean", "bool" },
            { "Single", "float" },
            { "Double", "double" },
            { "Decimal", "decimal" },
            { "SByte", "sbyte" },
            { "Byte", "byte" },
            { "Int16", "short" },
            { "UInt16", "ushort" },
            { "Int32", "int" },
            { "UInt32", "uint" },
            { "Int64", "long" },
            { "UInt64", "ulong" },
        };
        
        /// <summary>
        /// Adds the in loop.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stringBuilder">The string builder.</param>
        /// <param name="dataSet">The data set.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AddInLoop<T>(this StringBuilder stringBuilder, IEnumerable<T> dataSet, Action<StringBuilder, T> action)
        {
            foreach (var item in dataSet) 
            {
                action(stringBuilder, item);
            }
            return stringBuilder;
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns> A string Object. </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetTypeName(this Type type)
        {
            string typeName = null;

            bool isNullable = type.IsGenericType && type.Name == NullableTypeName;

            typeName = type.IsGenericType ? type.GenericTypeArguments[0].Name : type.Name;
            if (type.Namespace == System && TypesAliasList.ContainsKey(typeName))
            {
                typeName = TypesAliasList[typeName];
            }
            if (isNullable)
            {
                typeName += Question;
            }
            return typeName;
        }
    }
}