using InventoryPurchaseAPI.Models;

namespace InventoryPurchaseAPI.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<List<Inventory>> GetInventoryAsync();
        Task<List<Inventory>> GetInventoryByProductAsync(int productId);
    }
}
