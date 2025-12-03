using Microsoft.EntityFrameworkCore;
<<<<<<< HEAD
=======
using TrabajoPractico.Modelos;
>>>>>>> 1207c7b247e4aae2cdebf2a3f5b43888870a5e16
using TrabajoPractico.Models;


namespace ProyectoFinal.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
<<<<<<< HEAD
        public DbSet<Client> Clients { get; set; }
        public DbSet<Transaction> Transaccions { get; set; }
=======
        public DbSet<Client> Clientes { get; set; }
        public DbSet<Criptomoneda> Criptomonedas { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }
>>>>>>> 1207c7b247e4aae2cdebf2a3f5b43888870a5e16
    }
}

