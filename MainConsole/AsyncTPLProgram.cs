using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MainConsole
{
    public class AsyncTPLProgram
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Start Async Code 1");
            Console.WriteLine("UI thread: {0}",Thread.CurrentThread.ManagedThreadId);

            var task = new AsyncSample().StartWork();
            task.Wait();

            Console.WriteLine("Start Async Code 2");
            Console.WriteLine("UI thread: {0}", Thread.CurrentThread.ManagedThreadId);

            var task2 = new AsyncSample().StartWork2();
            task2.Wait();
        }

        private class AsyncSample
        {
            public async Task StartWork()
            {
                await DoWork()
                    .ContinueWith(a => DoWork1(a.Result), TaskContinuationOptions.OnlyOnRanToCompletion)
                    .ContinueWith(b => DoWork2(b.Result.Result), TaskContinuationOptions.OnlyOnRanToCompletion)
                    .ContinueWith(c => DoWork3(c.Result.Result));
            }

            public async Task StartWork2()
            {
                var d = await DoWork();
                var d1 = await DoWork1(d);
                var d2 = await DoWork2(d1);
                DoWork3(d2);
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

            private async Task<IEnumerable<DoneWork>> DoWork1(DoneWork pastWork)
            {
                return await Task.Run(() =>
                {
                    var start = DateTime.Now;
                    for (int i = 0; i < 5000000; i++)
                    {

                    }

                    return new List<DoneWork> { pastWork, Done("DoWork1", start) };
                });
            }
            private async Task<IEnumerable<DoneWork>> DoWork2(IEnumerable<DoneWork> pastWorks)
            {
                return await Task.Run(() => new List<DoneWork>(pastWorks) {Done("DoWork2", DateTime.Now)});
            }

            private void DoWork3(IEnumerable<DoneWork> pastWorks)
            {
                foreach (var pastWork in pastWorks)
                {
                    Console.WriteLine("{0} {1} {2} {3}", pastWork.Name, pastWork.Start.ToString("ss.fff"), pastWork.Finish.ToString("ss.fff"), pastWork.ThreadId);
                }
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
}
