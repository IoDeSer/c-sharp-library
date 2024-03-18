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
        internal static string WriteToString<T>(T obj, int number=0)
        {
            if (obj == null) return "|||";
                //throw new ArgumentNullException(null, $"The passed object or some of its components are null.");

            Type objectType = obj.GetType();


            if (objectType.IsPrimitive || objectType == typeof(string))
            {
                // TODO in string check and change symbols '|', "->" and '+'
                return $"|{obj}|";
            }
            else if (typeof(IEnumerable).IsAssignableFrom(objectType))
            {
                return IoSerProcessing.SerIEnumerable(obj, number);
            }
            else if (typeof(DateTime).IsAssignableFrom(objectType) || typeof(DateTimeOffset).IsAssignableFrom(objectType)
                 || typeof(TimeSpan).IsAssignableFrom(objectType))
            {
                return IoSerProcessing.SerDateTime(obj, number);
            }
            else if (objectType.IsClass)
            {
                return IoSerProcessing.SerClass(obj, number);
            }
            else if (objectType.IsValueType)
            {

                if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                    return IoSerProcessing.SerDictionary(obj, number);
                return IoSerProcessing.SerStruct(obj, number);
            }
            else
                throw new IoTypeNotSupportedException(objectType);

        }
    }
}
