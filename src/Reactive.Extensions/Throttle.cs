using System.Reactive.Concurrency;

// ReSharper disable once CheckNamespace
namespace System.Reactive.Linq;

internal class Throttle<T> : ObserverBase<T>, IObserver<Throttle<T>>
{
    private readonly object _gate = new object();
    private T _value;
    private bool _hasValue;
    private readonly bool _leading;
    private readonly bool _trailing;
    private readonly IScheduler _scheduler;
    private IDisposable? _serialCancelable;
    private ulong _id;
    private readonly IObservable<Throttle<T>> _notifier;
    private readonly IObserver<T> _destination;

    public Throttle(
        IObserver<T> destination,
        IObservable<Unit> notifier,
        bool leading,
        bool trailing,
        IScheduler scheduler
    )
    {
        _leading = leading;
        _trailing = trailing;
        _scheduler = scheduler;
        _notifier = notifier.SubscribeOn(_scheduler).ObserveOn(_scheduler).Select(_ => this);
        _destination = destination;
        _value = default!;
    }

    protected override void OnCompletedCore()
    {
        if (_trailing)
        {
            Done();
        }

        _destination.OnCompleted();
    }

    protected override void OnErrorCore(Exception error)
    {
        _destination.OnError(error);
    }

    protected override void OnNextCore(T value)
    {
        lock (_gate)
        {
            _hasValue = true;
            _value = value;
            _id = unchecked(_id + 1);
        }

        if (_serialCancelable == null)
        {
            if (_leading)
            {
                send();
            }
            else
            {
                throttle();
            }
        }
    }

    private void Done()
    {
        lock (_gate)
        {
            if (_serialCancelable != null)
            {
                _serialCancelable.Dispose();
                _serialCancelable = null;
            }
        }

        if (_trailing)
        {
            send();
        }
    }

    private void throttle()
    {
        lock (_gate)
        {
            if (_serialCancelable == null)
            {
                _serialCancelable = _notifier
                                   .ObserveOn(_scheduler)
                                   .SubscribeSafe(this);
            }
        }
    }

    private void send()
    {
        if (_hasValue)
        {
            _destination.OnNext(_value);
            throttle();
        }

        _hasValue = false;
        _value = default!;
    }

    void IObserver<Throttle<T>>.OnCompleted()
    {
        Done();
    }

    void IObserver<Throttle<T>>.OnError(Exception error)
    {
        OnError(error);
    }

    void IObserver<Throttle<T>>.OnNext(Throttle<T> value)
    {
        Done();
    }
}
