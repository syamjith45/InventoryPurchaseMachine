using InventoryPurchaseAPI.Data;
using InventoryPurchaseAPI.DTOs;
using InventoryPurchaseAPI.Models;
using InventoryPurchaseAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InventoryPurchaseAPI.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly AppDbContext _context;

        public PurchaseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Purchase> CreatePurchaseAsync(PurchaseCreateDto dto)
        {
            // Validate DTO
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Purchase data cannot be null.");
            }

            if (dto.SupplierId <= 0)
            {
                throw new ArgumentException("Invalid supplier ID.", nameof(dto.SupplierId));
            }

            if (dto.Items == null || dto.Items.Count == 0)
            {
                throw new ArgumentException("Purchase must have at least one item.", nameof(dto.Items));
            }

            // Validate supplier exists and is active
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == dto.SupplierId && s.IsActive);

            if (supplier == null)
            {
                throw new InvalidOperationException("Supplier not found or is inactive.");
            }

            // Validate all items and get products
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id) && p.IsActive)
                .ToListAsync();

            if (products.Count != productIds.Count)
            {
                throw new InvalidOperationException("One or more products not found or are inactive.");
            }

            var invoiceNumber = await GenerateInvoiceNumberAsync();

            // Calculate purchase items
            var purchaseItems = new List<PurchaseItem>();
            decimal purchaseSubTotal = 0;
            decimal purchaseTotalDiscount = 0;
            decimal purchaseTotalTax = 0;

            foreach (var itemDto in dto.Items)
            {
                // Validate item data
                ValidateItemData(itemDto);

                // Calculate line totals
                decimal lineSubTotal = itemDto.Quantity * itemDto.PurchaseRate;
                decimal lineDiscount = CalculateDiscount(lineSubTotal, itemDto.DiscountPercentage);
                decimal lineAmountAfterDiscount = lineSubTotal - lineDiscount;

                // Get taxes for this product
                var productTaxes = await GetProductTaxesAsync(itemDto.ProductId);

                // Calculate tax amount
                decimal lineTax = CalculateTax(lineAmountAfterDiscount, productTaxes);

                // Calculate line total
                decimal lineTotal = lineAmountAfterDiscount + lineTax;

                // Create purchase item
                var purchaseItem = new PurchaseItem
                {
                    ProductId = itemDto.ProductId,
                    LotNumber = await GenerateLotNumberAsync(),
                    ExpiryDate = DateTime.SpecifyKind(itemDto.ExpiryDate, DateTimeKind.Utc),
                    Quantity = itemDto.Quantity,
                    PurchaseRate = itemDto.PurchaseRate,
                    DiscountPercentage = itemDto.DiscountPercentage,
                    TaxAmount = lineTax,
                    LineTotal = lineTotal
                };

                purchaseItems.Add(purchaseItem);

                // Accumulate purchase totals
                purchaseSubTotal += lineSubTotal;
                purchaseTotalDiscount += lineDiscount;
                purchaseTotalTax += lineTax;
            }

            // Calculate grand total
            decimal purchaseGrandTotal = purchaseSubTotal - purchaseTotalDiscount + purchaseTotalTax;

            // Create purchase
            var purchase = new Purchase
            {
                SupplierId = dto.SupplierId,
                InvoiceNumber = invoiceNumber,
                InvoiceDate = DateTime.SpecifyKind(dto.InvoiceDate, DateTimeKind.Utc),
                Status = "Draft",
                SubTotal = purchaseSubTotal,
                TotalDiscount = purchaseTotalDiscount,
                TotalTax = purchaseTotalTax,
                GrandTotal = purchaseGrandTotal,
                CreatedAt = DateTime.UtcNow,
                PurchaseItems = purchaseItems
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            return purchase;
        }

        private void ValidateItemData(PurchaseItemCreateDto itemDto)
        {
            if (itemDto == null)
            {
                throw new ArgumentNullException(nameof(itemDto), "Purchase item cannot be null.");
            }

            if (itemDto.ProductId <= 0)
            {
                throw new ArgumentException("Invalid product ID.", nameof(itemDto.ProductId));
            }

            if (itemDto.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0.", nameof(itemDto.Quantity));
            }

            if (itemDto.PurchaseRate < 0)
            {
                throw new ArgumentException("Purchase rate cannot be negative.", nameof(itemDto.PurchaseRate));
            }

            if (itemDto.DiscountPercentage < 0 || itemDto.DiscountPercentage > 100)
            {
                throw new ArgumentException("Discount percentage must be between 0 and 100.", nameof(itemDto.DiscountPercentage));
            }
        }

        private decimal CalculateDiscount(decimal lineSubTotal, decimal discountPercentage)
        {
            if (discountPercentage == 0)
            {
                return 0;
            }

            return lineSubTotal * discountPercentage / 100;
        }

        private decimal CalculateTax(decimal lineAmount, List<Tax> taxes)
        {
            if (taxes == null || taxes.Count == 0)
            {
                return 0;
            }

            decimal totalTax = 0;
            decimal amountAfterSimpleTaxes = lineAmount;

            // First, calculate simple taxes (non-compound)
            var simpleTaxes = taxes.Where(t => !t.IsCompound).ToList();
            foreach (var tax in simpleTaxes)
            {
                decimal taxAmount = lineAmount * tax.Percentage / 100;
                totalTax += taxAmount;
            }

            // Then, calculate compound taxes on the amount including simple taxes
            var compoundTaxes = taxes.Where(t => t.IsCompound).ToList();
            decimal amountForCompoundTax = lineAmount + totalTax;

            foreach (var tax in compoundTaxes)
            {
                decimal taxAmount = amountForCompoundTax * tax.Percentage / 100;
                totalTax += taxAmount;
                amountForCompoundTax += taxAmount;
            }

            return totalTax;
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var sequenceValue = await GetNextSequenceValueAsync("invoice_number_seq");

            return $"INV-{sequenceValue:D4}";
        }

        private async Task<string> GenerateLotNumberAsync()
        {
            var sequenceValue = await GetNextSequenceValueAsync("lot_number_seq");

            return $"LOT-{sequenceValue:D4}";
        }

        private async Task<long> GetNextSequenceValueAsync(string sequenceName)
        {
            var connection = _context.Database.GetDbConnection();
            var openedConnection = connection.State != ConnectionState.Open;

            if (openedConnection)
            {
                await connection.OpenAsync();
            }

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = $"SELECT nextval('{sequenceName}')";
                var result = await command.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    throw new InvalidOperationException($"Sequence '{sequenceName}' did not return a value.");
                }

                return Convert.ToInt64(result);
            }
            finally
            {
                if (openedConnection)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private async Task<List<Tax>> GetProductTaxesAsync(int productId)
        {
            var taxes = await _context.ProductTaxes
                .Where(pt => pt.ProductId == productId)
                .Select(pt => pt.Tax)
                .Where(t => t.IsActive)
                .ToListAsync();

            return taxes;
        }

        public async Task<bool> PostPurchaseAsync(int purchaseId)
        {
            if (purchaseId <= 0)
            {
                throw new ArgumentException("Invalid purchase ID.", nameof(purchaseId));
            }

            // Load purchase with items
            var purchase = await _context.Purchases
                .Include(p => p.PurchaseItems)
                .FirstOrDefaultAsync(p => p.Id == purchaseId);

            if (purchase == null)
            {
                throw new InvalidOperationException("Purchase not found.");
            }

            // Check if purchase is in Draft status
            if (purchase.Status != "Draft")
            {
                throw new InvalidOperationException("Only Draft purchases can be posted.");
            }

            // Validate all items before posting
            ValidateItemsForPosting(purchase.PurchaseItems, purchase.InvoiceDate);

            // Check for duplicate product + lot combinations
            CheckForDuplicateLots(purchase.PurchaseItems);

            // Use transaction for inventory updates
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Update inventory for each item
                    foreach (var item in purchase.PurchaseItems)
                    {
                        await UpdateInventoryAsync(item);
                    }

                    // Update purchase status to Posted
                    purchase.Status = "Posted";
                    _context.Purchases.Update(purchase);
                    await _context.SaveChangesAsync();

                    // Commit transaction
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception)
                {
                    // Rollback on any error
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        private void ValidateItemsForPosting(ICollection<PurchaseItem> items, DateTime invoiceDate)
        {
            if (items == null || items.Count == 0)
            {
                throw new InvalidOperationException("Purchase has no items.");
            }

            var today = DateTime.UtcNow.Date;

            foreach (var item in items)
            {
                // Validate lot number
                if (string.IsNullOrWhiteSpace(item.LotNumber))
                {
                    throw new InvalidOperationException("Lot number is mandatory for all items.");
                }

                // Validate expiry date is later than invoice date
                if (item.ExpiryDate <= invoiceDate)
                {
                    throw new InvalidOperationException($"Expiry date must be later than invoice date for product item with lot {item.LotNumber}.");
                }

                // Validate expiry date is in the future
                if (item.ExpiryDate.Date <= today)
                {
                    throw new InvalidOperationException($"Product with lot {item.LotNumber} has already expired.");
                }
            }
        }

        private void CheckForDuplicateLots(ICollection<PurchaseItem> items)
        {
            // Check for duplicate product + lot combinations
            var lotGroups = items.GroupBy(i => new { i.ProductId, i.LotNumber })
                .Where(g => g.Count() > 1);

            if (lotGroups.Any())
            {
                var duplicate = lotGroups.First();
                throw new InvalidOperationException($"Duplicate product lot found: Product {duplicate.Key.ProductId}, Lot {duplicate.Key.LotNumber}.");
            }
        }

        private async Task UpdateInventoryAsync(PurchaseItem item)
        {
            // Find existing inventory record for this product + lot combination
            var existingInventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.LotNumber == item.LotNumber);

            if (existingInventory != null)
            {
                // Update existing inventory
                existingInventory.Quantity += item.Quantity;
                _context.Inventories.Update(existingInventory);
            }
            else
            {
                // Create new inventory record
                var newInventory = new Inventory
                {
                    ProductId = item.ProductId,
                    LotNumber = item.LotNumber,
                    ExpiryDate = item.ExpiryDate,
                    Quantity = item.Quantity
                };

                _context.Inventories.Add(newInventory);
            }

            // Save changes for each item (within the transaction)
            await _context.SaveChangesAsync();
        }

        public async Task<List<Purchase>> GetAllPurchasesAsync()
        {
            var purchases = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return purchases;
        }

        public async Task<Purchase?> GetPurchaseByIdAsync(int purchaseId)
        {
            if (purchaseId <= 0)
            {
                return null;
            }

            var purchase = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .FirstOrDefaultAsync(p => p.Id == purchaseId);

            return purchase;
        }
    }
}
