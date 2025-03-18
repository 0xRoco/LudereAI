using System.Net;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LudereAI.API.Controllers;


[ApiController, Authorize(Roles = "Admin"), Route("[controller]")]
public class AuditsController : ControllerBase
{
    private readonly ILogger<AuditsController> _logger;
    private readonly IAuditService _auditService;

    public AuditsController(ILogger<AuditsController> logger, IAuditService auditService)
    {
        _logger = logger;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<APIResult<IEnumerable<AuditLog>>>> Get()
    {
        try
        {
            var logs = await _auditService.GetLogs();
            return Ok(APIResult<IEnumerable<AuditLog>>.Success(data: logs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs");
            return StatusCode(500, APIResult<List<AuditLog>>.Error(HttpStatusCode.InternalServerError, "Internal server error"));
        }
    }
}