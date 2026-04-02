using aspnet_mongo.Models;
using Purchases.Application.Models;

namespace Purchases.Application.Repository
{
    public class PurchasesRepository
    {
        public async Task<IEnumerable<Purchase>> GetPurchases() 
        {
            await Task.Delay(1000);

            var purchases = new List<Purchase>();


            return purchases; 
        }
    }
}
