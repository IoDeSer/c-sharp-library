using System;
using System.Collections.Generic;
using System.Text;
using IoDeSer.Errors.Serialization;
using IoDeSer.Extensions;

namespace IoDeSer.Format
{
    /// <summary>
    /// Output buffer of the serialization, which knows the <see cref="IoFormat"/> and the current nesting level.
    /// </summary>
    internal sealed class IoWriter
    {
        readonly StringBuilder buffer = new StringBuilder();
        readonly IoFormat format;
        int depth;

        internal IoWriter(IoFormat format)
        {
            this.format = format;
        }

        public override string ToString() => buffer.ToString();

        /// <summary>
        /// Whitespace between two tokens: new line with indentation, a space or nothing.
        /// </summary>
        void TokenBreak()
        {
            if (format.IsMultiline)
            {
                buffer.Append('\n');
                if (format.Indent.IsTab)
                    buffer.Append('\t', depth);
                else
                    buffer.Append(' ', depth * format.Indent.SpacesCount);
            }
            else if (format.IsSpaced)
            {
                buffer.Append(' ');
            }
        }

        void Enter()
        {
            depth++;
            if (depth > IoValue.MaxDepth)
                throw new IoSerializationException($"Object is nested deeper than {IoValue.MaxDepth} levels. It might contain a reference cycle.");
        }

        /// <summary>
        /// Writes <c>tag|content|</c>, escaping the content.
        /// </summary>
        internal void WritePrimitive(char tag, string content)
        {
            buffer.Append(tag).Append('|').Append(content.EscapeString()).Append('|');
        }

        /// <summary>
        /// Writes <c>tag|content|</c>, where content needs no escaping (numbers, bools).
        /// </summary>
        internal void WritePlain(char tag, string content)
        {
            buffer.Append(tag).Append('|').Append(content).Append('|');
        }

        /// <summary>
        /// Writes <c>n||</c>.
        /// </summary>
        internal void WriteNone()
        {
            buffer.Append("n||");
        }

        /// <summary>
        /// Writes <c>tag| item + item ... |</c>, calling <paramref name="writeItem"/> for every element one level deeper.
        /// </summary>
        internal void WriteSequence<T>(char tag, IEnumerable<T> items, Action<T> writeItem)
        {
            buffer.Append(tag).Append('|');
            Enter();
            bool empty = true;
            foreach (T item in items)
            {
                if (!empty)
                {
                    TokenBreak();
                    buffer.Append('+');
                }
                empty = false;
                TokenBreak();
                writeItem(item);
            }
            depth--;
            if (!empty)
                TokenBreak();
            buffer.Append('|');
        }

        /// <summary>
        /// Writes <c>o| name->x name2->y |</c>, calling <paramref name="writeValue"/> for every field value one level deeper.
        /// </summary>
        internal void WriteObject<T>(IEnumerable<KeyValuePair<string, T>> fields, Action<T> writeValue)
        {
            buffer.Append("o|");
            Enter();
            bool empty = true;
            foreach (var field in fields)
            {
                empty = false;
                TokenBreak();
                buffer.Append(field.Key).Append("->");
                writeValue(field.Value);
            }
            depth--;
            if (!empty)
                TokenBreak();
            buffer.Append('|');
        }

        /// <summary>
        /// Writes <c>e| Variant->x |</c>, where <c>x</c> is written by <paramref name="writeValue"/> one level deeper.
        /// </summary>
        internal void WriteEnum(string variant, Action writeValue)
        {
            buffer.Append("e|");
            Enter();
            TokenBreak();
            buffer.Append(variant).Append("->");
            writeValue();
            depth--;
            TokenBreak();
            buffer.Append('|');
        }
    }
}
