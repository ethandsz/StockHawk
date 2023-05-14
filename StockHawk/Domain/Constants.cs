namespace StockHawk.Api;

public class Constants
{
    public const string BaseUrl = "https://api.bybit.com/spot/v3/public";

    public const string RequestSticksWithLimit = "{0}/quote/kline?symbol=BTCUSDT&interval={1}&limit={2}";
    
    public const string RequestSticksWithTimeLimit = "{0}/quote/kline?symbol=BTCUSDT&interval={1}&startTime={2}&endTime={3}";

    public const string RequestTicker = "{0}/quote/ticker/24hr?symbol=BTCUSDT";
}