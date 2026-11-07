using System;
using System.Collections.Generic;
using System.Linq;
using IoDeSer.Errors.Deserialization;
using IoDeSer.Format;

namespace IoDeSer
{
    /// <summary>
    /// Parsed .io value, independent of any C# type.
    /// <para>
    /// Every type is deserialized from this tree. It can also be used directly, to inspect a .io string without knowing its type, or to re-format it:
    /// <c>IoValue.Parse(ioString).ToIoString(IoFormat.Compact)</c>.
    /// </para>
    /// </summary>
    public abstract class IoValue
    {
        /// <summary>
        /// Maximum nesting level accepted by <see cref="Parse"/> and serialization.
        /// </summary>
        public const int MaxDepth = 256;

        internal IoValue() { }

        /// <summary>
        /// Type tag of this value: <c>i</c>, <c>f</c>, <c>b</c>, <c>c</c>, <c>s</c>, <c>n</c>, <c>a</c>, <c>m</c>, <c>o</c> or <c>e</c>.
        /// </summary>
        public abstract char Tag { get; }

        /// <summary>
        /// Parses .io formatted string. Whitespace between tokens is ignored.
        /// <para>
        /// Throws <see cref="IoDeserializationException"/> if the string is not in .io format.
        /// </para>
        /// </summary>
        public static IoValue Parse(string ioString) => IoParser.Parse(ioString);

        /// <summary>
        /// Writes this value as .io string in the given format (<see cref="IoFormat.Pretty"/> by default).
        /// </summary>
        public string ToIoString(IoFormat format = default(IoFormat))
        {
            IoWriter writer = new IoWriter(format);
            Write(writer);
            return writer.ToString();
        }

        /// <summary>
        /// Returns this value in <see cref="IoFormat.Compact"/> format.
        /// </summary>
        public override string ToString() => ToIoString(IoFormat.Compact);

        internal abstract void Write(IoWriter writer);

        /*
         * Helpers used by deserialization
         */

        internal string Snippet()
        {
            string s = ToString();
            return s.Length > 60 ? s.Substring(0, 60) + "..." : s;
        }

        IoDeserializationException Mismatch(char expected, Type type)
        {
            return new IoDeserializationException($"Expected value with tag '{expected}' for type '{type.Name}', found tag '{Tag}': {Snippet()}");
        }

        internal string ExpectPrimitive(char tag, Type type)
        {
            if (this is IoPrimitive primitive && primitive.Tag == tag)
                return primitive.Value;
            throw Mismatch(tag, type);
        }

        internal IReadOnlyList<IoValue> ExpectArray(Type type)
        {
            if (this is IoArray array)
                return array.Items;
            throw Mismatch('a', type);
        }

        internal IReadOnlyList<IoValue> ExpectArray(int length, Type type)
        {
            IReadOnlyList<IoValue> items = ExpectArray(type);
            if (items.Count != length)
                throw new IoDeserializationException($"Expected array of {length} elements for type '{type.Name}', found {items.Count} elements.");
            return items;
        }

        internal IReadOnlyList<KeyValuePair<IoValue, IoValue>> ExpectMap(Type type)
        {
            if (this is IoMap map)
                return map.Entries;
            throw Mismatch('m', type);
        }

        internal IReadOnlyList<KeyValuePair<string, IoValue>> ExpectObject(Type type)
        {
            if (this is IoObject obj)
                return obj.Fields;
            throw Mismatch('o', type);
        }

        internal IoEnum ExpectEnum(Type type)
        {
            if (this is IoEnum e)
                return e;
            throw Mismatch('e', type);
        }

