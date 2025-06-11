using System.ComponentModel.DataAnnotations;

namespace TrabajoPractico.DTOs
{
    public class ClienteDTO
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Nombre { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
