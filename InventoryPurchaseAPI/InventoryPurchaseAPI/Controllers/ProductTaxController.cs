
using InventoryPurchaseAPI.DTOs;
using InventoryPurchaseAPI.Models;
using InventoryPurchaseAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryPurchaseAPI.Controllers
{
    [ApiController]
    [Route("api/products/{productId}/taxes")]
    public class ProductTaxController : ControllerBase
    {
        private readonly IProductTaxService _productTaxService;

        public ProductTaxController(IProductTaxService productTaxService)
        {
            _productTaxService = productTaxService;
        }

        // GET: api/products/{productId}/taxes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tax>>> GetProductTaxes(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest("Invalid product ID.");
            }

            var taxes = await _productTaxService.GetProductTaxesAsync(productId);

            return Ok(taxes);
        }

        // POST: api/products/{productId}/taxes
        [HttpPost]
        public async Task<IActionResult> AssignTaxToProduct(
            int productId,
            [FromBody] ProductTaxDto dto)
        {
            if (productId <= 0)
            {
                return BadRequest("Invalid product ID.");
            }

            if (dto == null || dto.TaxId <= 0)
            {
                return BadRequest("Invalid tax ID.");
            }

            dto.ProductId = productId;

            try
            {
                var assigned =
                    await _productTaxService.AssignTaxToProductAsync(dto);

                if (!assigned)
                {
                    return BadRequest("Failed to assign tax to product.");
                }

                return Created(string.Empty, new
                {
                    productId,
                    taxId = dto.TaxId
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // DELETE: api/products/{productId}/taxes/{taxId}
        [HttpDelete("{taxId}")]
        public async Task<IActionResult> RemoveTaxFromProduct(
            int productId,
            int taxId)
        {
            if (productId <= 0)
            {
                return BadRequest("Invalid product ID.");
            }

            if (taxId <= 0)
            {
                return BadRequest("Invalid tax ID.");
            }

            try
            {
                var removed =
                    await _productTaxService.RemoveTaxFromProductAsync(
                        productId, taxId);

                if (!removed)
                {
                    return NotFound(
                        "Tax assignment not found for this product.");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
