using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.Extensions.Testing;

#pragma warning disable IDE0058, RCS1021, IDE0053, CS1998

namespace Rocket.Surgery.Extensions.Tests.DependencyInjection;

public class ExecuteScopedTests : LoggerTest
{
    public static IEnumerable<Func<(Type serviceType, Type implementationType)>> ExecuteScopedTypes()
    {
        var interfaces = typeof(IExecuteScopedOptional<>)
                        .Assembly.GetExportedTypes()
                        .Where(
                             z => z is
                             {
                                 IsGenericTypeDefinition: true,
                                 Name: ['I', 'E', 'x', 'e', 'c', 'u', 't', 'e', 'S', 'c', 'o', 'p', 'e', 'd', ..],
                             }
                         );
        var implementations = typeof(IExecuteScopedOptional<>)
                             .Assembly.GetExportedTypes()
                             .Where(
                                  z => z is
                                  {
                                      IsGenericTypeDefinition: true,
                                      Name: ['E', 'x', 'e', 'c', 'u', 't', 'e', 'S', 'c', 'o', 'p', 'e', 'd', ..],
                                  }
                              );

        List<Type> services = [typeof(Service1), typeof(Service2), typeof(Service3), typeof(Service4), typeof(Service5), typeof(Service6)];

        foreach ((var serviceType, var implementationType) in interfaces.Join(
                     implementations,
                     z => z.Name.Substring(1),
                     z => z.Name,
                     (serviceType, implementationType) => (serviceType, implementationType)
                 ))
        {
            yield return () => (
                             serviceType.MakeGenericType(services.Take(serviceType.GetGenericArguments().Length).ToArray()),
                             implementationType.MakeGenericType(services.Take(implementationType.GetGenericArguments().Length).ToArray())
                         );
        }

        yield return () => (
                         typeof(IExecuteScoped<Service1>),
                         typeof(ExecuteScoped<Service1>)
                     );
    }

    private readonly IServiceProvider _serviceProvider;

    public ExecuteScopedTests() : base(Defaults.LoggerTest)
    {
        var value = 0;
        _serviceProvider = new ServiceCollection()
                          .AddExecuteScopedServices()
                          .AddScoped(_ => new ScopedValue(value++))
                          .AddScoped<Service1>()
                          .AddScoped<Service2>()
                          .AddScoped<Service3>()
                          .AddScoped<Service4>()
                          .AddScoped<Service5>()
                          .AddScoped<Service6>()
                          .BuildServiceProvider();
    }

    [Test]
    public async Task Work_With_One_Service()
    {
        var executor = _serviceProvider.WithScoped<Service1>();
        executor.Invoke(s => s.ScopedValue).Value.ShouldBe(0);
        executor.Invoke(s => { s.ScopedValue.Value.ShouldBe(1); });
        (await executor.Invoke(async s => s.ScopedValue.Value).ConfigureAwait(false)).ShouldBe(2);
        await executor.Invoke(async s => { s.ScopedValue.Value.ShouldBe(3); }).ConfigureAwait(false);
    }

    [Test]
    public async Task Work_With_Two_Services()
    {
        var executor = _serviceProvider.WithScoped<Service1, Service2>();
        executor
           .Invoke(
                (s1, s2) =>
                {
                    s2.Service.ShouldBeSameAs(s1);
                    return s2.Service.ScopedValue;
                }
            )
           .Value.ShouldBe(0);
        executor.Invoke(
            (s1, s2) =>
            {
                s2.Service.ShouldBeSameAs(s1);
                s1.ScopedValue.Value.ShouldBe(1);
            }
        );
        (await executor
               .Invoke(
                    async (s1, s2) =>
                    {
                        s2.Service.ShouldBeSameAs(s1);
                        return s1.ScopedValue.Value;
                    }
                )
               .ConfigureAwait(false))
           .ShouldBe(2);
        await executor
             .Invoke(
                  async (s1, s2) =>
                  {
                      s2.Service.ShouldBeSameAs(s1);
                      s1.ScopedValue.Value.ShouldBe(3);
                  }
              )
             .ConfigureAwait(false);
    }

