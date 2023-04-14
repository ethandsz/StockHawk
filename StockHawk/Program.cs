using StockHawk.Api;
using StockHawk.ComputationalLayer;

var httpRequester = HttpRequester.Instance();
var holdLevelFinder = HoldLevelFinder.Instance();
try
{
    var result = await httpRequester.RequestSticksWithLimit("1m", "60");
    var levels = await holdLevelFinder.GetLevels(result);
    Console.WriteLine("Ry");
} 
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while retrieving the data: {ex.Message}");
}