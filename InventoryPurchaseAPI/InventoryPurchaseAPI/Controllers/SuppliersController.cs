using InventoryPurchaseAPI.DTOs;
using InventoryPurchaseAPI.Models;
using InventoryPurchaseAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryPurchaseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SuppliersController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        /// <summary>
        /// GET /api/suppliers - Get all active suppliers
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetAllSuppliers()
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            return Ok(suppliers);
        }

        /// <summary>
        /// GET /api/suppliers/{id} - Get supplier by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Supplier>> GetSupplierById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid supplier ID.");
            }

            var supplier = await _supplierService.GetSupplierByIdAsync(id);

            if (supplier == null)
            {
                return NotFound($"Supplier with ID {id} not found.");
            }

            return Ok(supplier);
        }

        /// <summary>
        /// POST /api/suppliers - Create a new supplier
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Supplier>> CreateSupplier([FromBody] SupplierCreateDto dto)
        {
            try
            {
                var supplier = await _supplierService.CreateSupplierAsync(dto);
                return CreatedAtAction(nameof(GetSupplierById), new { id = supplier.Id }, supplier);
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
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// PUT /api/suppliers/{id} - Update an existing supplier
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] SupplierUpdateDto dto)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid supplier ID.");
            }

            if (dto == null)
            {
                return BadRequest("Supplier data cannot be null.");
            }

            try
            {
                var updated = await _supplierService.UpdateSupplierAsync(id, dto);

                if (!updated)
                {
                    return NotFound($"Supplier with ID {id} not found.");
                }

                return NoContent();
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
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// DELETE /api/suppliers/{id} - Soft delete supplier (IsActive = false)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid supplier ID.");
            }

            var deleted = await _supplierService.DeleteSupplierAsync(id);

            if (!deleted)
            {
                return NotFound($"Supplier with ID {id} not found.");
            }

            return NoContent();
        }
    }
}
