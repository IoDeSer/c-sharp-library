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

        internal static string EscapeString(this string input)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in input)
            {
                switch (c)
                {
                    case '\u0008': // Backspace
                        result.Append("\\b");
                        break;
                    case '\u000C': // Form feed
                        result.Append("\\f");
                        break;
                    case '\n': // Newline
                        result.Append("\\n");
                        break;
                    case '\r': // Carriage return
                        result.Append("\\r");
                        break;
                    case '\t': // Tab
                        result.Append("\\t");
                        break;
                    case '\'': // Single quote
                        result.Append("\\'");
                        break;
                    case '\"': // Double quote
                        result.Append("\\\"");
                        break;
                    case '\\': // Backslash
                        result.Append("\\\\");
                        break;
                    case '\0': // Null character
                        result.Append("\\0");
                        break;
                    case '|':
                        result.Append("||");
                        break;
                    default:
                        result.Append(c); // Append as-is for non-special characters
                        break;
                }
            }

            return result.ToString();
        }

        internal static string UnescapeString(this string input)
        {
            StringBuilder result = new StringBuilder();
            bool isEscaping = false;

            foreach (char c in input)
            {
                if (isEscaping)
                {
                    switch (c)
                    {
                        case 'b':
                            result.Append('\u0008'); // Backspace
                            break;
                        case 'f':
                            result.Append('\u000C'); // Form feed
                            break;
                        case 'n':
                            result.Append('\n'); // Newline
                            break;
                        case 'r':
                            result.Append('\r'); // Carriage return
                            break;
                        case 't':
                            result.Append('\t'); // Tab
                            break;
                        case '\'':
                            result.Append('\''); // Single quote
                            break;
                        case '\"':
                            result.Append('\"'); // Double quote
                            break;
                        case '\\':
                            result.Append('\\'); // Backslash
                            break;
                        case '0':
                            result.Append('\0'); // Null character
                            break;
                        default:
                            result.Append(c); // If unknown, append as-is
                            break;
                    }
                    isEscaping = false; // Reset escaping flag
                }
                else if (c == '\\')
                {
                    isEscaping = true; // Start escape sequence
                }
                else
                {
                    result.Append(c); // Regular character
                }
            }

            return result.Replace("||", "|").ToString();
        }
    }
}
