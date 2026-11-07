using System;
using System.Collections;
using System.Globalization;
using System.Numerics;
using IoDeSer.DeSer.Processing;
using IoDeSer.Errors;
using IoDeSer.Extensions;
using IoDeSer.Format;
using IoDeSer.Helpers;

namespace IoDeSer.DeSer
{
    internal static class IoSer
    {
        internal static string WriteToString(object obj, IoFormat format)
        {
            IoWriter writer = new IoWriter(format);
            Write(writer, obj);
            return writer.ToString();
        }

        /// <summary>
        /// Writes <paramref name="obj"/> using its runtime type.
        /// </summary>
        internal static void Write(IoWriter writer, object obj)
        {
            if (obj == null)
            {
                writer.WriteNone();
                return;
            }

            Type objectType = obj.GetType();

            switch (obj)
            {
                case IoValue value:
                    value.Write(writer);
                    return;
                case bool b:
                    writer.WritePlain('b', b ? "true" : "false");
                    return;
                case char c:
                    writer.WritePrimitive('c', c.ToString());
                    return;
                case string s:
                    writer.WritePrimitive('s', s);
                    return;
                case double d:
                    writer.WritePlain('f', PrimitiveText.FormatDouble(d));
                    return;
                case float f:
                    writer.WritePlain('f', PrimitiveText.FormatSingle(f));
                    return;
                case decimal m:
                    writer.WritePlain('f', m.ToString(CultureInfo.InvariantCulture));
                    return;
                case BigInteger big:
                    writer.WritePlain('i', big.ToString(CultureInfo.InvariantCulture));
                    return;
                case DateTime dateTime:
                    writer.WritePrimitive('s', PrimitiveText.FormatDateTime(dateTime));
                    return;
                case DateTimeOffset dateTimeOffset:
                    writer.WritePrimitive('s', PrimitiveText.FormatDateTimeOffset(dateTimeOffset));
                    return;
                case TimeSpan timeSpan:
                    IoSerProcessing.SerTimeSpan(writer, timeSpan);
                    return;
            }

            switch (objectType)
            {
                case Type t when t.IsEnum:
                    IoSerProcessing.SerEnum(writer, obj);
                    return;

                case Type t when IoTypes.IsInteger(t):
                    writer.WritePlain('i', Convert.ToString(obj, CultureInfo.InvariantCulture));
                    return;

                case Type t when t.IsPrimitive:
                    // IntPtr, UIntPtr
                    throw new IoTypeNotSupportedException(objectType);

                case Type t when IoTypes.IsKeyValuePair(t) || IoTypes.IsValueTuple(t) || IoTypes.IsTuple(t):
                    IoSerProcessing.SerTuple(writer, obj);
                    return;

                case Type t when t.IsArray && ((Array)obj).Rank > 1:
                    IoSerProcessing.SerMultidimensionalArray(writer, (Array)obj);
                    return;

                case Type t when obj is IDictionary || IoTypes.DictionaryTypes(t) != null:
                    IoSerProcessing.SerDictionary(writer, (IEnumerable)obj);
                    return;

                case Type t when obj is IEnumerable enumerable:
                    IoSerProcessing.SerIEnumerable(writer, enumerable);
                    return;

                case Type t when t.IsClass || t.IsValueType:
                    IoSerProcessing.SerClass(writer, obj);
                    return;

                default:
                    throw new IoTypeNotSupportedException(objectType);
            }
        }
    }
}
