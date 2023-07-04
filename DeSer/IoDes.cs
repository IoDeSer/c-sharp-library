using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace IoDeSer
{
    internal static class IoDes
    {
        /// <summary>
        /// Helps in getting value from .io file pattern.
        /// </summary>
        static Regex ioPattern = new Regex(@"^[|]((.|[\n])*)[|]$");


        /// <summary>
        /// Returns <paramref name="str"/> without leading tabulators.
        /// </summary>
        static string DeleteTabulator(string str)
        {
            string ret = "";
            string[] lines = str.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                try
                {
                    ret += $"{lines[i].Substring(1)}\n";
                }
                catch (System.ArgumentOutOfRangeException) { }
            }
            ret = ret.Trim();
            return ret;
        }



        internal static object ReadFromString(string ioString, Type objectType)
        {
            object obj = null;

            Match ioMatch = ioPattern.Match(ioString);
            ioString = ioMatch.Groups[1].Value;



            if (objectType.IsPrimitive || objectType == typeof(string))
            {
                obj = Convert.ChangeType(ioString, objectType);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(objectType))
            {
                ioString = DeleteTabulator(ioString);


                string[] objects = ioString.Split("\n+\n").ToArray();
                if (objects.Length == 0)
                {
                    if (string.IsNullOrEmpty(ioString))
                        objects = new string[0];
                    else
                        objects = new string[1] { ioString };
                }
                if (objectType.IsArray)
                    obj = Array.CreateInstance(objectType.GetElementType(), objects.Length);
                else
                {
                    obj = Activator.CreateInstance(objectType);
                }

                for (int i = 0; i < objects.Length; i++)
                {
                    if (objectType.IsArray)
                    {
                        (obj as Array).SetValue(ReadFromString(objects[i].Trim(), objectType.GetElementType()), i);
                    }
                    else if (typeof(IDictionary).IsAssignableFrom(obj.GetType()))
                    {
                        Type keyType = objectType.GetGenericArguments()[0];
                        Type valueType = objectType.GetGenericArguments()[1];
                        Type makerType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
                        object keyValuePair = Activator.CreateInstance(makerType);
                        object kV = ReadFromString(objects[i].Trim(), keyValuePair.GetType());

                        (obj as IDictionary).Add(kV.GetType().GetProperty("Key").GetValue(kV), kV.GetType().GetProperty("Value").GetValue(kV));
                    }
                    else
                        (obj as IList).Add(ReadFromString(objects[i].Trim(), objectType.GetGenericArguments()[0]));
                }           //TODO somehow convert obj to any IEnumerable on runtime

            }
            else if (objectType.IsClass)
            {
                try
                {
                    obj = Activator.CreateInstance(objectType);
                }
                catch (System.MissingMethodException e)
                {
                    throw new MissingMethodException($"Object of type {objectType} must have parameterless constructor", e);
                }
                ioString = DeleteTabulator(ioString);

                PropertyInfo[] properties = obj.GetType().GetProperties();
                string[] propertiesStrings = new string[properties.Length];

                for (int i = 0; i < properties.Length; i++)
                {
                    IoItemNameAttribute ioNameOptionalAttribute = properties[i].GetCustomAttribute<IoItemNameAttribute>();
                    propertiesStrings[i] = ioNameOptionalAttribute != null ? ioNameOptionalAttribute.customPropertyName : properties[i].Name;
                }


                string[] lines = ioString.Split('\n');

                for (int l = 0; l < lines.Length; l++)
                {
                    string _LINE = lines[l];
                    string[] assignment = _LINE.Split("->");

                    string variableName = assignment[0].Trim();
                    int propertyIndex = -1;


                    for (int z = 0; z < propertiesStrings.Length; z++)
                    {

                        if (variableName == propertiesStrings[z])
                        {
                            propertyIndex = z;
                            break;
                        }
                    }

                    if (propertyIndex == -1)
                        throw new InvalidCastException($"Object of type {objectType} does not have property named {variableName}.");



                    PropertyInfo FoundProperty = properties[propertyIndex];

                    /*
                     * Primitive types and string are in the same line as property name
                     */
                    if (FoundProperty.PropertyType.IsPrimitive || FoundProperty.PropertyType == typeof(string))
                    {
                        FoundProperty.SetValue(obj, ReadFromString(assignment[1].Trim(), FoundProperty.PropertyType));
                    }
                    /*
                     * Classes and arrays are in new lines
                     */
                    else
                    {
                        l++;
                        int newObjectStart = l;
                        do
                        {
                            l++;
                        } while (lines[l] != "|");


                        int newObjectEnd = l;
                        string newObjectString = "|\n";

                        for (int l2 = newObjectStart; l2 < newObjectEnd; l2++)
                        {
                            newObjectString += lines[l2] + "\n";
                        }
                        newObjectString += "\n|";

                        object child = ReadFromString(newObjectString.Trim(), FoundProperty.PropertyType);
                        FoundProperty.SetValue(obj, child);

                    }


                }
            }else if(objectType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                ioString = DeleteTabulator(ioString);
                string[] keyValue = ioString.Split("\n+\n");
                object k = ReadFromString(keyValue[0], objectType.GetGenericArguments()[0]);
                object v = ReadFromString(keyValue[1], objectType.GetGenericArguments()[1]);
                obj = Activator.CreateInstance(objectType,k,v);
            }
            else
                throw new InvalidDataException($"Object of type {objectType} is not supported.");


            return obj;
        }
    }
}
