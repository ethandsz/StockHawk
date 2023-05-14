using StockHawk.Api.Models;

namespace StockHawk.Api;

public interface IHttpRequester
{ 
    Task<ByBitResponse> RequestSticksWithLimit(string timeInterval, string stickLimit);
    Task<ByBitResponse> RequestSticksWithTimeLimit(string timeInterval, string startTime, string? endTime);
    Task<Int64> GetCurrentTime();
}