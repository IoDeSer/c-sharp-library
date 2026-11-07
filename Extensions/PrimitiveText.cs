using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace IoDeSer.Extensions
{
    /// <summary>
    /// Text form of numbers and dates, compatible with the Rust library.
    /// </summary>
    internal static class PrimitiveText
    {
        static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        /*
         * Floats
         *  Rust writes the shortest form that reads back to the same value, without exponent: 1, 0.1, 100000000000000000000, NaN, inf, -inf
         */

        internal static string FormatDouble(double value)
        {
            if (double.IsNaN(value)) return "NaN";
            if (double.IsPositiveInfinity(value)) return "inf";
            if (double.IsNegativeInfinity(value)) return "-inf";
            return WithoutExponent(value.ToString("R", Invariant));
        }

        internal static string FormatSingle(float value)
        {
            if (float.IsNaN(value)) return "NaN";
            if (float.IsPositiveInfinity(value)) return "inf";
            if (float.IsNegativeInfinity(value)) return "-inf";
            return WithoutExponent(value.ToString("R", Invariant));
        }

        /// <summary>
        /// Changes "1.5E+20" into "150000000000000000000" and "1E-07" into "0.0000001".
        /// </summary>
        static string WithoutExponent(string number)
        {
            int e = number.IndexOfAny(new[] { 'E', 'e' });
            if (e < 0)
                return number;

            int exponent = int.Parse(number.Substring(e + 1), NumberStyles.AllowLeadingSign, Invariant);
            string mantissa = number.Substring(0, e);
            bool negative = mantissa.StartsWith("-");
            if (negative)
                mantissa = mantissa.Substring(1);

            int dot = mantissa.IndexOf('.');
            string digits = dot < 0 ? mantissa : mantissa.Remove(dot, 1);
            int point = (dot < 0 ? mantissa.Length : dot) + exponent;

            StringBuilder result = new StringBuilder();
            if (negative)
                result.Append('-');

            if (point <= 0)
            {
                result.Append("0.").Append('0', -point).Append(digits);
            }
            else if (point >= digits.Length)
            {
                result.Append(digits).Append('0', point - digits.Length);
            }
            else
            {
                result.Append(digits, 0, point).Append('.').Append(digits, point, digits.Length - point);
            }
            return result.ToString();
        }

        /// <summary>
        /// Accepts everything Rust writes (and reads): numbers with optional exponent, inf, infinity and NaN in any letter case.
        /// </summary>
        internal static bool TryParseDouble(string text, out double value)
        {
            string lower = text.ToLowerInvariant();
            string unsigned = lower.TrimStart('+', '-');
            bool negative = lower.StartsWith("-");

            if (lower.Length - unsigned.Length <= 1)
            {
                if (unsigned == "inf" || unsigned == "infinity")
                {
                    value = negative ? double.NegativeInfinity : double.PositiveInfinity;
                    return true;
                }
                if (unsigned == "nan")
                {
                    value = double.NaN;
                    return true;
                }
            }
            return double.TryParse(text, NumberStyles.Float, Invariant, out value);
        }

        /*
         * Dates
         *  RFC 3339, the same as chrono's to_rfc3339: 2024-03-05T07:08:09.120+00:00
         *  UTC is written as "+00:00" instead of "Z", because Rust's NaiveDateTime does not accept "Z".
         */

        internal static string FormatDateTime(DateTime value)
        {
            TimeSpan offset = value.Kind == DateTimeKind.Local ? TimeZoneInfo.Local.GetUtcOffset(value) : TimeSpan.Zero;
            return Format(value, offset);
        }

        internal static string FormatDateTimeOffset(DateTimeOffset value) => Format(value.DateTime, value.Offset);

        static string Format(DateTime local, TimeSpan offset)
        {
            StringBuilder result = new StringBuilder(local.ToString("yyyy-MM-dd'T'HH:mm:ss", Invariant));

            // 0, 3, 6 or 9 digits of fraction, like chrono does
            long ticks = local.Ticks % TimeSpan.TicksPerSecond;
            if (ticks != 0)
            {
                if (ticks % 10000 == 0)
                    result.Append('.').Append((ticks / 10000).ToString("000", Invariant));
                else if (ticks % 10 == 0)
                    result.Append('.').Append((ticks / 10).ToString("000000", Invariant));
                else
                    result.Append('.').Append((ticks * 100).ToString("000000000", Invariant));
            }

            result.Append(offset < TimeSpan.Zero ? '-' : '+');
            TimeSpan abs = offset.Duration();
            result.Append(abs.Hours.ToString("00", Invariant)).Append(':').Append(abs.Minutes.ToString("00", Invariant));
            return result.ToString();
        }

        static readonly Regex Rfc3339 = new Regex(
            @"^(\d{4})-(\d{2})-(\d{2})[Tt ](\d{2}):(\d{2}):(\d{2})(?:\.(\d+))?(?:([Zz])|([+-])(\d{2}):(\d{2}))$",
            RegexOptions.CultureInvariant);

        internal static bool TryParseDateTimeOffset(string text, out DateTimeOffset value)
        {
            value = default(DateTimeOffset);
            Match m = Rfc3339.Match(text);
            if (!m.Success)
                return false;

            try
            {
                TimeSpan offset = TimeSpan.Zero;
                if (!m.Groups[8].Success)
                {
                    offset = new TimeSpan(int.Parse(m.Groups[10].Value, Invariant), int.Parse(m.Groups[11].Value, Invariant), 0);
                    if (m.Groups[9].Value == "-")
                        offset = offset.Negate();
                }

                value = new DateTimeOffset(
                    int.Parse(m.Groups[1].Value, Invariant), int.Parse(m.Groups[2].Value, Invariant), int.Parse(m.Groups[3].Value, Invariant),
                    int.Parse(m.Groups[4].Value, Invariant), int.Parse(m.Groups[5].Value, Invariant), int.Parse(m.Groups[6].Value, Invariant),
                    offset);

                if (m.Groups[7].Success)
                {
                    // C# keeps 7 digits (100 ns), the rest is cut off
                    string fraction = m.Groups[7].Value.PadRight(7, '0').Substring(0, 7);
                    value = value.AddTicks(long.Parse(fraction, Invariant));
                }
                return true;
            }
            catch (ArgumentException)
            {
                // e.g. month 13 or offset bigger than 14 hours
                return false;
            }
        }
    }
}
