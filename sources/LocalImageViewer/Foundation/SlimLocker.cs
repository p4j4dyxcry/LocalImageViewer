using System;
using System.Threading;

namespace LocalImageViewer.Foundation
{
public enum LockType
{
    Read,
    UpgradeableRead,
    Write
}

public class SlimLocker
{
    private readonly ReaderWriterLockSlim _readerWriterLock = new();
    private const int TimeoutMsDefault = 10000;

    public IDisposable Lock(LockType lockType, int timeOutMs)
    {
        return lockType switch
        {
            LockType.Read => ReadLock(timeOutMs),
            LockType.UpgradeableRead => UpgradeableReadLock(timeOutMs),
            LockType.Write => WriteLock(timeOutMs),
            _ => throw new ArgumentOutOfRangeException(nameof(lockType))
        };
    }

    public IDisposable ReadLock(int timeOutMs = TimeoutMsDefault)
    {
        return new DisposableLock(_readerWriterLock.TryEnterReadLock,
            _readerWriterLock.ExitReadLock,
            timeOutMs);
    }

    public IDisposable UpgradeableReadLock(int timeOutMs = TimeoutMsDefault)
    {
        return new DisposableLock(_readerWriterLock.TryEnterUpgradeableReadLock,
            _readerWriterLock.ExitUpgradeableReadLock,
            timeOutMs);
    }

    public IDisposable WriteLock(int timeOutMs = TimeoutMsDefault)
    {
        return new DisposableLock(_readerWriterLock.TryEnterWriteLock,
            _readerWriterLock.ExitWriteLock,
            timeOutMs);
    }

    private class DisposableLock : IDisposable
    {
        private readonly Action _unLockAction;

        public DisposableLock(Func<int, bool> lockAction, Action unLockAction, int timeoutMs)
        {
            _unLockAction = unLockAction;
            lockAction(timeoutMs);
        }

        public void Dispose()
        {
            _unLockAction();
        }
    }
}    
}
