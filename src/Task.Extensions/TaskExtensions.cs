using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks;

/// <summary>
///     TaskExtensions.
/// </summary>
[PublicAPI]
public static class TaskExtensions
{
    /// <summary>
    ///     Continues the <see cref="Task" /> return on any scheduler context.
    /// </summary>
    /// <param name="task">The this.</param>
    /// <returns>ConfiguredTaskAwaitable.</returns>
    /// <exception cref="ArgumentNullException">this</exception>
    public static ConfiguredTaskAwaitable ContinueOnAnyContext(this Task task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return task.ConfigureAwait(false);
    }

    /// <summary>
    ///     Continues the <see cref="Task" /> return on any scheduler context.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="task">The this.</param>
    /// <returns>ConfiguredTaskAwaitable{T}.</returns>
    /// <exception cref="ArgumentNullException">this</exception>
    public static ConfiguredTaskAwaitable<T> ContinueOnAnyContext<T>(this Task<T> task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return task.ConfigureAwait(false);
    }
}
