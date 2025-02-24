﻿using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Rocket.Surgery.Extensions.Testing;

namespace Rocket.Surgery.Extensions.Tests.Observables;

public class DebounceTests() : LoggerTest(Defaults.LoggerTest)
{
    private readonly TestScheduler _scheduler = new();

    [Test]
    public void Should_Debounce_On_Leading_Edge()
    {
        var input = " a-b-c-d-|";
        var output = "a-------|";
        var observable = _scheduler.CreateHotObservable(input);

        var receiver = _scheduler.CreateObserver<char>();
        observable.Debounce(TimeSpan.FromTicks(20), true, false, _scheduler).Subscribe(receiver);
        _scheduler.Start();

        receiver.GetMarbles().ShouldBe(output);
    }

    [Test]
    public void Should_Debounce_With_Unit()
    {
        var input = " a-b-c-d-|";
        var output = "a-b--c--d--|";
        var observable = _scheduler.CreateHotObservable(input).ToSignal();

        var receiver = _scheduler.CreateObserver<char>();
        observable.Debounce(TimeSpan.FromTicks(15), true, true, _scheduler).Subscribe(receiver.CreateUnitObserver());
        _scheduler.Start();

        receiver.GetMarbles().ShouldBe(output);
    }

    [Test]
    public void Should_Debounce_On_Trailing_Edge()
    {
        var input = " a-b-c-d-|";
        var output = "--------d|";
        var observable = _scheduler.CreateHotObservable(input);

        var receiver = _scheduler.CreateObserver<char>();
        observable
           .Debounce(TimeSpan.FromTicks(20), false, true, _scheduler)
           .Subscribe(receiver);
        _scheduler.Start();

        receiver.GetMarbles().ShouldBe(output);
    }

    [Test]
    public void Should_Debounce_On_Both_Edges()
    {
        var input = " a-b-c-d-|";
        var output = "a-------d|";
        var observable = _scheduler.CreateHotObservable(input);

        var receiver = _scheduler.CreateObserver<char>();
        observable
           .Debounce(TimeSpan.FromTicks(30), true, true, _scheduler)
           .Subscribe(receiver);
        _scheduler.Start();

        receiver.GetMarbles().ShouldBe(output);
    }

    [Test]
    public void Should_Ignore_Overlap()
    {
        var input = " a-b-c-d-|";
        var output = "a-b--c--d--|";
        var observable = _scheduler.CreateHotObservable(input);

        var receiver = _scheduler.CreateObserver<char>();
        observable
           .Debounce(TimeSpan.FromTicks(15), true, true, _scheduler)
           .Subscribe(receiver);
        _scheduler.Start();

        receiver.GetMarbles().ShouldBe(output);
    }
}
