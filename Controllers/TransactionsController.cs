
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models;
using TrabajoPractico.DTOs;
using TrabajoPractico.Models;

namespace TrabajoPractico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TransactionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> Get()
        {
            var transacciones = await _context.Transacciones
                  .ToListAsync();
            var transaccionesDtos = transacciones.Select(t => new TransactionDto
            {
                CryptoCode = t.CryptoCode,
                Accion = t.Accion,
                IdCliente = t.ClienteId,
                Cantidad = t.Cantidad,     
                MontoARS = t.Monto,
                FechaHora = t.FechaHora
            }).ToList();
            return Ok(transaccionesDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDto>> Get(int id)
        {
            var transacciones = await _context.Transacciones
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transacciones == null)
                return NotFound();

            var dto = new TransactionDto
            {
                CryptoCode = transacciones.CryptoCode,
                Accion = transacciones.Accion,
                IdCliente = transacciones.ClienteId,
                Cantidad = transacciones.Cantidad,
                MontoARS = transacciones.Monto,
                FechaHora = transacciones.FechaHora
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<Transaccion>> Post(Transaccion tranc)
        {
            _context.Transacciones.Add(tranc);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = tranc }, tranc);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Transaccion transaccion)
        {
            if (id != transaccion.Id)
                return BadRequest();
            _context.Entry(transaccion).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var transaccion = await _context.Transacciones
                .FirstOrDefaultAsync(t => t.Id == id);
            if (transaccion == null)
            {
                return NotFound();
            }
            //Eliminar transaccion
            _context.Transacciones.Remove(transaccion);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
