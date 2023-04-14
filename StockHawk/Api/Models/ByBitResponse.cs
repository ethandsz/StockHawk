using System.Text.Json.Serialization;

namespace StockHawk.Api.Models;

public class ByBitResponse
{
    [JsonPropertyName("retMsg")]
    public string StatusMessage { get; set; }
    
    [JsonPropertyName("result")]
    public Result Result { get; set; }
    
    [JsonPropertyName("time")]
    public Int64 Time { get; set; }
}