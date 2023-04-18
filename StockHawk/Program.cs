using Bybit.Net.Clients;
using Bybit.Net.Enums;
using Bybit.Net.Objects;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using StockHawk.Api;
using StockHawk.ComputationalLayer;
using StockHawk.ComputationalLayer.Models;

var httpRequester = HttpRequester.Instance();
var holdLevelFinder = HoldLevelFinder.Instance();
var timeInterval = TimeInterval.OneHour;
var client = new BybitClient(new BybitClientOptions()
{
    LogLevel = LogLevel.Debug,
    ApiCredentials = new ApiCredentials("API-KEY", "SECRET"),
    SpotApiOptions = new RestApiClientOptions()
    {
        ApiCredentials = new ApiCredentials("API-KEY", "SECRET")              ,
        BaseAddress = BybitApiAddresses.Default.SpotRestClientAddress
    }
});
var symbolData = await client.SpotApiV3.Trading.PlaceOrderAsync("BTCUSDT", OrderSide.Buy, OrderType.LimitMaker, 0.0001m, 15000, TimeInForce.GoodTillCanceled);

try
{
    var result = await httpRequester.RequestSticksWithLimit(timeInterval, "1000"); //12:00 at 4/15/2023 15m 180sticks
    var levels = await holdLevelFinder.GetLevels(result, timeInterval);
    foreach (var level in levels)
    {
        Console.WriteLine($"{level.Level} - {level.TimeStamp} - {level.IsInverse}");
    }
} 
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while retrieving the data: {ex.Message}");
}