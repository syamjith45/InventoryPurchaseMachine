using InventoryPurchaseAPI.DTOs;
using InventoryPurchaseAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryPurchaseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoriesController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoriesController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// GET /api/inventories - Get all available inventory lots (non-expired)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<InventoryResponseDto>>> GetInventory()
        {
            try
            {
                var inventory = await _inventoryService.GetInventoryAsync();

                var responseDtos = inventory.Select(i => new InventoryResponseDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? string.Empty,
                    LotNumber = i.LotNumber,
                    ExpiryDate = i.ExpiryDate,
                    Quantity = i.Quantity
                }).ToList();

                return Ok(responseDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving inventory.", detail = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/inventories/{productId} - Get inventory lots for a specific product (non-expired)
        /// </summary>
        [HttpGet("{productId}")]
        public async Task<ActionResult<List<InventoryResponseDto>>> GetInventoryByProduct(int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    return BadRequest("Invalid product ID.");
                }

                var inventory = await _inventoryService.GetInventoryByProductAsync(productId);

                var responseDtos = inventory.Select(i => new InventoryResponseDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? string.Empty,
                    LotNumber = i.LotNumber,
                    ExpiryDate = i.ExpiryDate,
                    Quantity = i.Quantity
                }).ToList();

                return Ok(responseDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving inventory for the product.", detail = ex.Message });
            }
        }
    }
}
