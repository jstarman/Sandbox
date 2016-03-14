using System;
using System.IO;
using System.Text;
using Owin;
using Nancy;
using Nancy.Owin;
namespace OwinApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseNancy(options => options.PassThroughWhenStatusCodesAre(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError));
        }
    }

    public class StartupBare
    {
        public void Configuration(IAppBuilder app)
        {
            app.Run(context =>
            {
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                {
                    Console.WriteLine(reader.ReadToEnd());
                }
                
                context.Response.ContentType = "text/plain";
                return context.Response.WriteAsync("OK");
            });
        }
    }
}
