using System;
using System.Linq;
using System.Runtime.InteropServices;
using Nancy.Diagnostics;
using Owin;
//using Nancy;
//using Nancy.Owin;
namespace OwinApp
{
    //public class Startup
    //{
    //    public void Configuration(IAppBuilder app)
    //    {
    //        app.UseNancy(options => options.PassThroughWhenStatusCodesAre(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError));
    //    }
    //}

    public class StartupBare
    {
        public void Configuration(IAppBuilder app)
        {
            app.Run(context =>
            {
                //using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                //{
                Console.WriteLine(DateTime.Now);
                Console.WriteLine(context.Request.Uri.AbsoluteUri);
                var coll = context.Request.Headers.GetEnumerator();
                while(coll.MoveNext())
                {
                    var kvp = coll.Current;
                   
                    Console.WriteLine($"{kvp.Key} : {kvp.Value.GetValue(0)}");
                }
                

                context.Response.ContentType = "application/json";

                if (context.Request.Uri.AbsoluteUri.Contains("fibr"))
                {
                    context.Response.StatusCode = (int) System.Net.HttpStatusCode.OK;
                    var response = @"{""ProcessingStatus"": ""Unknown""}";
                    Console.WriteLine($"Responded with: {response}");
                    Console.WriteLine("============================");
                    return context.Response.WriteAsync(response);
                }

                context.Response.StatusCode =  (int)System.Net.HttpStatusCode.OK;
                var responseClient = @"{""ProcessingStatus"": ""Unknown""}";
                Console.WriteLine($"Responded with: {responseClient}");
                Console.WriteLine("============================");
                return context.Response.WriteAsync(responseClient);
            });
        }
    }
}
