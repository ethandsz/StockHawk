using System.Text.Json;
using System.Text.Json.Serialization;
using StockHawk.Api.Models;
using static StockHawk.Api.Constants;

namespace StockHawk.Api;

public sealed class HttpRequester : IHttpRequester
{

    private static HttpRequester? _instance;
    private readonly HttpClient _httpClient;
    
    public HttpRequester(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public static HttpRequester Instance()
    {
        if (_instance == null)
        {
            _instance = new HttpRequester(new HttpClient());
        }
        return _instance;
    }

    private readonly JsonSerializerOptions _jsonSerializerOptions = new() {NumberHandling = JsonNumberHandling.AllowReadingFromString};
    
    private async Task<T> GetFromJsonAsync<T>(Uri endpoint)
    {
        try
        {
            Console.WriteLine($"Sending request: {endpoint}");
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            Console.WriteLine("Success!");
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions)!;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"An error occurred while sending the request. Error: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new JsonException($"An error occurred while deserializing the JSON response. Error: {ex.Message}", ex);
        }
    }

    public async Task<ByBitResponse> RequestSticksWithLimit(string timeInterval, string stickLimit)
    {
        var endpoint = new Uri(string.Format(Constants.RequestSticksWithLimit, BaseUrl, timeInterval, stickLimit));
        return await GetFromJsonAsync<ByBitResponse>(endpoint);
    }

    public async Task<ByBitResponse> RequestSticksWithTimeLimit(string timeInterval, string startTime, string? endTime)
    {
        var endpoint = new Uri(String.Format(Constants.RequestSticksWithTimeLimit, BaseUrl, timeInterval, startTime, endTime));
        return await GetFromJsonAsync<ByBitResponse>(endpoint);
    }

    public async Task<Int64> GetCurrentTime()
    {
        var endpoint = new Uri(String.Format(RequestTicker, BaseUrl));
        var result = await GetFromJsonAsync<ByBitResponse>(endpoint);
        return result.Time;
    }
}