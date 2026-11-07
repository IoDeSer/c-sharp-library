using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IoDeSer.Attributes;

namespace IoDeSer.Helpers
{
    /// <summary>
    /// Reflection helpers shared by serialization and deserialization.
    /// </summary>
    internal static class IoTypes
    {
        static readonly HashSet<Type> integers = new HashSet<Type>
        {
            typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)
        };

        internal static bool IsInteger(Type type) => integers.Contains(type);

        internal static bool IsValueTuple(Type type)
        {
            return type.IsGenericType && type.FullName != null && type.FullName.StartsWith("System.ValueTuple`");
        }

        internal static bool IsTuple(Type type)
        {
            return type.IsGenericType && type.FullName != null && type.FullName.StartsWith("System.Tuple`");
        }

        internal static bool IsKeyValuePair(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
        }

        /// <summary>
        /// Returns key and value types if <paramref name="type"/> is or implements IDictionary&lt;K, V&gt; or IReadOnlyDictionary&lt;K, V&gt;.
        /// For non-generic IDictionary (e.g. Hashtable) both types are <see cref="object"/>.
        /// </summary>
        internal static Type[] DictionaryTypes(Type type)
        {
            IEnumerable<Type> interfaces = type.IsInterface ? new[] { type }.Concat(type.GetInterfaces()) : type.GetInterfaces();
            foreach (Type i in interfaces)
            {
                if (i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IDictionary<,>) || i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)))
                    return i.GetGenericArguments();
            }
            if (typeof(System.Collections.IDictionary).IsAssignableFrom(type))
                return new[] { typeof(object), typeof(object) };
            return null;
        }

        /// <summary>
        /// Returns T if <paramref name="type"/> is or implements IEnumerable&lt;T&gt;, otherwise <see cref="object"/>.
        /// </summary>
        internal static Type ElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            IEnumerable<Type> interfaces = type.IsInterface ? new[] { type }.Concat(type.GetInterfaces()) : type.GetInterfaces();
            foreach (Type i in interfaces)
            {
                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return i.GetGenericArguments()[0];
            }
            return typeof(object);
        }

        /// <summary>
        /// Name of the property in .io string: value of <see cref="IoItemNameAttribute"/> or the property name.
        /// </summary>
        internal static string IoName(PropertyInfo property)
        {
            IoItemNameAttribute nameAttribute = property.GetCustomAttribute<IoItemNameAttribute>();
            return nameAttribute != null ? nameAttribute.customPropertyName : property.Name;
        }

        /// <summary>
        /// Public, readable, not indexed and not ignored properties of <paramref name="type"/>, in .io order.
        /// </summary>
        internal static List<PropertyInfo> IoProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && p.GetCustomAttribute<IoItemIgnoreAttribute>() == null)
                .OrderBy(p => p, new IoPropertiesComparator(type)) // OrderBy is stable, so equal properties keep their default order
                .ToList();
        }
    }
}
