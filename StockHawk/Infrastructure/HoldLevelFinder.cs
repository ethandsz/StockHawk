﻿using StockHawk.Api;
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

    public async Task<List<HoldLevel>> GetLevels(ByBitResponse response, string timeInterval)
    {
        var possibleLevels = IdentifyHighLevelHoldLevels(response.ByBitResult.Wicks);
        var lowerLevels = await ConvertHighLevelHoldLevels(possibleLevels, timeInterval);
       // var untestedHoldLevels = await GetUntestedLevels(possibleLevels);

        return lowerLevels;
    }

    /// <summary>
    /// Finds contiguous ranges of bullish or bearish wicks in a list of candlestick wicks.
    /// </summary>
    /// <param name="wicks">The list of candlestick wicks.</param>
    /// <returns>A list of candlestick wicks representing the range of each identified pattern.</returns>
    private List<HoldLevel> IdentifyHighLevelHoldLevels(List<CandleStick> wicks)
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

    private async Task<List<HoldLevel>> ConvertHighLevelHoldLevels(List<HoldLevel> highLevels, string timeInterval)
    {
        var milliSeconds = timeInterval.ConvertToMillis();
        var holdLevels = new List<HoldLevel>();
        var currentTime = await _httpRequester.GetCurrentTime();
        var numOfLevels = highLevels.Count;
        foreach (var holdLevel in highLevels)
        {
            var startTime = holdLevel.TimeStamp;
            var sticks = new List<CandleStick>();
            while (startTime < currentTime)
            {
                var requestedSticks =
                    await _httpRequester.RequestSticksWithTimeLimit(timeInterval, startTime.ToString(),
                        currentTime.ToString());
                startTime += milliSeconds * 1000;
                sticks.AddRange(requestedSticks.ByBitResult.Wicks);
            }
            
            var sticksToTestForInverseLevel = sticks.Where(s => s.High > holdLevel.Level && s.Low < holdLevel.Level && s.TimeStamp > holdLevel.TimeStamp).ToList();
                    
            if (!await IsTested(sticksToTestForInverseLevel, holdLevel, timeInterval))
            {
                holdLevels.Add(holdLevel);
            }

            numOfLevels--;
            Console.WriteLine($"Progress: {100 * (double)(highLevels.Count - numOfLevels) / highLevels.Count}");
        }
        return holdLevels;
    }

    // private async Task<List<HoldLevel>> GetUntestedLevels(List<HoldLevel> holdLevels)
    // {
    //     var unTestedLevels = new List<HoldLevel>();
    //     var currentTime = await _httpRequester.GetCurrentTime();
    //     var numOfLevels = holdLevels.Count;
    //     foreach (var holdLevel in holdLevels)
    //     {
    //         var startTime = holdLevel.TimeStamp;
    //         var sticks = new List<CandleStick>();
    //         while (startTime < currentTime)
    //         {
    //             var requestedSticks =
    //                 await _httpRequester.RequestSticksWithTimeLimit("1m", startTime.ToString(), (startTime + 60000000).ToString());
    //             startTime += 60000000;
    //             // var possibleSticksInRange = sticks.Where(w => w.IsBullish != holdLevel.IsInverse);
    //             // if (holdLevel.IsInverse)
    //             // {
    //             //     var firstNonBullishClosedBody = possibleSticksInRange.Where(p => p.Open < holdLevel.Level).FirstOrDefault();
    //             // }
    //             sticks.AddRange(requestedSticks.Result.Wicks);
    //         }
    //         var possibleSticks = sticks.Where(w => w.IsBullish != holdLevel.IsInverse);
    //         if (holdLevel.IsInverse)
    //         {
    //             var firstNonBullishClosedBody = possibleSticks.Where(p => p.Open < holdLevel.Level).FirstOrDefault();
    //             if (firstNonBullishClosedBody is not null)
    //             {
    //                 possibleSticks = sticks.Where(s => s.TimeStamp > firstNonBullishClosedBody.TimeStamp);
    //                 possibleSticks = possibleSticks.Where(w => w.IsBullish == holdLevel.IsInverse);
    //                 if (!possibleSticks.Any(p => p.High > holdLevel.Level * .9999))
    //                 {
    //                     unTestedLevels.Add(holdLevel);
    //                 }
    //             }
    //             continue;
    //         }
    //         
    //         var firstBullishClosedBody = possibleSticks.Where(p => p.Open > holdLevel.Level).FirstOrDefault();
    //         if (firstBullishClosedBody is not null)
    //         {
    //             possibleSticks = sticks.Where(s => s.TimeStamp > firstBullishClosedBody.TimeStamp);
    //             possibleSticks = possibleSticks.Where(w => w.IsBullish == holdLevel.IsInverse);
    //             if (!possibleSticks.Any(p => p.Low < holdLevel.Level * 1.0001))
    //             {
    //                 unTestedLevels.Add(holdLevel);
    //             }
    //         }
    //
    //         numOfLevels--;
    //         Console.WriteLine($"Progress: {(double)(holdLevels.Count - numOfLevels) / holdLevels.Count}");
    //     }
    //
    //     return unTestedLevels;
    // }
    
    private async Task<bool> IsTested(List<CandleStick> candleSticks, HoldLevel holdLevel, string timeInterval)
    {
        if (candleSticks.Count == 0)
        {
            return true;
        }
        await _httpRequester.GetCurrentTime();
        foreach (var candleStick in candleSticks)
        {
            var response = await _httpRequester.RequestSticksWithTimeLimit("1m", candleStick.TimeStamp.ToString(),
                (candleStick.TimeStamp + timeInterval.ConvertToMillis()).ToString());
            var sticks = response.ByBitResult.Wicks;
            var possibleSticks = sticks.Where(w => w.IsBullish != holdLevel.IsInverse);
            if (holdLevel.IsInverse)
            {
                var firstNonBullishClosedBody = possibleSticks.Where(p => p.Open < holdLevel.Level).FirstOrDefault();
                if (firstNonBullishClosedBody is not null)
                {
                    possibleSticks = sticks.Where(s => s.TimeStamp > firstNonBullishClosedBody.TimeStamp);
                    possibleSticks = possibleSticks.Where(w => w.IsBullish == holdLevel.IsInverse);
                    if (possibleSticks.Any(p => p.High > holdLevel.Level * .9999))
                    {
                        return true;
                    }
                }
                continue;
            }
            
            var firstBullishClosedBody = possibleSticks.Where(p => p.Open > holdLevel.Level).FirstOrDefault();
            if (firstBullishClosedBody is not null)
            {
                possibleSticks = sticks.Where(s => s.TimeStamp > firstBullishClosedBody.TimeStamp);
                possibleSticks = possibleSticks.Where(w => w.IsBullish == holdLevel.IsInverse);
                if (possibleSticks.Any(p => p.Low < holdLevel.Level * 1.0001))
                {
                    return true;
                }
            }
        }

        return false;
    }
    
}