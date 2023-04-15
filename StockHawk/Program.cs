using StockHawk.Api;
using StockHawk.ComputationalLayer;

var httpRequester = HttpRequester.Instance();
var holdLevelFinder = HoldLevelFinder.Instance();
try
{
    var result = await httpRequester.RequestSticksWithLimit("1h", "500"); //12:00 at 4/15/2023 15m 180sticks
    var levels = await holdLevelFinder.GetLevels(result);
    foreach (var level in levels)
    {
        Console.WriteLine($"{level.Level} - {level.TimeStamp}");
    }
} 
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while retrieving the data: {ex.Message}");
}