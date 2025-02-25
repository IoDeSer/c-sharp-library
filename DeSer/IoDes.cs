using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using IoDeSer.DeSer.Processing;
using IoDeSer.Errors;
using IoDeSer.Errors.Deserialization;

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

            ErrorsCheck(ioString);

            Match ioMatch = ioPattern.Match(ioString);
            ioString = ioMatch.Groups[1].Value;

            switch (objectType)
            {
                case Type t when t.IsPrimitive:
                    return Convert.ChangeType(ioString, t);

                case Type t when t == typeof(string):
                    return IoDeProcessing.DeString(ioString);

                case Type t when typeof(DateTime).IsAssignableFrom(t) ||
                                      typeof(DateTimeOffset).IsAssignableFrom(t) ||
                                      typeof(TimeSpan).IsAssignableFrom(t) ||
                                      typeof(BigInteger).IsAssignableFrom(t):
                    return IoDeProcessing.DeDateTime(ref ioString, t);

                case Type t when typeof(IEnumerable).IsAssignableFrom(t):
                    return IoDeProcessing.DeIEnumerable(ref ioString, t);

                case Type t when t.IsClass:
                    return IoDeProcessing.DeClass(ref ioString, t);

                case Type t when t.IsEnum:
                    return IoDeProcessing.DeEnum(ref ioString, t);

                case Type t when t.IsValueType && t.IsGenericType && t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>):
                    return IoDeProcessing.DeDictionary(ref ioString, t);

                case Type t when t.IsValueType:
                    return IoDeProcessing.DeStruct(ref ioString, t);

                default:
                    throw new IoTypeNotSupportedException(objectType);
            }
        }

        private static void ErrorsCheck(string ioString)
        {
            if (String.IsNullOrEmpty(ioString))
            {
                throw new IoEmptyStringException();
            }

            if (ioString[0] != '|' || ioString[ioString.Length-1] != '|')
            {
                throw new IoVerticalBarsException();
            }
            //TODO add errors
        }
    }
}
