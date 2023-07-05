using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
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
        /// Throws <see cref="ArgumentNullException"/> if the object or some of its components <paramref name="obj"/> are null.
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
        /// Throws <see cref="ArgumentNullException"/> if the object or some of its components <paramref name="obj"/> are null.
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
        /// Reads the content of .io file as stream <paramref name="sr"/> and tries to convert it to type <paramref name="objectType"/>.
        /// <para>
        /// ---
        /// </para>
        /// <para>
        /// Throws <see cref="InvalidCastException"/> if the provided type <paramref name="objectType"/> does not contain property read from the file.
        /// </para>
        /// <para>
        /// Throws <see cref="InvalidDataException"/> when the type <paramref name="objectType"/> is not supported.
        /// </para>
        /// <para>
        /// Throws <see cref="MissingMethodException"/> when the type <paramref name="objectType"/> does not implement parameterless constructor.
        /// </para>
        /// </summary>
        /// <param name="sr">Stream to .io file.</param>
        /// <param name="objectType">The type of object that will be converted from stream <paramref name="sr"/>.</param>
        /// <returns>Object of type <paramref name="objectType"/>.</returns>
        public static T ReadFromFile<T>(StreamReader sr)
        {
            return ReadFromString<T>(sr.ReadToEnd().Trim());
        }
        /// <summary>
        /// Reads content of string <paramref name="ioString"/> in .io file format and tries to convert it to type <paramref name="objectType"/>.
        /// <para>
        /// ---
        /// </para>
        /// <para>
        /// Throws <see cref="InvalidCastException"/> if the provided type <paramref name="objectType"/> does not contain property read from the string <paramref name="ioString"/>.
        /// </para>
        /// <para>
        /// Throws <see cref="InvalidDataException"/> when the type <paramref name="objectType"/> is not supported.
        /// </para>
        /// <para>
        /// Throws <see cref="MissingMethodException"/> when the type <paramref name="objectType"/> does not implement parameterless constructor.
        /// </para>
        /// </summary>
        /// <param name="ioString">String in .io file format.</param>
        /// <param name="objectType">The type of object that will be converted from string <paramref name="ioString"/>.</param>
        /// <returns>Object of type <paramref name="objectType"/>.</returns>
        public static T ReadFromString<T>(string ioString)
        {
            return IoDes.ReadFromString<T>(ioString);
        }
        
    }
}
