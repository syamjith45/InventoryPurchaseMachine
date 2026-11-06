namespace InventoryPurchaseAPI.DTOs
{
    public class TaxCreateDto
    {
        public string Name { get; set; } = string.Empty;

        public decimal Percentage { get; set; }

        public bool IsCompound { get; set; } = false;
    }
}
