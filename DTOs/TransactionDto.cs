using System.Text.Json.Serialization;

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
