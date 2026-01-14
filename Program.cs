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

<<<<<<< HEAD
//Configurar CORS antes de controllers
=======
// 🔸 Configurar CORS antes de controllers
>>>>>>> 1207c7b247e4aae2cdebf2a3f5b43888870a5e16
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

<<<<<<< HEAD
//Middleware de CORS antes de Swagger y controllers
app.UseCors("PermitirTodo");

// Swagger
=======
// 🔸 Middleware de CORS antes de Swagger y controllers
app.UseCors("PermitirTodo");

// 🔸 Swagger
>>>>>>> 1207c7b247e4aae2cdebf2a3f5b43888870a5e16
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

<<<<<<< HEAD
//NO redirección a HTTPS (solo HTTP)
///app.UseHttpsRedirection();
=======
// 🔸 NO redirección a HTTPS (solo HTTP)
/// app.UseHttpsRedirection();
>>>>>>> 1207c7b247e4aae2cdebf2a3f5b43888870a5e16

app.UseAuthorization();

app.MapControllers();

app.Run();