    [Test]
    public async Task Work_With_Three_Services()
    {
        var executor = _serviceProvider.WithScoped<Service1, Service2, Service3>();
        executor
           .Invoke(
                (s1, s2, s3) =>
                {
                    s3.Service.ShouldBeSameAs(s2);
                    s2.Service.ShouldBeSameAs(s1);
                    return s2.Service.ScopedValue;
                }
            )
           .Value.ShouldBe(0);
        executor.Invoke(
            (s1, s2, s3) =>
            {
                s3.Service.ShouldBeSameAs(s2);
                s2.Service.ShouldBeSameAs(s1);
                s1.ScopedValue.Value.ShouldBe(1);
            }
        );
        (await executor
               .Invoke(
                    async (s1, s2, s3) =>
                    {
                        s3.Service.ShouldBeSameAs(s2);
                        s2.Service.ShouldBeSameAs(s1);
                        return s1.ScopedValue.Value;
                    }
                )
               .ConfigureAwait(false))
           .ShouldBe(2);
        await executor
             .Invoke(
                  async (s1, s2, s3) =>
                  {
                      s3.Service.ShouldBeSameAs(s2);
                      s2.Service.ShouldBeSameAs(s1);
                      s1.ScopedValue.Value.ShouldBe(3);
                  }
              )
             .ConfigureAwait(false);
    }

    [Test]
    public async Task Work_With_Four_Services()
    {
        var executor = _serviceProvider.WithScoped<Service1, Service2, Service3, Service4>();
        executor
           .Invoke(
                (s1, s2, s3, s4) =>
                {
                    s4.Service.ShouldBeSameAs(s3);
                    s3.Service.ShouldBeSameAs(s2);
                    s2.Service.ShouldBeSameAs(s1);
                    return s2.Service.ScopedValue;
                }
            )
           .Value.ShouldBe(0);
        executor.Invoke(
            (s1, s2, s3, s4) =>
            {
                s4.Service.ShouldBeSameAs(s3);
                s3.Service.ShouldBeSameAs(s2);
                s2.Service.ShouldBeSameAs(s1);
                s1.ScopedValue.Value.ShouldBe(1);
            }
        );
        (await executor
               .Invoke(
                    async (s1, s2, s3, s4) =>
                    {
                        s4.Service.ShouldBeSameAs(s3);
                        s3.Service.ShouldBeSameAs(s2);
                        s2.Service.ShouldBeSameAs(s1);
                        return s1.ScopedValue.Value;
                    }
                )
               .ConfigureAwait(false))
           .ShouldBe(2);
        await executor
             .Invoke(
                  async (s1, s2, s3, s4) =>
                  {
                      s4.Service.ShouldBeSameAs(s3);
                      s3.Service.ShouldBeSameAs(s2);
                      s2.Service.ShouldBeSameAs(s1);
                      s1.ScopedValue.Value.ShouldBe(3);
                  }
              )
             .ConfigureAwait(false);
    }

    [Test]
    public async Task Work_With_Five_Services()
    {
        var executor = _serviceProvider.WithScoped<Service1, Service2, Service3, Service4, Service5>();
        executor
           .Invoke(
                (s1, s2, s3, s4, s5) =>
                {
                    s5.Service.ShouldBeSameAs(s4);
                    s4.Service.ShouldBeSameAs(s3);
                    s3.Service.ShouldBeSameAs(s2);
                    s2.Service.ShouldBeSameAs(s1);
                    return s2.Service.ScopedValue;
                }
            )
           .Value.ShouldBe(0);
        executor.Invoke(
            (s1, s2, s3, s4, s5) =>
            {
                s5.Service.ShouldBeSameAs(s4);
                s4.Service.ShouldBeSameAs(s3);
                s3.Service.ShouldBeSameAs(s2);
                s2.Service.ShouldBeSameAs(s1);
                s1.ScopedValue.Value.ShouldBe(1);
            }
        );
        (await executor
               .Invoke(
                    async (s1, s2, s3, s4, s5) =>
                    {
                        s5.Service.ShouldBeSameAs(s4);
                        s4.Service.ShouldBeSameAs(s3);
                        s3.Service.ShouldBeSameAs(s2);
                        s2.Service.ShouldBeSameAs(s1);
                        return s1.ScopedValue.Value;
                    }
                )
               .ConfigureAwait(false))
           .ShouldBe(2);
        await executor
             .Invoke(
                  async (s1, s2, s3, s4, s5) =>
                  {
                      s5.Service.ShouldBeSameAs(s4);
                      s4.Service.ShouldBeSameAs(s3);
                      s3.Service.ShouldBeSameAs(s2);
                      s2.Service.ShouldBeSameAs(s1);
                      s1.ScopedValue.Value.ShouldBe(3);
                  }
              )
             .ConfigureAwait(false);
    }

