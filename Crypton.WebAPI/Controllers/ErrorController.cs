using Microsoft.AspNetCore.Mvc;

namespace Crypton.WebAPI.Controllers;

public sealed class ErrorController : ControllerBase
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("/error")]
    public IActionResult Error() => this.Problem();
}