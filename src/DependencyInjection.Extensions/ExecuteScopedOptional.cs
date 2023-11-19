using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.DependencyInjection;

internal class ExecuteScopedOptional<T>(IServiceScopeFactory serviceScopeFactoryFactory) : IExecuteScopedOptional<T>
    where T : notnull
{
    public void Invoke(Action<T?> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = serviceScopeFactoryFactory.CreateScope();
        action(scope.ServiceProvider.GetService<T>());
    }

    public TResult Invoke<TResult>(Func<T?, TResult> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = serviceScopeFactoryFactory.CreateScope();
        return action(scope.ServiceProvider.GetService<T>());
    }

    public async Task Invoke(Func<T?, CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = serviceScopeFactoryFactory.CreateScope();
        await action(scope.ServiceProvider.GetService<T>(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<TResult> Invoke<TResult>(Func<T?, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = serviceScopeFactoryFactory.CreateScope();
        return await action(scope.ServiceProvider.GetService<T>(), cancellationToken).ConfigureAwait(false);
    }

    public Task Invoke(Func<T?, Task> action)
    {
        return Invoke((a, _) => action(a), CancellationToken.None);
    }

    public Task<TResult> Invoke<TResult>(Func<T?, Task<TResult>> action)
    {
        return Invoke((a, _) => action(a), CancellationToken.None);
    }
}

internal class ExecuteScopedOptional<T1, T2> : IExecuteScopedOptional<T1, T2>
    where T1 : notnull
    where T2 : notnull
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ExecuteScopedOptional(IServiceScopeFactory serviceScopeFactoryFactory)
    {
        _serviceScopeFactory = serviceScopeFactoryFactory;
    }

    public void Invoke(Action<T1?, T2?> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>()
        );
    }

    public TResult Invoke<TResult>(Func<T1?, T2?, TResult> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        return action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>()
        );
    }

    public async Task Invoke(Func<T1?, T2?, CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        await action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>(),
            cancellationToken
        ).ConfigureAwait(false);
    }

    public async Task<TResult> Invoke<TResult>(Func<T1?, T2?, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        return await action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>(),
            cancellationToken
        ).ConfigureAwait(false);
    }

    public Task Invoke(Func<T1?, T2?, Task> action)
    {
        return Invoke((a, b, _) => action(a, b), CancellationToken.None);
    }

    public Task<TResult> Invoke<TResult>(Func<T1?, T2?, Task<TResult>> action)
    {
        return Invoke((a, b, _) => action(a, b), CancellationToken.None);
    }
}

internal class ExecuteScopedOptional<T1, T2, T3> : IExecuteScopedOptional<T1, T2, T3>
    where T1 : notnull
    where T2 : notnull
    where T3 : notnull
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ExecuteScopedOptional(IServiceScopeFactory serviceScopeFactoryFactory)
    {
        _serviceScopeFactory = serviceScopeFactoryFactory;
    }

    public void Invoke(Action<T1?, T2?, T3?> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>(),
            scope.ServiceProvider.GetService<T3>()
        );
    }

    public TResult Invoke<TResult>(Func<T1?, T2?, T3?, TResult> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        return action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>(),
            scope.ServiceProvider.GetService<T3>()
        );
    }

    public async Task Invoke(Func<T1?, T2?, T3?, CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        await action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>(),
            scope.ServiceProvider.GetService<T3>(),
            cancellationToken
        ).ConfigureAwait(false);
    }

    public async Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        return await action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>(),
            scope.ServiceProvider.GetService<T3>(),
            cancellationToken
        ).ConfigureAwait(false);
    }

    public Task Invoke(Func<T1?, T2?, T3?, Task> action)
    {
        return Invoke((a, b, c, _) => action(a, b, c), CancellationToken.None);
    }

    public Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, Task<TResult>> action)
    {
        return Invoke((a, b, c, _) => action(a, b, c), CancellationToken.None);
    }
}

