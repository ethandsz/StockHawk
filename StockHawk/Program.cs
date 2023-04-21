using Bybit.Net.Clients;
using Bybit.Net.Enums;
using Bybit.Net.Objects;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using StockHawk.Api;
using StockHawk.ComputationalLayer;
using StockHawk.ComputationalLayer.Models;

var httpRequester = HttpRequester.Instance();
var holdLevelFinder = HoldLevelFinder.Instance();
var timeInterval = TimeInterval.FifteenMinute;
var client = new BybitClient(new BybitClientOptions()
{
    LogLevel = LogLevel.Debug,
    ApiCredentials = new ApiCredentials("API-KEY", "SECRET"),
    SpotApiOptions = new RestApiClientOptions()
    {
        ApiCredentials = new ApiCredentials("API-KEY", "SECRET")              ,
        BaseAddress = BybitApiAddresses.Default.SpotRestClientAddress
    }
});

try
{
    var orderId = "26621952";
    while (true)
    {
        var currentOrders = await client.SpotApiV3.Trading.GetOpenOrdersAsync(orderCategory:1);
        if (currentOrders.Data is not null)
        {
            if (currentOrders.Data.Where(o => o.IsWorking).ToList().Count > 1)
            {
                var oldestId = currentOrders.Data.MinBy(d => d.UpdateTime).Id;
                var cancelOrder =
                    await client.SpotApiV3.Trading.CancelOrderAsync(orderCategory: 1,
                        orderId: Convert.ToInt64(oldestId));

            }
        }

        var result = await httpRequester.RequestSticksWithLimit(timeInterval, "500"); //12:00 at 4/15/2023 15m 180sticks
        var levels = await holdLevelFinder.GetLevels(result, timeInterval);
       

        var accountRequest = await client.SpotApiV3.Account.GetBalancesAsync();
        var priceRequest = await client.SpotApiV3.ExchangeData.GetPriceAsync("BTCUSDT");

        var currentPrice = decimal.ToDouble(priceRequest.Data.Price);
        levels = levels.Select(l =>
        {
            if (l.IsInverse)
            {
                if (currentPrice < l.Level)
                {
                    return l;
                }
            }

            if (!l.IsInverse)
            {
                if (currentPrice > l.Level)
                {
                    return l;
                }
            }

            return null;
        }).ToList()!;
        foreach (var level in levels)
        {
            Console.WriteLine($"{level.Level} - {level.TimeStamp} - {level.IsInverse}");
        }
        var closestLevel = levels.OrderBy(i => Math.Abs(currentPrice - i.Level)).First();

        var orderSide = closestLevel.IsInverse ? OrderSide.Sell : OrderSide.Buy;
        var assetRequired = closestLevel.IsInverse ? "BTC" : "USDT";
        var accountValue = decimal.ToDouble(accountRequest.Data.First(b => b.Asset == assetRequired).Total);
        
        var limitPrice = closestLevel.IsInverse ? closestLevel.Level * 0.999 : closestLevel.Level * 1.001;
        var triggerPrice = closestLevel.IsInverse ? closestLevel.Level * 0.998 : closestLevel.Level * 1.002;
        
        var quantity = closestLevel.IsInverse ? (decimal)(accountValue * 0.5) : (decimal)(accountValue * 0.5 / limitPrice);
        quantity = Math.Round(quantity, 6, MidpointRounding.AwayFromZero);
        limitPrice = Math.Round(limitPrice, 2, MidpointRounding.AwayFromZero);
        triggerPrice = Math.Round(triggerPrice, 2, MidpointRounding.AwayFromZero);
        var placeOrder = await client.SpotApiV3.Trading.PlaceOrderAsync("BTCUSDT", orderSide, OrderType.Limit, quantity,
            (decimal)limitPrice, TimeInForce.GoodTillCanceled, orderCategory: 1, triggerPrice: (decimal)triggerPrice);
        orderId = placeOrder.Data.Id;
    }
} 
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while retrieving the data: {ex.Message}");
}