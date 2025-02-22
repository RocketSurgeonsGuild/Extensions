using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Rocket.Surgery.Extensions.Testing;

namespace Rocket.Surgery.Extensions.Tests.Observables;

public class RealThrottleTests() : LoggerTest(Defaults.LoggerTest)
{
    private readonly TestScheduler _scheduler = new();

    [Test]
    public void Should_Throttle_On_Leading_Edge()
    {
        var input = " a-b-c-d-|";
        var output = "a---c----|";
        var observable = _scheduler.CreateHotObservable(input);

        var receiver = _scheduler.CreateObserver<char>();
        observable.RealThrottle(TimeSpan.FromTicks(20), scheduler: _scheduler).Subscribe(receiver);
        _scheduler.Start();

        receiver.GetMarbles().ShouldBe(output);
    }

    [Test]
    public void Should_Throttle_With_Unit()
    {
        var input = " a-b-c-d-|";
        var output = "a-b--c--d-|";
        var observable = _scheduler.CreateHotObservable(input).ToSignal();

        var receiver = _scheduler.CreateObserver<char>();
        observable.RealThrottle(TimeSpan.FromTicks(5), false, true, _scheduler).Subscribe(receiver.CreateUnitObserver());
        _scheduler.Start();

        receiver.GetMarbles().ShouldBe(output);
    }

    [Test]
    public void Should_Throttle_On_Trailing_Edge()
    {
        var input = " a-b-c-d-|";
        var output = "--b--c--d-|";
        var observable = _scheduler.CreateHotObservable(input);

        var receiver = _scheduler.CreateObserver<char>();
        observable
           .RealThrottle(TimeSpan.FromTicks(20), false, true, _scheduler)
           .Subscribe(receiver);
        _scheduler.Start();

        receiver.GetMarbles().ShouldBe(output);
    }

    [Test]
    public void Should_Ignore_Overlap()
    {
        var input = " a-b-c-d-|";
        var output = "a-b--c--d-|";
        var observable = _scheduler.CreateHotObservable(input);

        var receiver = _scheduler.CreateObserver<char>();
        observable
           .RealThrottle(TimeSpan.FromTicks(5), false, true, _scheduler)
           .Subscribe(receiver);
        _scheduler.Start();

        receiver.GetMarbles().ShouldBe(output);
    }

    [Test]
    public void Should_Throttle_On_Both_Edges()
    {
        var input = " a-b-c-d-|";
        var output = "a----c---d|";
        var observable = _scheduler.CreateHotObservable(input);

        var receiver = _scheduler.CreateObserver<char>();
        observable
           .RealThrottle(TimeSpan.FromTicks(40), true, true, _scheduler)
           .Subscribe(receiver);
        _scheduler.Start();

        receiver.GetMarbles().ShouldBe(output);
    }
}
