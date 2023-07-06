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
    internal static class IoDes
    {
        /// <summary>
        /// Helps in getting value from .io file pattern.
        /// </summary>
        static Regex ioPattern = new Regex(@"^[|]((.|[\n])*)[|]$");

        internal static object ReadFromString(string ioString, Type objectType)
        {
            Match ioMatch = ioPattern.Match(ioString);
            ioString = ioMatch.Groups[1].Value;

            if (objectType.IsPrimitive || objectType == typeof(string))
            {
                // TODO in string check and change special tokens for '|', "->" and '+'
                return Convert.ChangeType(ioString, objectType);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(objectType))
            {
                return IoDeProcessing.DeIEnumerable(ref ioString,  objectType);
            }
            else if (objectType.IsClass)
            {
                return IoDeProcessing.DeClass(ref ioString, objectType);
            }
            else if(objectType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                return IoDeProcessing.DeDictionary(ref ioString, objectType);
            }
            else
                throw new InvalidDataException($"Object of type {objectType} is not supported.");

        }
    }
}
