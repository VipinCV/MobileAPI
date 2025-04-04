using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MobileAPI.Helpers; 


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
 //builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

//var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
//builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddSingleton<PostgresHelper>();


builder.Services.AddHostedService<RedisSubscriber>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:8056") // your frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add services to the container.

var app = builder.Build();

//if (app.Environment.IsDevelopment() || app.Environment.IsProduction() )
//{
app.UseCors();
app.UseSwaggerUI();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
//});
////}
///
 

// Configure the HTTP request pipeline.
 
app.MapHub<NotificationHub>("/notificationhub");
app.UseHttpsRedirection(); 
app.UseAuthorization();
app.MapControllers();
app.Run();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.MapPost("/todoupdate", (Todo todo) => {
    if (todo is not null)
    {
        todo.Name = todo.NameField;
    }
    return todo;
});


app.MapGet("/MobileData", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new MobileData
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}


internal record MobileData(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}


class Todo
{
    public string? Name { get; set; }
    public string? NameField { get; set; }
    public bool IsComplete { get; set; }
}