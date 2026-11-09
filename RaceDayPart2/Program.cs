using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Read Azure Blob Storage settings from User Secrets or configuration.
var blobConnectionString = builder.Configuration["AzureBlobStorage:ConnectionString"];
var containerName = builder.Configuration["AzureBlobStorage:ContainerName"];

// Stop startup if the Azure settings are missing.
if (string.IsNullOrWhiteSpace(blobConnectionString) ||
    string.IsNullOrWhiteSpace(containerName))
{
    throw new InvalidOperationException(
        "Azure Blob Storage settings are missing. Check User Secrets.");
}

// Register Azure Blob Storage so controllers can upload images.
builder.Services.AddSingleton(
    new BlobServiceClient(blobConnectionString));

builder.Services.AddSingleton(serviceProvider =>
{
    var blobService = serviceProvider.GetRequiredService<BlobServiceClient>();
    return blobService.GetBlobContainerClient(containerName);
});

// Add services to the container with JSON cycle prevention.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Register Entity Framework Core with SQL Server.
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Swagger/OpenAPI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Seed the database with initial test records.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<RaceDayDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();