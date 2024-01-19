using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SecureBank.Extensions
{
    public static class IEnumerableExtensions
    {
        #region PUBLIC METHODS

        public static IEnumerable<IEnumerable<T>> GetCombinations<T>(this IEnumerable<T> elements, int n)
        {
            if (elements.Count() < n)
            {
                throw new ArgumentException("Array length can't be less than number of selected elements");
            }

            if (n < 1)
            {
                throw new ArgumentException("Number of selected elements can't be less than 1");
            }

            if (elements.Count() == n)
            {
                return new List<IEnumerable<T>> { elements.ToList() };
            }

            List<List<T>> result = new List<List<T>>();

            int[] indices = Enumerable.Range(0, n).ToArray();

            while (indices[0] <= elements.Count() - n)
            {
                List<T> combination = new List<T>();

                for (int i = 0; i < n; i++)
                {
                    combination.Add(elements.ElementAt(indices[i]));
                }

                result.Add(combination);

                int j = n - 1;
                while (j >= 0 && indices[j] == elements.Count() - n + j)
                {
                    j--;
                }

                if (j < 0)
                {
                    break;
                }

                indices[j]++;
                for (int k = j + 1; k < n; k++)
                {
                    indices[k] = indices[k - 1] + 1;
                }
            }

            return result;

        }

        #endregion



        #region PRIVATE METHODS



        #endregion
    }
}
