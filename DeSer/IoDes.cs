using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using IoDeSer.DeSer.Processing;

namespace IoDeSer.DeSer
{
    internal static class IoDes
    {
        /// <summary>
        /// Helps in getting value from .io file pattern.
        /// </summary>
        static Regex ioPattern = new Regex(@"^[|]((.|[\n])*)[|]$");

        internal static object ReadFromString(string ioString, Type objectType)
        {
            /*            Console.WriteLine($"TYP: {objectType}");
                        Console.WriteLine($"input: {ioString}\n\n");*/
            if (ioString == "|||") return null;

            Match ioMatch = ioPattern.Match(ioString);
            ioString = ioMatch.Groups[1].Value;

            if (objectType.IsPrimitive || objectType == typeof(string))
            {
                // TODO in string check and change special tokens for '|', "->" and '+'
                return Convert.ChangeType(ioString, objectType);
            }
            else if (typeof(DateTime).IsAssignableFrom(objectType) || typeof(DateTimeOffset).IsAssignableFrom(objectType)
                 || typeof(TimeSpan).IsAssignableFrom(objectType))
            {
                return IoDeProcessing.DeDateTime(ref ioString, objectType);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(objectType))
            {
                return IoDeProcessing.DeIEnumerable(ref ioString,  objectType);
            }
            else if (objectType.IsClass)
            {
                return IoDeProcessing.DeClass(ref ioString, objectType);
            }
            else if (objectType.IsValueType)
            {
                if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                    return IoDeProcessing.DeDictionary(ref ioString, objectType);
                return IoDeProcessing.DeStruct(ref ioString, objectType);
            }
            else
                throw new InvalidDataException($"Object of type {objectType} is not supported.");

        }
    }
}
