using System.Net;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LudereAI.API.Core;

public class RequireFeatureAttribute : ActionFilterAttribute
{
    private readonly string _featurePath;

    public RequireFeatureAttribute(string featurePath)
    {
        _featurePath = featurePath;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var featureFlagsService = context.HttpContext.RequestServices
            .GetRequiredService<IFeatureFlagsService>();

        if (!featureFlagsService.IsFeatureEnabled(_featurePath))
        {
            context.Result = new JsonResult(APIResult<string>.Error(
                HttpStatusCode.ServiceUnavailable, 
                "This feature is currently disabled"))
            {
                StatusCode = (int)HttpStatusCode.ServiceUnavailable
            };
        }
    }
}