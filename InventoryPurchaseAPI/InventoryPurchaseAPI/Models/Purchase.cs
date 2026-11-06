namespace InventoryPurchaseAPI.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        public int SupplierId { get; set; }

        public Supplier Supplier { get; set; } = null!;

        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime InvoiceDate { get; set; }

        public string Status { get; set; } = "Draft";  // Draft or Posted

        public decimal SubTotal { get; set; } = 0;

        public decimal TotalDiscount { get; set; } = 0;

        public decimal TotalTax { get; set; } = 0;

        public decimal GrandTotal { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for one-to-many relationship
        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    }
}
