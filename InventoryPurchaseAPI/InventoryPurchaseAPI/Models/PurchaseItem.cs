namespace InventoryPurchaseAPI.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }

        public int PurchaseId { get; set; }

        public Purchase Purchase { get; set; } = null!;

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public string LotNumber { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public decimal Quantity { get; set; }

        public decimal PurchaseRate { get; set; }

        public decimal DiscountPercentage { get; set; } = 0;

        public decimal TaxAmount { get; set; } = 0;

        public decimal LineTotal { get; set; }
    }
}
