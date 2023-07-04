using IoDeSer.Ordering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IoDeSer
{
    internal static class IoSer
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




        internal static string WriteToString<T>(T obj, int number)
        {
            if (obj == null)
                throw new ArgumentNullException(null, $"The passed object or some of its components are null.");

            Type objectType = obj.GetType();
            StringBuilder sb = new StringBuilder();


            if (objectType.IsPrimitive || objectType == typeof(string))
            {
                sb.Append($"|{obj}|");
            }
            else if (typeof(IEnumerable).IsAssignableFrom(objectType))
            {
                IEnumerable objArray = (IEnumerable)obj;
                string arrayReturn = "";
                bool isFirst = true;

                Array tryArray = (objArray as Array);
                if (tryArray != null && tryArray.Rank > 1)
                {
                    MultidimensionalArray multiDimensionalArray = new MultidimensionalArray(tryArray, tryArray.GetType().GetElementType());
                    Array jagged = multiDimensionalArray.ToJaggedArray();

                    objArray = jagged;
                }

                foreach (var element in objArray)
                {
                    if (!isFirst)
                        arrayReturn += $"\n{MakeShift(number + 1)}+\n";
                    else
                        isFirst = false;
                    arrayReturn += MakeShift(number + 1) + WriteToString(element, number + 1);

                }




                sb.Append($"|\n{arrayReturn}\n{MakeShift(number)}|");
            }
            else if (objectType.IsClass)
            {
                PropertyInfo[] objectClassProperties = obj.GetType().GetProperties();

                IoItemsOrderAttribute itemsOrder = objectType.GetCustomAttribute<IoItemsOrderAttribute>();
                if (itemsOrder == null)
                {
                    objectClassProperties = objectClassProperties.OrderBy(p => p.GetCustomAttribute<IoItemOrderAttribute>() == null ? int.MaxValue : p.GetCustomAttribute<IoItemOrderAttribute>().Order).ToArray();
                }
                else
                {
                    switch (itemsOrder.Order)
                    {
                        case ItemsOrder.ALPHABETICAL:
                            objectClassProperties = objectClassProperties.OrderBy(p => p.Name).ToArray();
                            break;
                        case ItemsOrder.ALPHABETICAL_REVERSE:
                            objectClassProperties = objectClassProperties.OrderByDescending(p => p.Name).ToArray();
                            break;
                        case ItemsOrder.LONGEST_FIRST:
                            objectClassProperties = objectClassProperties.OrderByDescending(p => p.Name.Length).ToArray();
                            break;
                        case ItemsOrder.SHORTEST_FIRST:
                            objectClassProperties = objectClassProperties.OrderBy(p => p.Name.Length).ToArray();
                            break;
                    }
                }



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

                            classReturn += $"{MakeShift(number + 1)}{propertyName}->{WriteToString(propertyValue, number + 1)}";
                            isFirst = false;
                        }
                    }
                }
                sb.Append($"|\n{classReturn}\n{MakeShift(number)}|");
            }else if(objectType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                object key = objectType.GetProperty("Key").GetValue(obj);
                object value = objectType.GetProperty("Value").GetValue(obj);
                Type[] types = objectType.GetGenericArguments();
                //TODO for classess
                sb.Append($"|\n{MakeShift(number+1)}{WriteToString(key, number+1)}\n{MakeShift(number + 1)}+\n{MakeShift(number+1)}{WriteToString(value,number+1)}\n{MakeShift(number)}|");

            }
            else
            {
                throw new NotSupportedException($"Object of type {objectType.Name} is not supported.");
            }

            return sb.ToString();
        }
    }
}
