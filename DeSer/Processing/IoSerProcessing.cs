using IoDeSer.Attributes;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using IoDeSer.Attributes.Ordering;
using IoDeSer.Extensions;
using IoDeSer.Helpers;
using System.Text.RegularExpressions;

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


        internal static string SerIEnumerable<T>(T obj, int number, PrintType printType = PrintType.OneLine)
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
                    arrayReturn += printType == PrintType.Pretty ? $"\n{MakeShift(number + 1)}+\n":"+";
                else
                    isFirst = false;
                arrayReturn += (printType==PrintType.Pretty?MakeShift(number + 1):"") + IoSer.WriteToString(element, number + 1, printType);

            }
            switch (printType)
            {
                case PrintType.OneLine:
                    return $"|{arrayReturn}|";
                default:
                    return $"|\n{arrayReturn}\n{MakeShift(number)}|";
            }
        }

        internal static string SerClass<T>(T obj, int number, PrintType printType = PrintType.OneLine)
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

                        if (!isFirst && printType==PrintType.Pretty)
                        {
                            classReturn += "\n";
                        }

                        classReturn += $"{(printType==PrintType.Pretty?MakeShift(number + 1):"")}{propertyName}->{IoSer.WriteToString(propertyValue, number + 1, printType)}";
                        isFirst = false;
                    }
                }
            }

            switch (printType)
            {
                case PrintType.OneLine:
                    return $"|{classReturn}|";
                default:
                    return $"|\n{classReturn}\n{MakeShift(number)}|";
            }
        }

        internal static string SerDictionary<T>(T obj, int number, PrintType printType = PrintType.OneLine)
        {
            Type objectType = obj.GetType();

            object key = objectType.GetProperty("Key").GetValue(obj);
            object value = objectType.GetProperty("Value").GetValue(obj);
            Type[] types = objectType.GetGenericArguments();

            switch (printType)
            {
                case PrintType.OneLine:
                    return $"|{IoSer.WriteToString(key, number + 1, printType)}+{IoSer.WriteToString(value, number + 1, printType)}|";
                default:
                    return $"|\n{MakeShift(number + 1)}{IoSer.WriteToString(key, number + 1, printType)}" +
                            $"\n{MakeShift(number + 1)}+" +
                            $"\n{MakeShift(number + 1)}{IoSer.WriteToString(value, number + 1, printType)}" +
                            $"\n{MakeShift(number)}|";
            }
        }

        internal static string SerStruct<T>(T obj, int number, PrintType printType = PrintType.OneLine)
        {
            return SerClass(obj, number, printType);
        }
    }
}
