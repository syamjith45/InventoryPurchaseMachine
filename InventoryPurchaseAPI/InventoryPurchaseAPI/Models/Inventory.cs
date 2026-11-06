namespace InventoryPurchaseAPI.Models
{
    public class Inventory
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public string LotNumber { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public decimal Quantity { get; set; }
    }
}
