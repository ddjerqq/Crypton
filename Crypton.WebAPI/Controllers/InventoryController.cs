// using Crypton.Infrastructure.Idempotency;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.ModelBinding;
//
// namespace Crypton.WebAPI.Controllers;
//
// [Authorize]
// [ApiController]
// [Produces("application/json")]
// [Route("/api/v1/[controller]")]
// public sealed class InventoryController : ControllerBase
// {
//     /// <summary>
//     /// Buy an item
//     /// </summary>
//     /// <response code="201">Success</response>
//     /// <response code="400">Insufficient funds</response>
//     [AllowAnonymous]
//     [RequireIdempotency]
//     [HttpPost("buy")]
//     public async Task<IActionResult> Buy([FromBody, BindRequired] BuyItemCommand command)
//     {
//         // return this.Created();
//         // return this.BadRequest(result.Errors);
//         throw new NotImplementedException();
//     }
//
//     /// <summary>
//     /// Use an item
//     /// </summary>
//     /// <response code="200">Success</response>
//     /// <response code="400">User does not own the item</response>
//     [AllowAnonymous]
//     [RequireIdempotency]
//     [HttpPost("use")]
//     public async Task<IActionResult> Use([FromBody, BindRequired] UseItemCommand command)
//     {
//         // return this.Created();
//         // return this.BadRequest(result.Errors);
//         throw new NotImplementedException();
//     }
//
//     /// <summary>
//     /// Sell item
//     /// </summary>
//     /// <response code="200">Success</response>
//     /// <response code="400">User does not own the item</response>
//     [AllowAnonymous]
//     [RequireIdempotency]
//     [HttpPost("sell")]
//     public async Task<IActionResult> Sell([FromBody, BindRequired] SellItemCommand command)
//     {
//         // return this.Created();
//         // return this.BadRequest(result.Errors);
//         throw new NotImplementedException();
//     }
//
//     /// <summary>
//     /// Sell all junk
//     /// </summary>
//     /// <response code="200">Success</response>
//     /// <response code="400">User does not own the item</response>
//     [AllowAnonymous]
//     [RequireIdempotency]
//     [HttpPost("sell_junk")]
//     public async Task<IActionResult> SellJunk([FromBody, BindRequired] SellJunkItemsCommand command)
//     {
//         // return this.Created();
//         // return this.BadRequest(result.Errors);
//         throw new NotImplementedException();
//     }
// }