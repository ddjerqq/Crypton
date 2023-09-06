using Crypton.Application.Caching;
using Crypton.Application.Common.Interfaces;
using Crypton.Application.Dtos;
using Crypton.Application.Inventory.Commands;
using Crypton.Domain.Entities;
using Crypton.Domain.ValueObjects;
using Crypton.Infrastructure.Idempotency;
using Crypton.WebAPI.Common.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace Crypton.WebAPI.Controllers;

[Authorize]
public sealed class InventoryController : ApiController
{
    private readonly IAppDbContext _dbContext;

    public InventoryController(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// See all available item types
    /// </summary>
    [AllowAnonymous]
    [HttpGet("types")]
    [OutputCache(PolicyName = CacheConstants.AllItemTypesPolicyName)]
    [ProducesResponseType<ItemType>(StatusCodes.Status200OK)]
    public async Task<IActionResult> SeeAllAvailableItemTypes(CancellationToken ct)
    {
        var types = await _dbContext.Set<ItemType>().ToListAsync(ct);
        return Ok(types);
    }

    /// <summary>
    /// Create an item type
    /// </summary>
    [HttpPost("types/create")]
    [ProducesResponseType<ItemType>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateItemType(
        [FromBody, BindRequired] CreateItemTypeCommand command,
        CancellationToken ct)
    {
        var types = await _dbContext.Set<ItemType>().ToListAsync(ct);
        return Ok(types);
    }

    /// <summary>
    /// Buy an item
    /// </summary>
    [RequireIdempotency]
    [HttpPost("buy")]
    [ProducesResponseType<ItemDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Buy(
        [FromBody, BindRequired] BuyItemCommand command,
        CancellationToken ct)
    {
        var item = await HandleCommandAsync<BuyItemCommand, Item>(command, ct);
        return Ok((ItemDto)item);
    }

    /// <summary>
    /// Send an item to another user
    /// </summary>
    [RequireIdempotency]
    [HttpPost("send")]
    public async Task<IActionResult> SendToUser(
        [FromBody, BindRequired] SendItemTransactionCommand command,
        CancellationToken ct)
    {
        await HandleCommandAsync(command, ct);
        return Ok();
    }

    /// <summary>
    /// Sell item
    /// </summary>
    [AllowAnonymous]
    [RequireIdempotency]
    [HttpPost("sell")]
    public async Task<IActionResult> Sell([FromBody, BindRequired] SellItemCommand command)
    {
        var soldFor = await HandleCommandAsync<SellItemCommand, decimal>(command);
        return Ok(soldFor);
    }
}