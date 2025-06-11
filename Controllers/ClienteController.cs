using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<ActionResult<IEnumerable<ClienteDTO>>> Get()
        {
            var clientes = await _context.Clientes
                  .ToListAsync();
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
        public async Task<ActionResult<Client>> Post(Client client)
        {
            _context.Clientes.Add(client);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = client.Id }, client);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Client cliente)
        {
            if (id != cliente.Id)
                return BadRequest();
            _context.Entry(cliente).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }
            //Eliminar transaccion
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}