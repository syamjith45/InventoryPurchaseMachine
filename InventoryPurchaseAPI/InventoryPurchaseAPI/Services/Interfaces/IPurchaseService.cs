using InventoryPurchaseAPI.DTOs;
using InventoryPurchaseAPI.Models;

namespace InventoryPurchaseAPI.Services.Interfaces
{
    public interface IPurchaseService
    {
        Task<Purchase> CreatePurchaseAsync(PurchaseCreateDto dto);
        Task<bool> PostPurchaseAsync(int purchaseId);
        Task<List<Purchase>> GetAllPurchasesAsync();
        Task<Purchase?> GetPurchaseByIdAsync(int purchaseId);
    }
}
