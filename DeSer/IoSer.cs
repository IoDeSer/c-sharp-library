using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using IoDeSer.DeSer.Processing;

namespace IoDeSer.DeSer
{
    internal static class IoSer
    {
        internal static string WriteToString<T>(T obj, int number)
        {
            if (obj == null)
                throw new ArgumentNullException(null, $"The passed object or some of its components are null.");

            Type objectType = obj.GetType();
            StringBuilder sb = new StringBuilder();


            if (objectType.IsPrimitive || objectType == typeof(string))
            {
                // TODO in string check and change symbols '|', "->" and '+'
                sb.Append($"|{obj}|");
            }
            else if (typeof(IEnumerable).IsAssignableFrom(objectType))
            {
                sb.Append(IoSerProcessing.SerIEnumerable(obj, number));
            }
            else if (objectType.IsClass)
            {
                sb.Append(IoSerProcessing.SerClass(obj, number));
            }else if(objectType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                sb.Append(IoSerProcessing.SerDictionary(obj, number));
            }
            else
            {
                throw new NotSupportedException($"Object of type {objectType.Name} is not supported.");
            }

            return sb.ToString();
        }
    }
}
