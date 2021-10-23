namespace Rocket.Surgery.Extensions;

/// <summary>
///     TopographicalSortExtensions.
/// </summary>
[PublicAPI]
public static class TopographicalSortExtensions
{
    /// <summary>
    ///     Topographicals the sort.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source.</param>
    /// <param name="dependencies">The dependencies.</param>
    public static IEnumerable<T> TopographicalSort<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> dependencies)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var sorted = new List<T>();
        var visited = new HashSet<T>();

        foreach (var item in source)
            Visit(item, visited, sorted, dependencies);

        return sorted;
    }

    private static void Visit<T>(T item, HashSet<T> visited, List<T> sorted, Func<T, IEnumerable<T>> dependencies)
    {
        if (!visited.Contains(item))
        {
            visited.Add(item);

            foreach (var dep in dependencies(item))
                Visit(dep, visited, sorted, dependencies);

            sorted.Add(item);
        }
        else
        {
            if (!sorted.Contains(item))
                throw new NotSupportedException($"Cyclic dependency found {item}");
        }
    }
}
