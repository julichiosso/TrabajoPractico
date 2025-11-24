using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models;
using System.Globalization;
using TrabajoPractico.Models;

var builder = WebApplication.CreateBuilder(args);

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

// Agregar DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 🔸 Configurar CORS antes de controllers
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => true) // Acepta cualquier origen (incluye localhost)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🔸 Middleware de CORS antes de Swagger y controllers
app.UseCors("PermitirTodo");

// 🔸 Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔸 NO redirección a HTTPS (solo HTTP)
/// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
