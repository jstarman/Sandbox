using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net.Core;

namespace MainConsole
{
    public class AsyncProgram
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Start Async Code");
            Console.WriteLine("UI thread: {0}",Thread.CurrentThread.ManagedThreadId);
            var tasks = new AsyncSample().StartWork();
            //Task.WhenAny(tasks);
            Console.WriteLine("Blocking...");
            tasks.ToList().ForEach(t => Console.WriteLine("{0} {1} {2} {3}", t.Result.Name, t.Result.Start.ToString("ss.fff"), t.Result.Finish.ToString("ss.fff"), t.Result.ThreadId));
            //Console.WriteLine("Press enter to exit");
            //Console.ReadLine();
        }

        private class AsyncSample
        {
            private readonly List<Func<Task<DoneWork>>> _workList;
            private readonly AsyncLocker m_lock = new AsyncLocker();

            public AsyncSample()
            {
                _workList = new List<Func<Task<DoneWork>>>
                {
                    DoWork,
                    DoWork1,
                    DoWork2
                };
            }

            public IEnumerable<Task<DoneWork>> StartWork()
            {
               return _workList.Select(async t =>
               {
                   using (var releaser = await m_lock.LockAsync())
                   {
                       return await t();
                   }
               });
            }

            private async Task<DoneWork> DoWork()
            {
                return await Task.Run(() =>
                {
                    var start = DateTime.Now;
                    for (int i = 0; i < 1000000000; i++)
                    {
                        
                    }
                    return Done("DoWork", start);
                });
            }

            private async Task<DoneWork> DoWork1()
            {
                return await Task.Run(() =>
                {
                    var start = DateTime.Now;
                    for (int i = 0; i < 5000000; i++)
                    {

                    }
                    return Done("DoWork1", start);
                });
            }
            private async Task<DoneWork> DoWork2()
            {
                return await Task.Run(() => Done("DoWork2", DateTime.Now));
            }

            private DoneWork Done(string name, DateTime time)
            {
                return new DoneWork { Finish = DateTime.Now, Name = name, ThreadId = Thread.CurrentThread.ManagedThreadId, Start = time};
            }
        }

        private class DoneWork
        {
            public string Name { get; set; }
            public DateTime Finish { get; set; }
            public int ThreadId { get; set; }
            public DateTime Start { get; set; }
        }
    }

    public class AsyncLock
    {
        public AsyncLock()
        {
            m_semaphore = new AsyncSemaphore(1);
            m_releaser = Task.FromResult(new Releaser(this));
        }

        private readonly AsyncSemaphore m_semaphore;
        private readonly Task<Releaser> m_releaser;

        public Task<Releaser> LockAsync()
        {
            var wait = m_semaphore.WaitAsync();
            return wait.IsCompleted ?
                m_releaser :
                wait.ContinueWith((_, state) => new Releaser((AsyncLock)state),
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public struct Releaser : IDisposable
        {
            private readonly AsyncLock m_toRelease;

            internal Releaser(AsyncLock toRelease) { m_toRelease = toRelease; }

            public void Dispose()
            {
                if (m_toRelease != null)
                    m_toRelease.m_semaphore.Release();
            }
        }

        public class AsyncSemaphore
        {
            private readonly static Task s_completed = Task.FromResult(true);
            private readonly Queue<TaskCompletionSource<bool>> m_waiters = new Queue<TaskCompletionSource<bool>>();
            private int m_currentCount;
            public AsyncSemaphore(int initialCount)
            {
                if (initialCount < 0) throw new ArgumentOutOfRangeException("initialCount");
                m_currentCount = initialCount;
            }

            public Task WaitAsync()
            {
                lock (m_waiters)
                {
                    if (m_currentCount > 0)
                    {
                        --m_currentCount;
                        return s_completed;
                    }
                    else
                    {
                        var waiter = new TaskCompletionSource<bool>();
                        m_waiters.Enqueue(waiter);
                        return waiter.Task;
                    }
                }
            }

            public void Release()
            {
                TaskCompletionSource<bool> toRelease = null;
                lock (m_waiters)
                {
                    if (m_waiters.Count > 0)
                        toRelease = m_waiters.Dequeue();
                    else
                        ++m_currentCount;
                }
                if (toRelease != null)
                    toRelease.SetResult(true);
            }
        }
    }

    public sealed class AsyncLocker
    {
        private readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1, 1);
        private readonly Task<IDisposable> m_releaser;

        public AsyncLocker()
        {
            m_releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = m_semaphore.WaitAsync();
            return wait.IsCompleted ?
                        m_releaser :
                        wait.ContinueWith((_, state) => (IDisposable)state,
                            m_releaser.Result, CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLocker m_toRelease;
            internal Releaser(AsyncLocker toRelease) { m_toRelease = toRelease; }
            public void Dispose() { m_toRelease.m_semaphore.Release(); }
        }
    }
}
