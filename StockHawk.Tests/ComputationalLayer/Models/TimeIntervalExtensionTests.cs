using NUnit.Framework;
using StockHawk.ComputationalLayer.Models;
using static StockHawk.ComputationalLayer.Models.TimeIntervalExtensions;

namespace StockHawk.Tests.ComputationalLayer.Models;

public class TimeIntervalExtensionTests
{
      [TestCase(TimeInterval.TwelveHour, 43200000)]
      [TestCase(TimeInterval.FourHour, 14400000)]
      [TestCase(TimeInterval.OneHour, 3600000)]
      [TestCase(TimeInterval.ThirtyMinute, 1800000)]
      [TestCase(TimeInterval.FifteenMinute, 900000)]
      [TestCase(TimeInterval.FiveMinute, 300000)]
      [TestCase(TimeInterval.ThreeMinute, 180000)]
      [TestCase(TimeInterval.OneMinute, 60000)]
      public void ConvertToMillis_GivenTimeInterval_ConvertToMilliseconds(string timeInterval, Int64 expected)
      {
            //Arrange, Act
            var actual = timeInterval.ConvertToMillis();
            
            //Assert
            Assert.AreEqual(actual, expected);
      }

      [TestCase(null)]
      public void ConvertToMillis_GivenNull_ThrowNullException(string data)
      {
            //Arrange, Act, Assert
            Assert.Throws<ArgumentNullException>(() => data.ConvertToMillis());
      }
}