using System;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Continues the <see cref="Task"/> return on any <see cref="IScheduler"/> context.
        /// </summary>
        /// <param name="this">The this.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">this</exception>
        public static ConfiguredTaskAwaitable ContinueOnAnyContext(this Task @this)
        {
            if (@this == null) throw new ArgumentNullException(nameof(@this));
            return @this.ConfigureAwait(false);
        }

        /// <summary>
        /// Continues the <see cref="Task"/> return on any <see cref="IScheduler"/> context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this">The this.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">this</exception>
        public static ConfiguredTaskAwaitable<T> ContinueOnAnyContext<T>(this Task<T> @this)
        {
            if (@this == null) throw new ArgumentNullException(nameof(@this));
            return @this.ConfigureAwait(false);
        }
    }
}
