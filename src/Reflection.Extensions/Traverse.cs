namespace Rocket.Surgery.Reflection;

internal static class Traverse
{
    public static IEnumerable<T> Across<T>(T first, Func<T, T?> next)
        where T : class
    {
        var item = first;
        while (item != null)
        {
            yield return item;
            item = next(item);
        }
    }
}
