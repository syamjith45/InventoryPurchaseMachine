using Microsoft.EntityFrameworkCore;
using InventoryPurchaseAPI.Data;
using InventoryPurchaseAPI.Services;
using InventoryPurchaseAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register Entity Framework Core with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Product Service
builder.Services.AddScoped<IProductService, ProductService>();

// Register Supplier Service
builder.Services.AddScoped<ISupplierService, SupplierService>();

// Register Tax Service
builder.Services.AddScoped<ITaxService, TaxService>();

// Register Product-Tax Service
builder.Services.AddScoped<IProductTaxService, ProductTaxService>();

// Register Purchase Service
builder.Services.AddScoped<IPurchaseService, PurchaseService>();

// Register Inventory Service
builder.Services.AddScoped<IInventoryService, InventoryService>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Swagger support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("ReactFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
