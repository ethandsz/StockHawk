using StockHawk.Api.Models;

namespace StockHawk.ComputationalLayer;

public interface IHoldLevelFinder
{ 
    List<CandleStick> IdentifyWickRanges(List<CandleStick> wicks);
}