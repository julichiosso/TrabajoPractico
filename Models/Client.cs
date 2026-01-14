<<<<<<< HEAD
﻿using System.ComponentModel.DataAnnotations;
=======
﻿using Microsoft.Build.Framework;

using System.ComponentModel.DataAnnotations;
>>>>>>> 1207c7b247e4aae2cdebf2a3f5b43888870a5e16


namespace TrabajoPractico.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }
<<<<<<< HEAD
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
=======

        public required string Nombre { get; set; }

        public required string Email { get; set; }

        public DateTime FechaRegistro { get; set; }



>>>>>>> 1207c7b247e4aae2cdebf2a3f5b43888870a5e16
    }
}
