using Microsoft.EntityFrameworkCore;
using TrabajoPractico.Models;


namespace ProyectoFinal.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Transaction> Transaccions { get; set; }
    }
}

