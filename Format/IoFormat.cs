using System;

namespace IoDeSer
{
    /// <summary>
    /// Indentation used by multi-line <see cref="IoFormat"/>s.
    /// </summary>
    public readonly struct IoIndent : IEquatable<IoIndent>
    {
        // default value of the struct is one tabulator per nesting level
        private readonly bool useSpaces;

        /// <summary>
        /// Number of spaces per nesting level. Equal to 0 for <see cref="Tab"/>.
        /// </summary>
        public byte SpacesCount { get; }

        private IoIndent(bool useSpaces, byte spacesCount)
        {
            this.useSpaces = useSpaces;
            SpacesCount = spacesCount;
        }

        /// <summary>
        /// One tabulator per nesting level.
        /// </summary>
        public static IoIndent Tab => default(IoIndent);

        /// <summary>
        /// Given number of spaces per nesting level.
        /// </summary>
        public static IoIndent Spaces(byte count) => new IoIndent(true, count);

        /// <summary>
        /// True if this indentation uses tabulators.
        /// </summary>
        public bool IsTab => !useSpaces;

        public bool Equals(IoIndent other) => useSpaces == other.useSpaces && SpacesCount == other.SpacesCount;
        public override bool Equals(object obj) => obj is IoIndent other && Equals(other);
        public override int GetHashCode() => useSpaces ? SpacesCount + 1 : 0;
        public static bool operator ==(IoIndent a, IoIndent b) => a.Equals(b);
        public static bool operator !=(IoIndent a, IoIndent b) => !a.Equals(b);
        public override string ToString() => useSpaces ? $"Spaces({SpacesCount})" : "Tab";
    }

    /// <summary>
    /// Layout of the serialized .io string.
    /// <para>
    /// The format changes only whitespace between tokens, so string written in any format can be read regardless of the format used for writing.
    /// </para>
    /// <code>
    /// IoFile.WriteToString(new[] { 1, 2 });                    // "a|\n\ti|1|\n\t+\n\ti|2|\n|"
    /// IoFile.WriteToString(new[] { 1, 2 }, IoFormat.Compact);  // "a| i|1| + i|2| |"
    /// IoFile.WriteToString(new[] { 1, 2 }, IoFormat.Minified); // "a|i|1|+i|2||"
    /// IoFile.WriteToString(new[] { 1, 2 }, IoFormat.Pretty.WithIndent(IoIndent.Spaces(2)));
    /// </code>
    /// </summary>
    public readonly struct IoFormat : IEquatable<IoFormat>
    {
        // default value of the struct is equal to Pretty
        private readonly bool oneLine;
        private readonly bool noSpaces;

        /// <summary>
        /// Indentation of nested values. Only multi-line formats use it.
        /// </summary>
        public IoIndent Indent { get; }

        private IoFormat(bool oneLine, bool noSpaces, IoIndent indent)
        {
            this.oneLine = oneLine;
            this.noSpaces = noSpaces;
            Indent = indent;
        }

        /// <summary>
        /// Every token in a new line, nested values indented with tabulators. Default format.
        /// </summary>
        public static IoFormat Pretty => default(IoFormat);

        /// <summary>
        /// One line, tokens separated with single spaces.
        /// </summary>
        public static IoFormat Compact => new IoFormat(true, false, IoIndent.Tab);

        /// <summary>
        /// One line without any whitespace between tokens.
        /// </summary>
        public static IoFormat Minified => new IoFormat(true, true, IoIndent.Tab);

        /// <summary>
        /// Returns copy of this format with changed indentation.
        /// </summary>
        public IoFormat WithIndent(IoIndent indent) => new IoFormat(oneLine, noSpaces, indent);

        /// <summary>
        /// True if the format writes new lines.
        /// </summary>
        public bool IsMultiline => !oneLine;

        internal bool IsSpaced => !noSpaces;

        public bool Equals(IoFormat other) => oneLine == other.oneLine && noSpaces == other.noSpaces && Indent == other.Indent;
        public override bool Equals(object obj) => obj is IoFormat other && Equals(other);
        public override int GetHashCode() => (oneLine ? 1 : 0) | (noSpaces ? 2 : 0) | (Indent.GetHashCode() << 2);
        public static bool operator ==(IoFormat a, IoFormat b) => a.Equals(b);
        public static bool operator !=(IoFormat a, IoFormat b) => !a.Equals(b);

        public override string ToString()
        {
            if (!oneLine)
                return Indent.IsTab ? "Pretty" : $"Pretty({Indent})";
            return noSpaces ? "Minified" : "Compact";
        }
    }
}
