using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using IoDeSer.Errors;
using IoDeSer.Errors.Deserialization;
using IoDeSer.Helpers;

namespace IoDeSer.DeSer.Processing
{
    internal static class IoDeProcessing
    {
        /// <summary>
        /// Value of unknown type (property of type <see cref="object"/>), chosen by the tag:
        /// i - long (or BigInteger), f - double, b - bool, c - char, s - string, a - List&lt;object&gt;,
        /// m - Dictionary&lt;object, object&gt;, o - Dictionary&lt;string, object&gt;, e - KeyValuePair&lt;string, object&gt;.
        /// </summary>
        internal static object DeObject(IoValue value)
        {
            switch (value.Tag)
            {
                case 'i':
                    BigInteger big = (BigInteger)IoDes.Read(value, typeof(BigInteger));
                    if (big >= long.MinValue && big <= long.MaxValue)
                        return (long)big;
                    return big;
                case 'f': return IoDes.Read(value, typeof(double));
                case 'b': return IoDes.Read(value, typeof(bool));
                case 'c': return IoDes.Read(value, typeof(char));
                case 's': return IoDes.Read(value, typeof(string));
                case 'a': return IoDes.Read(value, typeof(List<object>));
                case 'm': return IoDes.Read(value, typeof(Dictionary<object, object>));
                case 'o':
                    return value.ExpectObject(typeof(object)).ToDictionary(f => f.Key, f => DeObject(f.Value));
                case 'e':
                    IoEnum e = value.ExpectEnum(typeof(object));
                    return new KeyValuePair<string, object>(e.Variant, IoDes.Read(e.Value, typeof(object)));
                default:
                    return null;
            }
        }

        internal static object DeArray(IoValue value, Type objectType)
        {
            Type elementType = objectType.GetElementType();
            IReadOnlyList<IoValue> items = value.ExpectArray(objectType);

            Array array = Array.CreateInstance(elementType, items.Count);
            for (int i = 0; i < items.Count; i++)
                array.SetValue(IoDes.Read(items[i], elementType), i);
            return array;
        }

        /// <summary>
        /// Reads nested arrays into int[x,y,..]. All nested arrays on the same level must have the same length.
        /// </summary>
        internal static object DeMultidimensionalArray(IoValue value, Type objectType)
        {
            int rank = objectType.GetArrayRank();
            int[] lengths = new int[rank];

            IoValue level = value;
            for (int r = 0; r < rank; r++)
            {
                IReadOnlyList<IoValue> items = level.ExpectArray(objectType);
                lengths[r] = items.Count;
                if (items.Count == 0)
                    break;
                level = items[0];
            }

            Array array = Array.CreateInstance(objectType.GetElementType(), lengths);
            FillDimension(array, value, new int[rank], 0, objectType);
            return array;
        }

        static void FillDimension(Array array, IoValue value, int[] indices, int dimension, Type objectType)
        {
            IReadOnlyList<IoValue> items = value.ExpectArray(array.GetLength(dimension), objectType);
            for (int i = 0; i < items.Count; i++)
            {
                indices[dimension] = i;
                if (dimension == array.Rank - 1)
                    array.SetValue(IoDes.Read(items[i], objectType.GetElementType()), indices);
                else
                    FillDimension(array, items[i], indices, dimension + 1, objectType);
            }
        }

        internal static object DeIEnumerable(IoValue value, Type objectType)
        {
            Type elementType = IoTypes.ElementType(objectType);
            IReadOnlyList<IoValue> items = value.ExpectArray(objectType);

            Type listType = typeof(List<>).MakeGenericType(elementType);
            IList list = (IList)Activator.CreateInstance(listType);
            foreach (IoValue item in items)
                list.Add(IoDes.Read(item, elementType));

            if (objectType.IsAssignableFrom(listType))
                return list;

            if (objectType.IsInterface || objectType.IsAbstract)
            {
                Type setType = typeof(HashSet<>).MakeGenericType(elementType);
                if (objectType.IsAssignableFrom(setType))
                    return Activator.CreateInstance(setType, list);
                throw new IoTypeNotSupportedException(objectType);
            }

            // Stack enumerates from the top, but its constructor pushes in order
            if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Stack<>))
                listType.GetMethod("Reverse", Type.EmptyTypes).Invoke(list, null);

