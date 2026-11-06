using InventoryPurchaseAPI.Data;
using InventoryPurchaseAPI.DTOs;
using InventoryPurchaseAPI.Models;
using InventoryPurchaseAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPurchaseAPI.Services
{
    public class TaxService : ITaxService
    {
        private readonly AppDbContext _context;

        public TaxService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tax>> GetAllTaxesAsync()
        {
            return await _context.Taxes
                .Where(t => t.IsActive)
                .ToListAsync();
        }

        public async Task<Tax?> GetTaxByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _context.Taxes
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        }

        public async Task<Tax> CreateTaxAsync(TaxCreateDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Tax data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Tax name is required.", nameof(dto.Name));
            }

            if (dto.Percentage < 0)
            {
                throw new ArgumentException("Tax percentage cannot be negative.", nameof(dto.Percentage));
            }

            // Check for duplicate tax name
            var existingTax = await _context.Taxes
                .FirstOrDefaultAsync(t => t.Name == dto.Name && t.IsActive);

            if (existingTax != null)
            {
                throw new InvalidOperationException("A tax with this name already exists.");
            }

            var tax = new Tax
            {
                Name = dto.Name,
                Percentage = dto.Percentage,
                IsCompound = dto.IsCompound,
                IsActive = true
            };

            _context.Taxes.Add(tax);
            await _context.SaveChangesAsync();

            return tax;
        }

        public async Task<bool> UpdateTaxAsync(int id, TaxUpdateDto dto)
        {
            if (id <= 0)
            {
                return false;
            }

            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Tax data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Tax name is required.", nameof(dto.Name));
            }

            if (dto.Percentage < 0)
            {
                throw new ArgumentException("Tax percentage cannot be negative.", nameof(dto.Percentage));
            }

            var tax = await _context.Taxes
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            if (tax == null)
            {
                return false;
            }

            // Check for duplicate tax name (excluding current tax)
            var existingTax = await _context.Taxes
                .FirstOrDefaultAsync(t => t.Name == dto.Name && t.Id != id && t.IsActive);

            if (existingTax != null)
            {
                throw new InvalidOperationException("A tax with this name already exists.");
            }

            tax.Name = dto.Name;
            tax.Percentage = dto.Percentage;
            tax.IsCompound = dto.IsCompound;

            _context.Taxes.Update(tax);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteTaxAsync(int id)
        {
            if (id <= 0)
            {
                return false;
            }

            var tax = await _context.Taxes
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            if (tax == null)
            {
                return false;
            }

            // Check if tax is used in any purchases (if Purchase table exists)
            var isTaxUsed = await IsTaxUsedInPurchasesAsync(id);

            if (isTaxUsed)
            {
                throw new InvalidOperationException("Cannot delete or deactivate this tax because it is already used in posted purchases.");
            }

            tax.IsActive = false;
            _context.Taxes.Update(tax);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<bool> IsTaxUsedInPurchasesAsync(int taxId)
        {
            // Check if there's a Purchase table and if this tax is used in any purchases
            // This will work if Purchase and PurchaseTax relationship tables exist
            // If they don't exist yet, this returns false
            try
            {
                // Try to query from PurchaseTaxes if it exists
                // For now, return false as Purchase module is not yet created
                // This logic will be activated once Purchase module is implemented
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
