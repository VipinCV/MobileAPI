using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MobileAPI.Data;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Net;
using System.Text.Json;
using System.Xml.Linq;
namespace MobileAPI.Model;

[ApiController]
[Route("api/[controller]")]
public class DeepStudyInventoryController : ControllerBase
{
    private readonly DbDeepStudyServices _db;

    public DeepStudyInventoryController(DbDeepStudyServices db)
    {
        _db = db;
    }

    [HttpPost("add-supplier")]
    public async Task<IActionResult> AddSupplier([FromBody] SupplierDto dto)
    { 
        using var conn =new NpgsqlConnection(_db.DeepDBConnectionStringDeepDB());
        await conn.OpenAsync();

        using var transaction = await conn.BeginTransactionAsync();
        try
        {
            // Correct way to call a PROCEDURE in PostgreSQL
            using var cmd = new NpgsqlCommand("add_supplier", conn, transaction)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add parameters
            cmd.Parameters.AddWithValue("p_name", dto.Name);
            cmd.Parameters.AddWithValue("p_contact_name", dto.ContactName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_email", dto.Email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_phone", dto.Phone ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_address", dto.Address ?? (object)DBNull.Value);

            // Add output parameter
            var outParam = new NpgsqlParameter("p_supplier_id", NpgsqlDbType.Uuid)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outParam);

            await cmd.ExecuteNonQueryAsync();
            await transaction.CommitAsync();

            return Ok("Supplier added."); 
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
          
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("list-suppliers")]
    public async Task<IActionResult> GetSupplierList()
    {
        using var conn = new NpgsqlConnection(_db.DeepDBConnectionStringDeepDB());
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand("SELECT * FROM get_all_suppliers()", conn);

        using var reader = await cmd.ExecuteReaderAsync();

        var suppliers = new List<DeepSupplierDetails>(); // Replace with your actual DTO

        while (await reader.ReadAsync())
        {
            var supplier = new DeepSupplierDetails
            {
                SupplierId = reader.GetGuid(reader.GetOrdinal("supplier_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                ContactName = reader.IsDBNull(reader.GetOrdinal("contact_name")) ? null : reader.GetString(reader.GetOrdinal("contact_name")),
                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email")),
                Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString(reader.GetOrdinal("phone")),
                Address = reader.IsDBNull(reader.GetOrdinal("address")) ? null : reader.GetString(reader.GetOrdinal("address")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
            };

            suppliers.Add(supplier);
        }

        return Ok(suppliers);
    }

    [HttpPost("add-product")]
    public async Task<IActionResult> AddProduct([FromBody] DeepProductDto dto)
    {

        using var conn = new NpgsqlConnection(_db.DeepDBConnectionStringDeepDB());
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand("CALL add_product(@sku, @name, @desc, @price, @supplierId, @category, NULL)", conn);

        cmd.Parameters.AddWithValue("@sku", dto.SKU);
        cmd.Parameters.AddWithValue("@name", dto.Name);
        cmd.Parameters.AddWithValue("@desc", dto.Description ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@price", dto.UnitPrice);
        cmd.Parameters.AddWithValue("@supplierId", dto.SupplierId);
        cmd.Parameters.AddWithValue("@category", dto.Category ?? (object)DBNull.Value);

        var outParam = new NpgsqlParameter("p_product_id", NpgsqlDbType.Uuid)
        {
            Direction = ParameterDirection.Output
        };
        cmd.Parameters.Add(outParam);

        await cmd.ExecuteNonQueryAsync();
        return Ok("Product added.");
    }

    [HttpGet("list-products")] 
    public async Task<IActionResult> GetProductList()
    {
        using var conn = new NpgsqlConnection(_db.DeepDBConnectionStringDeepDB());
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand("SELECT * FROM get_all_products()", conn);

        using var reader = await cmd.ExecuteReaderAsync();

        var products = new List<DeepProductDetails>(); // Assuming you have a DTO class to hold the product data

        while (await reader.ReadAsync())
        {
            var product = new DeepProductDetails
            {
                ProductId = reader.GetGuid(reader.GetOrdinal("product_id")),
                SKU = reader.GetString(reader.GetOrdinal("sku")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                UnitPrice = reader.GetDecimal(reader.GetOrdinal("unit_price")),
                SupplierId = reader.GetGuid(reader.GetOrdinal("supplier_id")),
                SupplierName = reader.GetString(reader.GetOrdinal("supplier_name")),
                Category = reader.IsDBNull(reader.GetOrdinal("category")) ? null : reader.GetString(reader.GetOrdinal("category")),
                CurrentStock = reader.GetInt32(reader.GetOrdinal("current_stock")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
            };

            products.Add(product);
        }


        return Ok(products);
    }



    [HttpPost("purchase")]

    public async Task<IActionResult> CreatePurchase([FromBody] DeepCreatePurchaseDto dto)
    {
        using var conn = new NpgsqlConnection(_db.DeepDBConnectionStringDeepDB());
        await conn.OpenAsync();

        // ✅ Use procedure name only and set CommandType
        using var cmd = new NpgsqlCommand("create_purchase", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        // IN parameters
        cmd.Parameters.AddWithValue("p_supplier_id", dto.SupplierId);

        var itemsJson = JsonSerializer.Serialize(dto.Items);
        cmd.Parameters.AddWithValue("p_items", NpgsqlDbType.Jsonb, itemsJson);

        // OUT parameters - names must match PostgreSQL procedure exactly
        var purchaseIdParam = new NpgsqlParameter("p_purchase_id", NpgsqlDbType.Uuid)
        {
            Direction = ParameterDirection.Output
        };
        cmd.Parameters.Add(purchaseIdParam);

        var invoiceNumberParam = new NpgsqlParameter("p_invoice_number", NpgsqlDbType.Varchar)
        {
            Direction = ParameterDirection.Output
        };
        cmd.Parameters.Add(invoiceNumberParam);

        await cmd.ExecuteNonQueryAsync();

        var result = new
        {
            PurchaseId = purchaseIdParam.Value,
            InvoiceNumber = invoiceNumberParam.Value
        };

        return Ok(result);
    }


    [HttpPost("sale")]
    public async Task<IActionResult> CreateSale([FromBody] DeepCreateSaleDto dto)
    {
        using var conn = new NpgsqlConnection(_db.DeepDBConnectionStringDeepDB());
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand("create_sale", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        // IN parameters
        cmd.Parameters.AddWithValue("p_customer_name", dto.CustomerName);

        var itemsJson = JsonSerializer.Serialize(dto.Items);
        cmd.Parameters.AddWithValue("p_items", NpgsqlDbType.Jsonb, itemsJson);

        // OUT parameters
        var saleIdParam = new NpgsqlParameter("p_sale_id", NpgsqlDbType.Uuid)
        {
            Direction = ParameterDirection.Output
        };
        cmd.Parameters.Add(saleIdParam);

        var invoiceNumberParam = new NpgsqlParameter("p_invoice_number", NpgsqlDbType.Varchar)
        {
            Direction = ParameterDirection.Output
        };
        cmd.Parameters.Add(invoiceNumberParam);

        await cmd.ExecuteNonQueryAsync();

        var result = new
        {
            SaleId = saleIdParam.Value,
            InvoiceNumber = invoiceNumberParam.Value
        };

        return Ok(result);
    }

   

    //[HttpGet("sales-summary")]
    //public async Task<IActionResult> GetSalesSummary()
    //{
    //    var result = await _db.ReadAsync("SELECT * FROM inv_get_sales_summary()", reader => new SalesSummaryDto
    //    {
    //        ProductName = reader.GetString(0),
    //        TotalSold = reader.GetInt32(1),
    //        TotalRevenue = reader.GetDecimal(2)
    //    });

    //    return Ok(result);
    //}

    //[HttpGet("stock-status")]
    //public async Task<IActionResult> GetStockStatus()
    //{
    //    var result = await _db.ReadAsync("SELECT * FROM inv_get_stock_status()", reader => new StockStatusDto
    //    {
    //        ProductName = reader.GetString(0),
    //        StockQuantity = reader.GetInt32(1)
    //    });

    //    return Ok(result);
    //}

    //[HttpGet("vw-stock")]
    //public async Task<IActionResult> GetStockView()
    //{
    //    var result = await _db.ReadAsync("SELECT * FROM vwStock ORDER BY ProductName", reader => new StockViewDto
    //    {
    //        ProductId = reader.GetInt32(0),
    //        ProductName = reader.GetString(1),
    //        Price = reader.GetInt32(2),
    //        TotalPurchased = reader.GetInt32(3),
    //        TotalSold = reader.GetInt32(4),
    //        CurrentStock = reader.GetInt32(5)
    //    });

    //    return Ok(result);
    //}
}
