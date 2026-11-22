namespace SlahManager.Infrastructure;

public sealed class SingleInstanceGuard : IDisposable
{
    private readonly Mutex? _mutex;
    private readonly string _signalName;
    private EventWaitHandle? _signal;
    private volatile bool _disposed;

    private SingleInstanceGuard(Mutex? mutex, bool isFirst, string key)
    {
        _mutex = mutex;
        IsFirst = isFirst;
        _signalName = $@"Local\slahManager.{key}.show";
    }

    public bool IsFirst { get; }

    public static SingleInstanceGuard Acquire(string key)
    {
        try
        {
            var mutex = new Mutex(true, $@"Local\slahManager.{key}", out var createdNew);
            return new SingleInstanceGuard(mutex, createdNew, key);
        }
        catch (UnauthorizedAccessException)
        {
            return new SingleInstanceGuard(null, false, key);
        }
    }

    public void SignalExisting()
    {
        try
        {
            using var handle = EventWaitHandle.OpenExisting(_signalName);
            handle.Set();
        }
        catch (Exception exception)
        {
            Logger.Error("Could not signal the running instance", exception);
        }
    }

    public void ListenForSignal(Action onSignal)
    {
        _signal = new EventWaitHandle(false, EventResetMode.AutoReset, _signalName);

        var listener = new Thread(() =>
        {
            while (!_disposed)
            {
                if (_signal.WaitOne(500) && !_disposed)
                {
                    onSignal();
                }
            }
        })
        {
            IsBackground = true,
            Name = "slahManager.InstanceListener"
        };

        listener.Start();
    }

    public void Dispose()
    {
        _disposed = true;

        if (IsFirst)
        {
            try
            {
                _mutex?.ReleaseMutex();
            }
            catch (ApplicationException)
            {
            }
        }

        _mutex?.Dispose();
    }
}
