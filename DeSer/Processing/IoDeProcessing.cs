using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IoDeSer.Attributes;
using IoDeSer.Extensions;

namespace IoDeSer.DeSer.Processing
{
    internal static class IoDeProcessing
    {
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
        

        internal static object DeIEnumerable(ref string ioString, Type objectType)
        {
            bool _ONE_LINER = ioString.Split('\n').Length == 1;
            string[] objects;
            if (_ONE_LINER)
            {
                string lines_tokens = "";
                int l_i = 0;
                do
                {
                    lines_tokens += ioString[l_i++];
                } while (ioString[l_i] == '|');

                objects = ioString.Split($"+{lines_tokens}").ToArray();

                string[] objects_fix = new string[objects.Length];
                for (int i = 0; i < objects.Length; i++)
                {
                    if (i == 0)
                        objects_fix[i] = objects[i];
                    else
                        objects_fix[i] = lines_tokens + objects[i];
                }
                objects = objects_fix;
            }
            else
            {
                ioString = DeleteTabulator(ioString);
                objects = ioString.Split("\n+\n").ToArray();
            }


            if (objects.Length == 0)
            {
                if (string.IsNullOrEmpty(ioString))
                    objects = new string[0];
                else
                    objects = new string[1] { ioString };
            }


            object obj;

            if (objectType.IsArray)
            {
                obj = Array.CreateInstance(objectType.GetElementType(), objects.Length);

                for (int i = 0; i < objects.Length; i++)
                {
                    (obj as Array).SetValue(IoDes.ReadFromString(objects[i].Trim(), objectType.GetElementType()), i);
                }
            }
            else
            {
                // TODO SPRAWDZIĆ oneLine print type
                Type elementType = objectType.GetInterfaces()
                    .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Select(t => t.GetGenericArguments()[0])
                    .FirstOrDefault();


                obj = Activator.CreateInstance(objectType);

                for (int i = 0; i < objects.Length; i++)
                {
                    var input_data = IoDes.ReadFromString(objects[i].Trim(), elementType);
                    var temp = typeof(Enumerable).GetMethod("Append").MakeGenericMethod(elementType)
                        .Invoke(obj, new object[] { obj, input_data });

                    obj = Activator.CreateInstance(objectType, temp);
                }
                // TODO check for erros, at first glance it works?
            }

            return obj;
        }

        internal static object DeClass(ref string ioString, Type objectType)
        {
            bool _ONE_LINER = ioString.Split('\n').Length == 1;



            object obj = null;

            try
            {
                obj = Activator.CreateInstance(objectType);
            }
            catch (System.MissingMethodException e)
            {
                throw new MissingMethodException($"Object of type {objectType} must have parameterless constructor", e);
            }

            PropertyInfo[] properties = obj.GetType().GetProperties();
            string[] propertiesStrings = new string[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                IoItemNameAttribute ioNameOptionalAttribute = properties[i].GetCustomAttribute<IoItemNameAttribute>();
                propertiesStrings[i] = ioNameOptionalAttribute != null ? ioNameOptionalAttribute.customPropertyName : properties[i].Name;
            }

            if (_ONE_LINER)
                return DeClassOneLine(ref ioString, objectType, obj, propertiesStrings, properties);

            ioString = DeleteTabulator(ioString); // TODO
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
                    FoundProperty.SetValue(obj, IoDes.ReadFromString(assignment[1].Trim(), FoundProperty.PropertyType));
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

                    FoundProperty.SetValue(obj, IoDes.ReadFromString(newObjectString.Trim(), FoundProperty.PropertyType));

                }


            }

            return obj;
        }

        private static object DeClassOneLine(ref string ioString, Type objectType, object obj, string[] propertiesStrings, PropertyInfo[] properties)
        {
            Dictionary<string, int> property_indecies = new Dictionary<string, int>(); // TODO what if class has property of class that has the same property names???
            foreach (var item in propertiesStrings)
            {
                property_indecies.Add(item, ioString.IndexOf(item));
            }

            List<KeyValuePair<string, int>> ordered_indecies = property_indecies.OrderBy(x => x.Value).ToList();
            for (int i = 0; i < ordered_indecies.Count; i++)
            {
                var first = ordered_indecies[i];
                string sub;
                try
                {
                    var second = ordered_indecies[i + 1];
                    sub = ioString.Substring(first.Value+2+first.Key.Length, second.Value - (first.Value + 2 + first.Key.Length));
                }
                catch (System.ArgumentOutOfRangeException e)
                {
                    sub = ioString;
                    sub = ioString.Substring(first.Value + 2 + first.Key.Length);
                }


                int propertyIndex = -1;


                for (int z = 0; z < propertiesStrings.Length; z++)
                {

                    if (ordered_indecies[i].Key == propertiesStrings[z])
                    {
                        propertyIndex = z;
                        break;
                    }
                }

                if (propertyIndex == -1)
                    throw new InvalidCastException($"Object of type {objectType} does not have property named {ordered_indecies[i].Key}.");

                PropertyInfo FoundProperty = properties[propertyIndex];
                FoundProperty.SetValue(obj, IoDes.ReadFromString(sub, FoundProperty.PropertyType));
            }

            // TODO więcej testów

            return obj;
        }

        internal static object DeDictionary(ref string ioString, Type objectType)
        {
            ioString = DeleteTabulator(ioString);
            string[] keyValue = ioString.Split("\n+\n");

            // TODO dostosować do OneLine printType


            object k = IoDes.ReadFromString(keyValue[0], objectType.GetGenericArguments()[0]);
            object v = IoDes.ReadFromString(keyValue[1], objectType.GetGenericArguments()[1]);

            return Activator.CreateInstance(objectType, k, v);
        }

        internal static object DeStruct(ref string ioString, Type objectType)
        {
            return DeClass(ref ioString, objectType);
        }
    }
}
