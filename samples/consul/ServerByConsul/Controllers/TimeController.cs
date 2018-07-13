using Microsoft.AspNetCore.Mvc;
using System;

namespace ServerByConsul.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TimeController : Controller
    {
        [HttpGet]
        public ActionResult<DateTime> Get()
        {
            return Ok(DateTime.Now);
        }
    }
}