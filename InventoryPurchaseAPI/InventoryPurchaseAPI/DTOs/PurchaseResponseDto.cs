namespace InventoryPurchaseAPI.DTOs
{
    public class PurchaseResponseDto
    {
        public int Id { get; set; }

        public int SupplierId { get; set; }

        public string SupplierName { get; set; } = string.Empty;

        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime InvoiceDate { get; set; }

        public string Status { get; set; } = "Draft";

        public decimal SubTotal { get; set; }

        public decimal TotalDiscount { get; set; }

        public decimal TotalTax { get; set; }

        public decimal GrandTotal { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<PurchaseItemResponseDto> Items { get; set; } = new List<PurchaseItemResponseDto>();
    }

    public class PurchaseItemResponseDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string LotNumber { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public decimal Quantity { get; set; }

        public decimal PurchaseRate { get; set; }

        public decimal DiscountPercentage { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal LineTotal { get; set; }
    }
}
