using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.Extensions.Testing;
using Serilog.Events;

#pragma warning disable IDE0058, RCS1021, IDE0053, CS1998
#pragma warning disable CS8602

namespace Rocket.Surgery.Extensions.Tests.DependencyInjection;

public class ExecuteScopedOptionalTests : LoggerTest
{
    [Test]
    public async Task Work_With_One_Service()
    {
        var executor = _serviceProvider.WithScopedOptional<Service1>();
        executor.Invoke(s => s.ScopedValue).Value.Should().Be(0);
        executor.Invoke(s => { s.ScopedValue.Value.Should().Be(1); });
        ( await executor.Invoke(async s => s.ScopedValue.Value).ConfigureAwait(false) ).Should().Be(2);
        await executor.Invoke(async s => { s.ScopedValue.Value.Should().Be(3); }).ConfigureAwait(false);
    }

    [Test]
    public async Task Work_With_Two_Services()
    {
        var executor = _serviceProvider.WithScopedOptional<Service1, Service2>();
        executor
           .Invoke(
                (s1, s2) =>
                {
                    s2.Service.Should().BeSameAs(s1);
                    return s2.Service.ScopedValue;
                }
            )
           .Value.Should()
           .Be(0);
        executor.Invoke(
            (s1, s2) =>
            {
                s2.Service.Should().BeSameAs(s1);
                s1.ScopedValue.Value.Should().Be(1);
            }
        );
        ( await executor
               .Invoke(
                    async (s1, s2) =>
                    {
                        s2.Service.Should().BeSameAs(s1);
                        return s1.ScopedValue.Value;
                    }
                )
               .ConfigureAwait(false) )
           .Should()
           .Be(2);
        await executor
             .Invoke(
                  async (s1, s2) =>
                  {
                      s2.Service.Should().BeSameAs(s1);
                      s1.ScopedValue.Value.Should().Be(3);
                  }
              )
             .ConfigureAwait(false);
    }

    [Test]
    public async Task Work_With_Three_Services()
    {
        var executor = _serviceProvider.WithScopedOptional<Service1, Service2, Service3>();
        executor
           .Invoke(
                (s1, s2, s3) =>
                {
                    s3.Service.Should().BeSameAs(s2);
                    s2.Service.Should().BeSameAs(s1);
                    return s2.Service.ScopedValue;
                }
            )
           .Value.Should()
           .Be(0);
        executor.Invoke(
            (s1, s2, s3) =>
            {
                s3.Service.Should().BeSameAs(s2);
                s2.Service.Should().BeSameAs(s1);
                s1.ScopedValue.Value.Should().Be(1);
            }
        );
        ( await executor
               .Invoke(
                    async (s1, s2, s3) =>
                    {
                        s3.Service.Should().BeSameAs(s2);
                        s2.Service.Should().BeSameAs(s1);
                        return s1.ScopedValue.Value;
                    }
                )
               .ConfigureAwait(false) )
           .Should()
           .Be(2);
        await executor
             .Invoke(
                  async (s1, s2, s3) =>
                  {
                      s3.Service.Should().BeSameAs(s2);
                      s2.Service.Should().BeSameAs(s1);
                      s1.ScopedValue.Value.Should().Be(3);
                  }
              )
             .ConfigureAwait(false);
    }

    [Test]
    public async Task Work_With_Four_Services()
    {
        var executor = _serviceProvider.WithScopedOptional<Service1, Service2, Service3, Service4>();
        executor
           .Invoke(
                (s1, s2, s3, s4) =>
                {
                    s4.Service.Should().BeSameAs(s3);
                    s3.Service.Should().BeSameAs(s2);
                    s2.Service.Should().BeSameAs(s1);
                    return s2.Service.ScopedValue;
                }
            )
           .Value.Should()
           .Be(0);
        executor.Invoke(
            (s1, s2, s3, s4) =>
            {
                s4.Service.Should().BeSameAs(s3);
                s3.Service.Should().BeSameAs(s2);
                s2.Service.Should().BeSameAs(s1);
                s1.ScopedValue.Value.Should().Be(1);
            }
        );
        ( await executor
               .Invoke(
                    async (s1, s2, s3, s4) =>
                    {
                        s4.Service.Should().BeSameAs(s3);
                        s3.Service.Should().BeSameAs(s2);
                        s2.Service.Should().BeSameAs(s1);
                        return s1.ScopedValue.Value;
                    }
                )
               .ConfigureAwait(false) )
           .Should()
           .Be(2);
        await executor
             .Invoke(
                  async (s1, s2, s3, s4) =>
                  {
                      s4.Service.Should().BeSameAs(s3);
                      s3.Service.Should().BeSameAs(s2);
                      s2.Service.Should().BeSameAs(s1);
                      s1.ScopedValue.Value.Should().Be(3);
                  }
              )
             .ConfigureAwait(false);
    }

