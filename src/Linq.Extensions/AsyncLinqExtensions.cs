﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocket.Surgery.Linq
{
    /// <summary>
    ///  AsyncLinqExtensions.
    /// </summary>
    public static class AsyncLinqExtensions
    {
        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" /> to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" /> that contains elements from the input sequence that satisfy the condition.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
        public static IAsyncEnumerable<TSource> Filter<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source.Where(predicate);
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate. Each element's index is used in the logic of the predicate function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" /> to filter.</param>
        /// <param name="predicate">A function to test each source element for a condition; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" /> that contains elements from the input sequence that satisfy the condition.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
        public static IAsyncEnumerable<TSource> Filter<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            return source.Where(predicate);
        }

        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" /> whose elements are the result of invoking the transform function on each element of <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IAsyncEnumerable<TResult> Map<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select(selector);
        }

        /// <summary>
        /// Projects each element of a sequence into a new form by incorporating the element's index.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each source element; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" /> whose elements are the result of invoking the transform function on each element of <paramref name="source" />.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IAsyncEnumerable<TResult> Map<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, TResult> selector)
        {
            return source.Select(selector);
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" /> and flattens the resulting sequences into one sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the sequence returned by <paramref name="selector" />.</typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" /> whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IAsyncEnumerable<TResult> FlatMap<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, IAsyncEnumerable<TResult>> selector)
        {
            return source.SelectMany(selector);
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" />, and flattens the resulting sequences into one sequence. The index of each source element is used in the projected form of that element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the sequence returned by <paramref name="selector" />.</typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A transform function to apply to each source element; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" /> whose elements are the result of invoking the one-to-many transform function on each element of an input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IAsyncEnumerable<TResult> FlatMap<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, IAsyncEnumerable<TResult>> selector)
        {
            return source.SelectMany(selector);
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" />, flattens the resulting sequences into one sequence, and invokes a result selector function on each element therein.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TCollection">The type of the intermediate elements collected by <paramref name="collectionSelector" />.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element of the input sequence.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>An <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" /> whose elements are the result of invoking the one-to-many transform function <paramref name="collectionSelector" /> on each element of <paramref name="source" /> and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        public static IAsyncEnumerable<TResult> FlatMap<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, IAsyncEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            return source.SelectMany(collectionSelector, resultSelector);
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" />, flattens the resulting sequences into one sequence, and invokes a result selector function on each element therein. The index of each source element is used in the intermediate projected form of that element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TCollection">The type of the intermediate elements collected by <paramref name="collectionSelector" />.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each source element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>An <see cref="T:System.Collections.Generic.IAsyncEnumerable`1" /> whose elements are the result of invoking the one-to-many transform function <paramref name="collectionSelector" /> on each element of <paramref name="source" /> and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        public static IAsyncEnumerable<TResult> FlatMap<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, IAsyncEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            return source.SelectMany(collectionSelector, resultSelector);
        }

        /// <summary>
        /// Applies an accumulator function over an async sequence, returning the result of the aggregation as a single element in the result sequence. The specified seed value is used as the initial accumulator value.
        /// For aggregation behavior with incremental intermediate results, see <see cref="Observable.Scan{TSource, Accumulate}" />.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAccumulate">The type of the result of the aggregation.</typeparam>
        /// <param name="source">An async sequence to aggregate over.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="accumulator">An accumulator function to be invoked on each element.</param>
        /// <returns>An async sequence containing a single element with the final accumulator value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> or <paramref name="accumulator" /> is null.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IAsyncEnumerable in order to retain asynchronous behavior.</remarks>
        public static Task<TAccumulate> Reduce<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator)
        {
            return source.Aggregate(seed, accumulator);
        }

        /// <summary>
        /// Applies an accumulator function over an async sequence, returning the result of the aggregation as a single element in the result sequence. The specified seed value is used as the initial accumulator value,
        /// and the specified result selector function is used to select the result value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="source">An async sequence to aggregate over.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="accumulator">An accumulator function to be invoked on each element.</param>
        /// <param name="resultSelector">A function to transform the final accumulator value into the result value.</param>
        /// <returns>An async sequence containing a single element with the final accumulator value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> or <paramref name="accumulator" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IAsyncEnumerable in order to retain asynchronous behavior.</remarks>
        public static Task<TResult> Reduce<TSource, TAccumulate, TResult>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator, Func<TAccumulate, TResult> resultSelector)
        {
            return source.Aggregate(seed, accumulator, resultSelector);
        }

        /// <summary>
        /// Applies an accumulator function over an async sequence, returning the result of the aggregation as a single element in the result sequence.
        /// For aggregation behavior with incremental intermediate results, see <see cref="Observable.Scan{TSource}" />.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence and the result of the aggregation.</typeparam>
        /// <param name="source">An async sequence to aggregate over.</param>
        /// <param name="accumulator">An accumulator function to be invoked on each element.</param>
        /// <returns>An async sequence containing a single element with the final accumulator value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> or <paramref name="accumulator" /> is null.</exception>
        /// <exception cref="InvalidOperationException">(Asynchronous) The source sequence is empty.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IAsyncEnumerable in order to retain asynchronous behavior.</remarks>
        public static Task<TSource> Reduce<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, TSource, TSource> accumulator)
        {
            return source.Aggregate(accumulator);
        }
    }
}
