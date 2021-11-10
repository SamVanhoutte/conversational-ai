using System.Threading.Tasks;
using NasdaqBot.Models;

namespace NasdaqBot
{
    public interface IMarketService
    {
        Task<StockResult> GetStockResultAsync(string stockSymbol);
        Task<StockOrder> PlaceOrderAsync(BuyStockRequest request);
    }
}