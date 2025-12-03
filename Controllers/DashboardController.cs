using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models;
using System.Text.Json;

namespace ProyectoFinal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        public DashboardController(AppDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        // Devuelve todos los datos del dashboard en un único objeto JSON

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            var transacciones = await _context.Transaccions
                .OrderByDescending(t => t.Datetime)
                .ToListAsync();

            if (transacciones.Count == 0)
            {
                return Ok(new
                {
                    totalPortfolioValue = 0,
                    performance24h = 0,
                    totalPL = 0,
                    worstPerformer = "-",
                    recentTransactions = new List<object>()
                });
            }

            //Criptos unicas
            var cryptos = transacciones
                .Select(t => t.CryptoCode.ToLower())
                .Distinct()
                .ToList();

            // Obtener precios desde CriptoYa
            var preciosActuales = new Dictionary<string, decimal>();
            foreach (var crypto in cryptos)
            {
                try
                {
                    var url = $"https://criptoya.com/api/cripto/{crypto}/ars/1";
                    var json = await _httpClient.GetStringAsync(url);
                    using var doc = JsonDocument.Parse(json);
                    var precio = doc.RootElement.GetProperty("bid").GetDecimal();
                    preciosActuales[crypto] = precio;
                }
                catch
                {
                    preciosActuales[crypto] = 0;
                }
            }

            //Calcular valor total de la cartera
            decimal totalPortfolioValue = 0;
            foreach (var t in transacciones)
            {
                if (preciosActuales.TryGetValue(t.CryptoCode.ToLower(), out var precio))
                {
                    var valorActual = t.CryptoAmount * precio;
                    totalPortfolioValue += t.Action.ToLower() == "buy" ? valorActual : -valorActual;
                }
            }

            //Rendimiento y perdida/ganancia (simplificado)
            decimal performance24h = 0;
            decimal totalPL = 0;
            string worstPerformer = "-";

            //ultimas 5 transacciones
            var recientes = transacciones
                .Take(5)
                .Select(t => new
                {
                    clienteId = t.ClientId,
                    crypto = t.CryptoCode,
                    tipo = t.Action,
                    cantidad = t.CryptoAmount,
                    monto = t.Money,
                    fecha = t.Datetime.ToString("dd/MM/yyyy HH:mm")
                })
                .ToList();

            //Devolver el resumen
            return Ok(new
            {
                totalPortfolioValue,
                performance24h,
                totalPL,
                worstPerformer,
                recentTransactions = recientes
            });
        }

        // Endpoint específico que usa el front en Dashboard.vue

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var transacciones = await _context.Transaccions.ToListAsync();

            if (!transacciones.Any())
            {
                return Ok(new
                {
                    totalPortfolioValue = 0,
                    changePercent = 0,
                    dailyPerformance = 0,
                    dailyChange = 0,
                    totalPL = 0,
                    profitPercent = 0,
                    worstPerformer = "-",
                    worstPerformerChange = 0
                });
            }

            //Calcular valores totales
            var totalPortfolioValue = transacciones.Sum(t => t.Money);
            decimal totalCompras = transacciones.Where(t => t.Action.ToLower() == "buy" || t.Action.ToLower() == "compra").Sum(t => t.Money);
            decimal totalVentas = transacciones.Where(t => t.Action.ToLower() == "sale" || t.Action.ToLower() == "venta").Sum(t => t.Money);
            decimal totalPL = totalVentas - totalCompras;

            //Calcular el peor desempeño (el que menos monto tiene)
            string worstPerformer = transacciones
                .GroupBy(t => t.CryptoCode)
                .OrderBy(g => g.Sum(x => x.Money))
                .Select(g => g.Key)
                .FirstOrDefault() ?? "-";

            //Fecha actual
            var hoy = DateTime.Today;
            var transaccionesHoy = transacciones.Where(t => t.Datetime.Date == hoy);
            decimal dailyPerformance = transaccionesHoy.Sum(t => t.Money);

            //Construir objeto resumen
            var resumen = new
            {
                totalPortfolioValue,
                changePercent = 0,
                dailyPerformance,
                dailyChange = 0,
                totalPL,
                profitPercent = totalCompras == 0 ? 0 : (totalPL / totalCompras) * 100,
                worstPerformer,
                worstPerformerChange = 0
            };

            return Ok(resumen);
        }
    }
}