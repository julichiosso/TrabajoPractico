using Microsoft.Build.Framework;

using System.ComponentModel.DataAnnotations;


namespace TrabajoPractico.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        public required string Nombre { get; set; }

        public required string Email { get; set; }

        public DateTime FechaRegistro { get; set; }



        //ublic List<Transaccion> Transacciones { get; set; }
    }
}
