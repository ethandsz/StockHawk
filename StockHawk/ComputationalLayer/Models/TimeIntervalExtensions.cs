namespace StockHawk.ComputationalLayer.Models;

public static class TimeIntervalExtensions
{
    public static Int64 ConvertToMillis(this string timeInterval)
    {
        ArgumentNullException.ThrowIfNull(timeInterval);
        if (timeInterval.EndsWith('h'))
        {
            var hourInterval = Int16.Parse(timeInterval.TrimEnd('h'));
            return hourInterval * 3600000;
        }

        return Int16.Parse(timeInterval.TrimEnd('m')) * 60000;
    }
}