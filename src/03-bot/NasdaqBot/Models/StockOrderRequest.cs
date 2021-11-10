namespace NasdaqBot.Models
{
    public abstract class StockOrderRequest
    {
        public int Amount { get; set; }
        public string StockSymbol { get; set; }
        public double OrderLimit { get; set; }
    }
}