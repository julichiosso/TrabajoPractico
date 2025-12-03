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


        public ClienteController(AppDbContext context)
        {
            _context = context;
        }

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
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Client cliente)
        {
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

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
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
    }
}