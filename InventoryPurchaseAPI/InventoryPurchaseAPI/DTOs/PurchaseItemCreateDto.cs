namespace InventoryPurchaseAPI.DTOs
{
    public class PurchaseItemCreateDto
    {
        public int ProductId { get; set; }

        public DateTime ExpiryDate { get; set; }

        public decimal Quantity { get; set; }

        public decimal PurchaseRate { get; set; }

        public decimal DiscountPercentage { get; set; } = 0;
    }
}
