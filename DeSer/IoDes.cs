using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using IoDeSer.Processing;

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
                obj = IoDeProcessing.DeIEnumerable(ref ioString,  objectType);
            }
            else if (objectType.IsClass)
            {
                obj = IoDeProcessing.DeClass(ref ioString, objectType);
            }
            else if(objectType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                obj = IoDeProcessing.DeDictionary(ref ioString, objectType);
            }
            else
                throw new InvalidDataException($"Object of type {objectType} is not supported.");


            return obj;
        }
    }
}
