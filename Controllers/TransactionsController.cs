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
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient = new HttpClient();

        public TransactionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> Get()
        {
            var transacciones = await _context.Transacciones.ToListAsync();
            var transaccionesDtos = transacciones.Select(t => new TransactionDto
            {
                Id = t.Id,
                CryptoCode = t.CryptoCode,
                Action = t.Accion,
                ClientId = t.ClienteId,
                CryptoAmount = t.Cantidad,
                MontoARS = t.Monto,
                FechaHora = t.FechaHora
            }).ToList();

            return Ok(transaccionesDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDto>> GetTransaction(int id)
        {
            var t = await _context.Transacciones.FindAsync(id);
            if (t == null) return NotFound();

            var dto = new TransactionDto
            {
                Id = t.Id,
                CryptoCode = t.CryptoCode,
                Action = t.Accion,
                ClientId = t.ClienteId,
                CryptoAmount = t.Cantidad,
                MontoARS = t.Monto,
                FechaHora = t.FechaHora
            };

            return Ok(dto);
        }

        [HttpGet("by-client/{clientId}")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetByClient(int clientId)
        {
            var transacciones = await _context.Transacciones
                .Where(t => t.ClienteId == clientId)
                .ToListAsync();

            if (transacciones == null || !transacciones.Any())
                return NotFound("No se encontraron transacciones para este cliente.");

            var dtos = transacciones.Select(t => new TransactionDto
            {
                Id = t.Id,
                CryptoCode = t.CryptoCode,
                Action = t.Accion,
                ClientId = t.ClienteId,
                CryptoAmount = t.Cantidad,
                MontoARS = t.Monto,
                FechaHora = t.FechaHora
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("precio/{cripto}")]
        public async Task<IActionResult> GetPrecioActual(string cripto)
        {
            try
            {
                string simbolo = cripto.ToLower() switch
                {
                    "bitcoin" => "btc",
                    "ethereum" => "eth",
                    "usdc" => "usdc",
                    _ => cripto.ToLower()
                };

                using var http = new HttpClient();
                var url = $"https://criptoya.com/api/{simbolo}/ars/1";
                var response = await http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return BadRequest("No se pudo obtener el precio actual");

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonDocument.Parse(json).RootElement;

                decimal total = 0;
                int count = 0;

                foreach (var exchange in data.EnumerateObject())
                {
                    if (exchange.Value.TryGetProperty("ask", out var ask))
                    {
                        total += ask.GetDecimal();
                        count++;
                    }
                }

                if (count == 0)
                    return BadRequest("No se encontraron precios válidos");

                decimal promedio = total / count;
                return Ok(promedio);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener precio: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostTransaccion([FromBody] TransactionDto dto)
        {
            if (dto == null)
                return BadRequest("Payload inválido.");

            if (dto.CryptoAmount <= 0)
                return BadRequest("La cantidad debe ser mayor a 0.");

            var actionNormalized = (dto.Action ?? "").Trim().ToLower();
            if (actionNormalized == "compra") actionNormalized = "buy";
            if (actionNormalized == "venta") actionNormalized = "sale";

            if (actionNormalized != "buy" && actionNormalized != "sale")
                return BadRequest("Acción inválida.");

            var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == dto.ClientId);
            if (!clienteExiste)
                return BadRequest("Cliente no encontrado.");

            if (actionNormalized == "sale")
            {
                var comprados = await _context.Transacciones
                    .Where(t => t.ClienteId == dto.ClientId && t.CryptoCode == dto.CryptoCode && t.Accion == "buy")
                    .SumAsync(t => (decimal?)t.Cantidad) ?? 0;

                var vendidos = await _context.Transacciones
                    .Where(t => t.ClienteId == dto.ClientId && t.CryptoCode == dto.CryptoCode && t.Accion == "sale")
                    .SumAsync(t => (decimal?)t.Cantidad) ?? 0;

                var saldo = comprados - vendidos;

                if (dto.CryptoAmount > saldo)
                    return BadRequest($"No tiene saldo suficiente. Disponible: {saldo:N8}");
            }

            if (dto.MontoARS == 0)
            {
                var precioActual = await GetPrecioActual(dto.CryptoCode) as OkObjectResult;
                if (precioActual != null)
                    dto.MontoARS = Convert.ToDecimal(precioActual.Value);
            }

            var transaccion = new Transaccion
            {
                ClienteId = dto.ClientId,
                CryptoCode = dto.CryptoCode.ToLower(),
                Accion = actionNormalized,
                Cantidad = dto.CryptoAmount,
                Monto = dto.MontoARS,
                FechaHora = dto.FechaHora
            };

            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Transacción registrada con éxito.", transaccion });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TransactionDto dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos.");

            var transaccion = await _context.Transacciones.FindAsync(id);
            if (transaccion == null)
                return NotFound("La transacción no existe.");

            string actionNormalized = null;
            if (!string.IsNullOrWhiteSpace(dto.Action))
            {
                actionNormalized = dto.Action.Trim().ToLower();
                if (actionNormalized == "compra") actionNormalized = "buy";
                if (actionNormalized == "venta") actionNormalized = "sale";

                if (actionNormalized != "buy" && actionNormalized != "sale")
                    return BadRequest("Acción inválida. Use 'buy' o 'sale'.");
            }

            if (dto.ClientId != 0 && dto.ClientId != transaccion.ClienteId)
            {
                var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == dto.ClientId);
                if (!clienteExiste)
                    return BadRequest("Cliente no encontrado.");
            }

            var willBeSale = actionNormalized == "sale" || (string.IsNullOrEmpty(dto.Action) && transaccion.Accion == "sale");
            var targetClientId = dto.ClientId != 0 ? dto.ClientId : transaccion.ClienteId;
            var targetCryptoCode = string.IsNullOrEmpty(dto.CryptoCode) ? transaccion.CryptoCode : dto.CryptoCode;

            if (willBeSale)
            {
                var comprados = await _context.Transacciones
                    .Where(t => t.ClienteId == targetClientId && t.CryptoCode == targetCryptoCode && t.Accion == "buy")
                    .SumAsync(t => (decimal?)t.Cantidad) ?? 0;

                var vendidos = await _context.Transacciones
                    .Where(t => t.ClienteId == targetClientId && t.CryptoCode == targetCryptoCode && t.Accion == "sale" && t.Id != id)
                    .SumAsync(t => (decimal?)t.Cantidad) ?? 0;

                var saldo = comprados - vendidos;
                var cantidadNueva = dto.CryptoAmount != 0 ? dto.CryptoAmount : transaccion.Cantidad;

                if (cantidadNueva > saldo)
                    return BadRequest($"No tiene saldo suficiente. Disponible: {saldo:N8}");
            }

            if (!string.IsNullOrEmpty(dto.CryptoCode))
                transaccion.CryptoCode = dto.CryptoCode.ToLower();

            if (!string.IsNullOrEmpty(actionNormalized))
                transaccion.Accion = actionNormalized;

            if (dto.ClientId != 0)
                transaccion.ClienteId = dto.ClientId;

            if (dto.CryptoAmount != 0)
                transaccion.Cantidad = dto.CryptoAmount;

            if (dto.MontoARS != 0)
                transaccion.Monto = dto.MontoARS;

            if (dto.FechaHora != default(DateTime))
                transaccion.FechaHora = dto.FechaHora;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Transacción actualizada con éxito", transaccion });
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentTransactions()
        {
            var transacciones = await _context.Transacciones
                .OrderByDescending(t => t.FechaHora)
                .Take(5)
                .ToListAsync();

            var resultado = transacciones.Select(t => new
            {
                id = t.Id,
                clientName = $"Cliente #{t.ClienteId}",
                clientEmail = "-",
                type = t.Accion,
                crypto = t.CryptoCode,
                amount = t.Cantidad,
                valueUsd = t.Monto
            });

            return Ok(resultado);
        }

        [HttpGet("precio/historial/{cripto}")]
        public async Task<IActionResult> GetHistorialPrecio(string cripto)
        {
            try
            {
                using var httpClient = new HttpClient();

                var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "btc", "bitcoin" },
                    { "eth", "ethereum" },
                    { "usdc", "usd-coin" },
                };

                if (!mapping.ContainsKey(cripto))
                    return BadRequest("Criptomoneda no soportada.");

                var coinId = mapping[cripto];
                var url = $"https://api.coingecko.com/api/v3/coins/{coinId}/market_chart?vs_currency=usd&days=7";

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return BadRequest("No se pudo obtener el historial de precios desde CoinGecko.");

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonDocument.Parse(json).RootElement;

                var precios = data.GetProperty("prices")
                                  .EnumerateArray()
                                  .Select(p => new
                                  {
                                      fecha = DateTimeOffset.FromUnixTimeMilliseconds(p[0].GetInt64()).DateTime,
                                      precio = p[1].GetDecimal()
                                  })
                                  .ToList();

                return Ok(precios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.Transacciones.FindAsync(id);
            if (transaction == null) return NotFound();

            _context.Transacciones.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchTransaction(int id, [FromBody] TransactionDto dto)
        {
            var transaction = await _context.Transacciones.FindAsync(id);
            if (transaction == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.CryptoCode))
                transaction.CryptoCode = dto.CryptoCode;

            if (!string.IsNullOrEmpty(dto.Action))
                transaction.Accion = dto.Action;

            if (dto.ClientId != 0)
                transaction.ClienteId = dto.ClientId;

            if (dto.CryptoAmount != 0)
                transaction.Cantidad = dto.CryptoAmount;

            if (dto.MontoARS != 0)
                transaction.Monto = dto.MontoARS;

            if (dto.FechaHora != default(DateTime))
                transaction.FechaHora = dto.FechaHora;

            await _context.SaveChangesAsync();

            return Ok(transaction);
        }
    }
}
    

