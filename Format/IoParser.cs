using System.Collections.Generic;
using IoDeSer.Errors.Deserialization;
using IoDeSer.Extensions;

namespace IoDeSer.Format
{
    /// <summary>
    /// Parser of .io formatted strings.
    /// <code>
    /// value     = primitive | none | array | map | object | enum
    /// primitive = TAG '|' content '|'           TAG: i (integer), f (float), b (bool), c (char), s (string)
    /// none      = 'n||'
    /// array     = 'a||' | 'a|' value ('+' value)* '|'
    /// map       = 'm||' | 'm|' pair ('+' pair)* '|'          pair = 'a|' value '+' value '|'
    /// object    = 'o||' | 'o|' (name '->' value)* '|'        names are unique within one object
    /// enum      = 'e|' name '->' value '|'
    /// name      = one or more characters except whitespace, '|', '+' and the sequence '->'
    /// </code>
    /// Whitespace (space, \t, \r, \n) is allowed between all tokens, except between a tag and its opening bar and inside primitive content.
    /// </summary>
    internal sealed class IoParser
    {
        readonly string s;
        int p;
        int depth;

        IoParser(string s)
        {
            this.s = s;
        }

        internal static bool IsWhitespace(char c) => c == ' ' || c == '\t' || c == '\r' || c == '\n';

        internal static IoValue Parse(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new IoEmptyStringException();

            IoParser parser = new IoParser(input);
            if (input[0] == '﻿')
                parser.p = 1;

            parser.SkipWhitespace();
            if (parser.p == input.Length)
                throw new IoEmptyStringException();

            IoValue value = parser.Value();
            parser.SkipWhitespace();
            if (parser.p != input.Length)
                throw parser.Error("Unexpected characters after the end of the value");
            return value;
        }

        IoDeserializationException Error(string kind)
        {
            int length = System.Math.Min(40, s.Length - p);
            return new IoDeserializationException($"{kind} (at position {p}): \"{s.Substring(p, length)}\"");
        }

        char? Peek() => p < s.Length ? s[p] : (char?)null;

        void SkipWhitespace()
        {
            while (p < s.Length && IsWhitespace(s[p]))
                p++;
        }

        void ExpectBar(string after)
        {
            if (Peek() != '|')
                throw Error($"Expected '|' after {after}");
            p++;
        }

        /// <summary>
        /// Returns true and consumes the closing bar if the next non-whitespace character is '|'.
        /// </summary>
        bool TryClose()
        {
            SkipWhitespace();
            if (Peek() == '|')
            {
                p++;
                return true;
            }
            return false;
        }

        IoValue Value()
        {
            if (depth >= IoValue.MaxDepth)
                throw Error($"Maximum nesting level of {IoValue.MaxDepth} exceeded");
            depth++;
            IoValue value = ValueInner();
            depth--;
            return value;
        }

        IoValue ValueInner()
        {
            SkipWhitespace();
            if (p >= s.Length)
                throw Error("Expected a value, but the input has ended");

            char tag = s[p];
            if ("ifbcsnamoe".IndexOf(tag) < 0)
                throw Error($"Unknown type tag '{tag}'");
            p++;
            ExpectBar($"tag '{tag}'");

            switch (tag)
            {
                case 'n':
                    ExpectBar("'n|'");
                    return IoNone.Instance;

                case 'a':
                    return new IoArray(Sequence());

                case 'm':
                    List<KeyValuePair<IoValue, IoValue>> entries = new List<KeyValuePair<IoValue, IoValue>>();
                    foreach (IoValue pair in Sequence())
                    {
                        if (!(pair is IoArray kv) || kv.Items.Count != 2)
                            throw new IoDeserializationException($"Map entries must be arrays of exactly two elements: a| key + value |, found: {pair.Snippet()}");
                        entries.Add(new KeyValuePair<IoValue, IoValue>(kv.Items[0], kv.Items[1]));
                    }
                    return new IoMap(entries);

                case 'o':
                    List<KeyValuePair<string, IoValue>> fields = new List<KeyValuePair<string, IoValue>>();
                    HashSet<string> names = new HashSet<string>();
                    while (!TryClose())
                    {
                        int start = p;
                        string name = Name();
                        if (!names.Add(name))
                        {
                            p = start;
                            throw Error($"Field '{name}' is defined more than once");
                        }
                        fields.Add(new KeyValuePair<string, IoValue>(name, Value()));
                    }
                    return new IoObject(fields);

                case 'e':
                    string variant = Name();
                    IoValue variantValue = Value();
                    if (!TryClose())
                        throw Error("Expected '|' closing the enum value");
                    return new IoEnum(variant, variantValue);

                default:
                    return new IoPrimitive(tag, PrimitiveContent().UnescapeString());
            }
        }

        /// <summary>
        /// Elements separated by '+' until the closing bar. The opening bar is already consumed.
        /// </summary>
        List<IoValue> Sequence()
        {
            List<IoValue> items = new List<IoValue>();
            if (TryClose())
                return items;

            while (true)
            {
                items.Add(Value());
                SkipWhitespace();
                char? next = Peek();
                if (next == '+')
                {
                    p++;
                }
                else if (next == '|')
                {
                    p++;
                    return items;
                }
                else
                {
                    throw Error("Expected '+' or '|' after an element");
                }
            }
        }

        /// <summary>
        /// Field or variant name followed by '->'.
        /// </summary>
        string Name()
        {
            SkipWhitespace();
            int start = p;
            int end = start;
            while (end < s.Length)
            {
                char c = s[end];
                if ((c == '-' && end + 1 < s.Length && s[end + 1] == '>') || c == '|' || c == '+' || IsWhitespace(c))
                    break;
                end++;
            }
            if (end == start)
                throw Error("Expected 'name->'");

            string name = s.Substring(start, end - start);
            p = end;
            SkipWhitespace();
            if (string.CompareOrdinal(s, p, "->", 0, 2) != 0)
                throw Error($"Expected '->' after name '{name}'");
            p += 2;
            return name;
        }

        /// <summary>
        /// Raw content of a primitive up to the first unescaped bar, which is consumed.
        /// </summary>
        string PrimitiveContent()
        {
            int start = p;
            int i = p;
            while (i < s.Length)
            {
                if (s[i] == '\\')
                {
                    i += 2;
                }
                else if (s[i] == '|')
                {
                    p = i + 1;
                    return s.Substring(start, i - start);
                }
                else
                {
                    i++;
                }
            }
            throw Error("Primitive value is not terminated with '|'");
        }
    }
}
