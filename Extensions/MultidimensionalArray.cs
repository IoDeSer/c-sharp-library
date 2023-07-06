using System;
using System.Collections.Generic;

namespace IoDeSer.Extensions
{
    internal class MultidimensionalArray 
    {
        Array array;
        int[] maxies;
        public int Length { get; }
        Type primaryType;

        public MultidimensionalArray(Array array)
        {
            this.array = array;
            maxies = new int[array.Rank];
            Length = 1;
            for (int i = 0; i < array.Rank; i++)
            {
                maxies[i] = array.GetLength(i);
                Length *= maxies[i];
            }

            this.primaryType = this.array.GetType().GetElementType();
        }

        public Array ToJaggedArray()
        {
            Array arrays = new Array[maxies.Length];

            for (int i = 0; i < maxies.Length; i++)
            {
                if (i >= maxies.Length - 1)
                {
                    arrays.SetValue(Array.CreateInstance(primaryType, maxies[i]), i);
                }
                else
                {
                    arrays.SetValue(Array.CreateInstance(typeof(Array), maxies[i]), i);
                }

            }

            for (int i = 0; i < maxies.Length - 1; i++)
            {
                // TODO need deep copies not just 'getting' values
                for (int j = 0; j < maxies[i]; j++)
                {
                    //var next = arrays.GetValue(i + 1);       //createinstance(typof( typeof next??), next.lenght??) ???
                    var next = Array.CreateInstance(primaryType, maxies[i + 1]);

                    ((Array)arrays.GetValue(i)).SetValue(next, j);
                }
            }

           


            int[] indecies = new int[array.Rank];
            int iter = array.Rank - 1;
            Array newArr = (Array)arrays.GetValue(0);
            Array temp = null;

            for (int i = 0; i < Length; i++)
            {

                temp = (Array)newArr.GetValue(indecies[0]);
                for (int z = 1; z < indecies.Length - 1; z++)
                {
                    temp = (Array)temp.GetValue(indecies[z]);
                }
                temp.SetValue(array.GetValue(indecies), indecies[iter]);

                if (indecies[iter] + 1 >= maxies[iter])
                {
                    for (int r = array.Rank - 1; r >= 0; r--)
                    {
                        if (maxies[r] - 1 > indecies[r])
                        {
                            indecies[r]++;
                            break;
                        }
                        else
                        {
                            indecies[r] = 0;
                        }
                    }
                }
                else
                {
                    indecies[iter]++;
                }
            }

            return newArr;
        }






        public List<int[]> Indecies()
        {
            int[] indecies = new int[array.Rank];
            int iter = array.Rank - 1;
            List<int[]> indeciesList = new List<int[]>();

            for (int i = 0; i < Length; i++)
            {
                //Console.WriteLine(i + ".\t" + String.Join(" ", indecies));
                indeciesList.Add((int[])indecies.Clone());
                if (indecies[iter] + 1 >= maxies[iter])
                {
                    for (int r = array.Rank - 1; r >= 0; r--)
                    {
                        if (maxies[r] - 1 > indecies[r])
                        {
                            indecies[r]++;
                            break;
                        }
                        else
                        {
                            indecies[r] = 0;
                        }

                    }

                }
                else
                {
                    indecies[iter]++;
                }
            }
            return indeciesList;
        }

    }
}
