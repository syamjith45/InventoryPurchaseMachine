using InventoryPurchaseAPI.DTOs;
using InventoryPurchaseAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryPurchaseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;

        public PurchasesController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        /// <summary>
        /// GET /api/purchases - Get all purchases
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<PurchaseResponseDto>>> GetAllPurchases()
        {
            try
            {
                var purchases = await _purchaseService.GetAllPurchasesAsync();

                var responseDtos = purchases.Select(p => MapToResponseDto(p)).ToList();

                return Ok(responseDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving purchases.", detail = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/purchases/{id} - Get purchase by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PurchaseResponseDto>> GetPurchaseById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid purchase ID.");
                }

                var purchase = await _purchaseService.GetPurchaseByIdAsync(id);

                if (purchase == null)
                {
                    return NotFound();
                }

                var responseDto = MapToResponseDto(purchase);

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the purchase.", detail = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/purchases - Create a new purchase in Draft status
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PurchaseResponseDto>> CreatePurchase([FromBody] PurchaseCreateDto dto)
        {
            try
            {
                var purchase = await _purchaseService.CreatePurchaseAsync(dto);

                // Map to response DTO
                var responseDto = MapToResponseDto(purchase);

                return CreatedAtAction(nameof(CreatePurchase), new { id = purchase.Id }, responseDto);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the purchase.", detail = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/purchases/{id}/post - Post a purchase and update inventory
        /// </summary>
        [HttpPost("{id}/post")]
        public async Task<IActionResult> PostPurchase(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid purchase ID.");
                }

                var result = await _purchaseService.PostPurchaseAsync(id);

                if (!result)
                {
                    return BadRequest("Failed to post purchase.");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while posting the purchase.", detail = ex.Message });
            }
        }

        private PurchaseResponseDto MapToResponseDto(Models.Purchase purchase)
        {
            var items = purchase.PurchaseItems.Select(pi => new PurchaseItemResponseDto
            {
                Id = pi.Id,
                ProductId = pi.ProductId,
                ProductName = pi.Product?.Name ?? string.Empty,
                LotNumber = pi.LotNumber,
                ExpiryDate = pi.ExpiryDate,
                Quantity = pi.Quantity,
                PurchaseRate = pi.PurchaseRate,
                DiscountPercentage = pi.DiscountPercentage,
                TaxAmount = pi.TaxAmount,
                LineTotal = pi.LineTotal
            }).ToList();

            return new PurchaseResponseDto
            {
                Id = purchase.Id,
                SupplierId = purchase.SupplierId,
                SupplierName = purchase.Supplier?.Name ?? string.Empty,
                InvoiceNumber = purchase.InvoiceNumber,
                InvoiceDate = purchase.InvoiceDate,
                Status = purchase.Status,
                SubTotal = purchase.SubTotal,
                TotalDiscount = purchase.TotalDiscount,
                TotalTax = purchase.TotalTax,
                GrandTotal = purchase.GrandTotal,
                CreatedAt = purchase.CreatedAt,
                Items = items
            };
        }
    }
}
