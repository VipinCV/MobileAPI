using Microsoft.AspNetCore.Mvc;
using MobileAPI.Data;
namespace MobileAPI.Model;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly DbService _db;

    public InventoryController(DbService db)
    {
        _db = db;
    }

    [HttpPost("add-product")]
    public async Task<IActionResult> AddProduct(string name, decimal price, int stock)
    {
        await _db.ExecuteFunctionAsync("SELECT add_product(@name, @price, @stock)", new()
        {
            { "name", name },
            { "price", price },
            { "stock", stock }
        });
        return Ok("Product added.");
    }

    [HttpPost("make-purchase")]
    public async Task<IActionResult> MakePurchase(int productId, int qty)
    {
        await _db.ExecuteFunctionAsync("SELECT make_purchase(@productId, @qty)", new()
        {
            { "productId", productId },
            { "qty", qty }
        });
        return Ok("Purchase recorded.");
    }

    [HttpPost("complete-purchase")]
    public async Task<IActionResult> CompletePurchase(int productId, int qty)
    {
        await _db.ExecuteFunctionAsync("SELECT complete_purchase_transaction(@productId, @qty)", new()
        {
            { "productId", productId },
            { "qty", qty }
        });
        return Ok("Purchase transaction completed.");
    }

    [HttpPost("complete-sale")]
    public async Task<IActionResult> CompleteSale(int productId, int qty)
    {
        await _db.ExecuteFunctionAsync("SELECT complete_sale_transaction(@productId, @qty)", new()
        {
            { "productId", productId },
            { "qty", qty }
        });
        return Ok("Sale transaction completed.");
    }

    [HttpGet("sales-summary")]
    public async Task<IActionResult> GetSalesSummary()
    {
        var result = await _db.ReadAsync("SELECT * FROM get_sales_summary()", reader => new SalesSummaryDto
        {
            ProductName = reader.GetString(0),
            TotalSold = reader.GetInt32(1),
            TotalRevenue = reader.GetDecimal(2)
        });

        return Ok(result);
    }

    [HttpGet("stock-status")]
    public async Task<IActionResult> GetStockStatus()
    {
        var result = await _db.ReadAsync("SELECT * FROM get_stock_status()", reader => new StockStatusDto
        {
            ProductName = reader.GetString(0),
            StockQuantity = reader.GetInt32(1)
        });

        return Ok(result);
    }

    [HttpGet("vw-stock")]
    public async Task<IActionResult> GetStockView()
    {
        var result = await _db.ReadAsync("SELECT * FROM Stock ORDER BY ProductName", reader => new StockViewDto
        {
            ProductId = reader.GetInt32(0),
            ProductName = reader.GetString(1),
            TotalPurchased = reader.GetInt32(2),
            TotalSold = reader.GetInt32(3),
            CurrentStock = reader.GetInt32(4)
        });

        return Ok(result);
    }
}
