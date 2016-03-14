using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Util;
using Newtonsoft.Json;

namespace MainConsole
{
    public class LoggingProgram
    {
        private static void Main(string[] args)
        {
            var logger = LogManager.GetLogger("FileWatcherProgram");
            logger.Info("Hello World");
            try
            {
                throw new Exception("Testing how the logging works.");
            }
            catch (Exception e)
            {
                logger.Error("Some message", e);
            }
            var errors = LogManager.GetRepository().ConfigurationMessages.Count; //.Cast<LogLog>();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }

    public class HttpFireForgetAppender : AppenderSkeleton
    {
        private static readonly MediaTypeWithQualityHeaderValue JsonMediaTypeHeader = new MediaTypeWithQualityHeaderValue("application/json");

        protected override void Append(LoggingEvent loggingEvent)
        {
            Task.Factory.StartNew(() =>
            {
                var message = new LogMessage
                {
                    LoggerName = loggingEvent.LoggerName,
                    ExceptionString = loggingEvent.GetExceptionString(),
                    Level = loggingEvent.Level.Name,
                    Message = loggingEvent.RenderedMessage,
                    ThreadName = loggingEvent.ThreadName,
                    TimeStamp = loggingEvent.TimeStamp,
                    UserName = loggingEvent.UserName
                };

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(JsonMediaTypeHeader);
                    var t = client.PostAsync(Url, new StringContent(JsonConvert.SerializeObject(message)));
                    t.Wait();
                }
            });
        }

        private class LogMessage
        {
            public string LoggerName { get; set; }
            public string Level { get; set; }
            public string Message { get; set; }

            public string ThreadName { get; set; }
            public DateTime TimeStamp { get; set; }
            public string UserName { get; set; }
            public string ExceptionString { get; set; }
        }

        public string Url { get; set; }
    }
}
