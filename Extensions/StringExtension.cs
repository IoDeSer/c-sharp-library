using System.Globalization;
using System.Text;

namespace IoDeSer.Extensions
{
    internal static class StringExtension
    {
        /// <summary>
        /// Escapes content of <c>s|..|</c> and <c>c|..|</c> the same way as Rust library does:
        /// <c>\t \r \n \\ \' \" \0</c>, other control characters as <c>\u{XX}</c>
        /// and characters with special meaning: <c>|</c> as <c>\|</c>, <c>+</c> as <c>\+</c> and <c>-></c> as <c>-\></c>.
        /// </summary>
        internal static string EscapeString(this string input)
        {
            StringBuilder result = new StringBuilder(input.Length);

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                switch (c)
                {
                    case '\t': result.Append("\\t"); break;
                    case '\r': result.Append("\\r"); break;
                    case '\n': result.Append("\\n"); break;
                    case '\\': result.Append("\\\\"); break;
                    case '\'': result.Append("\\'"); break;
                    case '\"': result.Append("\\\""); break;
                    case '\0': result.Append("\\0"); break;
                    case '|': result.Append("\\|"); break;
                    case '+': result.Append("\\+"); break;
                    case '>':
                        if (result.Length > 0 && result[result.Length - 1] == '-')
                            result.Append("\\>");
                        else
                            result.Append('>');
                        break;
                    default:
                        if (char.IsHighSurrogate(c) && i + 1 < input.Length && char.IsLowSurrogate(input[i + 1]))
                        {
                            result.Append(c).Append(input[i + 1]);
                            i++;
                        }
                        else if (char.IsControl(c) || char.IsSurrogate(c))
                        {
                            // lone surrogate is not a valid character in other languages
                            result.Append("\\u{").Append(((int)c).ToString("x")).Append('}');
                        }
                        else
                        {
                            result.Append(c);
                        }
                        break;
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Reverses <see cref="EscapeString"/>. Unknown escape sequences are kept as the escaped character.
        /// </summary>
        internal static string UnescapeString(this string input)
        {
            if (input.IndexOf('\\') < 0)
                return input;

            StringBuilder result = new StringBuilder(input.Length);

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c != '\\')
                {
                    result.Append(c);
                    continue;
                }

                i++;
                if (i >= input.Length)
                    break;

                switch (input[i])
                {
                    case 'b': result.Append('\u0008'); break;
                    case 'f': result.Append('\u000C'); break;
                    case 'n': result.Append('\n'); break;
                    case 'r': result.Append('\r'); break;
                    case 't': result.Append('\t'); break;
                    case '0': result.Append('\0'); break;
                    case 'u':
                        // \u{XXXX}
                        int end = input.IndexOf('}', i);
                        if (i + 1 < input.Length && input[i + 1] == '{' && end > i + 2
                            && int.TryParse(input.Substring(i + 2, end - i - 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int code)
                            && code >= 0 && code <= 0x10FFFF)
                        {
                            if (code >= 0xD800 && code <= 0xDFFF)
                                result.Append((char)code);
                            else
                                result.Append(char.ConvertFromUtf32(code));
                            i = end;
                        }
                        else
                        {
                            result.Append('u');
                        }
                        break;
                    default:
                        // \| \+ \> \\ \' \"
                        result.Append(input[i]);
                        break;
                }
            }

            return result.ToString();
        }
    }
}
