namespace InventoryPurchaseAPI.Models
{
    public class Tax
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Percentage { get; set; }

        public bool IsCompound { get; set; } = false;

        public bool IsActive { get; set; } = true;

        // Navigation property for many-to-many relationship
        public ICollection<ProductTax> ProductTaxes { get; set; } = new List<ProductTax>();
    }
}
