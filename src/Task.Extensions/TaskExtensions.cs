using System;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Continues the <see cref="Task"/> return on any scheduler context.
        /// </summary>
        /// <param name="task">The this.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">this</exception>
        public static ConfiguredTaskAwaitable ContinueOnAnyContext(this Task task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            return task.ConfigureAwait(false);
        }

        /// <summary>
        /// Continues the <see cref="Task"/> return on any scheduler context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task">The this.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">this</exception>
        public static ConfiguredTaskAwaitable<T> ContinueOnAnyContext<T>(this Task<T> task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            return task.ConfigureAwait(false);
        }
    }
}
