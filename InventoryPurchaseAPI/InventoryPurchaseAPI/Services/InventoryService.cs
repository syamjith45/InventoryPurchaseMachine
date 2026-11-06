using InventoryPurchaseAPI.Data;
using InventoryPurchaseAPI.Models;
using InventoryPurchaseAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPurchaseAPI.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly AppDbContext _context;

        public InventoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Inventory>> GetInventoryAsync()
        {
            var today = DateTime.UtcNow.Date;

            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.ExpiryDate.Date > today)
                .OrderBy(i => i.ExpiryDate)
                .ToListAsync();

            return inventory;
        }

        public async Task<List<Inventory>> GetInventoryByProductAsync(int productId)
        {
            if (productId <= 0)
            {
                return new List<Inventory>();
            }

            var today = DateTime.UtcNow.Date;

            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.ProductId == productId && i.ExpiryDate.Date > today)
                .OrderBy(i => i.ExpiryDate)
                .ToListAsync();

            return inventory;
        }
    }
}