    [Test]
    public async Task Work_With_Five_Services()
    {
        var executor = _serviceProvider.WithScopedOptional<Service1, Service2, Service3, Service4, Service5>();
        executor
           .Invoke(
                (s1, s2, s3, s4, s5) =>
                {
                    s5.Service.Should().BeSameAs(s4);
                    s4.Service.Should().BeSameAs(s3);
                    s3.Service.Should().BeSameAs(s2);
                    s2.Service.Should().BeSameAs(s1);
                    return s2.Service.ScopedValue;
                }
            )
           .Value.Should()
           .Be(0);
        executor.Invoke(
            (s1, s2, s3, s4, s5) =>
            {
                s5.Service.Should().BeSameAs(s4);
                s4.Service.Should().BeSameAs(s3);
                s3.Service.Should().BeSameAs(s2);
                s2.Service.Should().BeSameAs(s1);
                s1.ScopedValue.Value.Should().Be(1);
            }
        );
        ( await executor
               .Invoke(
                    async (s1, s2, s3, s4, s5) =>
                    {
                        s5.Service.Should().BeSameAs(s4);
                        s4.Service.Should().BeSameAs(s3);
                        s3.Service.Should().BeSameAs(s2);
                        s2.Service.Should().BeSameAs(s1);
                        return s1.ScopedValue.Value;
                    }
                )
               .ConfigureAwait(false) )
           .Should()
           .Be(2);
        await executor
             .Invoke(
                  async (s1, s2, s3, s4, s5) =>
                  {
                      s5.Service.Should().BeSameAs(s4);
                      s4.Service.Should().BeSameAs(s3);
                      s3.Service.Should().BeSameAs(s2);
                      s2.Service.Should().BeSameAs(s1);
                      s1.ScopedValue.Value.Should().Be(3);
                  }
              )
             .ConfigureAwait(false);
    }

    [Test]
    public async Task Work_With_Six_Services()
    {
        var executor = _serviceProvider.WithScopedOptional<Service1, Service2, Service3, Service4, Service5, Service6>();
        executor
           .Invoke(
                (s1, s2, s3, s4, s5, s6) =>
                {
                    s6.Service.Should().BeSameAs(s5);
                    s5.Service.Should().BeSameAs(s4);
                    s4.Service.Should().BeSameAs(s3);
                    s3.Service.Should().BeSameAs(s2);
                    s2.Service.Should().BeSameAs(s1);
                    return s2.Service.ScopedValue;
                }
            )
           .Value.Should()
           .Be(0);
        executor.Invoke(
            (s1, s2, s3, s4, s5, s6) =>
            {
                s6.Service.Should().BeSameAs(s5);
                s5.Service.Should().BeSameAs(s4);
                s4.Service.Should().BeSameAs(s3);
                s3.Service.Should().BeSameAs(s2);
                s2.Service.Should().BeSameAs(s1);
                s1.ScopedValue.Value.Should().Be(1);
            }
        );
        ( await executor
               .Invoke(
                    async (s1, s2, s3, s4, s5, s6) =>
                    {
                        s6.Service.Should().BeSameAs(s5);
                        s5.Service.Should().BeSameAs(s4);
                        s4.Service.Should().BeSameAs(s3);
                        s3.Service.Should().BeSameAs(s2);
                        s2.Service.Should().BeSameAs(s1);
                        return s1.ScopedValue.Value;
                    }
                )
               .ConfigureAwait(false) )
           .Should()
           .Be(2);
        await executor
             .Invoke(
                  async (s1, s2, s3, s4, s5, s6) =>
                  {
                      s6.Service.Should().BeSameAs(s5);
                      s5.Service.Should().BeSameAs(s4);
                      s4.Service.Should().BeSameAs(s3);
                      s3.Service.Should().BeSameAs(s2);
                      s2.Service.Should().BeSameAs(s1);
                      s1.ScopedValue.Value.Should().Be(3);
                  }
              )
             .ConfigureAwait(false);
    }

    private int _value;
    private readonly IServiceProvider _serviceProvider;

    [Test]
    [MethodDataSource(nameof(ExecuteScopedOptionalTypes))]
    public async Task Should_Resolve_ExecuteScopedOptional(Type serviceType, Type implementationType) =>
        _serviceProvider.GetService(serviceType).Should().BeOfType(implementationType);


    public static IEnumerable<Func<(Type serviceType, Type implementationType)>> ExecuteScopedOptionalTypes()
    {
        var interfaces = typeof(IExecuteScopedOptional<>)
                        .Assembly.GetExportedTypes()
                        .Where(
                             z => z is
                             {
                                 IsGenericTypeDefinition: true,
                                 Name: ['I', 'E', 'x', 'e', 'c', 'u', 't', 'e', 'S', 'c', 'o', 'p', 'e', 'd', 'O', 'p', 't', 'i', 'o', 'n', 'a', 'l', ..]
                             }
                         );
        var implementations = typeof(IExecuteScopedOptional<>)
                             .Assembly.GetExportedTypes()
                             .Where(
                                  z => z is
                                  {
                                      IsGenericTypeDefinition: true,
                                      Name: ['E', 'x', 'e', 'c', 'u', 't', 'e', 'S', 'c', 'o', 'p', 'e', 'd', 'O', 'p', 't', 'i', 'o', 'n', 'a', 'l', ..]
                                  }
                              );

        List<Type> services = [typeof(Service1), typeof(Service2), typeof(Service3), typeof(Service4), typeof(Service5), typeof(Service6),];

        foreach (var (serviceType, implementationType) in interfaces.Join(
                     implementations,
                     z => z.Name.Substring(1),
                     z => z.Name,
                     (serviceType, implementationType) => ( serviceType, implementationType )
                 ))
        {
            yield return () => (
                             serviceType.MakeGenericType(services.Take(serviceType.GetGenericArguments().Length).ToArray()),
                             implementationType.MakeGenericType(services.Take(implementationType.GetGenericArguments().Length).ToArray())
                         );
        }

        yield return () => (
                         typeof(IExecuteScopedOptional<Service1>),
                         typeof(ExecuteScopedOptional<Service1>)
                     );
    }

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