            return CreateCollection(objectType, list, listType, elementType);
        }

        /// <summary>
        /// Creates collection using constructor that accepts list (IEnumerable&lt;T&gt;, IList&lt;T&gt;...) or parameterless constructor and Add method.
        /// </summary>
        static object CreateCollection(Type objectType, object list, Type listType, params Type[] addParameters)
        {
            ConstructorInfo fromList = objectType.GetConstructors()
                .FirstOrDefault(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType.IsAssignableFrom(listType));
            if (fromList != null)
                return fromList.Invoke(new[] { list });

            MethodInfo add = objectType.GetMethod("Add", addParameters);
            if (add != null && objectType.GetConstructor(Type.EmptyTypes) != null)
            {
                object collection = Activator.CreateInstance(objectType);
                foreach (object item in (IEnumerable)list)
                {
                    if (addParameters.Length == 2)
                    {
                        Type itemType = item.GetType();
                        add.Invoke(collection, new[] { itemType.GetProperty("Key").GetValue(item), itemType.GetProperty("Value").GetValue(item) });
                    }
                    else
                    {
                        add.Invoke(collection, new[] { item });
                    }
                }
                return collection;
            }

            throw new IoTypeNotSupportedException(objectType);
        }

        internal static object DeDictionary(IoValue value, Type objectType)
        {
            Type[] types = IoTypes.DictionaryTypes(objectType);
            Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(types);
            IDictionary dictionary = (IDictionary)Activator.CreateInstance(dictionaryType);

            foreach (var entry in value.ExpectMap(objectType))
            {
                object key = IoDes.Read(entry.Key, types[0]);
                if (key == null)
                    throw new IoDeserializationException($"Key of '{objectType.Name}' cannot be null.");
                if (dictionary.Contains(key))
                    throw new IoDeserializationException($"Key {entry.Key} is defined more than once in '{objectType.Name}'.");
                dictionary.Add(key, IoDes.Read(entry.Value, types[1]));
            }

            if (objectType.IsAssignableFrom(dictionaryType))
                return dictionary;
            if (objectType.IsInterface || objectType.IsAbstract)
                throw new IoTypeNotSupportedException(objectType);

            return CreateCollection(objectType, dictionary, dictionaryType, types);
        }

        internal static object DeTuple(IoValue value, Type objectType)
        {
            Type[] types = objectType.GetGenericArguments();
            if (types.Length > 7)
                throw new IoTypeNotSupportedException(objectType);

            IReadOnlyList<IoValue> items = value.ExpectArray(types.Length, objectType);
            object[] values = new object[types.Length];
            for (int i = 0; i < types.Length; i++)
                values[i] = IoDes.Read(items[i], types[i]);

            return Activator.CreateInstance(objectType, values);
        }

        internal static object DeClass(IoValue value, Type objectType)
        {
            IReadOnlyList<KeyValuePair<string, IoValue>> fields = value.ExpectObject(objectType);

            object obj;
            try
            {
                obj = Activator.CreateInstance(objectType);
            }
            catch (MissingMethodException e)
            {
                throw new IoMissingConstructorException(objectType, e);
            }

            Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo property in IoTypes.IoProperties(objectType))
                properties[IoTypes.IoName(property)] = property;

            // fields are matched by name, so their order does not matter
            foreach (var field in fields)
            {
                if (!properties.TryGetValue(field.Key, out PropertyInfo property))
                    throw new IoMissingPropertyException(objectType, field.Key);

                // properties without setter (e.g. calculated ones) are written, but not read
                if (property.GetSetMethod(true) == null)
                    continue;

                property.SetValue(obj, IoDes.Read(field.Value, property.PropertyType));
            }

            return obj;
        }

        /// <summary>
        /// Reads o| seconds->i|..| nanoseconds->i|..| |, the same as Duration in Rust.
        /// </summary>
        internal static object DeTimeSpan(IoValue value)
        {
            long? seconds = null;
            long? nanoseconds = null;

            foreach (var field in value.ExpectObject(typeof(TimeSpan)))
            {
                if (field.Key == "seconds")
                    seconds = (long)IoDes.Read(field.Value, typeof(long));
                else if (field.Key == "nanoseconds")
                    nanoseconds = (long)IoDes.Read(field.Value, typeof(long));
                else
                    throw new IoMissingPropertyException(typeof(TimeSpan), field.Key);
            }

            if (seconds == null || nanoseconds == null)
                throw new IoDeserializationException($"Field '{(seconds == null ? "seconds" : "nanoseconds")}' of 'TimeSpan' is missing.");
            if (nanoseconds < 0 || nanoseconds >= 1000000000)
                throw new IoDeserializationException("Field 'nanoseconds' of 'TimeSpan' must be from 0 to 999 999 999.");

            try
            {
                return new TimeSpan(checked(seconds.Value * TimeSpan.TicksPerSecond + nanoseconds.Value / 100));
            }
            catch (OverflowException e)
            {
                throw new IoDeserializationException($"Value of {seconds} seconds is too big for 'TimeSpan'.", e);
            }
        }

        /// <summary>
        /// Reads e| Name->n|| |. C# enums cannot have values, so the variant must be equal to n||.
        /// </summary>
        internal static object DeEnum(IoValue value, Type objectType)
        {
            IoEnum e = value.ExpectEnum(objectType);
            if (!(e.Value is IoNone))
                throw new IoDeserializationException($"Variant {e.Variant} of enum '{objectType.Name}' must be equal to n||, but is equal to {e.Value.Snippet()}.");

            try
            {
                return Enum.Parse(objectType, e.Variant);
            }
            catch (ArgumentException ex)
            {
                throw new IoDeserializationException($"Enum '{objectType.Name}' does not have member \"{e.Variant}\".", ex);
            }
        }
    }
}
