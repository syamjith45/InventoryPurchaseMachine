namespace InventoryPurchaseAPI.DTOs
{
    public class PurchaseCreateDto
    {
        public int SupplierId { get; set; }

        public DateTime InvoiceDate { get; set; }

        public List<PurchaseItemCreateDto> Items { get; set; } = new List<PurchaseItemCreateDto>();
    }
}
