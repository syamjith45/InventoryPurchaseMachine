using InventoryPurchaseAPI.Data;
using InventoryPurchaseAPI.DTOs;
using InventoryPurchaseAPI.Models;
using InventoryPurchaseAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPurchaseAPI.Services
{
    public class ProductTaxService : IProductTaxService
    {
        private readonly AppDbContext _context;

        public ProductTaxService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tax>> GetProductTaxesAsync(int productId)
        {
            if (productId <= 0)
            {
                return new List<Tax>();
            }

            // Check if product exists and is active
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

            if (product == null)
            {
                return new List<Tax>();
            }

            // Get all active taxes assigned to the product
            var taxes = await _context.ProductTaxes
                .Where(pt => pt.ProductId == productId)
                .Select(pt => pt.Tax)
                .Where(t => t.IsActive)
                .ToListAsync();

            return taxes;
        }

        public async Task<bool> AssignTaxToProductAsync(ProductTaxDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Product tax data cannot be null.");
            }

            if (dto.ProductId <= 0)
            {
                throw new ArgumentException("Invalid product ID.", nameof(dto.ProductId));
            }

            if (dto.TaxId <= 0)
            {
                throw new ArgumentException("Invalid tax ID.", nameof(dto.TaxId));
            }

            // Check if product exists and is active
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.IsActive);

            if (product == null)
            {
                throw new InvalidOperationException("Product not found or is inactive.");
            }

            // Check if tax exists and is active
            var tax = await _context.Taxes
                .FirstOrDefaultAsync(t => t.Id == dto.TaxId && t.IsActive);

            if (tax == null)
            {
                throw new InvalidOperationException("Tax not found or is inactive.");
            }

            // Check if tax is already assigned to product
            var existingAssignment = await _context.ProductTaxes
                .FirstOrDefaultAsync(pt => pt.ProductId == dto.ProductId && pt.TaxId == dto.TaxId);

            if (existingAssignment != null)
            {
                throw new InvalidOperationException("This tax is already assigned to the product.");
            }

            // Create new assignment
            var productTax = new ProductTax
            {
                ProductId = dto.ProductId,
                TaxId = dto.TaxId
            };

            _context.ProductTaxes.Add(productTax);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveTaxFromProductAsync(int productId, int taxId)
        {
            if (productId <= 0)
            {
                throw new ArgumentException("Invalid product ID.", nameof(productId));
            }

            if (taxId <= 0)
            {
                throw new ArgumentException("Invalid tax ID.", nameof(taxId));
            }

            var productTax = await _context.ProductTaxes
                .FirstOrDefaultAsync(pt => pt.ProductId == productId && pt.TaxId == taxId);

            if (productTax == null)
            {
                return false;
            }

            _context.ProductTaxes.Remove(productTax);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
