using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Crypton.WebAPI.Controllers;

public sealed class ErrorController : ControllerBase
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("/error")]
    public IActionResult Error([FromServices] IHostEnvironment env)
    {
        if (!env.IsDevelopment())
            return Problem();

        var error =
            HttpContext.Features.Get<IExceptionHandlerFeature>()!.Error;

        return Problem(error.StackTrace, error.Message);
    }
}