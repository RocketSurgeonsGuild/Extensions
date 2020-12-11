using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace Rocket.Surgery.DependencyInjection
{
    internal class ExecuteScoped<T> : IExecuteScoped<T>
        where T : notnull
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExecuteScoped(IServiceScopeFactory serviceScopeFactoryFactory) => _serviceScopeFactory = serviceScopeFactoryFactory;

        public void Invoke(Action<T> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            action(scope.ServiceProvider.GetRequiredService<T>());
        }

        public TResult Invoke<TResult>(Func<T, TResult> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return action(scope.ServiceProvider.GetRequiredService<T>());
        }

        public async Task Invoke(Func<T, CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            await action(scope.ServiceProvider.GetRequiredService<T>(), cancellationToken).ConfigureAwait(false);
        }

        public async Task<TResult> Invoke<TResult>(Func<T, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return await action(scope.ServiceProvider.GetRequiredService<T>(), cancellationToken).ConfigureAwait(false);
        }

        public Task Invoke(Func<T, Task> action) => Invoke((a, _) => action(a), CancellationToken.None);

        public Task<TResult> Invoke<TResult>(Func<T, Task<TResult>> action) => Invoke((a, _) => action(a), CancellationToken.None);
    }

    internal class ExecuteScoped<T1, T2> : IExecuteScoped<T1, T2>
        where T1 : notnull
        where T2 : notnull
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExecuteScoped(IServiceScopeFactory serviceScopeFactoryFactory) => _serviceScopeFactory = serviceScopeFactoryFactory;

        public void Invoke(Action<T1, T2> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>()
            );
        }

        public TResult Invoke<TResult>(Func<T1, T2, TResult> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>()
            );
        }

        public async Task Invoke(Func<T1, T2, CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                cancellationToken
            ).ConfigureAwait(false);
        }

        public async Task<TResult> Invoke<TResult>(Func<T1, T2, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                cancellationToken
            ).ConfigureAwait(false);
        }

        public Task Invoke(Func<T1, T2, Task> action) => Invoke((a, b, _) => action(a, b), CancellationToken.None);

        public Task<TResult> Invoke<TResult>(Func<T1, T2, Task<TResult>> action) => Invoke((a, b, _) => action(a, b), CancellationToken.None);
    }

    internal class ExecuteScoped<T1, T2, T3> : IExecuteScoped<T1, T2, T3>
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExecuteScoped(IServiceScopeFactory serviceScopeFactoryFactory) => _serviceScopeFactory = serviceScopeFactoryFactory;

        public void Invoke(Action<T1, T2, T3> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>()
            );
        }

        public TResult Invoke<TResult>(Func<T1, T2, T3, TResult> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>()
            );
        }

        public async Task Invoke(Func<T1, T2, T3, CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                cancellationToken
            ).ConfigureAwait(false);
        }

        public async Task<TResult> Invoke<TResult>(Func<T1, T2, T3, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                cancellationToken
            ).ConfigureAwait(false);
        }

        public Task Invoke(Func<T1, T2, T3, Task> action) => Invoke((a, b, c, _) => action(a, b, c), CancellationToken.None);

        public Task<TResult> Invoke<TResult>(Func<T1, T2, T3, Task<TResult>> action) => Invoke((a, b, c, _) => action(a, b, c), CancellationToken.None);
    }

    internal class ExecuteScoped<T1, T2, T3, T4> : IExecuteScoped<T1, T2, T3, T4>
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExecuteScoped(IServiceScopeFactory serviceScopeFactoryFactory) => _serviceScopeFactory = serviceScopeFactoryFactory;

        public void Invoke(Action<T1, T2, T3, T4> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>()
            );
        }

        public TResult Invoke<TResult>(Func<T1, T2, T3, T4, TResult> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>()
            );
        }

        public async Task Invoke(Func<T1, T2, T3, T4, CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                cancellationToken
            ).ConfigureAwait(false);
        }

        public async Task<TResult> Invoke<TResult>(Func<T1, T2, T3, T4, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                cancellationToken
            ).ConfigureAwait(false);
        }

        public Task Invoke(Func<T1, T2, T3, T4, Task> action) => Invoke((a, b, c, d, _) => action(a, b, c, d), CancellationToken.None);

        public Task<TResult> Invoke<TResult>(Func<T1, T2, T3, T4, Task<TResult>> action) => Invoke((a, b, c, d, _) => action(a, b, c, d), CancellationToken.None);
    }

    internal class ExecuteScoped<T1, T2, T3, T4, T5> : IExecuteScoped<T1, T2, T3, T4, T5>
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExecuteScoped(IServiceScopeFactory serviceScopeFactoryFactory) => _serviceScopeFactory = serviceScopeFactoryFactory;

        public void Invoke(Action<T1, T2, T3, T4, T5> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                scope.ServiceProvider.GetRequiredService<T5>()
            );
        }

        public TResult Invoke<TResult>(Func<T1, T2, T3, T4, T5, TResult> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                scope.ServiceProvider.GetRequiredService<T5>()
            );
        }

        public async Task Invoke(Func<T1, T2, T3, T4, T5, CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                scope.ServiceProvider.GetRequiredService<T5>(),
                cancellationToken
            ).ConfigureAwait(false);
        }

        public async Task<TResult> Invoke<TResult>(Func<T1, T2, T3, T4, T5, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                scope.ServiceProvider.GetRequiredService<T5>(),
                cancellationToken
            ).ConfigureAwait(false);
        }

        public Task Invoke(Func<T1, T2, T3, T4, T5, Task> action) => Invoke((a, b, c, d, e, _) => action(a, b, c, d, e), CancellationToken.None);

        public Task<TResult> Invoke<TResult>(Func<T1, T2, T3, T4, T5, Task<TResult>> action) => Invoke((a, b, c, d, e, _) => action(a, b, c, d, e), CancellationToken.None);
    }

    internal class ExecuteScoped<T1, T2, T3, T4, T5, T6> : IExecuteScoped<T1, T2, T3, T4, T5, T6>
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
        where T6 : notnull
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExecuteScoped(IServiceScopeFactory serviceScopeFactoryFactory) => _serviceScopeFactory = serviceScopeFactoryFactory;

        public void Invoke(Action<T1, T2, T3, T4, T5, T6> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                scope.ServiceProvider.GetRequiredService<T5>(),
                scope.ServiceProvider.GetRequiredService<T6>()
            );
        }

        public TResult Invoke<TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                scope.ServiceProvider.GetRequiredService<T5>(),
                scope.ServiceProvider.GetRequiredService<T6>()
            );
        }

        public async Task Invoke(Func<T1, T2, T3, T4, T5, T6, CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                scope.ServiceProvider.GetRequiredService<T5>(),
                scope.ServiceProvider.GetRequiredService<T6>(),
                cancellationToken
            ).ConfigureAwait(false);
        }

        public async Task<TResult> Invoke<TResult>(Func<T1, T2, T3, T4, T5, T6, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                scope.ServiceProvider.GetRequiredService<T5>(),
                scope.ServiceProvider.GetRequiredService<T6>(),
                cancellationToken
            ).ConfigureAwait(false);
        }

        public Task Invoke(Func<T1, T2, T3, T4, T5, T6, Task> action) => Invoke(
            (
                a,
                b,
                c,
                d,
                e,
                f,
                _
            ) => action(a, b, c, d, e, f),
            CancellationToken.None
        );

        public Task<TResult> Invoke<TResult>(Func<T1, T2, T3, T4, T5, T6, Task<TResult>> action) => Invoke(
            (
                a,
                b,
                c,
                d,
                e,
                f,
                _
            ) => action(a, b, c, d, e, f),
            CancellationToken.None
        );
    }
}