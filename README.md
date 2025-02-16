[![Nuget](https://img.shields.io/nuget/v/IoDeSer?style=for-the-badge&label=IoDeSer&color=%23e6e62c)](https://www.nuget.org/packages/IoDeSer)

# About
Repository stores code for C# library that allows to read from and write to .io file format. Is written in .NET Standard 2.0

### Functions and plans
- [X] Primitive types (int, char)
- [X] Strings
- [X] Classes
- [X] Arrays (and jagged arrays)
- [X] Any IEnumerable collection
- [X] Combinations of all above
- [ ] Multidimensional arrays are planned

### Example usage

```
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
        public string? SecondName { get; set; }
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
                        StreetName->|Strasse|
                        Number->|55|
                |
                +
                |
                        City->|Las Vegas|
                        StreetName->|Black|
                        Number->|1549|
                |
        |
        Age->|36|
        FirstName->|John|
        Surname->|Smith|
|
 */
```