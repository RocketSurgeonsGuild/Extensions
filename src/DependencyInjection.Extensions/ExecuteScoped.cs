using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.DependencyInjection
{
    internal class ExecuteScoped<T> : IExecuteScoped<T>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExecuteScoped(IServiceScopeFactory serviceScopeFactoryFactory) => _serviceScopeFactory = serviceScopeFactoryFactory;

        public void Invoke(Action<T> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            action(scope.ServiceProvider.GetService<T>());
        }

        public TResult Invoke<TResult>(Func<T, TResult> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return action(scope.ServiceProvider.GetService<T>());
        }

        public async Task Invoke(Func<T, Task> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            await action(scope.ServiceProvider.GetService<T>()).ConfigureAwait(false);
        }

        public async Task<TResult> Invoke<TResult>(Func<T, Task<TResult>> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return await action(scope.ServiceProvider.GetService<T>()).ConfigureAwait(false);
        }
    }

    internal class ExecuteScoped<T1, T2> : IExecuteScoped<T1, T2>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExecuteScoped(IServiceScopeFactory serviceScopeFactoryFactory) => _serviceScopeFactory = serviceScopeFactoryFactory;

        public void Invoke(Action<T1, T2> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>()
            );
        }

        public TResult Invoke<TResult>(Func<T1, T2, TResult> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>()
            );
        }

        public async Task Invoke(Func<T1, T2, Task> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            await action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>()
            ).ConfigureAwait(false);
        }

        public async Task<TResult> Invoke<TResult>(Func<T1, T2, Task<TResult>> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return await action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>()
            ).ConfigureAwait(false);
        }
    }

    internal class ExecuteScoped<T1, T2, T3> : IExecuteScoped<T1, T2, T3>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExecuteScoped(IServiceScopeFactory serviceScopeFactoryFactory) => _serviceScopeFactory = serviceScopeFactoryFactory;

        public void Invoke(Action<T1, T2, T3> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>()
            );
        }

        public TResult Invoke<TResult>(Func<T1, T2, T3, TResult> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>()
            );
        }

        public async Task Invoke(Func<T1, T2, T3, Task> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            await action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>()
            ).ConfigureAwait(false);
        }

        public async Task<TResult> Invoke<TResult>(Func<T1, T2, T3, Task<TResult>> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return await action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>()
            ).ConfigureAwait(false);
        }
    }

    internal class ExecuteScoped<T1, T2, T3, T4> : IExecuteScoped<T1, T2, T3, T4>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExecuteScoped(IServiceScopeFactory serviceScopeFactoryFactory) => _serviceScopeFactory = serviceScopeFactoryFactory;

        public void Invoke(Action<T1, T2, T3, T4> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>(),
                scope.ServiceProvider.GetService<T4>()
            );
        }

        public TResult Invoke<TResult>(Func<T1, T2, T3, T4, TResult> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>(),
                scope.ServiceProvider.GetService<T4>()
            );
        }

        public async Task Invoke(Func<T1, T2, T3, T4, Task> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            await action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>(),
                scope.ServiceProvider.GetService<T4>()
            ).ConfigureAwait(false);
        }

        public async Task<TResult> Invoke<TResult>(Func<T1, T2, T3, T4, Task<TResult>> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return await action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>(),
                scope.ServiceProvider.GetService<T4>()
            ).ConfigureAwait(false);
        }
    }

    internal class ExecuteScoped<T1, T2, T3, T4, T5> : IExecuteScoped<T1, T2, T3, T4, T5>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExecuteScoped(IServiceScopeFactory serviceScopeFactoryFactory) => _serviceScopeFactory = serviceScopeFactoryFactory;

        public void Invoke(Action<T1, T2, T3, T4, T5> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>(),
                scope.ServiceProvider.GetService<T4>(),
                scope.ServiceProvider.GetService<T5>()
            );
        }

        public TResult Invoke<TResult>(Func<T1, T2, T3, T4, T5, TResult> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>(),
                scope.ServiceProvider.GetService<T4>(),
                scope.ServiceProvider.GetService<T5>()
            );
        }

        public async Task Invoke(Func<T1, T2, T3, T4, T5, Task> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            await action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>(),
                scope.ServiceProvider.GetService<T4>(),
                scope.ServiceProvider.GetService<T5>()
            ).ConfigureAwait(false);
        }

        public async Task<TResult> Invoke<TResult>(Func<T1, T2, T3, T4, T5, Task<TResult>> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return await action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>(),
                scope.ServiceProvider.GetService<T4>(),
                scope.ServiceProvider.GetService<T5>()
            ).ConfigureAwait(false);
        }
    }

    internal class ExecuteScoped<T1, T2, T3, T4, T5, T6> : IExecuteScoped<T1, T2, T3, T4, T5, T6>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExecuteScoped(IServiceScopeFactory serviceScopeFactoryFactory) => _serviceScopeFactory = serviceScopeFactoryFactory;

        public void Invoke(Action<T1, T2, T3, T4, T5, T6> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>(),
                scope.ServiceProvider.GetService<T4>(),
                scope.ServiceProvider.GetService<T5>(),
                scope.ServiceProvider.GetService<T6>()
            );
        }

        public TResult Invoke<TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>(),
                scope.ServiceProvider.GetService<T4>(),
                scope.ServiceProvider.GetService<T5>(),
                scope.ServiceProvider.GetService<T6>()
            );
        }

        public async Task Invoke(Func<T1, T2, T3, T4, T5, T6, Task> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            await action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>(),
                scope.ServiceProvider.GetService<T4>(),
                scope.ServiceProvider.GetService<T5>(),
                scope.ServiceProvider.GetService<T6>()
            ).ConfigureAwait(false);
        }

        public async Task<TResult> Invoke<TResult>(Func<T1, T2, T3, T4, T5, T6, Task<TResult>> action)
        {
            if (action is null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            using var scope = _serviceScopeFactory.CreateScope();
            return await action(
                scope.ServiceProvider.GetService<T1>(),
                scope.ServiceProvider.GetService<T2>(),
                scope.ServiceProvider.GetService<T3>(),
                scope.ServiceProvider.GetService<T4>(),
                scope.ServiceProvider.GetService<T5>(),
                scope.ServiceProvider.GetService<T6>()
            ).ConfigureAwait(false);
        }
    }

}