namespace InventoryPurchaseAPI.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public decimal CostPrice { get; set; }

        public decimal SellingPrice { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property for many-to-many relationship with Tax
        public ICollection<ProductTax> ProductTaxes { get; set; } = new List<ProductTax>();

        // Navigation property for one-to-many relationship with PurchaseItem
        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();

        // Navigation property for one-to-many relationship with Inventory
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    }
}
