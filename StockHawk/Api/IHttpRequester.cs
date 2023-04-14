namespace StockHawk.Api;

public interface IHttpRequester
{ 
    Task<T> GetFromJsonAsync<T>(Uri endpoint);
}