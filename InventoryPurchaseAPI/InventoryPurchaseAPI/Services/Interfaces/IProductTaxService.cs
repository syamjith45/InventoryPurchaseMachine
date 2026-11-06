using InventoryPurchaseAPI.DTOs;
using InventoryPurchaseAPI.Models;

namespace InventoryPurchaseAPI.Services.Interfaces
{
    public interface IProductTaxService
    {
        Task<IEnumerable<Tax>> GetProductTaxesAsync(int productId);
        Task<bool> AssignTaxToProductAsync(ProductTaxDto dto);
        Task<bool> RemoveTaxFromProductAsync(int productId, int taxId);
    }
}
