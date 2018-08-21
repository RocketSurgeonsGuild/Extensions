using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive.Observable.Aliases;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace System.Reactive.Linq
{
    public static class ObservableExtensions
    {
        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence and merges the resulting observable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IObservable<TResult>> selector)
        {
            return source
                .Map(selector)
                .Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence and merges the resulting observable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="maxConcurrent">Maximum number of inner observable sequences being subscribed to concurrently.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxConcurrent" /> is less than or equal to zero.</exception>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IObservable<TResult>> selector, int maxConcurrent)
        {
            return source
                .Map(selector)
                .Merge(maxConcurrent);
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence by incorporating the element's index and merges the resulting observable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, IObservable<TResult>> selector)
        {
            return source
                .Map(selector)
                .Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence by incorporating the element's index and merges the resulting observable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="maxConcurrent">Maximum number of inner observable sequences being subscribed to concurrently.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxConcurrent" /> is less than or equal to zero.</exception>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, IObservable<TResult>> selector, int maxConcurrent)
        {
            return source
                .Map(selector)
                .Merge(maxConcurrent);
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, Task<TResult>> selector)
        {
            return source
                .Map((outer) => Observable.FromAsync(() => selector(outer)))
                .Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="maxConcurrent">Maximum number of inner observable sequences being subscribed to concurrently.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxConcurrent" /> is less than or equal to zero.</exception>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, Task<TResult>> selector, int maxConcurrent)
        {
            return source
                .Map((outer) => Observable.FromAsync(() => selector(outer)))
                .Merge(maxConcurrent);
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, Task<TResult>> selector)
        {
            return source
                .Map((outer, index) => Observable.FromAsync(() => selector(outer, index)))
                .Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="maxConcurrent">Maximum number of inner observable sequences being subscribed to concurrently.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxConcurrent" /> is less than or equal to zero.</exception>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, Task<TResult>> selector, int maxConcurrent)
        {
            return source
                .Map((outer, index) => Observable.FromAsync(() => selector(outer, index)))
                .Merge(maxConcurrent);
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task with cancellation support and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, CancellationToken, Task<TResult>> selector)
        {
            return source
                .Map((outer) => Observable.FromAsync(ct => selector(outer, ct)))
                .Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task with cancellation support and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="maxConcurrent">Maximum number of inner observable sequences being subscribed to concurrently.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxConcurrent" /> is less than or equal to zero.</exception>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, CancellationToken, Task<TResult>> selector, int maxConcurrent)
        {
            return source
                .Map((outer) => Observable.FromAsync(ct => selector(outer, ct)))
                .Merge(maxConcurrent);
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index with cancellation support and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, CancellationToken, Task<TResult>> selector)
        {
            return source
                .Map((outer, index) => Observable.FromAsync(ct => selector(outer, index, ct)))
                .Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index with cancellation support and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="maxConcurrent">Maximum number of inner observable sequences being subscribed to concurrently.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxConcurrent" /> is less than or equal to zero.</exception>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, CancellationToken, Task<TResult>> selector, int maxConcurrent)
        {
            return source
                .Map((outer, index) => Observable.FromAsync(ct => selector(outer, index, ct)))
                .Merge(maxConcurrent);
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence, invokes the result selector for the source element and each of the corresponding inner sequence's elements, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TCollection">The type of the elements in the projected intermediate sequences.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate sequence elements.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function collectionSelector on each element of the input sequence and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        public static IObservable<TResult> MergeMap<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, IObservable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            return source
                .Map((outer) =>
                {
                    return collectionSelector(outer)
                        .Map((inner) => resultSelector(outer, inner));
                }).Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence, invokes the result selector for the source element and each of the corresponding inner sequence's elements, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TCollection">The type of the elements in the projected intermediate sequences.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate sequence elements.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <param name="maxConcurrent">Maximum number of inner observable sequences being subscribed to concurrently.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function collectionSelector on each element of the input sequence and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxConcurrent" /> is less than or equal to zero.</exception>
        public static IObservable<TResult> MergeMap<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, IObservable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector, int maxConcurrent)
        {
            return source
                .Map((outer) =>
                {
                    return collectionSelector(outer)
                        .Map((inner) => resultSelector(outer, inner));
                }).Merge(maxConcurrent);
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence by incorporating the element's index, invokes the result selector for the source element and each of the corresponding inner sequence's elements, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TCollection">The type of the elements in the projected intermediate sequences.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate sequence elements.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence; the second parameter of the function represents the index of the source element and the fourth parameter represents the index of the intermediate element.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function collectionSelector on each element of the input sequence and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        public static IObservable<TResult> MergeMap<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, int, IObservable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
        {
            return source
                .Map((outer, outerIndex) =>
                {
                    return collectionSelector(outer, outerIndex)
                        .Map((inner, innerIndex) => resultSelector(outer, outerIndex, inner, innerIndex));
                }).Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence by incorporating the element's index, invokes the result selector for the source element and each of the corresponding inner sequence's elements, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TCollection">The type of the elements in the projected intermediate sequences.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate sequence elements.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence; the second parameter of the function represents the index of the source element and the fourth parameter represents the index of the intermediate element.</param>
        /// <param name="maxConcurrent">Maximum number of inner observable sequences being subscribed to concurrently.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function collectionSelector on each element of the input sequence and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxConcurrent" /> is less than or equal to zero.</exception>
        public static IObservable<TResult> MergeMap<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, int, IObservable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector, int maxConcurrent)
        {
            return source
                .Map((outer, outerIndex) =>
                {
                    return collectionSelector(outer, outerIndex)
                        .Map((inner, innerIndex) => resultSelector(outer, outerIndex, inner, innerIndex));
                }).Merge(maxConcurrent);
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> MergeMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, Task<TTaskResult>> taskSelector, Func<TSource, TTaskResult, TResult> resultSelector)
        {
            return source
                .Map((outer) =>
                {
                    return Observable.FromAsync(() => taskSelector(outer))
                        .Map((inner) => resultSelector(outer, inner));
                })
                .Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <param name="maxConcurrent">Maximum number of inner observable sequences being subscribed to concurrently.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxConcurrent" /> is less than or equal to zero.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> MergeMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, Task<TTaskResult>> taskSelector, Func<TSource, TTaskResult, TResult> resultSelector, int maxConcurrent)
        {
            return source
                .Map((outer) =>
                {
                    return Observable.FromAsync(() => taskSelector(outer))
                        .Map((inner) => resultSelector(outer, inner));
                })
                .Merge(maxConcurrent);
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> MergeMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, int, Task<TTaskResult>> taskSelector, Func<TSource, int, TTaskResult, TResult> resultSelector)
        {
            return source
                .Map((outer, index) =>
                {
                    return Observable.FromAsync(() => taskSelector(outer, index))
                        .Map((inner) => resultSelector(outer, index, inner));
                })
                .Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence; the second parameter of the function represents the index of the source element.</param>
        /// <param name="maxConcurrent">Maximum number of inner observable sequences being subscribed to concurrently.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxConcurrent" /> is less than or equal to zero.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> MergeMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, int, Task<TTaskResult>> taskSelector, Func<TSource, int, TTaskResult, TResult> resultSelector, int maxConcurrent)
        {
            return source
                .Map((outer, index) =>
                {
                    return Observable.FromAsync(() => taskSelector(outer, index))
                        .Map((inner) => resultSelector(outer, index, inner));
                })
                .Merge(maxConcurrent);
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task with cancellation support, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> MergeMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, CancellationToken, Task<TTaskResult>> taskSelector, Func<TSource, TTaskResult, TResult> resultSelector)
        {
            return source
                .Map((outer) =>
                {
                    return Observable.FromAsync(ct => taskSelector(outer, ct))
                        .Map((inner) => resultSelector(outer, inner));
                })
                .Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task with cancellation support, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <param name="maxConcurrent">Maximum number of inner observable sequences being subscribed to concurrently.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxConcurrent" /> is less than or equal to zero.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> MergeMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, CancellationToken, Task<TTaskResult>> taskSelector, Func<TSource, TTaskResult, TResult> resultSelector, int maxConcurrent)
        {
            return source
                .Map((outer) =>
                {
                    return Observable.FromAsync(ct => taskSelector(outer, ct))
                        .Map((inner) => resultSelector(outer, inner));
                })
                .Merge(maxConcurrent);
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index with cancellation support, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> MergeMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, int, CancellationToken, Task<TTaskResult>> taskSelector, Func<TSource, int, TTaskResult, TResult> resultSelector)
        {
            return source
                .Map((outer, index) =>
                {
                    return Observable.FromAsync(ct => taskSelector(outer, index, ct))
                        .Map((inner) => resultSelector(outer, index, inner));
                })
                .Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index with cancellation support, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence; the second parameter of the function represents the index of the source element.</param>
        /// <param name="maxConcurrent">Maximum number of inner observable sequences being subscribed to concurrently.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxConcurrent" /> is less than or equal to zero.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> MergeMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, int, CancellationToken, Task<TTaskResult>> taskSelector, Func<TSource, int, TTaskResult, TResult> resultSelector, int maxConcurrent)
        {
            return source
                .Map((outer, index) =>
                {
                    return Observable.FromAsync(ct => taskSelector(outer, index, ct))
                        .Map((inner) => resultSelector(outer, index, inner));
                })
                .Merge(maxConcurrent);
        }

        /// <summary>
        /// Projects each element of an observable sequence to an enumerable sequence and concatenates the resulting enumerable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner enumerable sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <remarks>The projected sequences are enumerated synchonously within the OnNext call of the source sequence. In order to do a concurrent, non-blocking merge, change the selector to return an observable sequence obtained using the <see cref="M:System.Reactive.Linq.Observable.ToObservable``1(System.Collections.Generic.IEnumerable{``0})" /> conversion.</remarks>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            return source
                .Map((outer) => selector(outer).ToObservable())
                .Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an enumerable sequence and concatenates the resulting enumerable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner enumerable sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="maxConcurrent">Maximum number of inner observable sequences being subscribed to concurrently.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxConcurrent" /> is less than or equal to zero.</exception>
        /// <remarks>The projected sequences are enumerated synchonously within the OnNext call of the source sequence. In order to do a concurrent, non-blocking merge, change the selector to return an observable sequence obtained using the <see cref="M:System.Reactive.Linq.Observable.ToObservable``1(System.Collections.Generic.IEnumerable{``0})" /> conversion.</remarks>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IEnumerable<TResult>> selector, int maxConcurrent)
        {
            return source
                .Map((outer) => selector(outer).ToObservable())
                .Merge(maxConcurrent);
        }

        /// <summary>
        /// Projects each element of an observable sequence to an enumerable sequence by incorporating the element's index and concatenates the resulting enumerable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner enumerable sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <remarks>The projected sequences are enumerated synchonously within the OnNext call of the source sequence. In order to do a concurrent, non-blocking merge, change the selector to return an observable sequence obtained using the <see cref="M:System.Reactive.Linq.Observable.ToObservable``1(System.Collections.Generic.IEnumerable{``0})" /> conversion.</remarks>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
        {
            return source
                .Map((outer, index) => selector(outer, index).ToObservable())
                .Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an enumerable sequence by incorporating the element's index and concatenates the resulting enumerable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner enumerable sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="maxConcurrent">Maximum number of inner observable sequences being subscribed to concurrently.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxConcurrent" /> is less than or equal to zero.</exception>
        /// <remarks>The projected sequences are enumerated synchonously within the OnNext call of the source sequence. In order to do a concurrent, non-blocking merge, change the selector to return an observable sequence obtained using the <see cref="M:System.Reactive.Linq.Observable.ToObservable``1(System.Collections.Generic.IEnumerable{``0})" /> conversion.</remarks>
        public static IObservable<TResult> MergeMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector, int maxConcurrent)
        {
            return source
                .Map((outer, index) => selector(outer, index).ToObservable())
                .Merge(maxConcurrent);
        }

        /// <summary>
        /// Projects each element of an observable sequence to an enumerable sequence, invokes the result selector for the source element and each of the corresponding inner sequence's elements, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TCollection">The type of the elements in the projected intermediate enumerable sequences.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate sequence elements.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function collectionSelector on each element of the input sequence and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>The projected sequences are enumerated synchonously within the OnNext call of the source sequence. In order to do a concurrent, non-blocking merge, change the selector to return an observable sequence obtained using the <see cref="M:System.Reactive.Linq.Observable.ToObservable``1(System.Collections.Generic.IEnumerable{``0})" /> conversion.</remarks>
        public static IObservable<TResult> MergeMap<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            return source
                .Map((outer) =>
                {
                    return collectionSelector(outer)
                        .ToObservable()
                        .Map((inner) => resultSelector(outer, inner));
                }).Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an enumerable sequence, invokes the result selector for the source element and each of the corresponding inner sequence's elements, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TCollection">The type of the elements in the projected intermediate enumerable sequences.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate sequence elements.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <param name="maxConcurrent">Maximum number of inner observable sequences being subscribed to concurrently.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function collectionSelector on each element of the input sequence and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxConcurrent" /> is less than or equal to zero.</exception>
        /// <remarks>The projected sequences are enumerated synchonously within the OnNext call of the source sequence. In order to do a concurrent, non-blocking merge, change the selector to return an observable sequence obtained using the <see cref="M:System.Reactive.Linq.Observable.ToObservable``1(System.Collections.Generic.IEnumerable{``0})" /> conversion.</remarks>
        public static IObservable<TResult> MergeMap<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector, int maxConcurrent)
        {
            return source
                .Map((outer) =>
                {
                    return collectionSelector(outer)
                        .ToObservable()
                        .Map((inner) => resultSelector(outer, inner));
                }).Merge(maxConcurrent);
        }

        /// <summary>
        /// Merges elements from all inner observable sequences into a single observable sequence, limiting the number of concurrent subscriptions to inner sequences.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TCollection">The type of the elements in the projected intermediate enumerable sequences.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate sequence elements.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence; the second parameter of the function represents the index of the source element and the fourth parameter represents the index of the intermediate element.</param>
        /// <returns>The observable sequence that merges the elements of the inner sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> is null.</exception>
        public static IObservable<TResult> MergeMap<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
        {
            return source
                .Map((outer, outerIndex) =>
                {
                    return collectionSelector(outer, outerIndex)
                        .ToObservable()
                        .Map((inner, innerIndex) => resultSelector(outer, outerIndex, inner, innerIndex));
                }).Merge();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence and merges the resulting observable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> ConcatMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IObservable<TResult>> selector)
        {
            return source
                .Map(selector)
                .Concat();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence by incorporating the element's index and merges the resulting observable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> ConcatMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, IObservable<TResult>> selector)
        {
            return source
                .Map(selector)
                .Concat();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> ConcatMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, Task<TResult>> selector)
        {
            return source
                .Map((outer) => Observable.FromAsync(() => selector(outer)))
                .Concat();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> ConcatMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, Task<TResult>> selector)
        {
            return source
                .Map((outer, index) => Observable.FromAsync(() => selector(outer, index)))
                .Concat();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task with cancellation support and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> ConcatMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, CancellationToken, Task<TResult>> selector)
        {
            return source
                .Map((outer) => Observable.FromAsync(ct => selector(outer, ct)))
                .Concat();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index with cancellation support and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> ConcatMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, CancellationToken, Task<TResult>> selector)
        {
            return source
                .Map((outer, index) => Observable.FromAsync(ct => selector(outer, index, ct)))
                .Concat();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence, invokes the result selector for the source element and each of the corresponding inner sequence's elements, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TCollection">The type of the elements in the projected intermediate sequences.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate sequence elements.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function collectionSelector on each element of the input sequence and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        public static IObservable<TResult> ConcatMap<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, IObservable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            return source
                .Map((outer) =>
                {
                    return collectionSelector(outer)
                        .Map((inner) => resultSelector(outer, inner));
                }).Concat();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence by incorporating the element's index, invokes the result selector for the source element and each of the corresponding inner sequence's elements, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TCollection">The type of the elements in the projected intermediate sequences.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate sequence elements.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence; the second parameter of the function represents the index of the source element and the fourth parameter represents the index of the intermediate element.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function collectionSelector on each element of the input sequence and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        public static IObservable<TResult> ConcatMap<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, int, IObservable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
        {
            return source
                .Map((outer, outerIndex) =>
                {
                    return collectionSelector(outer, outerIndex)
                        .Map((inner, innerIndex) => resultSelector(outer, outerIndex, inner, innerIndex));
                }).Concat();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> ConcatMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, Task<TTaskResult>> taskSelector, Func<TSource, TTaskResult, TResult> resultSelector)
        {
            return source
                .Map((outer) =>
                {
                    return Observable.FromAsync(() => taskSelector(outer))
                        .Map((inner) => resultSelector(outer, inner));
                })
                .Concat();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> ConcatMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, int, Task<TTaskResult>> taskSelector, Func<TSource, int, TTaskResult, TResult> resultSelector)
        {
            return source
                .Map((outer, index) =>
                {
                    return Observable.FromAsync(() => taskSelector(outer, index))
                        .Map((inner) => resultSelector(outer, index, inner));
                })
                .Concat();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task with cancellation support, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> ConcatMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, CancellationToken, Task<TTaskResult>> taskSelector, Func<TSource, TTaskResult, TResult> resultSelector)
        {
            return source
                .Map((outer) =>
                {
                    return Observable.FromAsync(ct => taskSelector(outer, ct))
                        .Map((inner) => resultSelector(outer, inner));
                })
                .Concat();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index with cancellation support, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> ConcatMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, int, CancellationToken, Task<TTaskResult>> taskSelector, Func<TSource, int, TTaskResult, TResult> resultSelector)
        {
            return source
                .Map((outer, index) =>
                {
                    return Observable.FromAsync(ct => taskSelector(outer, index, ct))
                        .Map((inner) => resultSelector(outer, index, inner));
                })
                .Concat();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an enumerable sequence and concatenates the resulting enumerable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner enumerable sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <remarks>The projected sequences are enumerated synchonously within the OnNext call of the source sequence. In order to do a concurrent, non-blocking merge, change the selector to return an observable sequence obtained using the <see cref="M:System.Reactive.Linq.Observable.ToObservable``1(System.Collections.Generic.IEnumerable{``0})" /> conversion.</remarks>
        public static IObservable<TResult> ConcatMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            return source
                .Map((outer) => selector(outer).ToObservable())
                .Concat();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an enumerable sequence by incorporating the element's index and concatenates the resulting enumerable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner enumerable sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <remarks>The projected sequences are enumerated synchonously within the OnNext call of the source sequence. In order to do a concurrent, non-blocking merge, change the selector to return an observable sequence obtained using the <see cref="M:System.Reactive.Linq.Observable.ToObservable``1(System.Collections.Generic.IEnumerable{``0})" /> conversion.</remarks>
        public static IObservable<TResult> ConcatMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
        {
            return source
                .Map((outer, index) => selector(outer, index).ToObservable())
                .Concat();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an enumerable sequence, invokes the result selector for the source element and each of the corresponding inner sequence's elements, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TCollection">The type of the elements in the projected intermediate enumerable sequences.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate sequence elements.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function collectionSelector on each element of the input sequence and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>The projected sequences are enumerated synchonously within the OnNext call of the source sequence. In order to do a concurrent, non-blocking merge, change the selector to return an observable sequence obtained using the <see cref="M:System.Reactive.Linq.Observable.ToObservable``1(System.Collections.Generic.IEnumerable{``0})" /> conversion.</remarks>
        public static IObservable<TResult> ConcatMap<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            return source
                .Map((outer) =>
                {
                    return collectionSelector(outer)
                        .ToObservable()
                        .Map((inner) => resultSelector(outer, inner));
                }).Concat();
        }

        /// <summary>
        /// Merges elements from all inner observable sequences into a single observable sequence, limiting the number of concurrent subscriptions to inner sequences.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TCollection">The type of the elements in the projected intermediate enumerable sequences.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate sequence elements.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence; the second parameter of the function represents the index of the source element and the fourth parameter represents the index of the intermediate element.</param>
        /// <returns>The observable sequence that merges the elements of the inner sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> is null.</exception>
        public static IObservable<TResult> ConcatMap<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
        {
            return source
                .Map((outer, outerIndex) =>
                {
                    return collectionSelector(outer, outerIndex)
                        .ToObservable()
                        .Map((inner, innerIndex) => resultSelector(outer, outerIndex, inner, innerIndex));
                }).Concat();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence and merges the resulting observable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> SwitchMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IObservable<TResult>> selector)
        {
            return source
                .Map(selector)
                .Switch();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence by incorporating the element's index and merges the resulting observable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> SwitchMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, IObservable<TResult>> selector)
        {
            return source
                .Map(selector)
                .Switch();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> SwitchMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, Task<TResult>> selector)
        {
            return source
                .Map((outer) => Observable.FromAsync(() => selector(outer)))
                .Switch();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> SwitchMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, Task<TResult>> selector)
        {
            return source
                .Map((outer, index) => Observable.FromAsync(() => selector(outer, index)))
                .Switch();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task with cancellation support and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> SwitchMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, CancellationToken, Task<TResult>> selector)
        {
            return source
                .Map((outer) => Observable.FromAsync(ct => selector(outer, ct)))
                .Switch();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index with cancellation support and merges all of the task results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the projected tasks and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of the tasks executed for each element of the input sequence.</returns>
        /// <remarks>This overload supports composition of observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IObservable<TResult> SwitchMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, CancellationToken, Task<TResult>> selector)
        {
            return source
                .Map((outer, index) => Observable.FromAsync(ct => selector(outer, index, ct)))
                .Switch();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence, invokes the result selector for the source element and each of the corresponding inner sequence's elements, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TCollection">The type of the elements in the projected intermediate sequences.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate sequence elements.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function collectionSelector on each element of the input sequence and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        public static IObservable<TResult> SwitchMap<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, IObservable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            return source
                .Map((outer) =>
                {
                    return collectionSelector(outer)
                        .Map((inner) => resultSelector(outer, inner));
                }).Switch();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence by incorporating the element's index, invokes the result selector for the source element and each of the corresponding inner sequence's elements, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TCollection">The type of the elements in the projected intermediate sequences.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate sequence elements.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence; the second parameter of the function represents the index of the source element and the fourth parameter represents the index of the intermediate element.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function collectionSelector on each element of the input sequence and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        public static IObservable<TResult> SwitchMap<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, int, IObservable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
        {
            return source
                .Map((outer, outerIndex) =>
                {
                    return collectionSelector(outer, outerIndex)
                        .Map((inner, innerIndex) => resultSelector(outer, outerIndex, inner, innerIndex));
                }).Switch();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> SwitchMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, Task<TTaskResult>> taskSelector, Func<TSource, TTaskResult, TResult> resultSelector)
        {
            return source
                .Map((outer) =>
                {
                    return Observable.FromAsync(() => taskSelector(outer))
                        .Map((inner) => resultSelector(outer, inner));
                })
                .Switch();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> SwitchMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, int, Task<TTaskResult>> taskSelector, Func<TSource, int, TTaskResult, TResult> resultSelector)
        {
            return source
                .Map((outer, index) =>
                {
                    return Observable.FromAsync(() => taskSelector(outer, index))
                        .Map((inner) => resultSelector(outer, index, inner));
                })
                .Switch();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task with cancellation support, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> SwitchMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, CancellationToken, Task<TTaskResult>> taskSelector, Func<TSource, TTaskResult, TResult> resultSelector)
        {
            return source
                .Map((outer) =>
                {
                    return Observable.FromAsync(ct => taskSelector(outer, ct))
                        .Map((inner) => resultSelector(outer, inner));
                })
                .Switch();
        }

        /// <summary>
        /// Projects each element of an observable sequence to a task by incorporating the element's index with cancellation support, invokes the result selector for the source element and the task result, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TTaskResult">The type of the results produced by the projected intermediate tasks.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate task results.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="taskSelector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of obtaining a task for each element of the input sequence and then mapping the task's result and its corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="taskSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>This overload supports using LINQ query comprehension syntax in C# and Visual Basic to compose observable sequences and tasks, without requiring manual conversion of the tasks to observable sequences using <see cref="M:System.Reactive.Threading.Tasks.TaskObservableExtensions.ToObservable``1(System.Threading.Tasks.Task{``0})" />.</remarks>
        public static IObservable<TResult> SwitchMap<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, int, CancellationToken, Task<TTaskResult>> taskSelector, Func<TSource, int, TTaskResult, TResult> resultSelector)
        {
            return source
                .Map((outer, index) =>
                {
                    return Observable.FromAsync(ct => taskSelector(outer, index, ct))
                        .Map((inner) => resultSelector(outer, index, inner));
                })
                .Switch();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an enumerable sequence and concatenates the resulting enumerable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner enumerable sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <remarks>The projected sequences are enumerated synchonously within the OnNext call of the source sequence. In order to do a concurrent, non-blocking merge, change the selector to return an observable sequence obtained using the <see cref="M:System.Reactive.Linq.Observable.ToObservable``1(System.Collections.Generic.IEnumerable{``0})" /> conversion.</remarks>
        public static IObservable<TResult> SwitchMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            return source
                .Map((outer) => selector(outer).ToObservable())
                .Switch();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an enumerable sequence by incorporating the element's index and concatenates the resulting enumerable sequences into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the projected inner enumerable sequences and the elements in the merged result sequence.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="selector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <remarks>The projected sequences are enumerated synchonously within the OnNext call of the source sequence. In order to do a concurrent, non-blocking merge, change the selector to return an observable sequence obtained using the <see cref="M:System.Reactive.Linq.Observable.ToObservable``1(System.Collections.Generic.IEnumerable{``0})" /> conversion.</remarks>
        public static IObservable<TResult> SwitchMap<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
        {
            return source
                .Map((outer, index) => selector(outer, index).ToObservable())
                .Switch();
        }

        /// <summary>
        /// Projects each element of an observable sequence to an enumerable sequence, invokes the result selector for the source element and each of the corresponding inner sequence's elements, and merges the results into one observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TCollection">The type of the elements in the projected intermediate enumerable sequences.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate sequence elements.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>An observable sequence whose elements are the result of invoking the one-to-many transform function collectionSelector on each element of the input sequence and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="collectionSelector" /> or <paramref name="resultSelector" /> is null.</exception>
        /// <remarks>The projected sequences are enumerated synchonously within the OnNext call of the source sequence. In order to do a concurrent, non-blocking merge, change the selector to return an observable sequence obtained using the <see cref="M:System.Reactive.Linq.Observable.ToObservable``1(System.Collections.Generic.IEnumerable{``0})" /> conversion.</remarks>
        public static IObservable<TResult> SwitchMap<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            return source
                .Map((outer) =>
                {
                    return collectionSelector(outer)
                        .ToObservable()
                        .Map((inner) => resultSelector(outer, inner));
                }).Switch();
        }

        /// <summary>
        /// Merges elements from all inner observable sequences into a single observable sequence, limiting the number of concurrent subscriptions to inner sequences.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TCollection">The type of the elements in the projected intermediate enumerable sequences.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence, obtained by using the selector to combine source sequence elements with their corresponding intermediate sequence elements.</typeparam>
        /// <param name="source">An observable sequence of elements to project.</param>
        /// <param name="collectionSelector">A transform function to apply to each element; the second parameter of the function represents the index of the source element.</param>
        /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence; the second parameter of the function represents the index of the source element and the fourth parameter represents the index of the intermediate element.</param>
        /// <returns>The observable sequence that merges the elements of the inner sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> is null.</exception>
        public static IObservable<TResult> SwitchMap<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
        {
            return source
                .Map((outer, outerIndex) =>
                {
                    return collectionSelector(outer, outerIndex)
                        .ToObservable()
                        .Map((inner, innerIndex) => resultSelector(outer, outerIndex, inner, innerIndex));
                }).Switch();
        }

        /// <summary>
        /// Applies an accumulator function over an observable sequence, returning the result of the aggregation as a single element in the result sequence. The specified seed value is used as the initial accumulator value.
        /// For aggregation behavior with incremental intermediate results, see <see cref="Observable.Scan{TSource, Accumulate}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAccumulate">The type of the result of the aggregation.</typeparam>
        /// <param name="source">An observable sequence to aggregate over.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="accumulator">An accumulator function to be invoked on each element.</param>
        /// <returns>An observable sequence containing a single element with the final accumulator value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="accumulator"/> is null.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public static IObservable<TAccumulate> Reduce<TSource, TAccumulate>(this IObservable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator)
        {
            return source.Aggregate(seed, accumulator);
        }

        /// <summary>
        /// Applies an accumulator function over an observable sequence, returning the result of the aggregation as a single element in the result sequence. The specified seed value is used as the initial accumulator value,
        /// and the specified result selector function is used to select the result value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="source">An observable sequence to aggregate over.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="accumulator">An accumulator function to be invoked on each element.</param>
        /// <param name="resultSelector">A function to transform the final accumulator value into the result value.</param>
        /// <returns>An observable sequence containing a single element with the final accumulator value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="accumulator"/> or <paramref name="resultSelector"/> is null.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public static IObservable<TResult> Reduce<TSource, TAccumulate, TResult>(this IObservable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator, Func<TAccumulate, TResult> resultSelector)
        {
            return source.Aggregate(seed, accumulator, resultSelector);
        }

        /// <summary>
        /// Applies an accumulator function over an observable sequence, returning the result of the aggregation as a single element in the result sequence.
        /// For aggregation behavior with incremental intermediate results, see <see cref="Observable.Scan{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence and the result of the aggregation.</typeparam>
        /// <param name="source">An observable sequence to aggregate over.</param>
        /// <param name="accumulator">An accumulator function to be invoked on each element.</param>
        /// <returns>An observable sequence containing a single element with the final accumulator value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="accumulator"/> is null.</exception>
        /// <exception cref="InvalidOperationException">(Asynchronous) The source sequence is empty.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public static IObservable<TSource> Reduce<TSource>(this IObservable<TSource> source, Func<TSource, TSource, TSource> accumulator)
        {
            return source.Aggregate(accumulator);
        }

        /// <summary>
        /// Returns a <see cref="Unit"/> at the completion of an observable sequence.
        /// </summary>
        /// <param name="source">The observable sequence that requires a return value.</param>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IObservable<Unit> ToSignal<TSource>(this IObservable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source
                .Select(_ => Unit.Default);
        }

        /// <summary>
        /// Applies a filter on an observable sequence returning only items from the sequence that are not null.
        /// </summary>
        /// <param name="source">The source observable sequence</param>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <returns></returns>
        public static IObservable<TSource> WhereNotNull<TSource>(this IObservable<TSource> source)
        {
            return source.Where(x => x != null);
        }
    }
}
