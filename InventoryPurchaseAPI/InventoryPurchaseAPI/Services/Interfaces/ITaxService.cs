using InventoryPurchaseAPI.DTOs;
using InventoryPurchaseAPI.Models;

namespace InventoryPurchaseAPI.Services.Interfaces
{
    public interface ITaxService
    {
        Task<IEnumerable<Tax>> GetAllTaxesAsync();
        Task<Tax?> GetTaxByIdAsync(int id);
        Task<Tax> CreateTaxAsync(TaxCreateDto dto);
        Task<bool> UpdateTaxAsync(int id, TaxUpdateDto dto);
        Task<bool> DeleteTaxAsync(int id);
    }
}
