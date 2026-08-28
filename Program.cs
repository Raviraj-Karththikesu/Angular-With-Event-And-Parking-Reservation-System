using Event_and_parking_reservation_system.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register AppDbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
{
    string? connectionString =
        builder.Configuration.GetConnectionString(
            "DefaultConnection");

    options.UseSqlServer(connectionString);
});

var app = builder.Build();

// Enable Swagger only during development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();