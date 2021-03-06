﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoonApiLib.Helper
{
    public class AsyncLock
    {
        public struct Releaser : IDisposable
        {
            private readonly AsyncLock _toRelease;

            internal Releaser(AsyncLock toRelease) { _toRelease = toRelease; }

            public void Dispose()
            {
                if (_toRelease != null)
                    _toRelease._semaphore.Release();
            }
        }

        private readonly AsyncSemaphore _semaphore;
        private readonly Task<Releaser> _releaser;
        public AsyncLock()
        {
            _semaphore = new AsyncSemaphore(1);
            _releaser = Task.FromResult(new Releaser(this));
        }
        public Task<Releaser> LockAsync()
        {
            var wait = _semaphore.WaitAsync();
            return wait.IsCompleted ?
                _releaser :
                wait.ContinueWith((_, state) => new Releaser((AsyncLock)state),
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
    }
}
