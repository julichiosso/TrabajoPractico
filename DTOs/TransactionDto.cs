using System.Text.Json.Serialization;
<<<<<<< HEAD
namespace TrabajoPractico.DTOs
{
    public class TransactionDTO
    {
        public int Id { get; set; }
        public string CryptoCode { get; set; }
        public string Action { get; set; }
        public decimal CryptoAmount { get; set; }
        public decimal Money { get; set; }

        public DateTime Datetime { get; set; }

        public int ClientId { get; set; }

    }
}



=======

public class TransactionDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("cryptoCode")]
    public string CryptoCode { get; set; } = string.Empty;

    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("clientId")]
    public int ClientId { get; set; }

    [JsonPropertyName("cryptoAmount")]
    public decimal CryptoAmount { get; set; }

    [JsonPropertyName("montoARS")]
    public decimal MontoARS { get; set; }

    [JsonPropertyName("fechaHora")]
    public DateTime FechaHora { get; set; }
}
>>>>>>> 1207c7b247e4aae2cdebf2a3f5b43888870a5e16
