using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net;
using System.Numerics;
using System.Xml.Linq;

namespace MobileAPI.Data
{
    public class DeepstudyDB
    {
        public SupplierResult AddSupplier(SupplierDto dto)
        {

              using var conn = new NpgsqlConnection(Environment.GetEnvironmentVariable("DATABASE_URL_DEEPSTUDY")); 
              conn.OpenAsync();

            using var transaction =   conn.BeginTransaction();
            try
            {
                // Call stored procedure
                using var cmd = new NpgsqlCommand("CALL add_supplier(@name, @contact, @email, @phone, @address, NULL)", conn, transaction);

                cmd.Parameters.AddWithValue("@name", dto.Name);
                cmd.Parameters.AddWithValue("@contact", dto.ContactName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@email", dto.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@phone", dto.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@address", dto.Address ?? (object)DBNull.Value);

                // Get output parameter
                var outParam = new NpgsqlParameter("p_supplier_id", NpgsqlDbType.Uuid)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParam);

                cmd.ExecuteNonQuery();
                transaction.Commit();

                return GetSupplier((Guid)outParam.Value);
            }
            catch (Exception ex)
            {
                transaction.RollbackAsync();

            }
            return null;
        }

        public SupplierResult GetSupplier(Guid id)
        {
              using var conn = new NpgsqlConnection(Environment.GetEnvironmentVariable("DATABASE_URL_DEEPSTUDY"));
              conn.Open();

            using var cmd = new NpgsqlCommand(
                "SELECT * FROM get_supplier_by_id(@id)",
                conn);

            cmd.Parameters.AddWithValue("@id", id);

            using var reader =   cmd.ExecuteReader();

            if (!reader.HasRows)
                return  null;

              reader.Read();

            return  new SupplierResult   { 
                    SupplierId = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    ContactName = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Address = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CreatedAt = reader.GetDateTime(6)
           };
        }

        public List<SupplierResult> GetAllSuppliers()
        {
            using var conn = new NpgsqlConnection(Environment.GetEnvironmentVariable("DATABASE_URL_DEEPSTUDY"));
              conn.Open();

            using var cmd = new NpgsqlCommand("SELECT * FROM get_all_suppliers()", conn);
            using var reader =   cmd.ExecuteReader();

            var suppliers = new List<SupplierResult>();

            while ( reader.Read())
            {
                suppliers.Add(new SupplierResult
                {
                    SupplierId = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    ContactName = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Address = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CreatedAt = reader.GetDateTime(6)
                });
            }

            return  suppliers;
        }
    }

  
}

public class SupplierDto
{
    [Required]
    public string Name { get; set; }
    public string? ContactName { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    [Phone]
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

public record SupplierResult
{
   public Guid SupplierId { get; set; }
    public  string Name { get; set; }
    public  string? ContactName { get; set; }
    public  string? Email { get; set; }
    public  string? Phone { get; set; }
    public  string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductDto
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