using Microsoft.AspNetCore.Mvc;
using MobileAPI.Helpers; 
namespace MobileAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PolygonController : ControllerBase
{
    private readonly PostgresHelper _postgresHelper;

    public PolygonController(PostgresHelper postgresHelper)
    {
        _postgresHelper = postgresHelper;
    }

    // ✅ GET All Polygons
    [HttpGet]
    public IActionResult GetPolygons()
    {
        var polygons = _postgresHelper.GetPolygons();
        return Ok(polygons);
    }

    // ✅ POST New Polygon
    [HttpPost]
    public IActionResult CreatePolygon([FromBody] PolygonDto polygonDto)
    {
        try
        {
            bool success = _postgresHelper.InsertPolygon(polygonDto.Name, polygonDto.Description, polygonDto.Coordinates);
            if (!success) return StatusCode(500, "Failed to insert polygon.");
            return Ok("✅ Polygon added successfully!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"❌ Internal Server Error: {ex.Message}");
        }
    }
}

// DTO for receiving polygon data from the frontend
public class PolygonDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Format: [[lng, lat], [lng, lat], ...] (must form a closed loop)
    public List<List<double>> Coordinates { get; set; } = new();
}
