using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.DependencyInjection;

internal class ExecuteScoped<T>(IServiceScopeFactory serviceScopeFactoryFactory) : IExecuteScoped<T>
    where T : notnull
{
    public void Invoke(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        action(scope.ServiceProvider.GetRequiredService<T>());
    }

    public TResult Invoke<TResult>(Func<T, TResult> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        return action(scope.ServiceProvider.GetRequiredService<T>());
    }

    public async Task Invoke(Func<T, CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        await action(scope.ServiceProvider.GetRequiredService<T>(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<TResult> Invoke<TResult>(Func<T, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<T>(), cancellationToken).ConfigureAwait(false);
    }

    public Task Invoke(Func<T, Task> action) => Invoke((a, _) => action(a), CancellationToken.None);

    public Task<TResult> Invoke<TResult>(Func<T, Task<TResult>> action) => Invoke((a, _) => action(a), CancellationToken.None);
}

internal class ExecuteScoped<T1, T2>(IServiceScopeFactory serviceScopeFactoryFactory) : IExecuteScoped<T1, T2>
    where T1 : notnull
    where T2 : notnull
{
    public void Invoke(Action<T1, T2> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        action(
            scope.ServiceProvider.GetRequiredService<T1>(),
            scope.ServiceProvider.GetRequiredService<T2>()
        );
    }

    public TResult Invoke<TResult>(Func<T1, T2, TResult> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        return action(
            scope.ServiceProvider.GetRequiredService<T1>(),
            scope.ServiceProvider.GetRequiredService<T2>()
        );
    }

    public async Task Invoke(Func<T1, T2, CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                cancellationToken
            )
           .ConfigureAwait(false);
    }

    public async Task<TResult> Invoke<TResult>(Func<T1, T2, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        return await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                cancellationToken
            )
           .ConfigureAwait(false);
    }

    public Task Invoke(Func<T1, T2, Task> action) => Invoke((a, b, _) => action(a, b), CancellationToken.None);

    public Task<TResult> Invoke<TResult>(Func<T1, T2, Task<TResult>> action) => Invoke((a, b, _) => action(a, b), CancellationToken.None);
}

internal class ExecuteScoped<T1, T2, T3>(IServiceScopeFactory serviceScopeFactoryFactory) : IExecuteScoped<T1, T2, T3>
    where T1 : notnull
    where T2 : notnull
    where T3 : notnull
{
    public void Invoke(Action<T1, T2, T3> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        action(
            scope.ServiceProvider.GetRequiredService<T1>(),
            scope.ServiceProvider.GetRequiredService<T2>(),
            scope.ServiceProvider.GetRequiredService<T3>()
        );
    }

    public TResult Invoke<TResult>(Func<T1, T2, T3, TResult> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        return action(
            scope.ServiceProvider.GetRequiredService<T1>(),
            scope.ServiceProvider.GetRequiredService<T2>(),
            scope.ServiceProvider.GetRequiredService<T3>()
        );
    }

    public async Task Invoke(Func<T1, T2, T3, CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                cancellationToken
            )
           .ConfigureAwait(false);
    }

    public async Task<TResult> Invoke<TResult>(Func<T1, T2, T3, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        return await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                cancellationToken
            )
           .ConfigureAwait(false);
    }

    public Task Invoke(Func<T1, T2, T3, Task> action) => Invoke((a, b, c, _) => action(a, b, c), CancellationToken.None);

    public Task<TResult> Invoke<TResult>(Func<T1, T2, T3, Task<TResult>> action) => Invoke((a, b, c, _) => action(a, b, c), CancellationToken.None);
}

