using Microsoft.EntityFrameworkCore;
using TrabajoPractico.Modelos;
using TrabajoPractico.Models;


namespace ProyectoFinal.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Client> Clientes { get; set; }
        public DbSet<Criptomoneda> Criptomonedas { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }
    }
}

