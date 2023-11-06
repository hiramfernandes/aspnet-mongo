using aspnet_mongo.Models;

namespace aspnet_mongo.Repository
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