        /// <summary>
        /// Name of a field or enum variant cannot be empty or contain whitespace, '|', '+' or "->".
        /// </summary>
        internal static bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (c == '|' || c == '+' || IoParser.IsWhitespace(c) || (c == '-' && i + 1 < name.Length && name[i + 1] == '>'))
                    return false;
            }
            return true;
        }

        internal static void CheckName(string name)
        {
            if (!IsValidName(name))
                throw new ArgumentException($"Name \"{name}\" cannot be empty or contain whitespace, '|', '+' or \"->\".");
        }
    }

    /// <summary>
    /// <c>i|..|</c>, <c>f|..|</c>, <c>b|..|</c>, <c>c|..|</c> or <c>s|..|</c> with an already unescaped content.
    /// </summary>
    public sealed class IoPrimitive : IoValue
    {
        readonly char tag;

        /// <summary>
        /// Unescaped content between vertical bars.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Creates primitive value. <paramref name="tag"/> must be one of: i, f, b, c, s.
        /// </summary>
        public IoPrimitive(char tag, string value)
        {
            if (tag != 'i' && tag != 'f' && tag != 'b' && tag != 'c' && tag != 's')
                throw new ArgumentException($"'{tag}' is not a tag of a primitive value.", nameof(tag));
            this.tag = tag;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override char Tag => tag;

        internal override void Write(IoWriter writer) => writer.WritePrimitive(tag, Value);
    }

    /// <summary>
    /// <c>n||</c> - null.
    /// </summary>
    public sealed class IoNone : IoValue
    {
        /// <summary>
        /// The only instance of <see cref="IoNone"/>.
        /// </summary>
        public static readonly IoNone Instance = new IoNone();

        IoNone() { }

        public override char Tag => 'n';

        internal override void Write(IoWriter writer) => writer.WriteNone();
    }

    /// <summary>
    /// <c>a| x + y |</c> - arrays, collections, tuples.
    /// </summary>
    public sealed class IoArray : IoValue
    {
        /// <summary>
        /// Elements of the array.
        /// </summary>
        public IReadOnlyList<IoValue> Items { get; }

        public IoArray(IEnumerable<IoValue> items)
        {
            Items = items.ToList();
        }

        public override char Tag => 'a';

        internal override void Write(IoWriter writer) => writer.WriteSequence('a', Items, item => item.Write(writer));
    }

    /// <summary>
    /// <c>m| a| key + value | + ... |</c> - dictionaries.
    /// </summary>
    public sealed class IoMap : IoValue
    {
        /// <summary>
        /// Key-value pairs of the map.
        /// </summary>
        public IReadOnlyList<KeyValuePair<IoValue, IoValue>> Entries { get; }

        public IoMap(IEnumerable<KeyValuePair<IoValue, IoValue>> entries)
        {
            Entries = entries.ToList();
        }

        public override char Tag => 'm';

        internal override void Write(IoWriter writer)
        {
            writer.WriteSequence('m', Entries, entry =>
                writer.WriteSequence('a', new[] { entry.Key, entry.Value }, item => item.Write(writer)));
        }
    }

    /// <summary>
    /// <c>o| name->x name2->y |</c> - classes and structures.
    /// </summary>
    public sealed class IoObject : IoValue
    {
        /// <summary>
        /// Fields of the object in their order.
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, IoValue>> Fields { get; }

        /// <summary>
        /// Creates object. Names must be unique and cannot contain whitespace, '|', '+' or "->".
        /// </summary>
        public IoObject(IEnumerable<KeyValuePair<string, IoValue>> fields)
        {
            Fields = fields.ToList();
            HashSet<string> names = new HashSet<string>();
            foreach (var field in Fields)
            {
                CheckName(field.Key);
                if (!names.Add(field.Key))
                    throw new ArgumentException($"Field \"{field.Key}\" is defined more than once.");
            }
        }

        public override char Tag => 'o';

        internal override void Write(IoWriter writer) => writer.WriteObject(Fields, value => value.Write(writer));
    }

    /// <summary>
    /// <c>e| Variant->x |</c> - enums.
    /// </summary>
    public sealed class IoEnum : IoValue
    {
        /// <summary>
        /// Name of the variant.
        /// </summary>
        public string Variant { get; }

        /// <summary>
        /// Value of the variant. <see cref="IoNone"/> for C# enums.
        /// </summary>
        public IoValue Value { get; }

        public IoEnum(string variant, IoValue value)
        {
            CheckName(variant);
            Variant = variant;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override char Tag => 'e';

        internal override void Write(IoWriter writer) => writer.WriteEnum(Variant, () => Value.Write(writer));
    }
}
