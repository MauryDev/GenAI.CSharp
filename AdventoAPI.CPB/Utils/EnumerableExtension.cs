

namespace AdventoAPI.CPB.Utils;

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
