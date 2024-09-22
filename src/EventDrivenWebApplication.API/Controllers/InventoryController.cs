using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventDrivenWebApplication.Api.Controllers;

/// <summary>
/// Controller for managing inventory items.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryController"/> class.
    /// </summary>
    /// <param name="inventoryService">The inventory service to be used by the controller.</param>
    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>
    /// Retrieves all inventory items.
    /// </summary>
    /// <returns>A list of inventory items.</returns>
    /// <response code="200">Returns a list of inventory items.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItem>>> GetInventoryItems()
    {
        try
        {
            IEnumerable<InventoryItem> items = await _inventoryService.GetInventoryItemsAsync(HttpContext.RequestAborted);
            return Ok(items);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a single inventory item by its ID.
    /// </summary>
    /// <param name="id">The ID of the inventory item to retrieve.</param>
    /// <returns>The requested inventory item or a 404 Not Found if the item does not exist.</returns>
    /// <response code="200">Returns the requested inventory item.</response>
    /// <response code="404">If the inventory item with the specified ID is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryItem>> GetInventoryItem(int id)
    {
        try
        {
            InventoryItem? item = await _inventoryService.GetInventoryItemUsingProductIdAsync(id, HttpContext.RequestAborted);
            return Ok(item);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Adds a new inventory item.
    /// </summary>
    /// <param name="inventoryItem">The inventory item to add.</param>
    /// <returns>A 201 Created response with the location of the newly created inventory item.</returns>
    /// <response code="201">Returns the newly created inventory item.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost]
    public async Task<ActionResult<InventoryItem>> AddInventoryItem([FromBody] InventoryItem inventoryItem)
    {
        try
        {
            await _inventoryService.AddInventoryItemAsync(inventoryItem, HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetInventoryItem), new { id = inventoryItem.Id }, inventoryItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing inventory item.
    /// </summary>
    /// <param name="id">The ID of the inventory item to update.</param>
    /// <param name="inventoryItem">The updated inventory item data.</param>
    /// <returns>A 204 No Content response if the update is successful, or a 404 Not Found if the item does not exist.</returns>
    /// <response code="204">If the update is successful.</response>
    /// <response code="400">If the inventory item ID in the request body does not match the ID in the URL.</response>
    /// <response code="404">If the inventory item with the specified ID is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInventoryItem(int id, [FromBody] InventoryItem inventoryItem)
    {
        if (id != inventoryItem.Id)
        {
            return BadRequest("Inventory item ID mismatch.");
        }

        try
        {
            await _inventoryService.UpdateInventoryAsync(id, inventoryItem.QualityValidated, HttpContext.RequestAborted);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Marks an inventory item as checked.
    /// </summary>
    /// <param name="id">The ID of the inventory item to mark as checked.</param>
    /// <returns>A 204 No Content response if the operation is successful, or a 404 Not Found if the item does not exist.</returns>
    /// <response code="204">If the item is successfully marked as checked.</response>
    /// <response code="404">If the inventory item with the specified ID is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPatch("{id}/markchecked")]
    public async Task<IActionResult> MarkItemChecked(int id)
    {
        try
        {
            await _inventoryService.MarkItemCheckedAsync(id, HttpContext.RequestAborted);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Deletes an inventory item by its ID.
    /// </summary>
    /// <param name="id">The ID of the inventory item to delete.</param>
    /// <returns>A 204 No Content response if the deletion is successful, or a 404 Not Found if the item does not exist.</returns>
    /// <response code="204">If the item is successfully deleted.</response>
    /// <response code="404">If the inventory item with the specified ID is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInventoryItem(int id)
    {
        try
        {
            await _inventoryService.DeleteInventoryItemAsync(id, HttpContext.RequestAborted);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
