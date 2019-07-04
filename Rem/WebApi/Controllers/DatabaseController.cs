using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rem.Models;

namespace Rem.WebApi.Controllers
{
    [Route("api/database")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private BotContext _botContext;

        public DatabaseController(BotContext botContext)
        {
            _botContext = botContext;
        }

        // GET api/values
        [HttpGet]
        [Route("images")]
        public ActionResult<IEnumerable<Image>> Get()
        {
            return _botContext.Images.ToList();
        }

        /*
        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
        */
    }
}