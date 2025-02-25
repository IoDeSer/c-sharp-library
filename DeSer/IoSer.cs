using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using IoDeSer.DeSer.Processing;
using IoDeSer.Errors;

namespace IoDeSer.DeSer
{
    internal static class IoSer
    {
        internal static string Write(Type type, object obj, int number=0)
        {
            var ser = typeof(IoSer).GetMethod("WriteToString", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            return (string)ser.MakeGenericMethod(type).Invoke(null, new object[] { obj, number });
        }

        internal static string WriteToString<T>(T obj, int number=0)
        {
            if (obj == null) return "|||";
                //throw new ArgumentNullException(null, $"The passed object or some of its components are null.");

            Type objectType = obj.GetType();

            switch (objectType)
            {
                case Type t when t.IsPrimitive:
                    return $"|{obj}|";

                case Type t when t == typeof(string):
                    return IoSerProcessing.SerString(obj as string);

                case Type t when typeof(IEnumerable).IsAssignableFrom(t):
                    return IoSerProcessing.SerIEnumerable(obj, number);

                case Type t when typeof(DateTime).IsAssignableFrom(t) ||
                                  typeof(DateTimeOffset).IsAssignableFrom(t) ||
                                  typeof(TimeSpan).IsAssignableFrom(t):
                    return IoSerProcessing.SerDateTime(obj, number);

                case Type t when t.IsClass:
                    return IoSerProcessing.SerClass(obj, number);

                case Type t when t.IsEnum:
                    return IoSerProcessing.SerEnum(obj, number);

                case Type t when t.IsValueType:
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                        return IoSerProcessing.SerDictionary(obj, number);
                    return IoSerProcessing.SerStruct(obj, number);

                default:
                    throw new IoTypeNotSupportedException(objectType);
            }

        }
    }
}
