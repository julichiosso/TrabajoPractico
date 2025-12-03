using ProyectoFinal.Models;
using System.ComponentModel.DataAnnotations;

namespace TrabajoPractico.Modelos
{
    public class Criptomoneda
    {
        [Key]
        public string Codigo { get; set; }

        public string Nombre { get; set; }
       

        public List<Transaccion> Transacciones { get; set; }
    }
}
