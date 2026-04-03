using MongoDB.Driver;
using Purchases.Application.Contracts;
using Purchases.Application.Models;
using Purchases.Application.Models.DTO.Purchase;
using Purchases.Application.Models.DTO.Vendor;
using Purchases.Application.Repository;

namespace Purchases.Application.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchasesRepository _purchasesRepository;
        private readonly IVendorService _vendorService;

        public PurchaseService(
            IPurchasesRepository purchasesRepository,
            IVendorService vendorService)
        {
            _purchasesRepository = purchasesRepository;
            _vendorService = vendorService;
        }

        public async Task<IEnumerable<GetPurchaseDto>> GetAllAsync(int pageSize = 50)
        {
            var purchases = await _purchasesRepository.GetAllAsync(pageSize);

            // TODO: Optimize vendors retrieval
            var vendors = await _vendorService.GetAllAsync();
            var purchaseDtos = purchases.Select(purchase => MapFrom(purchase, vendors));

            return purchaseDtos;
        }

        public async Task<Purchase?> GetAsync(string id) =>
            await _purchasesRepository.GetAsync(id);

        public async Task CreateAsync(Purchase newPurchase) =>
            await _purchasesRepository.CreateAsync(newPurchase);

        public async Task UpdateAsync(string id, Purchase updatedPurchase) =>
            await _purchasesRepository.UpdateAsync(id, updatedPurchase);

        public async Task RemoveAsync(string id) =>
            await _purchasesRepository.RemoveAsync(id);

        private GetPurchaseDto MapFrom(Purchase purchase, IEnumerable<GetVendorDto> vendors)
        {
            var vendor = vendors.FirstOrDefault(x => x.Id == purchase.VendorId);
            return new GetPurchaseDto()
            {
                Id = purchase.Id,
                PurchaseDate = purchase.PurchaseDate,
                PurchaseUrl = purchase.PurchaseUrl,
                TotalAmount = purchase.TotalAmount,
                VendorId = vendor?.Id,
                VendorName = vendor?.Name,
                VendorLogoUrl = vendor?.LogoUrl,
                Items = purchase.Items?.Select(item =>
                    new PurchaseItemDto()
                    {
                        Description = item.Description,
                        UnitPrice = item.UnitPrice,
                        Tags = item.Tags
                    }
                ).ToArray()
            };
        }
    }
}

