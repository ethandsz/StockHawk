using StockHawk.Api;
using StockHawk.Api.Models;
using StockHawk.ComputationalLayer.Models;

namespace StockHawk.ComputationalLayer;

public class HoldLevelFinder : IHoldLevelFinder
{

    private static HoldLevelFinder? _instance;
    private HttpRequester _httpRequester = HttpRequester.Instance();

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

    public async Task<List<HoldLevel>> GetLevels(ByBitResponse response)
    {
        var possibleLevels = IdentifyWickRanges(response.Result.Wicks);
        var untestedHoldLevels = await GetUntestedLevels(possibleLevels);
        
        throw new NotImplementedException();
    }

    /// <summary>
    /// Finds contiguous ranges of bullish or bearish wicks in a list of candlestick wicks.
    /// </summary>
    /// <param name="wicks">The list of candlestick wicks.</param>
    /// <returns>A list of candlestick wicks representing the range of each identified pattern.</returns>
    private List<HoldLevel> IdentifyWickRanges(List<CandleStick> wicks)
    {
        List<CandleStick> possibleHoldLevelSticks = new List<CandleStick>();
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
                possibleHoldLevelSticks.Add(previousCandleStick);
                isInsideRange = false;
            }
        }
        
        var holdLevels = possibleHoldLevelSticks.Select(s =>
        {
            if (s.IsBullish)
            {
                return new HoldLevel { IsInverse = true, Level = s.Low, TimeStamp = s.TimeStamp};
            }

            return new HoldLevel { IsInverse = false, Level = s.High, TimeStamp = s.TimeStamp};
        }).ToList();

        return holdLevels;
    }

    private async Task<List<HoldLevel>> GetUntestedLevels(List<HoldLevel> holdLevels)
    {
        var unTestedLevels = new List<HoldLevel>();
        var currentTime = await _httpRequester.GetCurrentTime();
        foreach (var holdLevel in holdLevels)
        {
            var candleStickRange =
               await _httpRequester.RequestSticksWithTimeLimit("1m", holdLevel.TimeStamp.ToString(),  currentTime.ToString());
            var possibleSticks = candleStickRange.Result.Wicks.Where(w => w.IsBullish != holdLevel.IsInverse);
            if (holdLevel.IsInverse)
            {
                var firstNonBullishClosedBody = possibleSticks.Where(p => p.Open < holdLevel.Level).FirstOrDefault();
                if (firstNonBullishClosedBody is not null)
                {
                    candleStickRange = await _httpRequester.RequestSticksWithTimeLimit("1m",
                        firstNonBullishClosedBody.TimeStamp.ToString(), currentTime.ToString());
                    possibleSticks = candleStickRange.Result.Wicks.Where(w => w.IsBullish == holdLevel.IsInverse);
                    if (!possibleSticks.Any(p => p.High > holdLevel.Level * .9999))
                    {
                        unTestedLevels.Add(holdLevel);
                    }
                }
                continue;
            }
            
            var firstBullishClosedBody = possibleSticks.Where(p => p.Open > holdLevel.Level).FirstOrDefault();
            if (firstBullishClosedBody is not null)
            {
                candleStickRange = await _httpRequester.RequestSticksWithTimeLimit("1m",
                    firstBullishClosedBody.TimeStamp.ToString(), currentTime.ToString());
                possibleSticks = candleStickRange.Result.Wicks.Where(w => w.IsBullish == holdLevel.IsInverse);
                if (!possibleSticks.Any(p => p.Low < holdLevel.Level * 1.0001))
                {
                    unTestedLevels.Add(holdLevel);
                }
            }
        }

        return unTestedLevels;
    }
}