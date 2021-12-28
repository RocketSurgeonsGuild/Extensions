using System.Diagnostics;
using Microsoft.Extensions.Logging;

#pragma warning disable CA2254
// ReSharper disable TemplateIsNotCompileTimeConstantProblem
namespace Rocket.Surgery.Extensions.Logging;

/// <summary>
///     LoggingExtensions.
/// </summary>
public static class LoggingAbstractionsExtensions
{
    /// <summary>
    ///     Times the trace.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="message">The message.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>IDisposable.</returns>
    public static IDisposable TimeTrace(this ILogger logger, string message, params object[] args)
    {
        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (!logger.IsEnabled(LogLevel.Trace)) return NoopDisposable.Instance;

        var scope = logger.BeginScope(new { });
        logger.LogTrace($"Starting: {message}", args);
        return new Disposable(
            scope,
            elapsed =>
            {
                var a = args.Concat(new object[] { elapsed }).ToArray();
                logger.LogTrace($"Finished: {message} in {{ElapsedMilliseconds}}ms", a);
            }
        );
    }

    /// <summary>
    ///     Times the debug.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="message">The message.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>IDisposable.</returns>
    public static IDisposable TimeDebug(this ILogger logger, string message, params object[] args)
    {
        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (!logger.IsEnabled(LogLevel.Debug)) return NoopDisposable.Instance;

        var scope = logger.BeginScope(new { });
        logger.LogDebug($"Starting: {message}", args);
        return new Disposable(
            scope,
            elapsed =>
            {
                var a = args.Concat(new object[] { elapsed }).ToArray();
                logger.LogDebug($"Finished: {message} in {{ElapsedMilliseconds}}ms", a);
            }
        );
    }

    /// <summary>
    ///     Times the information.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="message">The message.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>IDisposable.</returns>
    public static IDisposable TimeInformation(this ILogger logger, string message, params object[] args)
    {
        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (!logger.IsEnabled(LogLevel.Information)) return NoopDisposable.Instance;

        var scope = logger.BeginScope(new { });
        logger.LogInformation($"Starting: {message}", args);
        return new Disposable(
            scope,
            elapsed =>
            {
                var a = args.Concat(new object[] { elapsed }).ToArray();
                logger.LogInformation($"Finished: {message} in {{ElapsedMilliseconds}}ms", a);
            }
        );
    }

    private class NoopDisposable : IDisposable
    {
        private NoopDisposable()
        {
        }

        public void Dispose()
        {
        }

        public static readonly NoopDisposable Instance = new();
    }

    /// <summary>
    ///     Disposable.
    ///     Implements the <see cref="IDisposable" />
    /// </summary>
    /// <seealso cref="IDisposable" />
    private class Disposable : IDisposable
    {
        private readonly IDisposable _disposable;
        private readonly Action<long> _action;
        private readonly Stopwatch _sw;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Disposable" /> class.
        /// </summary>
        /// <param name="disposable">The disposable.</param>
        /// <param name="action">The action.</param>
        public Disposable(IDisposable disposable, Action<long> action)
        {
            _disposable = disposable;
            _action = action;
            _sw = new Stopwatch();
            _sw.Start();
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _sw.Stop();
            _action(_sw.ElapsedMilliseconds);
            _disposable.Dispose();
        }
    }
}
