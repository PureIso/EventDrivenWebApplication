using Asp.Versioning;
using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventDrivenWebApplication.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryController>? _logger;

    public InventoryController(IInventoryService inventoryService, ILogger<InventoryController>? logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetInventoryItems()
    {
        IEnumerable<InventoryItem> inventoryItems = await _inventoryService.GetInventoryItemsAsync();
        return Ok(inventoryItems);
    }
}