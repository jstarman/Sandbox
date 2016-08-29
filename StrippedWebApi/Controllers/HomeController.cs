using System.Web.Http;

namespace WebApplication2.Controllers
{
    [RoutePrefix("")]
    public class HomeController : ApiController
    {
        [Route("{id}")]
        public IHttpActionResult Get(string id)
        {
            return Ok("hello world");
        }
    }
}
