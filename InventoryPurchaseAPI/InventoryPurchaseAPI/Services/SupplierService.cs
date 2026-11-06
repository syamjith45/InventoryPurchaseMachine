using InventoryPurchaseAPI.Data;
using InventoryPurchaseAPI.DTOs;
using InventoryPurchaseAPI.Models;
using InventoryPurchaseAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace InventoryPurchaseAPI.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly AppDbContext _context;

        public SupplierService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync()
        {
            return await _context.Suppliers
                .Where(s => s.IsActive)
                .ToListAsync();
        }

        public async Task<Supplier?> GetSupplierByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
        }

        public async Task<Supplier> CreateSupplierAsync(SupplierCreateDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Supplier data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Supplier name is required.", nameof(dto.Name));
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                throw new ArgumentException("Supplier email is required.", nameof(dto.Email));
            }

            if (!IsValidEmail(dto.Email))
            {
                throw new ArgumentException("Invalid email format.", nameof(dto.Email));
            }

            // Check for duplicate supplier by name and email
            var existingSupplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Name == dto.Name && s.Email == dto.Email && s.IsActive);

            if (existingSupplier != null)
            {
                throw new InvalidOperationException("A supplier with this name and email already exists.");
            }

            var supplier = new Supplier
            {
                Name = dto.Name,
                ContactPerson = dto.ContactPerson,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                IsActive = true
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return supplier;
        }

        public async Task<bool> UpdateSupplierAsync(int id, SupplierUpdateDto dto)
        {
            if (id <= 0)
            {
                return false;
            }

            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Supplier data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Supplier name is required.", nameof(dto.Name));
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                throw new ArgumentException("Supplier email is required.", nameof(dto.Email));
            }

            if (!IsValidEmail(dto.Email))
            {
                throw new ArgumentException("Invalid email format.", nameof(dto.Email));
            }

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (supplier == null)
            {
                return false;
            }

            // Check for duplicate supplier by name and email (excluding current supplier)
            var existingSupplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => (s.Name == dto.Name && s.Email == dto.Email) && s.Id != id && s.IsActive);

            if (existingSupplier != null)
            {
                throw new InvalidOperationException("A supplier with this name and email already exists.");
            }

            supplier.Name = dto.Name;
            supplier.ContactPerson = dto.ContactPerson;
            supplier.Phone = dto.Phone;
            supplier.Email = dto.Email;
            supplier.Address = dto.Address;

            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            if (id <= 0)
            {
                return false;
            }

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (supplier == null)
            {
                return false;
            }

            supplier.IsActive = false;
            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
    }
}
