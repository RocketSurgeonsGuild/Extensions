using System.Reactive;
using System.Text;
using Microsoft.Reactive.Testing;

namespace Rocket.Surgery.Extensions.Tests.Observables;

internal static class Helpers
{
    public static IObservable<char> CreateColdObservable(this TestScheduler scheduler, string marbles)
    {
        var marbleList = FromMarbles(marbles).ToArray();
        return scheduler.CreateColdObservable(marbleList);
    }

    public static IObservable<char> CreateHotObservable(this TestScheduler scheduler, string marbles)
    {
        var marbleList = FromMarbles(marbles).ToArray();
        return scheduler.CreateHotObservable(marbleList);
    }

    public static string GetMarbles(this ITestableObserver<char> observer)
    {
        return ToMarbles(observer.Messages);
    }

    public static IObserver<Unit> CreateUnitObserver(this IObserver<char> observer)
    {
        return new UnitObserver(observer);
    }

    private class UnitObserver : IObserver<Unit>
    {
        private readonly IObserver<char> _observer;
        private char _next;

        public UnitObserver(IObserver<char> observer)
        {
            _observer = observer;
            _next = 'a';
        }

        public void OnCompleted()
        {
            _observer.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _observer.OnError(error);
        }

        public void OnNext(Unit value)
        {
            _observer.OnNext(_next++);
        }
    }

    private static IEnumerable<Recorded<Notification<char>>> FromMarbles(string marbles)
    {
        var time = 0L;
        var syncEvents = false;
        foreach (var character in marbles)
        {
            if (character == ' ')
            {
                continue;
            }

            if (character == '(')
            {
                syncEvents = true;
                continue;
            }

            if (character == ')')
            {
                syncEvents = false;
                continue;
            }

            var marbleEvents = character switch
            {
                '#' => EnumerableEx.Return(ReactiveTest.OnError<char>(time, new Exception("end"))),
                '|' => EnumerableEx.Return(ReactiveTest.OnCompleted(time, character)),
                '-' => Enumerable.Empty<Recorded<Notification<char>>>(),
                _   => EnumerableEx.Return(ReactiveTest.OnNext(time, character)),
            };

            foreach (var marble in marbleEvents)
                yield return marble;

            if (syncEvents)
                continue;

            time += 10;
        }
    }

    private static string ToMarbles(IEnumerable<Recorded<Notification<char>>> events)
    {
        var lastTime = 0L;
        var sb = new StringBuilder();
        foreach (var e in events)
        {
            var ticksSince = (int)( e.Time - lastTime ) / 10;
            if (ticksSince > 0)
            {
                sb.Append(string.Join("", Enumerable.Range(0, ticksSince).Select(_ => '-')));
            }

            lastTime = e.Time;
            switch (e.Value.Kind)
            {
                case NotificationKind.OnNext:
                    sb.Append(e.Value.Value);
                    break;
                case NotificationKind.OnError:
                    sb.Append('#');
                    break;
                case NotificationKind.OnCompleted:
                    sb.Append('|');
                    break;
            }
        }

        return sb.ToString();
    }
}
