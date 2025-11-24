namespace TrabajoPractico.DTOs
{
    public class TransactionResponseDto
    {
            public int Id { get; set; }
            public string Action { get; set; }            // "purchase" o "sale"
            public string Crypto_Code { get; set; }       // ej: BTC, ETH
            public decimal Crypto_Amount { get; set; }    // cantidad de cripto
            public string Money { get; set; }             // monto en ARS formateado
            public string Datetime { get; set; }          // fecha formateada ISO
    }
}
