using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using IoDeSer.DeSer.Processing;

namespace IoDeSer.DeSer
{
    public static class IoDes
    {
        /// <summary>
        /// Helps in getting value from .io file pattern.
        /// </summary>
        static Regex ioPattern = new Regex(@"^[|]((.|[\n])*)[|]$");

        // TODO in production this should not be PUBLIC
        public static T ReadFromString<T>(string ioString)
        {

            Match ioMatch = ioPattern.Match(ioString);
            ioString = ioMatch.Groups[1].Value;


            Type objectType = typeof(T);

            if (objectType.IsPrimitive || objectType == typeof(string))
            {
                return (T)Convert.ChangeType(ioString, objectType);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(objectType))
            {
                return (T)IoDeProcessing.DeIEnumerable(ref ioString,  objectType);
            }
            else if (objectType.IsClass)
            {
                return (T)IoDeProcessing.DeClass(ref ioString, objectType);
            }
            else if(objectType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                return (T)IoDeProcessing.DeDictionary(ref ioString, objectType);
            }
            else
                throw new InvalidDataException($"Object of type {objectType} is not supported.");

        }
    }
}
