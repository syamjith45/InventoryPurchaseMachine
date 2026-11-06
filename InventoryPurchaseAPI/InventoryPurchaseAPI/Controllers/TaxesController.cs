using InventoryPurchaseAPI.DTOs;
using InventoryPurchaseAPI.Models;
using InventoryPurchaseAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryPurchaseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaxesController : ControllerBase
    {
        private readonly ITaxService _taxService;

        public TaxesController(ITaxService taxService)
        {
            _taxService = taxService;
        }

        /// <summary>
        /// GET /api/taxes - Get all active taxes
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tax>>> GetAllTaxes()
        {
            var taxes = await _taxService.GetAllTaxesAsync();
            return Ok(taxes);
        }

        /// <summary>
        /// GET /api/taxes/{id} - Get tax by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Tax>> GetTaxById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid tax ID.");
            }

            var tax = await _taxService.GetTaxByIdAsync(id);

            if (tax == null)
            {
                return NotFound($"Tax with ID {id} not found.");
            }

            return Ok(tax);
        }

        /// <summary>
        /// POST /api/taxes - Create a new tax
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Tax>> CreateTax([FromBody] TaxCreateDto dto)
        {
            try
            {
                var tax = await _taxService.CreateTaxAsync(dto);
                return CreatedAtAction(nameof(GetTaxById), new { id = tax.Id }, tax);
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
        /// PUT /api/taxes/{id} - Update an existing tax
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTax(int id, [FromBody] TaxUpdateDto dto)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid tax ID.");
            }

            if (dto == null)
            {
                return BadRequest("Tax data cannot be null.");
            }

            try
            {
                var updated = await _taxService.UpdateTaxAsync(id, dto);

                if (!updated)
                {
                    return NotFound($"Tax with ID {id} not found.");
                }

                return NoContent();
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
        /// DELETE /api/taxes/{id} - Soft delete tax (IsActive = false)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTax(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid tax ID.");
            }

            try
            {
                var deleted = await _taxService.DeleteTaxAsync(id);

                if (!deleted)
                {
                    return NotFound($"Tax with ID {id} not found.");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
