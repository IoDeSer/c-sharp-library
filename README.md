[![Nuget](https://img.shields.io/nuget/v/IoDeSer?style=for-the-badge&label=IoDeSer&color=%23e6e62c)](https://www.nuget.org/packages/IoDeSer)

# Introduction
Repository stores code for C# library that allows to read from and write to .io file format. Written in .NET Standard 2.0

### About
.io file is a text format that contains serialized object. Every value starts with a one-letter **type tag** followed by its content between two vertical bars, e.g. `i|37|` or `s|text|`. Iterables and structures span multiple lines in the pretty form, but whitespace between tokens is not significant, so the same object can also be written in one line. It is build to be cross-platform, language-independent and human-readable.

The format is the same as in the [Rust library](https://github.com/IoDeSer/rust-library) (version 0.7.0), so .io strings written in C# can be read in Rust and the other way around.

You can read more about .io de/serialization file format goal and mission [here](https://github.com/IoDeSer).

### Functions and plans
- [X] Primitive types (int, char)
- [X] Strings
- [X] Classes and structures
- [X] Arrays (and jagged arrays)
- [X] Multidimensional arrays
- [X] Any IEnumerable collection (List)
- [X] Dictionaries
- [X] Tuples (ValueTuple, Tuple, KeyValuePair)
- [X] Dates and time
- [X] Null 
- [X] Enums

## Format
| Tag | Value | Example |
|---|---|---|
| `i` | integers (`sbyte`..`ulong`, `BigInteger`) | `i\|-42\|` |
| `f` | `float`, `double`, `decimal` | `f\|3.14\|` |
| `b` | `bool` | `b\|true\|` |
| `c` | `char` | `c\|x\|` |
| `s` | `string`, `DateTime`, `DateTimeOffset` | `s\|text\|` |
| `n` | `null` | `n\|\|` |
| `a` | arrays, collections, tuples, `KeyValuePair` | `a\| i\|1\| + i\|2\| \|` |
| `m` | dictionaries, every entry is a two element array | `m\| a\| s\|key\| + i\|1\| \| \|` |
| `o` | classes, structures, `TimeSpan` | `o\| Name->s\|John\| Age->i\|21\| \|` |
| `e` | enums | `e\| Green->n\|\| \|` |

The layout of the serialized string is chosen with an optional argument `IoFormat` of **WriteToString** / **WriteToFile**. Without it, the pretty form is used:
```cs
var v = new List<string> { "a", "b" };
IoFile.WriteToString(v);                       // IoFormat.Pretty - new lines and tabulators
IoFile.WriteToString(v, IoFormat.Compact);     // a| s|a| + s|b| |
IoFile.WriteToString(v, IoFormat.Minified);    // a|s|a|+s|b||
IoFile.WriteToString(v, IoFormat.Pretty.WithIndent(IoIndent.Spaces(4)));
```

Deserialization ignores whitespace (space, `\t`, `\r`, `\n`) between tokens, so every format is accepted and all of these are equal:
```text
o|
	Name->s|John|
	Tags->a|
		s|a|
		+
		s|b|
	|
|

o| Name->s|John| Tags->a| s|a| + s|b| | |

o|Name->s|John|Tags->a|s|a|+s|b|||
```
A .io string of unknown type can be read as `IoValue` and re-formatted: `IoValue.Parse(ioString).ToIoString(IoFormat.Compact)`.

Inside `s|..|` and `c|..|` the characters with special meaning are escaped with a backslash: `|` as `\|`, `+` as `\+`, `->` as `-\>` and `\` as `\\`. Other characters use the same escapes as Rust (`\n`, `\t`, `\"`, `\u{..}`, ...).

Dates are written in RFC 3339 format with an offset, e.g. `s|2024-03-05T07:08:09.120+00:00|`. `DateTime` with `DateTimeKind.Utc` or `Unspecified` is written with `+00:00` offset, `Local` with the local offset. `TimeSpan` is written as `o| seconds->i|..| nanoseconds->i|..| |`, the same as `Duration` in Rust.

## Capabilities
- Serialization of [supported types](#functions-and-plans) to string or stream using static methods **WriteToString** / **WriteToFile** with optional `IoFormat` (pretty, compact or minified),
- Deserialization of [supported types](#functions-and-plans) from string or stream using static methods **ReadFromString** / **ReadFromFile**,
- Renaming class properties in serialized string using **IoItemName** attribute (the name cannot contain whitespace, `|`, `+` or `->`),
- Changing order of properties in class in serialized string using **IoItemOrder** (for specific property) / **IoItemsOrder** (for the whole class),
- Ignoring public properties in de/serialization with **IoItemIgnore** attribute,
- Parsing any .io formatted string without knowing its type into a generic **IoValue** tree.

Properties are matched by their names during deserialization, so their order in .io string does not matter. Properties without a setter are serialized, but skipped during deserialization.

Limitations: `null` can only be read into reference types and `Nullable<T>`. C# enums cannot hold values, so enums from Rust with values (e.g. `e| Numeric->i|65| |`) cannot be read. Values nested deeper than `IoValue.MaxDepth` (256) levels are rejected.

Refer to [example](example-usage) below to see, how you can utilize those capabilities and how they affect the serialized string.

### Example usage

```cs
using IoDeSer;
using IoDeSer.Attributes;
using IoDeSer.Attributes.Ordering;
using System.Diagnostics;

namespace ProgramNamespace
{
    class Person<T>
    {
        [IoItemName("FirstName")]
        public string Name { get; set; }
        [IoItemOrder(ItemOrder.LAST)]
        public string SecondName { get; set; }
        [IoItemName("Surname")]
        public string LastName { get; set; }
        [IoItemOrder(2)]
        public byte Age { get; set; }
        [IoItemOrder(ItemOrder.FIRST)]
        public List<Address<T>> Addresses { get; set; }
    }

    [IoItemsOrder(ItemsOrder.SHORTEST_FIRST)]
    class Address<T>
    {
        public string City { get; set; }
        public string StreetName { get; set; }
        public T Number { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Person<int> person = new Person<int>()
            {
                Name="John", SecondName=null, LastName="Smith", Age=36,
                Addresses = new List<Address<int>>
                {
                    new Address<int> {City="Hamburg", StreetName="Strasse", Number=55},
                    new Address<int> {City="Las Vegas", StreetName="Black", Number=1549}
                }
            };

            string io_string = IoFile.WriteToString(person);
            Console.Write(io_string);
            Person<int> read_person = IoFile.ReadFromString<Person<int>>(io_string);
            Debug.Assert(read_person.Name == person.Name);
            Debug.Assert(read_person.Age == person.Age);
            Debug.Assert(read_person.Addresses[1].City == person.Addresses[1].City);
        }
    }
}

/*
Output:
o|
	Addresses->a|
		o|
			City->s|Hamburg|
			Number->i|55|
			StreetName->s|Strasse|
		|
		+
		o|
			City->s|Las Vegas|
			Number->i|1549|
			StreetName->s|Black|
		|
	|
	Age->i|36|
	FirstName->s|John|
	SecondName->n||
	Surname->s|Smith|
|
 */
```
