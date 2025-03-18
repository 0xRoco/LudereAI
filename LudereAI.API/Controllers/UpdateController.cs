using System.Net;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.API.Controllers;

[ApiController, Route("[controller]")]

public class UpdateController : ControllerBase
{
    private ILogger<UpdateController> _logger;
    private readonly string _updateInfoPath;

    public UpdateController(ILogger<UpdateController> logger)
    {
        _logger = logger;
        _updateInfoPath = Path.Combine(AppContext.BaseDirectory, "update-info.json");
    }

    [HttpGet]
    public async Task<ActionResult<APIResult<UpdateInfoDTO>>> Get()
    {
        if (!System.IO.File.Exists(_updateInfoPath))
            return Ok(APIResult<UpdateInfoDTO>.Error(HttpStatusCode.NotFound, "Update info not found"));

        var updateInfo = (await System.IO.File.ReadAllTextAsync(_updateInfoPath)).FromJson<UpdateInfoDTO>();
        
        return Ok(APIResult<UpdateInfoDTO>.Success(data: updateInfo));
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<APIResult<bool>>> SetUpdateInfo([FromBody] UpdateInfoDTO updateInfo)
    {
        try
        {
            await System.IO.File.WriteAllTextAsync(_updateInfoPath, updateInfo.ToJson());
            return Ok(APIResult<bool>.Success(data: true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write update info to file");
            return BadRequest(APIResult<bool>.Error(HttpStatusCode.InternalServerError, ex.Message));
        }
    }
}