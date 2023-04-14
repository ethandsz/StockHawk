using System.Text.Json.Serialization;

namespace StockHawk.Api.Models;

public class CandleStick
{
    [JsonPropertyName("t")]
    public Int64 TimeStamp { get; set; }
    [JsonPropertyName("s")]
    public string Ticker { get; set; }
    [JsonPropertyName("c")]
    public double Close { get; set; }
    [JsonPropertyName("h")]
    public double High { get; set; }
    [JsonPropertyName("l")]
    public double Low { get; set; }
    [JsonPropertyName("o")]
    public double Open { get; set; }
    [JsonPropertyName("v")]
    public double Volume { get; set; }
    
    public bool IsBullish
    {
        get => Open < Close;
    }
    
}