using System.ComponentModel.DataAnnotations;

namespace MobileAPI.Model
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    } 
    public class SalesSummaryDto
    {
        public string ProductName { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class StockStatusDto
    {
        public string ProductName { get; set; }
        public int StockQuantity { get; set; }
    }

    public class StockViewDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
         public int Price  { get; set; }
        public int TotalPurchased { get; set; }
        public int TotalSold { get; set; }
        public int CurrentStock { get; set; }
    }

    // Deep DB


    public class DeepProductDto
    {

        [Required]
        public string SKU { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Range(0.01, 10000)]
        public decimal UnitPrice { get; set; }
        [Required]
        public Guid SupplierId { get; set; }
        public string? Category { get; set; }
    }
    public class DeepProductDetails
    {
        public Guid ProductId { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        [Range(0.01, 10000)]
        public decimal UnitPrice { get; set; }
        public Guid SupplierId { get; set; }
        public string? Category { get; set; }
        public string? SupplierName { get; set; }
        public Int32? CurrentStock { get; set; }
        public DateTime? CreatedAt { get; set; }

         


    }
    public class DeepSupplierDetails
    {
        public Guid SupplierId { get; set; }
        public string Name { get; set; }
        public string? ContactName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DeepPurchaseItemDto
    {
        public Guid product_id { get; set; }
        public int quantity { get; set; }
        public decimal unit_cost { get; set; }
        public DateTime expiration_date { get; set; }
        public string batch_number { get; set; } 
    }

    public class DeepCreatePurchaseDto
    {
        public Guid SupplierId { get; set; }
        public List<DeepPurchaseItemDto> Items { get; set; }
    }


    public class DeepCreateSaleDto
    {
        public string CustomerName { get; set; }
        public List<SaleItemDto> Items { get; set; }
    }

    public class SaleItemDto
    {
        public Guid product_id { get; set; }
        public int quantity { get; set; }
        public decimal unit_price { get; set; }
      
    }


}
