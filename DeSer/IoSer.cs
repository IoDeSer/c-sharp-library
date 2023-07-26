using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using IoDeSer.DeSer.Processing;

namespace IoDeSer.DeSer
{
    internal static class IoSer
    {
        internal static string WriteToString<T>(T obj, int number=0, PrintType printType = PrintType.Pretty)
        {
            if (obj == null)
                throw new ArgumentNullException(null, $"The passed object or some of its components are null.");

            Type objectType = obj.GetType();


            if (objectType.IsPrimitive || objectType == typeof(string))
            {
                // TODO in string check and change symbols '|', "->" and '+'
                return $"|{obj}|";
            }
            else if (typeof(IEnumerable).IsAssignableFrom(objectType))
            {
                return IoSerProcessing.SerIEnumerable(obj, number, printType);
            }
            else if (objectType.IsClass)
            {
                return IoSerProcessing.SerClass(obj, number, printType);
            }else if (objectType.IsValueType)
            {
                
                if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                    return IoSerProcessing.SerDictionary(obj, number, printType);
                return IoSerProcessing.SerStruct(obj, number, printType);
            }
            else
                throw new NotSupportedException($"Object of type {objectType.Name} is not supported.");

        }
    }
}