    [Test]
    public async Task Work_With_Six_Services()
    {
        var executor = _serviceProvider.WithScoped<Service1, Service2, Service3, Service4, Service5, Service6>();
        executor
           .Invoke(
                (s1, s2, s3, s4, s5, s6) =>
                {
                    s6.Service.ShouldBeSameAs(s5);
                    s5.Service.ShouldBeSameAs(s4);
                    s4.Service.ShouldBeSameAs(s3);
                    s3.Service.ShouldBeSameAs(s2);
                    s2.Service.ShouldBeSameAs(s1);
                    return s2.Service.ScopedValue;
                }
            )
           .Value.ShouldBe(0);
        executor.Invoke(
            (s1, s2, s3, s4, s5, s6) =>
            {
                s6.Service.ShouldBeSameAs(s5);
                s5.Service.ShouldBeSameAs(s4);
                s4.Service.ShouldBeSameAs(s3);
                s3.Service.ShouldBeSameAs(s2);
                s2.Service.ShouldBeSameAs(s1);
                s1.ScopedValue.Value.ShouldBe(1);
            }
        );
        (await executor
               .Invoke(
                    async (s1, s2, s3, s4, s5, s6) =>
                    {
                        s6.Service.ShouldBeSameAs(s5);
                        s5.Service.ShouldBeSameAs(s4);
                        s4.Service.ShouldBeSameAs(s3);
                        s3.Service.ShouldBeSameAs(s2);
                        s2.Service.ShouldBeSameAs(s1);
                        return s1.ScopedValue.Value;
                    }
                )
               .ConfigureAwait(false))
           .ShouldBe(2);
        await executor
             .Invoke(
                  async (s1, s2, s3, s4, s5, s6) =>
                  {
                      s6.Service.ShouldBeSameAs(s5);
                      s5.Service.ShouldBeSameAs(s4);
                      s4.Service.ShouldBeSameAs(s3);
                      s3.Service.ShouldBeSameAs(s2);
                      s2.Service.ShouldBeSameAs(s1);
                      s1.ScopedValue.Value.ShouldBe(3);
                  }
              )
             .ConfigureAwait(false);
    }

    [Test]
    [MethodDataSource(nameof(ExecuteScopedTypes))]
    public async Task Should_Resolve_ExecuteScoped(Type serviceType, Type implementationType) =>
        _serviceProvider.GetService(serviceType).ShouldBeOfType(implementationType);

    private class ScopedValue
    {
        public ScopedValue(int value) => Value = value;

        public int Value { get; }
    }

    private class Service1
    {
        public Service1(ScopedValue scopedValue) => ScopedValue = scopedValue;

        public ScopedValue ScopedValue { get; }
    }

    private class Service2
    {
        public Service2(Service1 service) => Service = service;

        public Service1 Service { get; }
    }

    private class Service3
    {
        public Service3(Service2 service) => Service = service;

        public Service2 Service { get; }
    }

    private class Service4
    {
        public Service4(Service3 service) => Service = service;

        public Service3 Service { get; }
    }

    private class Service5
    {
        public Service5(Service4 service) => Service = service;

        public Service4 Service { get; }
    }

    private class Service6
    {
        public Service6(Service5 service) => Service = service;

        public Service5 Service { get; }
    }
}
