using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Reactive.Testing;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Tests.Observables
{
    public class RealThrottleTests : LoggerTest
    {
        private readonly TestScheduler _scheduler;

        public RealThrottleTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper, LogLevel.Information)
        {
            _scheduler = new TestScheduler();
        }

        [Fact]
        public void Should_Throttle_On_Leading_Edge()
        {
            var input = " a-b-c-d-|";
            var output = "a---c----|";
            var observable = _scheduler.CreateHotObservable(input);

            var receiver = _scheduler.CreateObserver<char>();
            observable.RealThrottle(TimeSpan.FromTicks(20), scheduler: _scheduler).Subscribe(receiver);
            _scheduler.Start();

            receiver.GetMarbles().Should().Be(output);
        }

        [Fact]
        public void Should_Throttle_On_Trailing_Edge()
        {
            var input = " a-b-c-d-|";
            var output = "--b--c--d-|";
            var observable = _scheduler.CreateHotObservable(input);

            var receiver = _scheduler.CreateObserver<char>();
            observable.RealThrottle(TimeSpan.FromTicks(20), leading: false, trailing: true, scheduler: _scheduler)
               .Subscribe(receiver);
            _scheduler.Start();

            receiver.GetMarbles().Should().Be(output);
        }

        [Fact]
        public void Should_Ignore_Overlap()
        {
            var input = " a-b-c-d-|";
            var output = "a-b--c--d-|";
            var observable = _scheduler.CreateHotObservable(input);

            var receiver = _scheduler.CreateObserver<char>();
            observable.RealThrottle(TimeSpan.FromTicks(5), leading: false, trailing: true, scheduler: _scheduler)
               .Subscribe(receiver);
            _scheduler.Start();

            receiver.GetMarbles().Should().Be(output);
        }

        [Fact]
        public void Should_Throttle_On_Both_Edges()
        {
            var input = " a-b-c-d-|";
            var output = "a----c---d|";
            var observable = _scheduler.CreateHotObservable(input);

            var receiver = _scheduler.CreateObserver<char>();
            observable.RealThrottle(TimeSpan.FromTicks(40), leading: true, trailing: true, scheduler: _scheduler)
               .Subscribe(receiver);
            _scheduler.Start();

            receiver.GetMarbles().Should().Be(output);
        }
    }
}