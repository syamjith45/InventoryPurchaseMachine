using InventoryPurchaseAPI.Data;
using InventoryPurchaseAPI.DTOs;
using InventoryPurchaseAPI.Models;
using InventoryPurchaseAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPurchaseAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<Product> CreateProductAsync(ProductCreateDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Product data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Product name is required.", nameof(dto.Name));
            }

            if (string.IsNullOrWhiteSpace(dto.Code))
            {
                throw new ArgumentException("Product code is required.", nameof(dto.Code));
            }

            var product = new Product
            {
                Name = dto.Name,
                Code = dto.Code,
                Category = dto.Category,
                CostPrice = dto.CostPrice,
                SellingPrice = dto.SellingPrice,
                IsActive = true
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<bool> UpdateProductAsync(int id, ProductUpdateDto dto)
        {
            if (id <= 0)
            {
                return false;
            }

            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Product data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Product name is required.", nameof(dto.Name));
            }

            if (string.IsNullOrWhiteSpace(dto.Code))
            {
                throw new ArgumentException("Product code is required.", nameof(dto.Code));
            }

            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (existingProduct == null)
            {
                return false;
            }

            existingProduct.Name = dto.Name;
            existingProduct.Code = dto.Code;
            existingProduct.Category = dto.Category;
            existingProduct.CostPrice = dto.CostPrice;
            existingProduct.SellingPrice = dto.SellingPrice;

            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            if (id <= 0)
            {
                return false;
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
            {
                return false;
            }

            product.IsActive = false;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
