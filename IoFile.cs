using System;
using System.IO;
using IoDeSer.DeSer;

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
        /// Throws <see cref="ArgumentNullException"/> if the object <paramref name="obj"/>  or some of its components are null.
        /// </para>
        /// <para>
        /// Throws <see cref="NotSupportedException"/> if the object's type is not supported.
        /// </para>
        /// </summary>
        /// <returns>String in .io file format.</returns>
        public static string WriteToString<T>(T obj)
        {
            return IoSer.WriteToString(obj, 0);
        }
        /// <summary>
        /// Converts the provided object <paramref name="obj"/> to .io file format and writes it to the stream <paramref name="sw"/>.
        /// <para>
        /// ---</para>
        /// <para>
        /// Throws <see cref="ArgumentNullException"/> if the object <paramref name="obj"/> or some of its components are null.
        /// </para>
        /// <para>
        /// Throws <see cref="NotSupportedException"/> if the object's type is not supported.
        /// </para>
        /// </summary>
        public static void WriteToFile<T>(T obj, StreamWriter sw)
        {
            sw.Write(WriteToString(obj));
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
        /// ---
        /// </para>
        /// <para>
        /// Throws <see cref="InvalidCastException"/> if the provided type <typeparamref name="T"/> does not contain some property from the file.
        /// </para>
        /// <para>
        /// Throws <see cref="InvalidDataException"/> when the type <typeparamref name="T"/> is not supported.
        /// </para>
        /// <para>
        /// Throws <see cref="MissingMethodException"/> when the type <typeparamref name="T"/> does not implement parameterless constructor.
        /// </para>
        /// </summary>
        /// <param name="sr">Stream to .io file.</param>
        /// <typeparam name="T">The type of object that will be converted from stream <paramref name="sr"/>.</typeparam>
        /// <returns>Object of type <typeparamref name="T"/>.</returns>
        public static T ReadFromFile<T>(StreamReader sr)
        {
            return ReadFromString<T>(sr.ReadToEnd().Trim());
        }
        /// <summary>
        /// Reads content of string <paramref name="ioString"/> in .io file format and tries to convert it to type <typeparamref name="T"/>.
        /// <para>
        /// ---
        /// </para>
        /// <para>
        /// Throws <see cref="InvalidCastException"/> if the provided type <typeparamref name="T"/> does not contain some property from the string <paramref name="ioString"/>.
        /// </para>
        /// <para>
        /// Throws <see cref="InvalidDataException"/> when the type <typeparamref name="T"/> is not supported.
        /// </para>
        /// <para>
        /// Throws <see cref="MissingMethodException"/> when the type <typeparamref name="T"/> does not implement parameterless constructor.
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
