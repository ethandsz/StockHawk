using StockHawk.Api;
using StockHawk.ComputationalLayer;
using StockHawk.ComputationalLayer.Models;

var httpRequester = HttpRequester.Instance();
var holdLevelFinder = HoldLevelFinder.Instance();
var timeInterval = TimeInterval.OneHour;
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