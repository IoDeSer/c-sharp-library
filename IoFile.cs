using System;
using System.IO;
using IoDeSer.DeSer;
using IoDeSer.Errors;
using IoDeSer.Errors.Deserialization;
using IoDeSer.Errors.Serialization;

namespace IoDeSer
{
    /// <summary>
    /// Class managing read/write conversion of .io file format.
    /// </summary>
    public static class IoFile
    {
        /*
         *
         * Writers
         *  Object -> TO
         *
         */

        /// <summary>
        /// Converts the provided object <paramref name="obj"/> to .io file format.
        /// <para>
        /// ---</para>
        /// <para>
        /// Throws <see cref="IoTypeNotSupportedException"/> if the object's type is not supported.
        /// </para>
        /// <para>
        /// Throws <see cref="IoSerializationException"/> if the object is nested too deep (e.g. has reference cycle) or property names are wrong.
        /// </para>
        /// </summary>
        /// <param name="obj">Object to serialize. Null values are written as <c>n||</c>.</param>
        /// <param name="format">Layout of the string, <see cref="IoFormat.Pretty"/> by default.</param>
        /// <returns>String in .io file format.</returns>
        public static string WriteToString<T>(T obj, IoFormat format = default(IoFormat))
        {
            return IoSer.WriteToString(obj, format);
        }
        /// <summary>
        /// Converts the provided object <paramref name="obj"/> to .io file format and writes it to the stream <paramref name="sw"/>.
        /// <para>
        /// ---</para>
        /// <para>
        /// Throws <see cref="IoTypeNotSupportedException"/> if the object's type is not supported.
        /// </para>
        /// <para>
        /// Throws <see cref="IoSerializationException"/> if the object is nested too deep (e.g. has reference cycle) or property names are wrong.
        /// </para>
        /// </summary>
        /// <param name="obj">Object to serialize. Null values are written as <c>n||</c>.</param>
        /// <param name="sw">Stream to .io file.</param>
        /// <param name="format">Layout of the string, <see cref="IoFormat.Pretty"/> by default.</param>
        public static void WriteToFile<T>(T obj, StreamWriter sw, IoFormat format = default(IoFormat))
        {
            sw.Write(WriteToString(obj, format));
        }




        /*
         *
         * Readers
         *  Object <- FROM
         *
         */


        /// <summary>
        /// Reads the content of .io file as stream <paramref name="sr"/> and tries to convert it to type <typeparamref name="T"/>.
        /// <para>
        /// The file can be written in any <see cref="IoFormat"/>, whitespace between tokens is ignored.
        /// </para>
        /// <para>
        /// ---
        /// </para>
        /// <para>
        /// Throws <see cref="IoDeserializationException"/> if the file is not in .io format or does not match type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Throws <see cref="IoMissingPropertyException"/> if the provided type <typeparamref name="T"/> does not contain some property from the file.
        /// </para>
        /// <para>
        /// Throws <see cref="IoMissingConstructorException"/> when the type <typeparamref name="T"/> does not implement parameterless constructor.
        /// </para>
        /// <para>
        /// Throws <see cref="IoTypeNotSupportedException"/> when the type <typeparamref name="T"/> is not supported.
        /// </para>
        /// </summary>
        /// <param name="sr">Stream to .io file.</param>
        /// <typeparam name="T">The type of object that will be converted from stream <paramref name="sr"/>.</typeparam>
        /// <returns>Object of type <typeparamref name="T"/>.</returns>
        public static T ReadFromFile<T>(StreamReader sr)
        {
            return ReadFromString<T>(sr.ReadToEnd());
        }
        /// <summary>
        /// Reads content of string <paramref name="ioString"/> in .io file format and tries to convert it to type <typeparamref name="T"/>.
        /// <para>
        /// The string can be written in any <see cref="IoFormat"/>, whitespace between tokens is ignored.
        /// </para>
        /// <para>
        /// ---
        /// </para>
        /// <para>
        /// Throws <see cref="IoDeserializationException"/> if the string is not in .io format or does not match type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Throws <see cref="IoMissingPropertyException"/> if the provided type <typeparamref name="T"/> does not contain some property from the string <paramref name="ioString"/>.
        /// </para>
        /// <para>
        /// Throws <see cref="IoMissingConstructorException"/> when the type <typeparamref name="T"/> does not implement parameterless constructor.
        /// </para>
        /// <para>
        /// Throws <see cref="IoTypeNotSupportedException"/> when the type <typeparamref name="T"/> is not supported.
        /// </para>
        /// </summary>
        /// <param name="ioString">String in .io file format.</param>
        /// <typeparam name="T">The type of object that will be converted from string <paramref name="ioString"/>.</typeparam>
        /// <returns>Object of type <typeparamref name="T"/>.</returns>
        public static T ReadFromString<T>(string ioString)
        {
            return (T)IoDes.ReadFromString(ioString, typeof(T));
        }

    }
}
