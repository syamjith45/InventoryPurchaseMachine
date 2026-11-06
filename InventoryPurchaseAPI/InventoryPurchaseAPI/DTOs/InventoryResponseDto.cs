namespace InventoryPurchaseAPI.DTOs
{
    public class InventoryResponseDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string LotNumber { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public decimal Quantity { get; set; }
    }
}
