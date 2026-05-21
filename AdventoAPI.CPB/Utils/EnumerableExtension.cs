using System;
using System.Collections.Generic;
using System.Text;

namespace AdventoAPI.CPB.Utils
{
    internal static class EnumerableExtension
    {
        extension<T>(T val)
        {
            public IEnumerable<T> ToSingleIEnumerable()
            {
                yield return val;
            }
        }
    }
}