internal class ExecuteScopedOptional<T1, T2, T3, T4> : IExecuteScopedOptional<T1, T2, T3, T4>
    where T1 : notnull
    where T2 : notnull
    where T3 : notnull
    where T4 : notnull
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ExecuteScopedOptional(IServiceScopeFactory serviceScopeFactoryFactory)
    {
        _serviceScopeFactory = serviceScopeFactoryFactory;
    }

    public void Invoke(Action<T1?, T2?, T3?, T4?> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>(),
            scope.ServiceProvider.GetService<T3>(),
            scope.ServiceProvider.GetService<T4>()
        );
    }

    public TResult Invoke<TResult>(Func<T1?, T2?, T3?, T4?, TResult> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        return action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>(),
            scope.ServiceProvider.GetService<T3>(),
            scope.ServiceProvider.GetService<T4>()
        );
    }

    public async Task Invoke(Func<T1?, T2?, T3?, T4?, CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        await action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>(),
            scope.ServiceProvider.GetService<T3>(),
            scope.ServiceProvider.GetService<T4>(),
            cancellationToken
        ).ConfigureAwait(false);
    }

    public async Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, T4?, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        return await action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>(),
            scope.ServiceProvider.GetService<T3>(),
            scope.ServiceProvider.GetService<T4>(),
            cancellationToken
        ).ConfigureAwait(false);
    }

    public Task Invoke(Func<T1?, T2?, T3?, T4?, Task> action)
    {
        return Invoke((a, b, c, d, _) => action(a, b, c, d), CancellationToken.None);
    }

    public Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, T4?, Task<TResult>> action)
    {
        return Invoke((a, b, c, d, _) => action(a, b, c, d), CancellationToken.None);
    }
}

internal class ExecuteScopedOptional<T1, T2, T3, T4, T5> : IExecuteScopedOptional<T1, T2, T3, T4, T5>
    where T1 : notnull
    where T2 : notnull
    where T3 : notnull
    where T4 : notnull
    where T5 : notnull
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ExecuteScopedOptional(IServiceScopeFactory serviceScopeFactoryFactory)
    {
        _serviceScopeFactory = serviceScopeFactoryFactory;
    }

    public void Invoke(Action<T1?, T2?, T3?, T4?, T5?> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
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

    public TResult Invoke<TResult>(Func<T1?, T2?, T3?, T4?, T5?, TResult> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
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

    public async Task Invoke(Func<T1?, T2?, T3?, T4?, T5?, CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        await action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>(),
            scope.ServiceProvider.GetService<T3>(),
            scope.ServiceProvider.GetService<T4>(),
            scope.ServiceProvider.GetService<T5>(),
            cancellationToken
        ).ConfigureAwait(false);
    }

    public async Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, T4?, T5?, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        return await action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>(),
            scope.ServiceProvider.GetService<T3>(),
            scope.ServiceProvider.GetService<T4>(),
            scope.ServiceProvider.GetService<T5>(),
            cancellationToken
        ).ConfigureAwait(false);
    }

    public Task Invoke(Func<T1?, T2?, T3?, T4?, T5?, Task> action)
    {
        return Invoke((a, b, c, d, e, _) => action(a, b, c, d, e), CancellationToken.None);
    }

    public Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, T4?, T5?, Task<TResult>> action)
    {
        return Invoke((a, b, c, d, e, _) => action(a, b, c, d, e), CancellationToken.None);
    }
}

internal class ExecuteScopedOptional<T1, T2, T3, T4, T5, T6> : IExecuteScopedOptional<T1, T2, T3, T4, T5, T6>
    where T1 : notnull
    where T2 : notnull
    where T3 : notnull
    where T4 : notnull
    where T5 : notnull
    where T6 : notnull
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ExecuteScopedOptional(IServiceScopeFactory serviceScopeFactoryFactory)
    {
        _serviceScopeFactory = serviceScopeFactoryFactory;
    }

    public void Invoke(Action<T1?, T2?, T3?, T4?, T5?, T6?> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
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

    public TResult Invoke<TResult>(Func<T1?, T2?, T3?, T4?, T5?, T6?, TResult> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
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

    public async Task Invoke(Func<T1?, T2?, T3?, T4?, T5?, T6?, CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        await action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>(),
            scope.ServiceProvider.GetService<T3>(),
            scope.ServiceProvider.GetService<T4>(),
            scope.ServiceProvider.GetService<T5>(),
            scope.ServiceProvider.GetService<T6>(),
            cancellationToken
        ).ConfigureAwait(false);
    }

    public async Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, T4?, T5?, T6?, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        using var scope = _serviceScopeFactory.CreateScope();
        return await action(
            scope.ServiceProvider.GetService<T1>(),
            scope.ServiceProvider.GetService<T2>(),
            scope.ServiceProvider.GetService<T3>(),
            scope.ServiceProvider.GetService<T4>(),
            scope.ServiceProvider.GetService<T5>(),
            scope.ServiceProvider.GetService<T6>(),
            cancellationToken
        ).ConfigureAwait(false);
    }

    public Task Invoke(Func<T1?, T2?, T3?, T4?, T5?, T6?, Task> action)
    {
        return Invoke(
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

    public Task<TResult> Invoke<TResult>(Func<T1?, T2?, T3?, T4?, T5?, T6?, Task<TResult>> action)
    {
        return Invoke(
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
