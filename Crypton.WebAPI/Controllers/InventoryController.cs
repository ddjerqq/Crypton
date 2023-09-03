using Crypton.Application.Common.Interfaces;
using Crypton.Application.Dtos;
using Crypton.Application.Inventory;
using Crypton.Application.Inventory.Commands;
using Crypton.Domain.Entities;
using Crypton.Domain.ValueObjects;
using Crypton.Infrastructure.Idempotency;
using Crypton.WebAPI.Common.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Crypton.WebAPI.Controllers;

[Authorize]
public sealed class InventoryController : ApiController
{
    private readonly IAppDbContext _dbContext;

    public InventoryController(IAppDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    /// <summary>
    /// See all available item types
    /// </summary>
    [HttpGet("types")]
    public async Task<IActionResult> SeeAllAvailableItemTypes(CancellationToken ct)
    {
        var types = await this._dbContext.Set<ItemType>().ToListAsync(ct);
        return this.Ok(types);
    }

    /// <summary>
    /// Buy an item
    /// </summary>
    [RequireIdempotency]
    [HttpPost("buy")]
    [ProducesResponseType<ItemDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Buy([FromBody, BindRequired] BuyItemCommand command, CancellationToken ct)
    {
        var item = await this.HandleCommandAsync<BuyItemCommand, Item>(command, ct);
        return this.Ok((ItemDto)item);
    }

    // /// <summary>
    // /// Use an item
    // /// </summary>
    // [AllowAnonymous]
    // [RequireIdempotency]
    // [HttpPost("use")]
    // public async Task<IActionResult> Use([FromBody, BindRequired] UseItemCommand command)
    // {
    //     // return this.Created();
    //     // return this.BadRequest(result.Errors);
    //     throw new NotImplementedException();
    // }

    // /// <summary>
    // /// Sell item
    // /// </summary>
    // [AllowAnonymous]
    // [RequireIdempotency]
    // [HttpPost("sell")]
    // public async Task<IActionResult> Sell([FromBody, BindRequired] SellItemCommand command)
    // {
    //     // return this.Created();
    //     // return this.BadRequest(result.Errors);
    //     throw new NotImplementedException();
    // }

    // /// <summary>
    // /// Sell all junk
    // /// </summary>
    // [AllowAnonymous]
    // [RequireIdempotency]
    // [HttpPost("sell_junk")]
    // public async Task<IActionResult> SellJunk()
    // {
    //     // return this.Created();
    //     // return this.BadRequest(result.Errors);
    //     var command = new SellJunkItemsCommand();
    //     throw new NotImplementedException();
    // }
}