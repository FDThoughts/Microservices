namespace eShop.Services.Session.API.V1.Controllers
{
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System.Net;
    using V1.Models;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiversion}/[controller]")]
    [DisableCors]
    public class SessionController : ControllerBase
    {
        [HttpGet("cart")]
        [ProducesResponseType(typeof(string),
            (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public virtual IActionResult GetCart() {
            return Ok(HttpContext.Session.GetString("cart"));
        }

        [HttpPost("cart")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public virtual IActionResult StoreCart([FromBody]
            ProductSelection[] products
        ) {
            var jsonData = JsonConvert
                .SerializeObject(products);
            HttpContext.Session.SetString("cart",
                jsonData);
            return CreatedAtAction(nameof(StoreCart), null);
        }
    }
}