using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MobileAPI.Data;
using Npgsql;
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
        await _db.ExecuteFunctionAsync("SELECT inv_add_product(@name, @price, @stock)", new()
        {
            { "name", name },
            { "price", price },
            { "stock", stock }
        });
        return Ok("Product added.");
    }

    [HttpGet("list-products")]
    public async Task<IActionResult> GetProductList()
    {
        var result = await _db.ReadAsync("SELECT * FROM inv_list_products()", reader => new ProductDto
        {
            ProductId = reader.GetInt32(0),
            ProductName = reader.GetString(1),
            Price = reader.GetDecimal(2),
            StockQuantity = reader.GetInt32(3)
        });

        return Ok(result);
    }

    [HttpPost("make-purchase")]
    public async Task<IActionResult> MakePurchase(int productId, int qty)
    {
        await _db.ExecuteFunctionAsync("SELECT inv_make_purchase(@productId, @qty)", new()
        {
            { "productId", productId },
            { "qty", qty }
        });
        return Ok("Purchase recorded.");
    }

    [HttpPost("complete-purchase")]
    public async Task<IActionResult> CompletePurchase(int productId, int qty)
    {
        await _db.ExecuteFunctionAsync("SELECT inv_complete_purchase_transaction(@productId, @qty)", new()
        {
            { "productId", productId },
            { "qty", qty }
        });
        return Ok("Purchase transaction completed.");
    }

    [HttpPost("complete-sale")]
    public async Task<IActionResult> CompleteSale(int productId, int qty,string billno)
    {
        await _db.ExecuteFunctionAsync("SELECT inv_complete_sale_transaction(@productId, @qty,@billno)", new()
        {
            { "productId", productId },
            { "qty", qty },
             { "billno", billno }
        });
        return Ok("Sale transaction completed.");
    }

    [HttpGet("sales-summary")]
    public async Task<IActionResult> GetSalesSummary()
    {
        var result = await _db.ReadAsync("SELECT * FROM inv_get_sales_summary()", reader => new SalesSummaryDto
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
        var result = await _db.ReadAsync("SELECT * FROM inv_get_stock_status()", reader => new StockStatusDto
        {
            ProductName = reader.GetString(0),
            StockQuantity = reader.GetInt32(1)
        });

        return Ok(result);
    }

    [HttpGet("vw-stock")]
    public async Task<IActionResult> GetStockView()
    {
        var result = await _db.ReadAsync("SELECT * FROM vwStock ORDER BY ProductName", reader => new StockViewDto
        {
            ProductId = reader.GetInt32(0),
            ProductName = reader.GetString(1),
            Price = reader.GetInt32(2),
            TotalPurchased = reader.GetInt32(3),
            TotalSold = reader.GetInt32(4),
            CurrentStock = reader.GetInt32(5)
        });

        return Ok(result);
    }
}
