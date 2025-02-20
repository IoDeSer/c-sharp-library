[![Nuget](https://img.shields.io/nuget/v/IoDeSer?style=for-the-badge&label=IoDeSer&color=%23e6e62c)](https://www.nuget.org/packages/IoDeSer)

# Introduction
Repository stores code for C# library that allows to read from and write to .io file format. Written in .NET Standard 2.0

### About
.io file is a text format that contains serialized object. Each object is stored between two vertical bars *|X|* in one line for primitive object or more, for iterables and structures. It is build to be cross-platform, language-independent and human-readable.

You can read more about .io de/serialization file format goal and mission [here](https://github.com/IoDeSer).

### Functions and plans
- [X] Primitive types (int, char)
- [X] Strings
- [X] Classes and structures
- [X] Arrays (and jagged arrays)
- [X] Any IEnumerable collection (List)
- [X] Dictionaries
- [X] Dates and time
- [X] Null 
- [ ] Multidimensional arrays are planned

## Capabilities
- Serialization of [supported types](#functions-and-plans) to string or stream using static methods **WriteToString** / **WriteToFile**,
- Deserialization of [supported types](#functions-and-plans) from or stream using static methods **ReadFromString** / **ReadFromFile**,
- Renaming class properties in serialized string using **IoItemName** attribute,
- Changing order of properties in class in serialized string using **IoItemOrder** (for specific property) / **IoItemsOrder** (for the whole class),
- Ignoring public properties in serialization with **IoItemIgnore** attribute.

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

    [IoItemsOrder(ItemsOrder.LONGEST_FIRST)]
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
|
        Addresses->|
                |
                        City->|Hamburg|
                        Number->|55|
                        StreetName->|Strasse|
                |
                +
                |
                        City->|Las Vegas|
                        Number->|1549|
                        StreetName->|Black|
                |
        |
        Surname->|Smith|
        Firstname->|John|
        SecondName->|||
|
 */
```
