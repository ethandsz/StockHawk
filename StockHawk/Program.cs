using StockHawk.Api;
using StockHawk.Api.Models;
using StockHawk.ComputationalLayer;

var httpRequester = HttpRequester.Instance();
var holdLevelFinder = HoldLevelFinder.Instance();
var endpoint = new Uri("https://api.bybit.com/spot/v3/public/quote/kline?symbol=BTCUSDT&interval=1h&limit=25");
try 
{
    var result = await httpRequester.GetFromJsonAsync<ByBitResponse>(endpoint);
    var wicks = result.Result.Wicks;
    holdLevelFinder.IdentifyWickRanges(wicks!);
    Console.WriteLine("Ry");
} 
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while retrieving the data: {ex.Message}");
}