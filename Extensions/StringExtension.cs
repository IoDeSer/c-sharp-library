using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IoDeSer.Extensions
{
    internal static class StringExtension
    {
        static string[] Split(this string str, string[] splits, string splitter, int next)
        {
            int indexOf = str.IndexOf(splitter);
            int start = indexOf + splitter.Length;
            int end = str.Length - (indexOf + splitter.Length);

            if (indexOf == -1)
            {
                splits[next] = str;
                return splits;
            }
            else
            {
                string strNew = str.Substring(0, indexOf);
                splits[next] = strNew;
                next++;
                str.Substring(start, end).Split(splits, splitter, next);
                return splits;
            }
        }


        internal static string[] Split(this string str, string splitter)
        {
            Regex regex = new Regex(Regex.Escape(splitter));
            int count = regex.Matches(str).Count;

            if (count == 0)
                return new string[0];
            else
            {
                string[] splits = new string[count + 1];
                return str.Split(splits, splitter, 0);
            }
        }
    }
}
