using InventoryPurchaseAPI.DTOs;
using InventoryPurchaseAPI.Models;
using InventoryPurchaseAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryPurchaseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// GET /api/products - Get all active products
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        /// <summary>
        /// GET /api/products/{id} - Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid product ID.");
            }

            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            return Ok(product);
        }

        /// <summary>
        /// POST /api/products - Create a new product
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] ProductCreateDto dto)
        {
            try
            {
                var createdProduct = await _productService.CreateProductAsync(dto);
                return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// PUT /api/products/{id} - Update an existing product
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDto dto)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid product ID.");
            }

            if (dto == null)
            {
                return BadRequest("Product data cannot be null.");
            }

            try
            {
                var updated = await _productService.UpdateProductAsync(id, dto);

                if (!updated)
                {
                    return NotFound($"Product with ID {id} not found.");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// DELETE /api/products/{id} - Soft delete product (IsActive = false)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid product ID.");
            }

            var deleted = await _productService.DeleteProductAsync(id);

            if (!deleted)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            return NoContent();
        }
    }
}
