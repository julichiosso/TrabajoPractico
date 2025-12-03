using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProyectoFinal.Models;
using TrabajoPractico.DTOs;
using TrabajoPractico.Models;

namespace TrabajoPractico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly AppDbContext _context;
<<<<<<< HEAD

=======
   
>>>>>>> 1207c7b247e4aae2cdebf2a3f5b43888870a5e16

        public ClienteController(AppDbContext context)
        {
            _context = context;
        }

<<<<<<< HEAD
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDTO>>> GetClients()
        {
            var clientes = await _context.Clients.ToListAsync();

          
            var clientesDTOs = clientes.Select(c => new ClientDTO
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email
            }).ToList();

            return Ok(clientesDTOs);
        }

        [HttpGet("id")]
        public async Task<ActionResult<ClientDTO>> GetById(int id)
        {
            var cliente = await _context.Clients.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            var clientDTO = new ClientDTO
            {
                Id = cliente.Id,
                Name = cliente.Name,
                Email = cliente.Email
            };
            return Ok(clientDTO);
        }

        [HttpPost]
        public async Task<ActionResult<ClientDTO>> Post(ClientDTO dto)
        {
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("El nombre es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
            {
                return BadRequest("Debe ingresar un email válido");
            }

            if (await _context.Clients.AnyAsync(c => c.Email == dto.Email))
            {
                return BadRequest("Ya existe un cliente con ese email");
            }


            var cliente = new Client
            {
                Id = dto.Id,
                Name = dto.Name,
                Email = dto.Email,
            };

            _context.Clients.Add(cliente);
            await _context.SaveChangesAsync();

            var respuesta = new ClientDTO
            {
                Id = cliente.Id,
                Name = cliente.Name,
                Email = cliente.Email,
            };

            return Ok(respuesta); 
=======

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDTO>>> Get()
        {
            var clientes = await _context.Clientes.OrderBy(c => c.FechaRegistro).ToListAsync();

            var clientesDatos = clientes.Select(c => new ClienteDTO
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Email = c.Email
            }).ToList();
            return Ok(clientesDatos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDTO>> Get(int id)
        {
            var clientes = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clientes == null)
                return NotFound();

            var dto = new ClienteDTO
            {
                Id = clientes.Id,
                Nombre = clientes.Nombre,
                Email = clientes.Email
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<ClienteDTO>> Post(ClienteDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = new Client
            {
                Nombre = dto.Nombre!,
                Email = dto.Email!,
                FechaRegistro = DateTime.UtcNow
            };

            _context.Clientes.Add(client);
            await _context.SaveChangesAsync();

            var response = new ClienteDTO
            {
                Id = client.Id, 
                Nombre = client.Nombre,
                Email = client.Email,
                FechaRegistro = client.FechaRegistro
            };

            return CreatedAtAction(nameof(Get), new { id = client.Id }, response);
>>>>>>> 1207c7b247e4aae2cdebf2a3f5b43888870a5e16
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Client cliente)
        {
<<<<<<< HEAD
            if (cliente == null)
            {
                return BadRequest("Debe enviar los datos del cliente.");
            }

            if (id != cliente.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cliente enviado.");
            }

            if (string.IsNullOrWhiteSpace(cliente.Name))
            {
                return BadRequest("El nombre no puede estar vacío.");
            }

            if (string.IsNullOrWhiteSpace(cliente.Email))
            {
                return BadRequest("El email no puede estar vacío.");
            }

            var clienteExistente = await _context.Clients.FindAsync(id);

            if (clienteExistente == null)
            {
                return NotFound("No se encontró el cliente que intenta actualizar.");
            }

            clienteExistente.Name = cliente.Name;
            clienteExistente.Email = cliente.Email;

            _context.Entry(clienteExistente).State = EntityState.Modified;

            await _context.SaveChangesAsync();

=======
            if (id != cliente.Id)
                return BadRequest();
            _context.Entry(cliente).State = EntityState.Modified;
            await _context.SaveChangesAsync();
>>>>>>> 1207c7b247e4aae2cdebf2a3f5b43888870a5e16
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
<<<<<<< HEAD
            var cliente = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cliente == null)
            {
                return NotFound(); 
            }
       
            _context.Clients.Remove(cliente);
            await _context.SaveChangesAsync();
            return NoContent();
        }
=======
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return NoContent();
        }

>>>>>>> 1207c7b247e4aae2cdebf2a3f5b43888870a5e16
    }
}