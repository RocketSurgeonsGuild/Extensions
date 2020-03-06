using System;
using System.Reactive.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Reactive.Testing;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Tests.Observables
{
    public class DebounceTests : LoggerTest
    {
        private readonly TestScheduler _scheduler;

        public DebounceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper, LogLevel.Information)
        {
            _scheduler = new TestScheduler();
        }

        [Fact]
        public void Should_Debounce_On_Leading_Edge()
        {
            var input = " a-b-c-d-|";
            var output = "a-------|";
            var observable = _scheduler.CreateHotObservable(input);

            var receiver = _scheduler.CreateObserver<char>();
            observable.Debounce(TimeSpan.FromTicks(20), leading: true, trailing: false, scheduler: _scheduler).Subscribe(receiver);
            _scheduler.Start();

            receiver.GetMarbles().Should().Be(output);
        }

        [Fact]
        public void Should_Debounce_With_Unit()
        {
            var input = " a-b-c-d-|";
            var output = "a-b--c--d--|";
            var observable = _scheduler.CreateHotObservable(input).ToSignal();

            var receiver = _scheduler.CreateObserver<char>();
            observable.Debounce(TimeSpan.FromTicks(15), leading: true, trailing: true, scheduler: _scheduler).Subscribe(receiver.CreateUnitObserver());
            _scheduler.Start();

            receiver.GetMarbles().Should().Be(output);
        }

        [Fact]
        public void Should_Debounce_On_Trailing_Edge()
        {
            var input = " a-b-c-d-|";
            var output = "--------d|";
            var observable = _scheduler.CreateHotObservable(input);

            var receiver = _scheduler.CreateObserver<char>();
            observable.Debounce(TimeSpan.FromTicks(20), leading: false, trailing: true, scheduler: _scheduler)
               .Subscribe(receiver);
            _scheduler.Start();

            receiver.GetMarbles().Should().Be(output);
        }

        [Fact]
        public void Should_Debounce_On_Both_Edges()
        {
            var input = " a-b-c-d-|";
            var output = "a-------d|";
            var observable = _scheduler.CreateHotObservable(input);

            var receiver = _scheduler.CreateObserver<char>();
            observable.Debounce(TimeSpan.FromTicks(30), leading: true, trailing: true, scheduler: _scheduler)
               .Subscribe(receiver);
            _scheduler.Start();

            receiver.GetMarbles().Should().Be(output);
        }

        [Fact]
        public void Should_Ignore_Overlap()
        {
            var input = " a-b-c-d-|";
            var output = "a-b--c--d--|";
            var observable = _scheduler.CreateHotObservable(input);

            var receiver = _scheduler.CreateObserver<char>();
            observable.Debounce(TimeSpan.FromTicks(15), leading: true, trailing: true, scheduler: _scheduler)
               .Subscribe(receiver);
            _scheduler.Start();

            receiver.GetMarbles().Should().Be(output);
        }
    }
}