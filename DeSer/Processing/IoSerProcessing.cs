using IoDeSer.Attributes;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using IoDeSer.Attributes.Ordering;
using IoDeSer.Extensions;
using IoDeSer.Helpers;

namespace IoDeSer.DeSer.Processing
{
    internal static class IoSerProcessing
    {
        /// <summary>
        /// Returns <paramref name="number"/> of tabulators as string.
        /// </summary>
        static string MakeShift(int number)
        {
            string shift = "";
            for (int i = 0; i < number; i++)
            {
                shift += "\t";
            }
            return shift;
        }


        internal static string SerIEnumerable<T>(T obj, int number)
        {
            IEnumerable objArray = (IEnumerable)obj;
            string arrayReturn = "";
            bool isFirst = true;

            Array tryArray = (objArray as Array);
            if (tryArray != null && tryArray.Rank > 1)
            {
                MultidimensionalArray multiDimensionalArray = new MultidimensionalArray(tryArray);
                Array jagged = multiDimensionalArray.ToJaggedArray();

                objArray = jagged;
            }

            foreach (var element in objArray)
            {
                if (!isFirst)
                    arrayReturn += $"\n{MakeShift(number + 1)}+\n";
                else
                    isFirst = false;
                arrayReturn += MakeShift(number + 1) + IoSer.WriteToString(element, number + 1);

            }
            return $"|\n{arrayReturn}\n{MakeShift(number)}|";
        }

        internal static string SerClass<T>(T obj, int number)
        {
            PropertyInfo[] objectClassProperties = obj.GetType().GetProperties();
            Array.Sort(objectClassProperties, new IoPropertiesComparator<T>());


            string classReturn = "";
            bool isFirst = true;

            foreach (PropertyInfo property in objectClassProperties)
            {
                if (property.GetCustomAttribute<IoItemIgnoreAttribute>() == null)
                {
                    var propertyValue = property.GetValue(obj);
                    if (propertyValue != null)
                    {


                        IoItemNameAttribute ioNameOptionalAttribute = property.GetCustomAttribute<IoItemNameAttribute>();
                        string propertyName = ioNameOptionalAttribute != null ? ioNameOptionalAttribute.customPropertyName : property.Name;

                        if (!isFirst)
                        {
                            classReturn += "\n";
                        }

                        classReturn += $"{MakeShift(number + 1)}{propertyName}->{IoSer.WriteToString(propertyValue, number + 1)}";
                        isFirst = false;
                    }
                }
            }
            return $"|\n{classReturn}\n{MakeShift(number)}|";
        }

        internal static string SerDictionary<T>(T obj, int number)
        {
            Type objectType = obj.GetType();

            object key = objectType.GetProperty("Key").GetValue(obj);
            object value = objectType.GetProperty("Value").GetValue(obj);
            Type[] types = objectType.GetGenericArguments();

            return $"|\n{MakeShift(number + 1)}{IoSer.WriteToString(key, number + 1)}\n{MakeShift(number + 1)}+\n{MakeShift(number + 1)}{IoSer.WriteToString(value, number + 1)}\n{MakeShift(number)}|";
        }

        internal static string SerStruct<T>(T obj, int number)
        {
            return SerClass(obj, number);
        }
    }
}
