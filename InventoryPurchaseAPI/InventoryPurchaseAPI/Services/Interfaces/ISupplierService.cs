using InventoryPurchaseAPI.DTOs;
using InventoryPurchaseAPI.Models;

namespace InventoryPurchaseAPI.Services.Interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<Supplier>> GetAllSuppliersAsync();
        Task<Supplier?> GetSupplierByIdAsync(int id);
        Task<Supplier> CreateSupplierAsync(SupplierCreateDto dto);
        Task<bool> UpdateSupplierAsync(int id, SupplierUpdateDto dto);
        Task<bool> DeleteSupplierAsync(int id);
    }
}
