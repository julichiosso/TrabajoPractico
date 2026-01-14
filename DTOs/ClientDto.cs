using System.ComponentModel.DataAnnotations;

namespace TrabajoPractico.DTOs
{
    public class ClientDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }
    }
}
