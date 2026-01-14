using System.Text.Json.Serialization;

namespace TrabajoPractico.DTOs
{
    public class CriptoYaPriceDto
    {
        [JsonPropertyName("ask")]
        public decimal Ask { get; set; }

        [JsonPropertyName("bid")]
        public decimal Bid { get; set; }

        [JsonPropertyName("totalAsk")]
        public decimal TotalAsk { get; set; }

        [JsonPropertyName("totalBid")]
        public decimal TotalBid { get; set; }

        [JsonPropertyName("time")]
        public long Time { get; set; }
    }
}
