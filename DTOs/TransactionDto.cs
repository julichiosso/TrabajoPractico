namespace TrabajoPractico.DTOs
{
    public class TransactionDto
    {
        public string CryptoCode { get; set; } = string.Empty;        // ejemplo: "usdc"
        public string Accion { get; set; } = string.Empty;          // "purchase" o "sale"
        public int IdCliente { get; set; }
        public decimal Cantidad { get; set; }

        //public int ClientId { get; set; }                // ID del cliente
        public decimal MontoARS { get; set; }        // Cantidad de criptomonedas
        public DateTime FechaHora { get; set; }               // Fecha/hora de la operación
    }
}
