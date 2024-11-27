using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.DependencyInjection;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable IDE0058, RCS1021, IDE0053, CS1998

namespace Rocket.Surgery.Extensions.Tests.DependencyInjection;

public class ExecuteScopedTests : LoggerTest<XUnitTestContext>
{
    [Fact]
    public async Task Work_With_One_Service()
    {
        var executor = _serviceProvider.WithScoped<Service1>();
        executor.Invoke(s => s.ScopedValue).Value.Should().Be(0);
        executor.Invoke(s => { s.ScopedValue.Value.Should().Be(1); });
        ( await executor.Invoke(async s => s.ScopedValue.Value).ConfigureAwait(false) ).Should().Be(2);
        await executor.Invoke(async s => { s.ScopedValue.Value.Should().Be(3); }).ConfigureAwait(false);
    }

    [Fact]
    public async Task Work_With_Two_Services()
    {
        var executor = _serviceProvider.WithScoped<Service1, Service2>();
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

    [Fact]
    public async Task Work_With_Three_Services()
    {
        var executor = _serviceProvider.WithScoped<Service1, Service2, Service3>();
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

    [Fact]
    public async Task Work_With_Four_Services()
    {
        var executor = _serviceProvider.WithScoped<Service1, Service2, Service3, Service4>();
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

    [Fact]
    public async Task Work_With_Five_Services()
    {
        var executor = _serviceProvider.WithScoped<Service1, Service2, Service3, Service4, Service5>();
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

    [Fact]
    public async Task Work_With_Six_Services()
    {
        var executor = _serviceProvider.WithScoped<Service1, Service2, Service3, Service4, Service5, Service6>();
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

    public ExecuteScopedTests(ITestOutputHelper outputHelper) : base(XUnitTestContext.Create(outputHelper))
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

    private readonly IServiceProvider _serviceProvider;

    [Theory]
    [InlineData(typeof(IExecuteScoped<Service1>), typeof(ExecuteScoped<Service1>))]
    [InlineData(typeof(IExecuteScoped<Service1, Service2>), typeof(ExecuteScoped<Service1, Service2>))]
    [InlineData(typeof(IExecuteScoped<Service1, Service2, Service3>), typeof(ExecuteScoped<Service1, Service2, Service3>))]
    [InlineData(typeof(IExecuteScoped<Service1, Service2, Service3, Service4>), typeof(ExecuteScoped<Service1, Service2, Service3, Service4>))]
    [InlineData(
        typeof(IExecuteScoped<Service1, Service2, Service3, Service4, Service5>),
        typeof(ExecuteScoped<Service1, Service2, Service3, Service4, Service5>)
    )]
    [InlineData(
        typeof(IExecuteScoped<Service1, Service2, Service3, Service4, Service5, Service6>),
        typeof(ExecuteScoped<Service1, Service2, Service3, Service4, Service5, Service6>)
    )]
    public async Task Should_Resolve_ExecuteScoped(Type serviceType, Type implementationType) =>
        _serviceProvider.GetService(serviceType).Should().BeOfType(implementationType);

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
