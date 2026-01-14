using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models;
using System.Net.Http;
using System.Text.Json;
using TrabajoPractico.DTOs;
using TrabajoPractico.Models;

namespace TrabajoPractico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<TransactionDTO>>> GetTransactionsByClientId(int clientId)
        {
           
            var transacciones = await _context.Transaccions
                                             .Where(t => t.ClientId == clientId)
                                             .OrderByDescending(t => t.Datetime)
                                             .ToListAsync();

            if (!transacciones.Any())
            {
                return Ok(new List<TransactionDTO>());
            }

            var transaccionesDto = transacciones.Select(t => new TransactionDTO
            {
                Id = t.Id,
                CryptoCode = t.CryptoCode,
                Action = t.Action,
                CryptoAmount = t.CryptoAmount,
                Money = t.Money,
                Datetime = t.Datetime,
                ClientId = t.ClientId
            }).ToList();

            return Ok(transaccionesDto);
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionDTO>>> Get()
        {
            var transacciones = await _context.Transaccions.ToListAsync();

            var transaccionesDto = transacciones.Select(t => new TransactionDTO
            {
                Id = t.Id,
                CryptoCode = t.CryptoCode,
                Action = t.Action,
                CryptoAmount = t.CryptoAmount,
                Money = t.Money,
                Datetime = t.Datetime,
                ClientId = t.ClientId
            }).ToList();
            return Ok(transaccionesDto);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDTO>> GetTransactionById(int id)
        {
            var transaction = await _context.Transaccions
                                            .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            var transactionDto = new TransactionDTO
            {
                Id = transaction.Id,
                CryptoCode = transaction.CryptoCode,
                Action = transaction.Action,
                CryptoAmount = transaction.CryptoAmount,
                Money = transaction.Money,
                Datetime = transaction.Datetime,
                ClientId = transaction.ClientId
            };

            return Ok(transactionDto);
        }
        [HttpPost]
        public async Task<ActionResult<TransactionDTO>> Post(Transaction transaccion)
        {
            if (string.IsNullOrWhiteSpace(transaccion.CryptoCode))
            {
                return BadRequest("El codigo de la cripto no puede ser nulo");
            }
            if (transaccion.Action != "buy" || transaccion.Action != "sale")
            {
                return BadRequest("Es obligatorio indicar una accion");
            }
            if (transaccion.CryptoAmount <= 0)
            {
                return BadRequest("La cantidad de cripto debe ser mayor a 0");
            }

            var clienteExiste = await _context.Clients.AnyAsync(c => c.Id == transaccion.ClientId);
            if (!clienteExiste)
            {
                return BadRequest("El cliente no existe");
            }

            if (transaccion.Action == "sale")
            {
                var transaccionesCliente = await _context.Transaccions
                    .Where(t => t.ClientId == transaccion.ClientId &&
                                t.CryptoCode == transaccion.CryptoCode)
                    .ToListAsync();

                decimal saldoActual = 0;

                foreach (var mov in transaccionesCliente)
                {
                    if (mov.Action == "buy")
                        saldoActual += mov.CryptoAmount;   
                    else if (mov.Action == "sale")
                        saldoActual -= mov.CryptoAmount;  
                }


                if (saldoActual < transaccion.CryptoAmount)
                    return BadRequest("No tenes suficiente saldo para vender esa cantidad.");
            }


            transaccion.Datetime = DateTime.UtcNow;

            _context.Transaccions.Add(transaccion);
            await _context.SaveChangesAsync(); //guardar en la bd



            var respuesta = new TransactionDTO
            {
                Id = transaccion.Id,
                CryptoCode = transaccion.CryptoCode,
                Action = transaccion.Action,
                CryptoAmount = transaccion.CryptoAmount,
                Money = transaccion.Money,
                Datetime = transaccion.Datetime,
                ClientId = transaccion.ClientId
            };

            return CreatedAtAction(nameof(GetTransactionById), new { id = transaccion.Id }, respuesta);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return BadRequest("El id no coincide.");
            }

            var original = await _context.Transaccions.FindAsync(id);
            if (original == null)
            {
                return NotFound("No existe la transaccion.");
            }
                

            if (transaction.Action == "sale")
            {
                var lista = await _context.Transaccions
                    .Where(t => t.ClientId == original.ClientId &&
                                t.CryptoCode == transaction.CryptoCode)
                    .ToListAsync();

                decimal saldo = 0;

                foreach (var t in lista)
                {
                    if (t.Action == "buy")
                        saldo += t.CryptoAmount;
                    else
                        saldo -= t.CryptoAmount;
                }

                if (original.Action == "buy")
                {
                    saldo -= original.CryptoAmount;
                }
                else
                {
                    saldo += original.CryptoAmount;
                }
                    

                if (transaction.CryptoAmount > saldo)
                {
                    return BadRequest("Saldo insuficiente para vender esa cantidad.");
                }
            }

            original.CryptoCode = transaction.CryptoCode;
            original.Action = transaction.Action;
            original.CryptoAmount = transaction.CryptoAmount;
            original.Money = transaction.Money;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _context.Transaccions.FindAsync(id);

            if (transaction == null)
                return NotFound();

            _context.Transaccions.Remove(transaction);

            await _context.SaveChangesAsync();

            return NoContent();
        }


    }
}
    

