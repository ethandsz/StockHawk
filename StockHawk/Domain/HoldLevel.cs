namespace StockHawk.ComputationalLayer.Models;

public class HoldLevel
{
    public bool IsInverse { get; set; }
    public double Level { get; set; }
    public Int64 TimeStamp { get; set; }
}