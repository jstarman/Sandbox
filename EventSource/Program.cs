using EventStore.ClientAPI;
using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace EventSource
{
    class Program
    {
        static void Main(string[] args)
        {
            var connection =
            EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));

            // Don't forget to tell the connection to connect!
            connection.ConnectAsync().Wait();

            var myEvent = Helper.ToEvent("purchasedItemCreated", new PurchasedItem { Id = 1, CreatedDate = DateTime.Now, Uri = "XY123ABC" });

            connection.AppendToStreamAsync("purchaseditems", ExpectedVersion.NoStream, myEvent).Wait();

            var streamEvents =
                connection.ReadStreamEventsForwardAsync("newstream", 0, 10, false).Result;

            Helper.Print(streamEvents.Events);

            //Console.WriteLine("Read event with data: {0}, metadata: {1}",
            //    Helper.ToString(returnedEvent.Data),
            //    Helper.ToString(returnedEvent.Metadata));
        }


    }

    public static class Helper
    {
        public static void Print(ResolvedEvent[] events)
        {
            foreach (var e in events)
            {
                Console.WriteLine($"EventId {e.Event.EventId}");
                Console.WriteLine($"Event Number {e.Event.EventNumber}");
                Console.WriteLine($"Event StreamId {e.Event.EventStreamId}");
                Console.WriteLine($"Event Type {e.Event.EventType}");
                Console.WriteLine($"Event Created {e.Event.Created}");
                Console.WriteLine("Event Data");
                Console.WriteLine(ToString(e.Event.Data));
                Console.WriteLine("Event Meta");
                Console.WriteLine(ToString(e.Event.Metadata));
            }
        }

        public static EventData ToEvent(string eventName, object payload, object metaData = null)
        {
            return new EventData(Guid.NewGuid(), eventName, true,
                                        ToBytes(payload),
                                        ToBytes(metaData));
        }

        public static string ToString(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        public static byte[] ToBytes(object data)
        {
            if (data  == null) return null;

            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
        }
    }
    
    public class PurchasedItem
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Uri { get; set; }
    }
}