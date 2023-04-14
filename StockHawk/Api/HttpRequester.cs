using System.Text.Json;
using System.Text.Json.Serialization;

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
    
    public async Task<T> GetFromJsonAsync<T>(Uri endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
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

}