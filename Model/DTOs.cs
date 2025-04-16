namespace MobileAPI.Model
{
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
        public int TotalPurchased { get; set; }
        public int TotalSold { get; set; }
        public int CurrentStock { get; set; }
    }

}
