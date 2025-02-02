using LudereAI.Shared;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.API.Controllers;


[ApiController, Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<APIResult<bool>> Get()
    {
        return Ok(APIResult<bool>.Success(message: "Healthy", data: true));
    }
}