using StockHawk.Api.Models;

namespace StockHawk.ComputationalLayer;

public class HoldLevelFinder : IHoldLevelFinder
{

    private static HoldLevelFinder? _instance;
    
    private HoldLevelFinder()
    {
        
    }

    public static HoldLevelFinder Instance()
    {
        if (_instance == null)
        {
            _instance = new HoldLevelFinder();
        }

        return _instance;
    }
    
    /// <summary>
    /// Finds contiguous ranges of bullish or bearish wicks in a list of candlestick wicks.
    /// </summary>
    /// <param name="wicks">The list of candlestick wicks.</param>
    /// <returns>A list of candlestick wicks representing the range of each identified pattern.</returns>
    public List<CandleStick> IdentifyWickRanges(List<CandleStick> wicks)
    {
        List<CandleStick> holdLevels = new List<CandleStick>();
        bool isInsideRange = false;

        // Iterate through the list of wicks from end to beginning.
        for (int i = wicks.Count - 2; i >= 0; i--)
        {
            CandleStick currentCandleStick = wicks[i];
            CandleStick previousCandleStick = wicks[i + 1];

            if (currentCandleStick.IsBullish == previousCandleStick.IsBullish)
            {
                // The current wick is part of the same pattern as the previous wick.
                isInsideRange = true;
            }
            else if (isInsideRange)
            {
                // The current wick is the end of a pattern.
                holdLevels.Add(previousCandleStick);
                isInsideRange = false;
            }
        }

        return holdLevels;
    }
}