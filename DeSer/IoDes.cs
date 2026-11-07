using System;
using System.Collections;
using System.Globalization;
using System.Numerics;
using IoDeSer.DeSer.Processing;
using IoDeSer.Errors;
using IoDeSer.Errors.Deserialization;
using IoDeSer.Extensions;
using IoDeSer.Helpers;

namespace IoDeSer.DeSer
{
    internal static class IoDes
    {
        internal static object ReadFromString(string ioString, Type objectType)
        {
            return Read(IoValue.Parse(ioString), objectType);
        }

        internal static object Read(IoValue value, Type objectType)
        {
            if (typeof(IoValue).IsAssignableFrom(objectType))
            {
                if (!objectType.IsInstanceOfType(value))
                    throw new IoDeserializationException($"Expected {objectType.Name}, found value with tag '{value.Tag}'.");
                return value;
            }

            if (value is IoNone)
            {
                if (objectType.IsValueType && Nullable.GetUnderlyingType(objectType) == null)
                    throw new IoDeserializationException($"Value n|| (null) cannot be assigned to non-nullable type '{objectType.Name}'.");
                return null;
            }

            objectType = Nullable.GetUnderlyingType(objectType) ?? objectType;

            switch (objectType)
            {
                case Type t when t == typeof(object):
                    return IoDeProcessing.DeObject(value);

                case Type t when t == typeof(bool):
                    string b = value.ExpectPrimitive('b', t);
                    if (bool.TryParse(b, out bool boolean))
                        return boolean;
                    throw ParseError(b, t);

                case Type t when t == typeof(char):
                    string c = value.ExpectPrimitive('c', t);
                    if (c.Length != 1)
                        throw new IoDeserializationException($"Expected exactly one character for type 'Char', found \"{c}\".");
                    return c[0];

                case Type t when t == typeof(string):
                    return value.ExpectPrimitive('s', t);

                case Type t when IoTypes.IsInteger(t):
                    string integer = value.ExpectPrimitive('i', t);
                    try
                    {
                        return Convert.ChangeType(decimal.Parse(integer, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture), t, CultureInfo.InvariantCulture);
                    }
                    catch (Exception e) when (e is FormatException || e is OverflowException)
                    {
                        throw ParseError(integer, t, e);
                    }

                case Type t when t == typeof(BigInteger):
                    string big = value.ExpectPrimitive('i', t);
                    if (BigInteger.TryParse(big, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out BigInteger bigInteger))
                        return bigInteger;
                    throw ParseError(big, t);

                case Type t when t == typeof(double) || t == typeof(float):
                    string f = value.ExpectPrimitive('f', t);
                    if (!PrimitiveText.TryParseDouble(f, out double d))
                        throw ParseError(f, t);
                    return t == typeof(float) ? (object)(float)d : d;

                case Type t when t == typeof(decimal):
                    string m = value.ExpectPrimitive('f', t);
                    if (decimal.TryParse(m, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal dec))
                        return dec;
                    throw ParseError(m, t);

                case Type t when t == typeof(DateTime) || t == typeof(DateTimeOffset):
                    string date = value.ExpectPrimitive('s', t);
                    if (!PrimitiveText.TryParseDateTimeOffset(date, out DateTimeOffset dto))
                        throw ParseError(date, t);
                    if (t == typeof(DateTimeOffset))
                        return dto;
                    return dto.Offset == TimeSpan.Zero ? dto.UtcDateTime : dto.LocalDateTime;

                case Type t when t == typeof(TimeSpan):
                    return IoDeProcessing.DeTimeSpan(value);

                case Type t when t.IsEnum:
                    return IoDeProcessing.DeEnum(value, t);

                case Type t when t.IsPrimitive:
                    throw new IoTypeNotSupportedException(t);

                case Type t when IoTypes.IsKeyValuePair(t) || IoTypes.IsValueTuple(t) || IoTypes.IsTuple(t):
                    return IoDeProcessing.DeTuple(value, t);

                case Type t when t.IsArray && t.GetArrayRank() > 1:
                    return IoDeProcessing.DeMultidimensionalArray(value, t);

                case Type t when t.IsArray:
                    return IoDeProcessing.DeArray(value, t);

                case Type t when IoTypes.DictionaryTypes(t) != null:
                    return IoDeProcessing.DeDictionary(value, t);

                case Type t when typeof(IEnumerable).IsAssignableFrom(t):
                    return IoDeProcessing.DeIEnumerable(value, t);

                case Type t when (t.IsClass || t.IsValueType) && !t.IsAbstract:
                    return IoDeProcessing.DeClass(value, t);

                default:
                    throw new IoTypeNotSupportedException(objectType);
            }
        }

        static IoDeserializationException ParseError(string text, Type type, Exception inner = null)
        {
            return new IoDeserializationException($"Value \"{text}\" cannot be converted to type '{type.Name}'.", inner);
        }
    }
}