internal class ExecuteScoped<T1, T2, T3, T4>(IServiceScopeFactory serviceScopeFactoryFactory) : IExecuteScoped<T1, T2, T3, T4>
    where T1 : notnull
    where T2 : notnull
    where T3 : notnull
    where T4 : notnull
{
    public void Invoke(Action<T1, T2, T3, T4> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        action(
            scope.ServiceProvider.GetRequiredService<T1>(),
            scope.ServiceProvider.GetRequiredService<T2>(),
            scope.ServiceProvider.GetRequiredService<T3>(),
            scope.ServiceProvider.GetRequiredService<T4>()
        );
    }

    public TResult Invoke<TResult>(Func<T1, T2, T3, T4, TResult> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        return action(
            scope.ServiceProvider.GetRequiredService<T1>(),
            scope.ServiceProvider.GetRequiredService<T2>(),
            scope.ServiceProvider.GetRequiredService<T3>(),
            scope.ServiceProvider.GetRequiredService<T4>()
        );
    }

    public async Task Invoke(Func<T1, T2, T3, T4, CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                cancellationToken
            )
           .ConfigureAwait(false);
    }

    public async Task<TResult> Invoke<TResult>(Func<T1, T2, T3, T4, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        return await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                cancellationToken
            )
           .ConfigureAwait(false);
    }

    public Task Invoke(Func<T1, T2, T3, T4, Task> action) => Invoke((a, b, c, d, _) => action(a, b, c, d), CancellationToken.None);

    public Task<TResult> Invoke<TResult>(Func<T1, T2, T3, T4, Task<TResult>> action) => Invoke((a, b, c, d, _) => action(a, b, c, d), CancellationToken.None);
}

internal class ExecuteScoped<T1, T2, T3, T4, T5>(IServiceScopeFactory serviceScopeFactoryFactory) : IExecuteScoped<T1, T2, T3, T4, T5>
    where T1 : notnull
    where T2 : notnull
    where T3 : notnull
    where T4 : notnull
    where T5 : notnull
{
    public void Invoke(Action<T1, T2, T3, T4, T5> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
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
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
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
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                scope.ServiceProvider.GetRequiredService<T5>(),
                cancellationToken
            )
           .ConfigureAwait(false);
    }

    public async Task<TResult> Invoke<TResult>(Func<T1, T2, T3, T4, T5, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        return await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                scope.ServiceProvider.GetRequiredService<T5>(),
                cancellationToken
            )
           .ConfigureAwait(false);
    }

    public Task Invoke(Func<T1, T2, T3, T4, T5, Task> action) => Invoke((a, b, c, d, e, _) => action(a, b, c, d, e), CancellationToken.None);

    public Task<TResult> Invoke<TResult>(Func<T1, T2, T3, T4, T5, Task<TResult>> action) => Invoke((a, b, c, d, e, _) => action(a, b, c, d, e), CancellationToken.None);
}

internal class ExecuteScoped<T1, T2, T3, T4, T5, T6>(IServiceScopeFactory serviceScopeFactoryFactory) : IExecuteScoped<T1, T2, T3, T4, T5, T6>
    where T1 : notnull
    where T2 : notnull
    where T3 : notnull
    where T4 : notnull
    where T5 : notnull
    where T6 : notnull
{
    public void Invoke(Action<T1, T2, T3, T4, T5, T6> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
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
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
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
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                scope.ServiceProvider.GetRequiredService<T5>(),
                scope.ServiceProvider.GetRequiredService<T6>(),
                cancellationToken
            )
           .ConfigureAwait(false);
    }

    public async Task<TResult> Invoke<TResult>(Func<T1, T2, T3, T4, T5, T6, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var scope = serviceScopeFactoryFactory.CreateScope();
        return await action(
                scope.ServiceProvider.GetRequiredService<T1>(),
                scope.ServiceProvider.GetRequiredService<T2>(),
                scope.ServiceProvider.GetRequiredService<T3>(),
                scope.ServiceProvider.GetRequiredService<T4>(),
                scope.ServiceProvider.GetRequiredService<T5>(),
                scope.ServiceProvider.GetRequiredService<T6>(),
                cancellationToken
            )
           .ConfigureAwait(false);
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
        ) => action(
            a,
            b,
            c,
            d,
            e,
            f
        ),
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
        ) => action(
            a,
            b,
            c,
            d,
            e,
            f
        ),
        CancellationToken.None
    );
}
