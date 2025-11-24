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


        // GET transacciones por cliente
        [HttpGet("by-client/{id}")]
        public async Task<ActionResult<IEnumerable<Transaccion>>> GetByClient(int id)
        {
            var transacciones = await _context.Transacciones
                .Where(t => t.ClienteId == id)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();

            if (transacciones == null || !transacciones.Any())
                return NotFound("No hay transacciones para este cliente.");

            return Ok(transacciones);
        }




        [HttpGet("precio/{cripto}")]
        public async Task<IActionResult> GetPrecioActual(string cripto)
        {
            try
            {
                // Normalizamos los nombres
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
        // POST nueva transacción
        [HttpPost]
        public async Task<IActionResult> PostTransaccion([FromBody] TransactionDto dto)
        {
            // Validar la cantidad
            if (dto.CryptoAmount <= 0)
                return BadRequest("La cantidad debe ser mayor a 0.");

            // Validar la acción
            if (dto.Action != "buy" && dto.Action != "sale")
                return BadRequest("Acción inválida. Debe ser 'buy' o 'sale'.");

            // Verificamos existencia del cliente
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == dto.ClientId);
            if (!clienteExiste)
                return BadRequest("Cliente no encontrado.");

            // Validar saldo en caso de venta
            if (dto.Action == "sale")
            {
                var comprados = await _context.Transacciones
                    .Where(t => t.ClienteId == dto.ClientId && t.CryptoCode == dto.CryptoCode && t.Accion == "buy")
                    .SumAsync(t => (decimal?)t.Cantidad) ?? 0;

                var vendidos = await _context.Transacciones
                    .Where(t => t.ClienteId == dto.ClientId && t.CryptoCode == dto.CryptoCode && t.Accion == "sale")
                    .SumAsync(t => (decimal?)t.Cantidad) ?? 0;

                var saldoDisponible = comprados - vendidos;

                if (dto.CryptoAmount > saldoDisponible)
                    return BadRequest($"No es posible realizar la venta. El saldo disponible de {dto.CryptoCode.ToUpper()} es insuficiente. Disponible: {saldoDisponible:N8}.");
            }

            // Obtener precio desde CriptoYa
            decimal precioARS;
            try
            {
                var crypto = dto.CryptoCode.ToLower();
                var url = $"https://criptoya.com/api/binance/{crypto}/ars";

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, $"Error desde CriptoYa: {content}");

                var data = System.Text.Json.JsonSerializer.Deserialize<CriptoYaPriceDto>(
                    content,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (data == null || data.Ask <= 0)
                    return BadRequest($"No se pudo obtener un precio válido desde CriptoYa. Respuesta: {content}");

                precioARS = data.Ask;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener el precio desde CriptoYa: {ex.Message}");
            }

            var montoTotal = Math.Round(dto.CryptoAmount * precioARS, 2);

            var transaccion = new Transaccion
            {
                ClienteId = dto.ClientId,
                CryptoCode = dto.CryptoCode.ToLower(),
                Accion = dto.Action,
                Cantidad = dto.CryptoAmount,
                Monto = montoTotal,
                FechaHora = dto.FechaHora
            };

            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Transacción registrada con éxito.", transaccion });
        }

        // PUT modificar transacción
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Transaccion transaccion)
        {
            if (id != transaccion.Id)
                return BadRequest();
           
            _context.Entry(transaccion).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE eliminar transacción
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var transaccion = await _context.Transacciones.FindAsync(id);

            if (transaccion == null)
                return NotFound();

            _context.Transacciones.Remove(transaccion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        public class CriptoYaPriceDto
        {
            public decimal Ask { get; set; }
            public decimal Bid { get; set; }
            public decimal Last { get; set; }
        }


        // 🕒 GET: api/Transactions/recent
        // Devuelve las últimas 5 transacciones registradas
        // ===============================================================
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
                clientEmail = "-", // lo podés reemplazar si tenés tabla Clientes
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

                // Mapeo de símbolos a IDs de CoinGecko
                var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "btc", "bitcoin" },
            { "eth", "ethereum" },
            { "usdc", "usd-coin" },
            
        };

                if (!mapping.ContainsKey(cripto))
                    return BadRequest("Criptomoneda no soportada.");

                var coinId = mapping[cripto];

                // Endpoint de CoinGecko: precios históricos (últimos 7 días)

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
    }
}
    

