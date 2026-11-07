using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IoDeSer.Errors.Serialization;
using IoDeSer.Format;
using IoDeSer.Helpers;

namespace IoDeSer.DeSer.Processing
{
    internal static class IoSerProcessing
    {
        internal static void SerIEnumerable(IoWriter writer, IEnumerable enumerable)
        {
            writer.WriteSequence('a', enumerable.Cast<object>(), element => IoSer.Write(writer, element));
        }

        /// <summary>
        /// Multidimensional array is written as nested arrays: int[2,3] as a| a| x + x + x | + a| x + x + x | |.
        /// </summary>
        internal static void SerMultidimensionalArray(IoWriter writer, Array array)
        {
            SerDimension(writer, array, new int[array.Rank], 0);
        }

        static void SerDimension(IoWriter writer, Array array, int[] indices, int dimension)
        {
            IEnumerable<int> range = Enumerable.Range(0, array.GetLength(dimension));
            writer.WriteSequence('a', range, i =>
            {
                indices[dimension] = i;
                if (dimension == array.Rank - 1)
                    IoSer.Write(writer, array.GetValue(indices));
                else
                    SerDimension(writer, array, indices, dimension + 1);
            });
        }

        /// <summary>
        /// Dictionary is written as m| a| key + value | + ... |.
        /// </summary>
        internal static void SerDictionary(IoWriter writer, IEnumerable dictionary)
        {
            // Hashtable enumerates DictionaryEntry, generic dictionaries KeyValuePair<K, V>
            IEnumerable<KeyValuePair<object, object>> entries = dictionary.Cast<object>().Select(e =>
            {
                if (e is DictionaryEntry entry)
                    return new KeyValuePair<object, object>(entry.Key, entry.Value);
                Type entryType = e.GetType();
                return new KeyValuePair<object, object>(entryType.GetProperty("Key").GetValue(e), entryType.GetProperty("Value").GetValue(e));
            });

            writer.WriteSequence('m', entries, entry =>
                writer.WriteSequence('a', new[] { entry.Key, entry.Value }, item => IoSer.Write(writer, item)));
        }

        /// <summary>
        /// KeyValuePair, ValueTuple and Tuple are written as arrays, the same as tuples in Rust.
        /// </summary>
        internal static void SerTuple(IoWriter writer, object tuple)
        {
            writer.WriteSequence('a', TupleItems(tuple), item => IoSer.Write(writer, item));
        }

        internal static IEnumerable<object> TupleItems(object tuple)
        {
            Type type = tuple.GetType();
            if (IoTypes.IsKeyValuePair(type))
                return new[] { type.GetProperty("Key").GetValue(tuple), type.GetProperty("Value").GetValue(tuple) };

            int count = type.GetGenericArguments().Length;
            if (count > 7)
                throw new IoSerializationException($"Tuples with more than 7 elements are not supported ({type.Name}).");

            if (IoTypes.IsValueTuple(type))
                return Enumerable.Range(1, count).Select(i => type.GetField("Item" + i).GetValue(tuple));
            return Enumerable.Range(1, count).Select(i => type.GetProperty("Item" + i).GetValue(tuple));
        }

        internal static void SerClass(IoWriter writer, object obj)
        {
            Type objectType = obj.GetType();
            List<PropertyInfo> properties = IoTypes.IoProperties(objectType);

            HashSet<string> names = new HashSet<string>();
            List<KeyValuePair<string, PropertyInfo>> fields = new List<KeyValuePair<string, PropertyInfo>>();
            foreach (PropertyInfo property in properties)
            {
                string name = IoTypes.IoName(property);
                if (!IoValue.IsValidName(name))
                    throw new IoSerializationException($"Name \"{name}\" of property {objectType.Name}.{property.Name} cannot be empty or contain whitespace, '|', '+' or \"->\".");
                if (!names.Add(name))
                    throw new IoSerializationException($"Name \"{name}\" is used by more than one property of {objectType.Name}.");
                fields.Add(new KeyValuePair<string, PropertyInfo>(name, property));
            }

            writer.WriteObject(fields, property => IoSer.Write(writer, property.GetValue(obj)));
        }

        /// <summary>
        /// Written as o| seconds->i|..| nanoseconds->i|..| |, the same as Duration in Rust. Nanoseconds are always from 0 to 999 999 999.
        /// </summary>
        internal static void SerTimeSpan(IoWriter writer, TimeSpan span)
        {
            long seconds = span.Ticks / TimeSpan.TicksPerSecond;
            long remainder = span.Ticks % TimeSpan.TicksPerSecond;
            if (remainder < 0)
            {
                seconds--;
                remainder += TimeSpan.TicksPerSecond;
            }

            var fields = new[]
            {
                new KeyValuePair<string, object>("seconds", seconds),
                new KeyValuePair<string, object>("nanoseconds", remainder * 100),
            };
            writer.WriteObject(fields, value => IoSer.Write(writer, value));
        }

        /// <summary>
        /// Written as e| Name->n|| |, the same as unit variant of enum in Rust.
        /// </summary>
        internal static void SerEnum(IoWriter writer, object value)
        {
            // [Flags] combinations are written as "A,B"
            string name = value.ToString().Replace(" ", "");
            writer.WriteEnum(name, writer.WriteNone);
        }
    }
}
