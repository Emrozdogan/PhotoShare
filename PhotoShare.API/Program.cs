using Microsoft.EntityFrameworkCore;
using PhotoShare.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Use SQLite for easier setup
builder.Services.AddDbContext<PhotoShareDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve static files (uploaded photos) - ONLY ONCE, HERE!
app.UseStaticFiles();
// Add this after app.UseStaticFiles();
app.UseDefaultFiles();

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.MapControllers();

app.Run();