using StockHawk.Api.Models;
using StockHawk.ComputationalLayer.Models;

namespace StockHawk.ComputationalLayer;

public interface IHoldLevelFinder
{ 
    Task<List<HoldLevel>> GetLevels(ByBitResponse response, string timeInterval);
}