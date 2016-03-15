using System;
using System.Collections.Generic;
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
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var asyncSample = new AsyncSample();
            Console.WriteLine(asyncSample.RunOrder);
            var tasks = asyncSample.StartWork();
            Console.WriteLine(asyncSample.RunOrder);
            Task.WhenAny(tasks);
            Console.WriteLine(asyncSample.RunOrder);
            Console.WriteLine("Waiting...");
            tasks.ToList().ForEach(t => Console.WriteLine("{0} {1} {2}", t.Result.Name, t.Result.WorkTime.ToString("mm.fff"), t.Result.ThreadId));
            Console.WriteLine(asyncSample.RunOrder);
            //Console.WriteLine("Press enter to exit");
            //Console.ReadLine();
        }

        private class AsyncSample
        {
            private readonly List<Func<Task<DoneWork>>> _workList;
            private readonly AsyncLock m_lock = new AsyncLock();
            public string RunOrder { get; set; }

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
                    for (int i = 0; i < 1000000000; i++)
                    {
                        
                    }
                    return Done("DoWork");
                });
            }

            private async Task<DoneWork> DoWork1()
            {
                return await Task.Run(() =>
                {
                    for (int i = 0; i < 1000000; i++)
                    {

                    }
                    return Done("DoWork1");
                });
            }
            private async Task<DoneWork> DoWork2()
            {
                return await Task.Run(() => Done("DoWork2"));
            }

            private DoneWork Done(string name)
            {
                return new DoneWork { WorkTime = DateTime.Now, Name = name, ThreadId = Thread.CurrentThread.ManagedThreadId };
            }
        }

        private class DoneWork
        {
            public string Name { get; set; }
            public DateTime WorkTime { get; set; }
            public int ThreadId { get; set; }
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
}
